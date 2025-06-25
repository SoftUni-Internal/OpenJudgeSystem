namespace OJS.Services.Mentor.Business.Implementations;

using System.Globalization;
using System.Net;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Build.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OJS.Common.Constants;
using OJS.Common.Enumerations;
using OJS.Common.Extensions;
using OJS.Data.Models;
using OJS.Data.Models.Mentor;
using OJS.Services.Common.Data;
using OJS.Services.Infrastructure.Cache;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Mentor.Business;
using OJS.Services.Mentor.Models;
using OJS.Services.Ui.Business;
using OJS.Services.Ui.Data;
using OpenAI;
using OpenAI.Chat;
using TiktokenSharp;
using static OJS.Common.GlobalConstants.FileExtensions;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using static OJS.Common.GlobalConstants.Settings;

public class MentorBusinessService(
    IDataService<UserMentor> userMentorData,
    IDataService<MentorPromptTemplate> mentorPromptTemplateData,
    IHttpClientFactory httpClientFactory,
    IDataService<Setting> settingData,
    IContestsDataService contestsData,
    ICacheService cache,
    ILogger<MentorBusinessService> logger,
    OpenAIClient openAiClient,
    IResourcesBusinessService resourcesBusinessService)
    : IMentorBusinessService
{
    private const string DocumentNotFoundOrEmpty = "Judge was unable to find the problem's description. Please contact an administrator and report the problem.";

    public async Task<ConversationResponseModel> StartConversation(ConversationRequestModel model)
    {
        var settings = await settingData
            .GetQuery(s => s.Name.Contains(Mentor))
            .AsNoTracking()
            .ToDictionaryAsync(k => k.Name, v => v.Value);

        var maxUserInputLength = CalculateMaxUserInputLength(settings);

        /*
         *  No message ( user, information ) should have a length greater than 'maxUserInputLength'.
         *  Keeping in mind that the max output token count is about 2 times less than
         *  the max input token count, this rule can be applied to the assistant messages as well.
         */
        if (model.Messages.Any(m => m.Content.Length > maxUserInputLength))
        {
            throw new BusinessServiceException($"Your message exceeds the {maxUserInputLength}-character limit. Please shorten it.");
        }

        var userMentor = await userMentorData
            .GetQuery(um => um.Id == model.UserId)
            .FirstOrDefaultAsync();

        if (userMentor is null)
        {
            userMentor = new UserMentor
            {
                Id = model.UserId,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow,
                RequestsMade = 0,
                TotalRequestsMade = 0,
                QuotaLimit = null,
                QuotaResetTime = DateTime.UtcNow.AddMinutes(GetNumericValue(settings, nameof(MentorQuotaResetTimeInMinutes))),
            };

            await userMentorData.Add(userMentor);
            await userMentorData.SaveChanges();
        }

        if (DateTime.UtcNow >= userMentor.QuotaResetTime)
        {
            userMentor.RequestsMade = 0;
            userMentor.QuotaResetTime = DateTime.UtcNow.AddMinutes(GetNumericValue(settings, nameof(MentorQuotaResetTimeInMinutes)));
        }

        if (userMentor.RequestsMade >= (userMentor.QuotaLimit ?? GetNumericValue(settings, nameof(MentorQuotaLimit))))
        {
            model.Messages.Add(new ConversationMessageModel
            {
                Content = $"Достигнахте лимита на съобщенията си, моля опитайте отново след {GetTimeUntilNextMessage(userMentor.QuotaResetTime)}.",
                Role = MentorMessageRole.Information,
                SequenceNumber = model.Messages.Max(m => m.SequenceNumber) + 1,
            });

            return GetResponseModel(model, maxUserInputLength);
        }

        var messagesToSend = new List<ChatMessage>();

        var systemMessage = await cache.Get(
            string.Format(CultureInfo.InvariantCulture, CacheConstants.MentorSystemMessageForProblem, model.ProblemId),
            async () => await this.GetSystemMessage(model),
            CacheConstants.OneHourInSeconds);
        systemMessage.Content = RemoveRedundantWhitespace(systemMessage.Content);
        messagesToSend.Add(CreateChatMessage(systemMessage.Role, systemMessage.Content));

        var recentMessages = model.Messages
            .Where(m => m.Role is not MentorMessageRole.System and not MentorMessageRole.Information)
            .OrderByDescending(m => m.SequenceNumber)
            .Take(GetNumericValue(settings, nameof(MentorMessagesSentCount)))
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new ConversationMessageModel
            {
                Content = m.Content,
                Role = m.Role,
                SequenceNumber = m.SequenceNumber,
            })
            .ToList();

        foreach (var message in recentMessages)
        {
            message.Content = RemoveRedundantWhitespace(message.Content);
        }

        if (!Enum.TryParse<OpenAIModels>(settings[MentorModel], out var openAiModel))
        {
            throw new BusinessServiceException($"The provided mentor model \"{settings[MentorModel]}\" is invalid.");
        }

        var mentorModel = openAiModel.ToModelString();

        var encoding = await TikToken.EncodingForModelAsync(mentorModel);
        var allContent = systemMessage.Content + string.Join("", recentMessages.Select(m => m.Content));
        var tokenCount = encoding.Encode(allContent).Count;

        var maxInputTokens = GetNumericValue(settings, nameof(MentorMaxInputTokenCount));
        if (tokenCount > maxInputTokens)
        {
            this.TruncateMessages(model.ProblemId, recentMessages, encoding, tokenCount - maxInputTokens);
        }

        messagesToSend.AddRange(recentMessages.Select(m => CreateChatMessage(m.Role, m.Content)));

        var chat = openAiClient.GetChatClient(mentorModel);
        var response = await chat.CompleteChatAsync(messagesToSend, new ChatCompletionOptions
        {
            MaxOutputTokenCount = GetNumericValue(settings, nameof(MentorMaxOutputTokenCount)),
            EndUserId = model.UserId,
            Metadata =
            {
                { "CategoryName", model.CategoryName },
                { "ContestName", model.ContestName },
                { "ProblemName", model.ProblemName },
                { "SubmissionTypeName" , model.SubmissionTypeName },
                { "ContestId", model.ContestId.ToString(CultureInfo.InvariantCulture) },
                { "ProblemId", model.ProblemId.ToString(CultureInfo.InvariantCulture) },
                { "UserId", model.UserId },
                { "ProblemIsExtractedSuccessfully", systemMessage.ProblemIsExtractedSuccessfully ? "true" : "false" },
            },
        });

        if (response is null)
        {
            throw new BusinessServiceException("Unable to process your request at this time. Please try again in a few moments.");
        }

        var assistantContent = string.Join(Environment.NewLine, response.Value.Content.Select(part => part.Text).Where(text => !string.IsNullOrEmpty(text)));

        model.Messages.Add(new ConversationMessageModel
        {
            Content = assistantContent,
            Role = MentorMessageRole.Assistant,
            SequenceNumber = model.Messages.Max(m => m.SequenceNumber) + 1,
        });

        userMentor.RequestsMade++;
        userMentor.TotalRequestsMade++;
        await userMentorData.SaveChanges();

        return GetResponseModel(model, maxUserInputLength);
    }

    private string ExtractSectionFromDocument(
        byte[] bytes,
        string problemName,
        int problemNumber,
        int problemId,
        int contestId)
    {
        try
        {
            using var memoryStream = new MemoryStream(bytes);
            using var wordDocument = WordprocessingDocument.Open(memoryStream, false);

            var body = wordDocument.MainDocumentPart?.Document.Body;
            if (body is null)
            {
                throw new BusinessServiceException(DocumentNotFoundOrEmpty + " Document body is null.");
            }

            var sections = new Dictionary<string, (int Index, List<string> Data, OpenXmlElement Element)>();
            string? currentHeading = null;
            var sectionCount = 0;

            // First pass: Extract all sections into the dictionary
            foreach (var element in body.Elements())
            {
                if (element is Paragraph paragraph)
                {
                    var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                    var text = paragraph.InnerText.Trim();

                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var isHeading = !string.IsNullOrEmpty(styleId) && (styleId.StartsWith("Heading2", StringComparison.Ordinal) || styleId == "2");

                    if (isHeading)
                    {
                        sectionCount++;
                        currentHeading = text;
                        if (!sections.ContainsKey(currentHeading))
                        {
                            sections[currentHeading] = (sectionCount, new List<string>(), paragraph);
                        }
                    }
                    else if (currentHeading != null)
                    {
                        sections[currentHeading].Data.Add(text);
                    }
                }
            }

            // Second pass: Find the matching section and process it fully
            foreach (var section in sections)
            {
                // Case 1: Match by name
                if (problemName.Contains(section.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return ProcessMatchedSection(section.Value.Element, section.Key);
                }

                // Case 2: Match by section number
                if (section.Value.Index == problemNumber)
                {
                    return ProcessMatchedSection(section.Value.Element, section.Key);
                }
            }

            return string.Empty;
        }
        catch (Exception)
        {
            logger.LogFileParsingFailure(problemId, contestId);
            throw new BusinessServiceException(DocumentNotFoundOrEmpty);
        }
    }

    private static string ProcessMatchedSection(OpenXmlElement sectionElement, string sectionHeading)
    {
        var resultContent = new StringBuilder();
        resultContent.AppendLine($"## {sectionHeading}");

        // Process all content until the next heading of same or higher level
        var currentElement = sectionElement.NextSibling();
        while (currentElement != null)
        {
            if (currentElement is Paragraph paragraph)
            {
                var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                var isHeading = !string.IsNullOrEmpty(styleId) && (styleId.StartsWith("Heading2", StringComparison.Ordinal) || styleId == "2");

                if (isHeading)
                {
                    break;
                }

                var text = paragraph.InnerText.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    resultContent.AppendLine(text);
                }
            }
            else if (currentElement is Table table)
            {
                ProcessTable(table, resultContent);
            }

            currentElement = currentElement.NextSibling();
        }

        return resultContent.ToString().Trim();
    }

    private static void ProcessTable(Table table, StringBuilder matchingSectionContent)
    {
        var rows = table.Elements<TableRow>().ToList();
        if (rows.Count == 0)
        {
            return;
        }

        /*
         * Currently, this implementation accepts that the first row will be of headers,
         * this might need to be generalized after testing.
         */
        var headerRow = rows.First();
        var headers = headerRow.Elements<TableCell>()
            .Select(cell => cell.InnerText.Trim())
            .ToList();

        foreach (var row in rows.Skip(1))
        {
            var cells = row.Elements<TableCell>().ToList();
            var rowValues = cells.Select(cell =>
            {
                var runs = cell.Descendants<Text>();
                return string.Join(" ", runs.Select(r => r.Text.Trim()));
            }).ToList();

            if (rowValues.All(string.IsNullOrEmpty))
            {
                continue;
            }

            var rowData = new StringBuilder();
            for (var i = 0; i < Math.Min(headers.Count, rowValues.Count); i++)
            {
                if (!string.IsNullOrEmpty(rowValues[i]))
                {
                    if (rowData.Length > 0)
                    {
                        rowData.Append(" ⦚ ");
                    }

                    rowData.Append(CultureInfo.InvariantCulture, $"{headers[i]}: {rowValues[i]}");
                }
            }

            matchingSectionContent.AppendLine(rowData.ToString());
        }
    }

    private void TruncateMessages(
        int problemId,
        List<ConversationMessageModel> messages,
        TikToken encoding,
        int tokensToRemove)
    {
        if (tokensToRemove <= 0 || messages.Count == 0)
        {
            return;
        }

        var initialMessageCount = messages.Count;

        var messagesToTruncate = messages
            .OrderBy(m => m.SequenceNumber)
            .ToList();

        foreach (var message in messagesToTruncate)
        {
            var messageTokens = encoding.Encode(message.Content).Count;
            var percentageTruncated = 0.0;

            // If entire message fits within remaining tokens to remove, remove the whole message
            if (messageTokens <= tokensToRemove)
            {
                messages.Remove(message);
                tokensToRemove -= messageTokens;
                percentageTruncated = 100.0;
            }
            else
            {
                // If message is too long, truncate content
                var remainingTokens = Math.Max(0, messageTokens - tokensToRemove);
                var truncatedContent = TruncateContent(encoding, message.Content, remainingTokens);

                // Calculate percentage truncated, do not remove the parenthesis
                percentageTruncated = ((double)(messageTokens - remainingTokens) / messageTokens) * 100;

                message.Content = truncatedContent;
                tokensToRemove = 0;
            }

            // Log the percentage of content truncated
            logger.LogPercentageOfMessageContentTruncated(percentageTruncated, problemId);

            // Stop if we've removed enough tokens
            if (tokensToRemove <= 0)
            {
                break;
            }
        }

        var removedMessageCount = initialMessageCount - messages.Count;
        if (removedMessageCount > 0)
        {
            // Log the number of messages removed
            logger.LogTruncatedMentorMessages(initialMessageCount, removedMessageCount, problemId);
        }
    }


    private static string TruncateContent(TikToken encoding, string content, int maxTokens)
    {
        // If content is already within token limit, return as-is
        if (encoding.Encode(content).Count <= maxTokens)
        {
            return content;
        }

        // Tokenize the content
        var tokens = encoding.Encode(content);

        // Truncate to specified number of tokens
        var truncatedTokens = tokens.Take(maxTokens).ToList();

        // Decode back to string
        return encoding.Decode(truncatedTokens);
    }

    private static ChatMessage CreateChatMessage(MentorMessageRole role, string content)
        => role switch
        {
            MentorMessageRole.System => ChatMessage.CreateSystemMessage(content),
            MentorMessageRole.User => ChatMessage.CreateUserMessage(content),
            MentorMessageRole.Assistant => ChatMessage.CreateAssistantMessage(content),
            _ => throw new BuildAbortedException("The provided message role is not supported."),
        };

    private static string GetTimeUntilNextMessage(DateTimeOffset quotaResetTime)
    {
        var timeUntilReset = quotaResetTime.UtcDateTime - DateTime.UtcNow;

        var timeComponents = new List<string>();

        if (timeUntilReset.Days > 0)
        {
            timeComponents.Add($"{timeUntilReset.Days} {(timeUntilReset.Days > 1 ? "дни" : "ден")}");
        }

        if (timeUntilReset.Hours > 0)
        {
            timeComponents.Add($"{timeUntilReset.Hours} {(timeUntilReset.Hours > 1 ? "часа" : "час")}");
        }

        if (timeUntilReset.Minutes > 0)
        {
            timeComponents.Add($"{timeUntilReset.Minutes} {(timeUntilReset.Minutes > 1 ? "минути" : "минута")}");
        }

        if (timeUntilReset.Seconds > 0)
        {
            timeComponents.Add($"{timeUntilReset.Seconds} {(timeUntilReset.Seconds > 1 ? "секунди" : "секунда")}");
        }

        return string.Join(", ", timeComponents);
    }

    private static string RemoveRedundantWhitespace(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        var sb = new StringBuilder(content.Length);
        var previousWasSpace = false;

        foreach (var c in content)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!previousWasSpace)
                {
                    sb.Append(' ');
                    previousWasSpace = true;
                }
            }
            else
            {
                sb.Append(c);
                previousWasSpace = false;
            }
        }

        return sb.ToString().Trim();
    }

    private static int GetProblemNumber(string problemName)
    {
        if (string.IsNullOrWhiteSpace(problemName))
        {
            return int.MaxValue;
        }

        var firstWord = problemName.Split(' ').First();
        var numericPart = new string(firstWord.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());

        if (!string.IsNullOrEmpty(numericPart))
        {
            numericPart = numericPart.TrimEnd('.');
            if (int.TryParse(numericPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
            {
                return number;
            }
        }

        return int.MaxValue;
    }

    private static int GetNumericValue(Dictionary<string, string> settings, string key)
        => int.Parse(settings[key], CultureInfo.InvariantCulture);

    private static ConversationResponseModel GetResponseModel(
        ConversationRequestModel model,
        int maxUserInputLength)
    {
        var responseModel = model.Map<ConversationResponseModel>();
        responseModel.ProblemId = model.ProblemId;
        responseModel.MaxUserInputLength = maxUserInputLength;
        return responseModel;
    }

    private static int CalculateMaxUserInputLength(Dictionary<string, string> settings)
        // The parenthesis should not be removed, they are used to define the priority of the arithmetic operations.
        => (GetNumericValue(settings, nameof(MentorMaxInputTokenCount)) * 4 * GetNumericValue(settings, nameof(PercentageOfMentorMaxInputTokenCountUsedByUser))) / 100;

    public async Task<ConversationMessageModel> GetSystemMessage(ConversationRequestModel model)
    {
        /*
         *  In the first version of the mentor, there will be only a single
         *  template in the database. This is why we can be sure that
         *  .FirstOrDefaultAsync() will always return a template. This
         *  will be changed in the future.
         */
        var template = await mentorPromptTemplateData
            .GetQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync();

        var problemsResources = await contestsData
            .GetByIdQuery(model.ContestId)
            .AsNoTracking()
            .Select(c => c.ProblemGroups
                .SelectMany(pg => pg.Problems
                    .SelectMany(p => p.Resources
                        .Where(r => r.Type == ProblemResourceType.ProblemDescription))))
            .SelectMany(resources => resources)
            .ToListAsync();

        var wordFiles = problemsResources
            .Where(pr =>
                   (pr.File is not null && pr.FileExtension?.Equals(WithoutLeadingDot(Docx), StringComparison.Ordinal) == true) ||
                   (pr.Link is not null && pr.Link.Split('.').Last().Equals(WithoutLeadingDot(Docx), StringComparison.Ordinal)))
            .ToList();

        /*
         *  There are 2 cases when it comes to document retrieval:
         *  1. The problem has its own problem resource ( Word file ) ( e.g. online / onsite exam ).
         *  2. All the problems' descriptions are in a single Word file ( e.g. Lab / Exercise ).
         */
        var problemsDescription = wordFiles.FirstOrDefault(pr => pr.ProblemId == model.ProblemId) ?? wordFiles.FirstOrDefault();

        if (problemsDescription is null)
        {
            logger.LogProblemDescriptionResourceNotFound(model.ProblemId, model.ContestId);
            throw new BusinessServiceException(DocumentNotFoundOrEmpty);
        }

        var file = problemsDescription.File ??
            (problemsDescription.Link is not null
            ? await this.DownloadDocument(problemsDescription.Link, model.ProblemId, model.ContestId)
            : []);

        var number = GetProblemNumber(model.ProblemName);
        var text = this.ExtractSectionFromDocument(file, model.ProblemName, number, model.ProblemId, model.ContestId);

        return new ConversationMessageModel
        {
            Content = string.Format(
                CultureInfo.InvariantCulture,
                template!.Template,
                model.ProblemName,
                text,
                model.ContestName,
                model.CategoryName,
                model.SubmissionTypeName),
            Role = MentorMessageRole.System,
            // The system message should always be first (in ascending order)
            SequenceNumber = int.MinValue,
            ProblemIsExtractedSuccessfully = !string.IsNullOrWhiteSpace(text),
        };
    }

    private async Task<byte[]> DownloadDocument(string link, int problemId, int contestId)
    {
        link = resourcesBusinessService.SafeConvertToSvnLink(link);
        using var client = this.CreateClientForLink(link);
        var fileBytes = await this.FetchResource(link, client, problemId, contestId);

        if (!IsExpectedFormat(fileBytes))
        {
            logger.LogInvalidDocumentFormat(problemId, contestId, link);
            throw new BusinessServiceException(DocumentNotFoundOrEmpty);
        }

        return fileBytes;
    }

    private HttpClient CreateClientForLink(string link)
    {
        if (!Uri.TryCreate(link, UriKind.Absolute, out var uri))
        {
            throw new BusinessServiceException($"Invalid URL provided for Problem: {link}", nameof(link));
        }

        // 1) Try the SVN client for links from the SVN server
        var svnClient = httpClientFactory.CreateClient(ServiceConstants.SvnHttpClientName);
        if (svnClient.BaseAddress != null
            && string.Equals(svnClient.BaseAddress.Host, uri.Host, StringComparison.OrdinalIgnoreCase))
        {
            return svnClient;
        }

        // 2) Fallback: use the default client
        return httpClientFactory.CreateClient(ServiceConstants.DefaultHttpClientName);
    }

    private async Task<byte[]> FetchResource(string link, HttpClient client, int problemId, int contestId)
    {
        try
        {
            var response = await client.GetAsync(link, HttpCompletionOption.ResponseHeadersRead);

            if (response is not { IsSuccessStatusCode: true })
            {
                logger.LogHttpRequestFailure(
                    problemId,
                    contestId,
                    response?.StatusCode ?? HttpStatusCode.ServiceUnavailable,
                    response?.RequestMessage?.RequestUri?.AbsoluteUri ?? link,
                    response?.ReasonPhrase);
                throw new BusinessServiceException(DocumentNotFoundOrEmpty);
            }

            var fileBytes = await response.Content.ReadAsByteArrayAsync();

            if (fileBytes.Length == 0)
            {
                logger.LogFileNotFoundOrEmpty(problemId, contestId, link);
                throw new BusinessServiceException(DocumentNotFoundOrEmpty);
            }

            return fileBytes;
        }
        catch (Exception ex)
        {
            logger.LogResourceDownloadFailure(problemId, contestId, link, ex);
            throw new BusinessServiceException(DocumentNotFoundOrEmpty);
        }
    }

    private static bool IsExpectedFormat(byte[] fileBytes)
    {
        // .docx files are ZIP archives, so they start with the ZIP file signature: "PK"
        const byte p = 0x50; // ASCII for 'P'
        const byte k = 0x4B; // ASCII for 'K'

        return fileBytes.Length >= 2 && fileBytes[0] == p && fileBytes[1] == k;
    }
}
