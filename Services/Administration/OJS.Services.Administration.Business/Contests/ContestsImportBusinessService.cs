namespace OJS.Services.Administration.Business.Contests;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OJS.Common.Enumerations;
using OJS.Data.Models;
using OJS.Data.Models.Checkers;
using OJS.Data.Models.Contests;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Submissions;
using OJS.Data.Models.Tests;
using OJS.Services.Administration.Business.ProblemGroups;
using OJS.Services.Administration.Business.Problems;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.Contests;
using OJS.Services.Common.Models;
using OJS.Services.Infrastructure.Configurations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

public class ContestsImportBusinessService(
    IHttpClientFactory httpClientFactory,
    IOptions<ApplicationUrlsConfig> urlsConfig,
    IContestsDataService contestsData,
    IContestCategoriesDataService contestCategoriesData,
    ICheckersDataService checkersData,
    ISubmissionTypesDataService submissionTypesData,
    ITestRunsDataService testRunsData,
    IProblemsBusinessService problemsBusiness,
    IProblemGroupsBusinessService problemGroupsBusiness) : IContestsImportBusinessService
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient();
    private readonly ApplicationUrlsConfig urls = urlsConfig.Value;

    public async Task<ServiceResult<string>> ImportContestsFromCategory(int sourceContestCategoryId, int destinationContestCategoryId,
        bool dryRun = true)
    {
        if (sourceContestCategoryId == 0 || destinationContestCategoryId == 0)
        {
            return new ServiceResult<string>("Invalid contest category ids.");
        }

        var contestIds = await this.httpClient.GetFromJsonAsync<int[]>($"{this.urls.LegacyJudgeUrl}/api/Contests/GetExistingIdsForCategory?contestCategoryId={sourceContestCategoryId}&apiKey={this.urls.LegacyJudgeApiKey}");

        if (contestIds == null)
        {
            return new ServiceResult<string>("Failed to get contest IDs.");
        }

        var destinationContestCategory = await contestCategoriesData.OneById(destinationContestCategoryId);

        if (destinationContestCategory == null)
        {
            return new ServiceResult<string>($"Destination contest category with id {destinationContestCategoryId} does not exist.");
        }

        var result = new StringBuilder();
        result.AppendLine(CultureInfo.InvariantCulture, $"<p>Importing contests from category #{sourceContestCategoryId} into category \"{destinationContestCategory.Name}\" #{destinationContestCategoryId}</p>");
        result.AppendLine(CultureInfo.InvariantCulture, $"<p>{contestIds.Length} contests will be imported. These are the source contest ids: <b>{string.Join(", ", contestIds)}</b></p>");
        result.AppendLine("<hr>");

        if (dryRun)
        {
            result.AppendLine("<b>Dry run is enabled. This will not import any contests.</b>");
            result.AppendLine("<hr>");
        }

        var checkers = await checkersData.All().ToListAsync();
        var submissionTypes = await submissionTypesData.All().ToListAsync();

        foreach (var contestId in contestIds)
        {
            var contest = await this.httpClient.GetFromJsonAsync<ContestLegacyExportServiceModel>($"{this.urls.LegacyJudgeUrl}/api/Contests/Export/{contestId}?apiKey={this.urls.LegacyJudgeApiKey}");
            if (contest == null)
            {
                result.AppendLine(CultureInfo.InvariantCulture, $"<p><b>Skip:</b> Failed to get source contest <b>#{contestId}</b>. Skipping it...</p>");
                continue;
            }

            var existingContest = await contestsData
                .WithQueryFilters()
                .GetQuery()
                .Include(c => c.ProblemGroups)
                    .ThenInclude(pg => pg.Problems)
                    .ThenInclude(p => p.Checker)
                .Include(c => c.ProblemGroups)
                    .ThenInclude(pg => pg.Problems)
                    .ThenInclude(p => p.Tests)
                .Include(c => c.ProblemGroups)
                    .ThenInclude(pg => pg.Problems)
                    .ThenInclude(p => p.Resources)
                .Include(c => c.ProblemGroups)
                    .ThenInclude(pg => pg.Problems)
                    .ThenInclude(p => p.SubmissionTypesInProblems)
                    .ThenInclude(sp => sp.SubmissionType)
                .FirstOrDefaultAsync(c => c.CategoryId == destinationContestCategoryId && c.Name == contest.Name);

            if (dryRun)
            {
                result.AppendLine(existingContest == null
                    ? $"<p><b>Import as new:</b> (src <b>#{contestId}</b>) Contest <b>\"{contest.Name}\"</b> will be imported as new contest.</p>"
                    : $"<p><b>Update:</b> (src <b>#{contestId}</b>) Contest <b>\"{contest.Name}\"</b> already exists and will be updated.</p>");
            }

            if (existingContest == null)
            {
                await this.ImportNewContest(destinationContestCategoryId, contest, checkers, submissionTypes, result, dryRun);
            }
            else
            {
                await this.UpdateContest(existingContest, contest, checkers, submissionTypes, result, dryRun);
            }
        }

        result.AppendLine("<hr>");
        result.AppendLine(dryRun
            ? "<p>Dry run completed. To import, set dryRun to false.</p>"
            : "<p>Import completed.</p>");
        result.AppendLine("<hr>");

        return ServiceResult<string>.Success(result.ToString());
    }

    private static DateTime? ConvertTimeToUtc(DateTime? dateTime)
        => dateTime != null
            ? TimeZoneInfo.ConvertTimeToUtc(dateTime.Value, TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time"))
            : null;

    private async Task ImportNewContest(
        int destinationContestCategoryId,
        ContestLegacyExportServiceModel contest,
        List<Checker> checkers,
        List<SubmissionType> submissionTypes,
        StringBuilder result,
        bool dryRun)
    {
        var newContest = new Contest
        {
            CategoryId = destinationContestCategoryId,
            Name = contest.Name,
            Description = contest.Description,
            Type = (ContestType)contest.Type,
            StartTime = ConvertTimeToUtc(contest.StartTime),
            EndTime = ConvertTimeToUtc(contest.EndTime),
            PracticeStartTime = ConvertTimeToUtc(contest.PracticeStartTime),
            PracticeEndTime = ConvertTimeToUtc(contest.PracticeEndTime),
            ContestPassword = contest.ContestPassword,
            PracticePassword = contest.PracticePassword,
            LimitBetweenSubmissions = contest.LimitBetweenSubmissions,
            OrderBy = contest.OrderBy,
            NumberOfProblemGroups = contest.NumberOfProblemGroups,
            AllowParallelSubmissionsInTasks = !contest.UsersCantSubmitConcurrently,
            IsVisible = contest.IsVisible,
            VisibleFrom = ConvertTimeToUtc(contest.VisibleFrom),
            NewIpPassword = contest.NewIpPassword,
            AutoChangeTestsFeedbackVisibility = contest.AutoChangeTestsFeedbackVisibility,
            Duration = contest.Duration,
            ProblemGroups = contest.ProblemGroups.Select(pg => new ProblemGroup
            {
                OrderBy = pg.OrderBy,
                Type = pg.Type == null ? null : (ProblemGroupType)pg.Type,
                Problems = pg.Problems.Select(p => new Problem
                {
                    Name = p.Name,
                    MaximumPoints = p.MaximumPoints,
                    TimeLimit = p.TimeLimit,
                    MemoryLimit = p.MemoryLimit,
                    SourceCodeSizeLimit = p.SourceCodeSizeLimit,
                    CheckerId = checkers.FirstOrDefault(c => c.Name == p.Checker?.Name)?.Id,
                    OrderBy = p.OrderBy,
                    SolutionSkeleton = p.SolutionSkeleton,
                    ShowDetailedFeedback = p.ShowDetailedFeedback,
                    AdditionalFiles = p.AdditionalFiles,
                    DefaultSubmissionTypeId = submissionTypes.FirstOrDefault(s => s.Name == p.DefaultSubmissionType?.Name)?.Id,
                    SubmissionTypesInProblems = p.SubmissionTypes.Select(st => new SubmissionTypeInProblem
                    {
                        SubmissionTypeId = submissionTypes.FirstOrDefault(s => s.Name == st.Name)?.Id ?? 0,
                        TimeLimit = st.ProblemSubmissionTypeExecutionDetails.FirstOrDefault(x => x.ProblemId == p.Id && x.SubmissionTypeId == st.Id)?.TimeLimit,
                        MemoryLimit = st.ProblemSubmissionTypeExecutionDetails.FirstOrDefault(x => x.ProblemId == p.Id && x.SubmissionTypeId == st.Id)?.MemoryLimit,
                        SolutionSkeleton = st.ProblemSubmissionTypeExecutionDetails.FirstOrDefault(x => x.ProblemId == p.Id && x.SubmissionTypeId == st.Id)?.SolutionSkeleton,
                    }).ToList(),
                    Tests = p.Tests.Select(t => new Test
                    {
                        InputData = t.InputData,
                        OutputData = t.OutputData,
                        OrderBy = t.OrderBy,
                        HideInput = t.HideInput,
                        IsTrialTest = t.IsTrialTest,
                        IsOpenTest = t.IsOpenTest,
                    }).ToList(),
                    Resources = p.Resources.Select(r => new ProblemResource
                    {
                        File = r.File,
                        FileExtension = r.FileExtension,
                        Link = r.Link,
                        Name = r.Name,
                        OrderBy = r.OrderBy,
                        Type = (ProblemResourceType)r.Type,
                    }).ToList(),
                }).ToList(),
            }).ToList(),
        };

        if (!dryRun)
        {
            await contestsData.Add(newContest);
            await contestsData.SaveChanges();
        }

        result.AppendLine(CultureInfo.InvariantCulture, $"<p>Contest <b>\"{contest.Name}\"</b> was imported successfully.</p>");
        result.AppendLine("<hr>");
    }

    private async Task UpdateContest(
        Contest existingContest,
        ContestLegacyExportServiceModel sourceContest,
        List<Checker> checkers,
        List<SubmissionType> submissionTypes,
        StringBuilder result,
        bool dryRun)
    {
        // Update basic contest properties
        existingContest.Name = sourceContest.Name;
        existingContest.Description = sourceContest.Description;
        existingContest.Type = (ContestType)sourceContest.Type;
        existingContest.StartTime = ConvertTimeToUtc(sourceContest.StartTime);
        existingContest.EndTime = ConvertTimeToUtc(sourceContest.EndTime);
        existingContest.PracticeStartTime = ConvertTimeToUtc(sourceContest.PracticeStartTime);
        existingContest.PracticeEndTime = ConvertTimeToUtc(sourceContest.PracticeEndTime);
        existingContest.ContestPassword = sourceContest.ContestPassword;
        existingContest.PracticePassword = sourceContest.PracticePassword;
        existingContest.LimitBetweenSubmissions = sourceContest.LimitBetweenSubmissions;
        existingContest.OrderBy = sourceContest.OrderBy;
        existingContest.NumberOfProblemGroups = sourceContest.NumberOfProblemGroups;
        existingContest.AllowParallelSubmissionsInTasks = !sourceContest.UsersCantSubmitConcurrently;
        existingContest.IsVisible = sourceContest.IsVisible;
        existingContest.VisibleFrom = ConvertTimeToUtc(sourceContest.VisibleFrom);
        existingContest.NewIpPassword = sourceContest.NewIpPassword;
        existingContest.AutoChangeTestsFeedbackVisibility = sourceContest.AutoChangeTestsFeedbackVisibility;
        existingContest.Duration = sourceContest.Duration;

        // Process each problem in the group
        foreach (var sourceProblemGroup in sourceContest.ProblemGroups)
        {
            // Try to find existing problem group by OrderBy
            var existingProblemGroup = existingContest.ProblemGroups
                .FirstOrDefault(pg => Math.Abs(pg.OrderBy - sourceProblemGroup.OrderBy) < 0.001);

            if (existingProblemGroup == null)
            {
                // Create new problem group if no match found
                existingProblemGroup = new ProblemGroup
                {
                    ContestId = existingContest.Id,
                    Problems = [],
                };
                existingContest.ProblemGroups.Add(existingProblemGroup);
            }

            // Update problem group properties
            existingProblemGroup.OrderBy = sourceProblemGroup.OrderBy;
            existingProblemGroup.Type = sourceProblemGroup.Type == null ? null : (ProblemGroupType)sourceProblemGroup.Type;

            // Process each problem in the group
            foreach (var sourceProblem in sourceProblemGroup.Problems)
            {
                // Try to find existing problem by name within the group
                var existingProblem = existingProblemGroup.Problems
                    .FirstOrDefault(p => p.Name == sourceProblem.Name);

                if (existingProblem == null)
                {
                    // Create new problem if no match found
                    result.AppendLine(CultureInfo.InvariantCulture, $"<p><b>----Add:</b> Problem <b>\"{sourceProblem.Name}\"</b> will be added.</p>");
                    existingProblem = new Problem();
                    existingProblemGroup.Problems.Add(existingProblem);
                }
                else
                {
                    // Clear existing collections to update them
                    result.AppendLine(CultureInfo.InvariantCulture, $"<p><b>----Update:</b> Problem <b>\"{sourceProblem.Name}\"</b> will be updated. Tests will be replaced. Test runs will be deleted.</p>");
                    existingProblem.Tests.Clear();
                    existingProblem.Resources.Clear();
                    existingProblem.SubmissionTypesInProblems.Clear();

                    if (!dryRun)
                    {
                        // Delete test runs for the problem, as they are no longer valid after importing new tests
                        await testRunsData.DeleteByProblem(existingProblem.Id);
                    }
                }

                // Update problem properties
                existingProblem.Name = sourceProblem.Name;
                existingProblem.MaximumPoints = sourceProblem.MaximumPoints;
                existingProblem.TimeLimit = sourceProblem.TimeLimit;
                existingProblem.MemoryLimit = sourceProblem.MemoryLimit;
                existingProblem.SourceCodeSizeLimit = sourceProblem.SourceCodeSizeLimit;
                existingProblem.CheckerId = checkers.FirstOrDefault(c => c.Name == sourceProblem.Checker?.Name)?.Id;
                existingProblem.OrderBy = sourceProblem.OrderBy;
                existingProblem.SolutionSkeleton = sourceProblem.SolutionSkeleton;
                existingProblem.ShowDetailedFeedback = sourceProblem.ShowDetailedFeedback;
                existingProblem.AdditionalFiles = sourceProblem.AdditionalFiles;
                existingProblem.DefaultSubmissionTypeId = submissionTypes
                    .FirstOrDefault(s => s.Name == sourceProblem.DefaultSubmissionType?.Name)?.Id;

                // Add tests
                foreach (var sourceTest in sourceProblem.Tests)
                {
                    existingProblem.Tests.Add(new Test
                    {
                        InputData = sourceTest.InputData,
                        OutputData = sourceTest.OutputData,
                        OrderBy = sourceTest.OrderBy,
                        HideInput = sourceTest.HideInput,
                        IsTrialTest = sourceTest.IsTrialTest,
                        IsOpenTest = sourceTest.IsOpenTest,
                    });
                }

                // Add resources
                foreach (var sourceResource in sourceProblem.Resources)
                {
                    existingProblem.Resources.Add(new ProblemResource
                    {
                        File = sourceResource.File,
                        FileExtension = sourceResource.FileExtension,
                        Link = sourceResource.Link,
                        Name = sourceResource.Name,
                        OrderBy = sourceResource.OrderBy,
                        Type = (ProblemResourceType)sourceResource.Type,
                    });
                }

                // Add submission types
                foreach (var sourceSubmissionType in sourceProblem.SubmissionTypes)
                {
                    var submissionTypeId = submissionTypes
                        .FirstOrDefault(s => s.Name == sourceSubmissionType.Name)?.Id ?? 0;

                    var executionDetails = sourceSubmissionType.ProblemSubmissionTypeExecutionDetails
                        .FirstOrDefault(x => x.ProblemId == sourceProblem.Id &&
                                           x.SubmissionTypeId == sourceSubmissionType.Id);

                    existingProblem.SubmissionTypesInProblems.Add(new SubmissionTypeInProblem
                    {
                        SubmissionTypeId = submissionTypeId,
                        TimeLimit = executionDetails?.TimeLimit,
                        MemoryLimit = executionDetails?.MemoryLimit,
                        SolutionSkeleton = executionDetails?.SolutionSkeleton,
                    });
                }
            }
        }

        if (!dryRun)
        {
            contestsData.Update(existingContest);
            await contestsData.SaveChanges();
        }

        // Remove problem groups and problems that aren't in the source
        var sourceGroupOrderByValues = sourceContest.ProblemGroups
            .Select(pg => pg.OrderBy)
            .ToList();

        var sourceProblemNames = sourceContest.ProblemGroups
            .SelectMany(pg => pg.Problems)
            .Select(p => p.Name)
            .ToList();

        var problemGroupsToRemove = existingContest.ProblemGroups
            .Where(pg => !sourceGroupOrderByValues.Any(sourceOrderBy =>
                Math.Abs(pg.OrderBy - sourceOrderBy) < 0.001))
            .ToList();

        foreach (var problemGroup in problemGroupsToRemove)
        {
            result.AppendLine(CultureInfo.InvariantCulture, $"<p><b>--Delete:</b> Problem group <b>\"{problemGroup.OrderBy}\"</b> will be deleted.</p>");

            if (!dryRun)
            {
                await problemGroupsBusiness.Delete(problemGroup.Id);
            }
        }

        foreach (var problemGroup in existingContest.ProblemGroups)
        {
            var problemsToRemove = problemGroup.Problems
                .Where(p => !sourceProblemNames.Contains(p.Name))
                .ToList();

            foreach (var problem in problemsToRemove)
            {
                result.AppendLine(CultureInfo.InvariantCulture, $"<p><b>----Delete:</b> Problem <b>\"{problem.Name}\"</b> will be deleted.</p>");

                if (!dryRun)
                {
                    await problemsBusiness.Delete(problem.Id);
                }
            }
        }

        result.AppendLine(CultureInfo.InvariantCulture, $"<p>Contest <b>\"{sourceContest.Name}\"</b> updated successfully.</p>");
        result.AppendLine("<hr>");
    }
}