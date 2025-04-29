namespace OJS.Services.Administration.Business.Contests;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Common;
using OJS.Common.Enumerations;
using OJS.Common.Extensions.Strings;
using OJS.Common.Utils;
using OJS.Data.Models;
using OJS.Data.Models.Contests;
using OJS.Data.Models.Participants;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Submissions;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Data.Excel;
using OJS.Services.Administration.Models;
using OJS.Services.Administration.Models.Contests;
using OJS.Services.Administration.Models.Participants;
using OJS.Services.Administration.Models.Problems;
using OJS.Services.Administration.Models.Submissions;
using OJS.Services.Common;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Common.Models.Contests.Results;
using OJS.Services.Common.Models.Files;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Resource = OJS.Common.Resources.ContestsGeneral;
using static OJS.Common.GlobalConstants.FileExtensions;
using static OJS.Common.GlobalConstants.Settings;

public class ContestsBusinessService : AdministrationOperationService<Contest, int, ContestAdministrationModel>, IContestsBusinessService
{
    private readonly IContestsDataService contestsData;
    private readonly IIpsDataService ipsData;
    private readonly IContestsActivityService activityService;
    private readonly IContestsCacheService contestsCacheService;
    private readonly IParticipantsDataService participantsData;
    private readonly IUserProviderService userProviderService;
    private readonly IContestResultsAggregatorCommonService contestResultsAggregatorCommonService;
    private readonly IExcelService excelService;
    private readonly IProblemsDataService problemsDataService;
    private readonly IParticipantsCommonDataService participantsCommonDataService;
    private readonly IParticipantScoresDataService participantScoresDataService;
    private readonly ISubmissionsDataService submissionsDataService;
    private readonly IZipArchivesService zipArchivesService;
    private readonly IDataService<LecturerInContest> lecturerInContestDataService;
    private readonly ISettingsCacheService settingsCache;

    public ContestsBusinessService(
        IContestsDataService contestsData,
        IIpsDataService ipsData,
        IContestsActivityService activityService,
        IContestsCacheService contestsCacheService,
        IParticipantsDataService participantsData,
        IUserProviderService userProviderService,
        IContestResultsAggregatorCommonService contestResultsAggregatorCommonService,
        IExcelService excelService,
        IProblemsDataService problemsDataService,
        IParticipantsCommonDataService participantsCommonDataService,
        IParticipantScoresDataService participantScoresDataService,
        ISubmissionsDataService submissionsDataService,
        IZipArchivesService zipArchivesService,
        IDataService<LecturerInContest> lecturerInContestDataService,
        ISettingsCacheService settingsCache)
    {
        this.contestsData = contestsData;
        this.ipsData = ipsData;
        this.activityService = activityService;
        this.contestsCacheService = contestsCacheService;
        this.participantsData = participantsData;
        this.userProviderService = userProviderService;
        this.contestResultsAggregatorCommonService = contestResultsAggregatorCommonService;
        this.excelService = excelService;
        this.problemsDataService = problemsDataService;
        this.participantsCommonDataService = participantsCommonDataService;
        this.participantScoresDataService = participantScoresDataService;
        this.submissionsDataService = submissionsDataService;
        this.zipArchivesService = zipArchivesService;
        this.lecturerInContestDataService = lecturerInContestDataService;
        this.settingsCache = settingsCache;
    }

    public async Task<IEnumerable<LecturerInContestActionsModel>> GetForLecturerInContest(string userId)
        => await this.lecturerInContestDataService
            .GetQuery(lic => lic.LecturerId == userId)
            .MapCollection<LecturerInContestActionsModel>()
            .ToListAsync();

    public async Task<bool> UserHasContestPermissions(
        int contestId,
        string? userId,
        bool isUserAdmin)
        => !string.IsNullOrWhiteSpace(userId) &&
           (isUserAdmin || await this.contestsData.IsUserLecturerInContestByContestAndUser(contestId, userId));

    public async Task<FileResponseModel> ExportResults(ContestResultsExportRequestModel model)
    {
        var contest = await this.contestsData.GetByIdWithProblems(model.Id);

        if (contest == null)
        {
            throw new BusinessServiceException("Contest not found.");
        }

        var official = model.Type == ContestExportResultType.Compete;

        var user = this.userProviderService.GetCurrentUser();

        var contestResultsModel = new ContestResultsModel
        {
            Contest = contest,
            Official = official,
            IsUserAdminOrLecturer = user.IsAdminOrLecturer,
            IsFullResults = false,
            TotalResultsCount = null,
            IsExportResults = false,
        };

        var contestActivity = this.activityService.GetContestActivity(contest.Map<ContestForActivityServiceModel>(), []);
        var contestResults = this.contestResultsAggregatorCommonService.GetContestResults(contestResultsModel, contestActivity);

        // Suggested file name in the "Save as" dialog which will be displayed to the end user
        var fileName = string.Format(
            Resource.ReportExcelFormat,
            official ? Resource.Contest : Resource.Practice,
            contest.Name,
            Excel);

        return await this.excelService.ExportContestResultsToExcel(contestResults, fileName);
    }

    public async Task<FileResponseModel> DownloadSubmissions(DownloadSubmissionsModel model)
    {
        var id = model.ContestId;
        var compete = model.ContestExportResultType == ContestExportResultType.Compete;
        var exportType = model.SubmissionExportType;

        var contestName = await this.contestsData.GetNameById(id);

        var problems = this.problemsDataService
            .GetQuery()
            .Where(p => p.ProblemGroup.ContestId == id)
            .OrderBy(x => x.OrderBy)
            .ThenBy(x => x.Name)
            .Select(ProblemModel.Model)
            .ToList();

        var participants = this.participantsCommonDataService
            .GetAllByContestAndIsOfficial(id, compete)
            .Select(ParticipantModel.Model)
            .OrderBy(x => x.UserName)
            .ToList();

        Func<int, int, Submission?>? getSubmissionFunc = exportType switch
        {
            SubmissionExportType.BestSubmissions => this.GetBestSubmission,
            SubmissionExportType.LastSubmissions => this.GetLastSubmission,
            _ => null,
        };

        var files = this.ZipParticipantsSolutions(participants, problems, getSubmissionFunc).ToList();
        var fileComment = PrepareSolutionsFileComment(compete, contestName!, participants, problems);

        files.Add(new InMemoryFile
        {
            FileName = "Comment.txt",
            Content = Encoding.UTF8.GetBytes(fileComment.ToString()),
        });

        var zipFile = await this.zipArchivesService.GetZipArchive(files);

        var zipFileName = string.Format(
            "{1} {2} submissions for {0}.zip",
            contestName,
            compete ? "Contest" : "Practice",
            exportType == SubmissionExportType.BestSubmissions ? "best" : exportType == SubmissionExportType.LastSubmissions ? "last" : "all");

        return new FileResponseModel
        {
            FileName = zipFileName, Content = zipFile, MimeType = GlobalConstants.MimeTypes.ApplicationZip,
        };
    }

    public async Task<ContestLegacyExportServiceModel> Export(int id)
    {
        var contest = await this.contestsData
            .GetByIdQuery(id)
            .AsSplitQuery()
            .Where(c => !c.IsDeleted)
            .Include(c => c.ProblemGroups)
            .ThenInclude(pg => pg.Problems)
            .ThenInclude(p => p.Tests)
            .Include(c => c.ProblemGroups)
            .ThenInclude(pg => pg.Problems)
            .ThenInclude(p => p.Checker)
            .Include(c => c.ProblemGroups)
            .ThenInclude(pg => pg.Problems)
            .ThenInclude(p => p.SubmissionTypesInProblems)
            .ThenInclude(sp => sp.SubmissionType)
            .Include(c => c.ProblemGroups)
            .ThenInclude(pg => pg.Problems)
            .ThenInclude(p => p.Resources)
            .FirstOrDefaultAsync();

        if (contest == null)
        {
            throw new BusinessServiceException($"Contest with Id:{id} not found.");
        }

        RemoveCircularReferences(contest);

        return contest.Map<ContestLegacyExportServiceModel>();
    }

    public async Task<IEnumerable<int>> GetExistingIds(IEnumerable<int> ids)
        => await this.contestsData
            .GetQuery()
            .Where(c => !c.IsDeleted && ids.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync();

    public async Task<ContestActivityModel> GetContestActivity(int contestId)
        => this.activityService.GetContestActivity(await this.contestsData.GetByIdQuery(contestId)
            .MapCollection<ContestForActivityServiceModel>().FirstAsync(), []).Map<ContestActivityModel>();

    public override async Task<ContestAdministrationModel> Get(int id)
        => await this.contestsData.GetByIdWithProblemsAndParticipants(id).Map<ContestAdministrationModel>();

    public override async Task<ContestAdministrationModel> Edit(ContestAdministrationModel model)
    {
        var contest = await this.contestsData.GetByIdQuery(model.Id)
            .Include(c => c.ProblemGroups)
            .FirstOrDefaultAsync();

        if (!model.IsOnlineExam && model.Duration != null)
        {
            model.Duration = null;
        }

        var originalContestPassword = contest!.ContestPassword;
        var originalPracticePassword = contest!.PracticePassword;

        contest.MapFrom(model);

        contest.IpsInContests.Clear();
        await this.AddIpsToContest(contest, model.AllowedIps);

        await this.contestsCacheService.ClearContestsCacheByContestId(contest.Id);

        this.contestsData.Update(contest);
        await this.contestsData.SaveChanges();

        await this.InvalidateParticipants(originalContestPassword, originalPracticePassword, model);
        return model;
    }

    public override async Task Delete(int id)
    {
        var contest = await this.contestsData.GetByIdQuery(id).FirstOrDefaultAsync();
        if (contest is null)
        {
            throw new BusinessServiceException($"Contest with Id:{id} not found.");
        }

        if (await this.IsContestActive(contest))
        {
            throw new BusinessServiceException("Cannot delete active contest.");
        }

        await this.contestsCacheService.ClearContestsCacheByContestId(contest.Id);

        this.contestsData.Delete(contest);
        await this.contestsData.SaveChanges();
    }

    public override async Task<ContestAdministrationModel> Create(ContestAdministrationModel model)
    {
        var contest = model.Map<Contest>();

        await this.AddIpsToContest(contest, model.AllowedIps);

        await this.contestsData.Add(contest);
        await this.contestsData.SaveChanges();
        return model;
    }

    public async Task TransferParticipantsToPracticeById(int contestId)
    {
        var contest = await this.contestsData.GetByIdWithParticipantsScoresAndSubmissions(contestId);

        var competeOnlyParticipants = contest!.Participants
                .GroupBy(p => p.UserId)
                .Where(g => g.Count() == 1 && g.All(p => p.IsOfficial))
                .Select(gr => gr.FirstOrDefault());

        foreach (var participant in competeOnlyParticipants)
        {
            foreach (var participantScore in participant!.Scores)
            {
                participantScore.IsOfficial = false;
            }

            participant.IsOfficial = false;
        }

        var competeAndPracticeParticipants = contest.Participants
            .GroupBy(p => p.UserId)
            .Where(g => g.Count() == 2)
            .ToDictionary(grp => grp.Key, grp => grp.OrderBy(p => p.IsOfficial));

        var participantsForDeletion = new List<Participant>();

        foreach (var competeAndPracticeParticipant in competeAndPracticeParticipants)
        {
            var unofficialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key].First();
            var officialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key].Last();
            participantsForDeletion.Add(officialParticipant);

            foreach (var officialParticipantSubmission in officialParticipant.Submissions)
            {
                officialParticipantSubmission.Participant = unofficialParticipant;
            }

            var scoresForDeletion = new List<ParticipantScore>();

            var pointsToAdd = 0;
            foreach (var officialParticipantScore in officialParticipant.Scores)
            {
                var unofficialParticipantScore = unofficialParticipant
                    .Scores
                    .FirstOrDefault(s => s.ProblemId == officialParticipantScore.ProblemId);

                if (unofficialParticipantScore != null)
                {
                    if (unofficialParticipantScore.Points < officialParticipantScore.Points ||
                        (unofficialParticipantScore.Points == officialParticipantScore.Points &&
                         unofficialParticipantScore.Id < officialParticipantScore.Id))
                    {
                        officialParticipantScore.IsOfficial = false;
                        officialParticipantScore.Participant = unofficialParticipant;
                        scoresForDeletion.Add(unofficialParticipantScore);
                        pointsToAdd += officialParticipantScore.Points - unofficialParticipantScore.Points;
                    }
                    else
                    {
                        scoresForDeletion.Add(officialParticipantScore);
                    }
                }
                else
                {
                    officialParticipantScore.IsOfficial = false;
                    officialParticipantScore.Participant = unofficialParticipant;
                    pointsToAdd += officialParticipantScore.Points;
                }
            }

            if (pointsToAdd > 0)
            {
                unofficialParticipant.TotalScoreSnapshot += pointsToAdd;
                unofficialParticipant.TotalScoreSnapshotModifiedOn = officialParticipant.TotalScoreSnapshotModifiedOn;
            }

            this.participantScoresDataService.DeleteMany(scoresForDeletion);
        }

        this.participantsData.DeleteMany(participantsForDeletion);
        await this.participantsData.SaveChanges();
    }

    public async Task BulkEdit(ContestsBulkEditModel model)
    {
        var contests = await this.contestsData
            .GetQuery(c => c.CategoryId == model.CategoryId)
            .ToListAsync();

        foreach (var contest in contests)
        {
            if (model.LimitBetweenSubmissions is not null)
            {
                contest.LimitBetweenSubmissions = model.LimitBetweenSubmissions.Value;
            }

            if (model.Type is not null)
            {
                contest.Type = Enum.Parse<ContestType>(model.Type);
            }

            contest.StartTime = model.StartTime;
            contest.EndTime = model.EndTime;
            contest.PracticeStartTime = model.PracticeStartTime;
            contest.PracticeEndTime = model.PracticeEndTime;
        }

        this.contestsData.UpdateMany(contests);
        await this.contestsData.SaveChanges();
    }

    public async Task AdjustLimitBetweenSubmissions(WorkersBusyRatioServiceModel model)
    {
        var settings = await this.settingsCache.GetRequiredValue<ContestLimitBetweenSubmissionsAdjustSettings>(ContestLimitBetweenSubmissionsAdjustmentSettings);

        var combinedRatio = (model.ExponentialMovingAverageRatio + model.RollingAverageRatio) / 2.0;

        double ratioFactor;
        if (combinedRatio < settings.BusyRatioModerateThreshold)
        {
            // Decrease the factor -> decrease limit between submissions
            ratioFactor = 1.0 / settings.BusyRatioMaxFactor;
        }
        else
        {
            // Smoothly scale factor from 1.0 up to max factor -> increase limit between submissions linearly
            ratioFactor = Calculator.LinearInterpolate(
                combinedRatio,
                settings.BusyRatioModerateThreshold,
                settings.BusyRatioCriticalThreshold,
                targetMin: 1.0,
                targetMax: settings.BusyRatioMaxFactor,
                clamp: true);
        }

        // Smoothly scale from 1.0 up to max factor as queue length grows between moderate and critical thresholds
        var queueFactor = Calculator.LinearInterpolate(
            model.SubmissionsAwaitingExecution,
            model.WorkersTotalCount * settings.QueueLenghtModerateThresholdMultiplier,
            model.WorkersTotalCount * settings.QueueLenghtCriticalThresholdMultiplier,
            targetMin: 1.0,
            targetMax: settings.QueueLenghtMaxFactor,
            clamp: true);

        var adjustingFactor = ratioFactor * queueFactor;

        await this.contestsData
            .GetAllVisible()
            .Where(c => c.AutoChangeLimitBetweenSubmissions)
            .UpdateFromQueryAsync(c => new Contest
            {
                // Will be clamped between 0 and max allowed limit
                LimitBetweenSubmissions = Math.Clamp(
                    (int)((c.LimitBetweenSubmissions <= 0 && adjustingFactor > 1
                              ? 1
                              : c.LimitBetweenSubmissions) // If the limit is 0, and we want to increase it, start from 1 or else it will stay 0 indefinitely
                        * adjustingFactor),
                    0,
                    settings.MaxLimitBetweenSubmissionsInSeconds),
            });
    }

    private static void RemoveCircularReferences(Contest contest)
    {
        foreach (var problemGroup in contest.ProblemGroups)
        {
            problemGroup.Contest = null;

            foreach (var problem in problemGroup.Problems)
            {
                problem.ProblemGroup = null;

                foreach (var test in problem.Tests)
                {
                    test.Problem = null;
                }

                foreach (var resource in problem.Resources)
                {
                    resource.Problem = null;
                }

                foreach (var submissionTypeInProblem in problem.SubmissionTypesInProblems)
                {
                    submissionTypeInProblem.Problem = null;
                }
            }
        }
    }

    private static StringBuilder PrepareSolutionsFileComment(
        bool isOfficialContest,
        string contestName,
        ICollection<ParticipantModel> participants,
        IEnumerable<ProblemModel> problems)
    {
        var fileComment = new StringBuilder();

        fileComment.AppendLine(string.Format("{1} submissions for {0}", contestName, isOfficialContest ? "Contest" : "Practice"));
        fileComment.AppendLine($"Number of participants: {participants.Count}");
        fileComment.AppendLine();

        fileComment.AppendLine("Problems:");
        foreach (var problem in problems)
        {
            fileComment.AppendLine(string.Format(
                "{0} - {1} points, time limit: {2:0.000} sec., memory limit: {3:0.00} MB",
                problem.Name,
                problem.MaximumPoints,
                problem.TimeLimit / 1000.0,
                problem.MemoryLimit / 1024.0 / 1024.0));
        }

        fileComment.AppendLine();
        fileComment.AppendLine("Participants:");
        participants.ForEach(p => fileComment.AppendLine(GetParticipantName(p)));

        return fileComment;
    }

    private static string GetParticipantName(ParticipantModel participant)
        => $"{participant.UserName} ({participant.FirstName} {participant.LastName})";

    private static InMemoryFile ZipSubmission(
        ProblemModel problem,
        Submission submission,
        string directoryName,
        bool includeDateInFileName = false)
    {
        var fileNameWithoutExtension =
            includeDateInFileName ? $"{problem.Name}-{submission.CreatedOn:dd-MM-yyyy HH:mm:ss:fff}" : problem.Name;

        var fileName =
            $"{fileNameWithoutExtension}.{submission.FileExtension ?? submission.SubmissionType!.FileNameExtension}-{submission.Id}"
                .ToValidFileName();

        var content = submission.IsBinaryFile ? submission.Content : submission.ContentAsString.ToByteArray();

        return new InMemoryFile
        {
            FileName = Path.Combine(directoryName, fileName),
            Content = content,
        };
    }

    private async Task AddIpsToContest(Contest contest, string? mergedIps)
    {
        if (!string.IsNullOrWhiteSpace(mergedIps))
        {
            var ipValues = mergedIps.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var ipValue in ipValues)
            {
                var ip = await this.ipsData.GetByValue(ipValue) ?? new Ip { Value = ipValue };

                contest.IpsInContests.Add(new IpInContest { Ip = ip, IsOriginallyAllowed = true });
            }
        }
    }

    private Submission? GetBestSubmission(int participantId, int problemId) =>
        this.submissionsDataService
            .GetQuery()
            .Where(submission => submission.ParticipantId == participantId && submission.ProblemId == problemId)
            .OrderByDescending(submission => submission.Points)
            .ThenByDescending(submission => submission.CreatedOn)
            .Include(s => s.SubmissionType)
            .FirstOrDefault();

    private Submission? GetLastSubmission(int participantId, int problemId) =>
        this.submissionsDataService
            .GetQuery()
            .Where(submission => submission.ParticipantId == participantId && submission.ProblemId == problemId)
            .OrderByDescending(submission => submission.CreatedOn)
            .Include(s => s.SubmissionType)
            .FirstOrDefault();

    private async Task InvalidateParticipants(
        string? originalContestPassword,
        string? originalPracticePassword,
        ContestAdministrationModel contest)
    {
        if (originalContestPassword != contest.ContestPassword &&
            !string.IsNullOrWhiteSpace(contest.ContestPassword))
        {
            await this.participantsData.InvalidateByContestAndIsOfficial(contest.Id, isOfficial: true);
        }

        if (originalPracticePassword != contest.PracticePassword &&
            !string.IsNullOrWhiteSpace(contest.PracticePassword))
        {
            await this.participantsData.InvalidateByContestAndIsOfficial(contest.Id, isOfficial: false);
        }
    }

    private IEnumerable<Submission> GetAllSubmissions(int participantId, int problemId) =>
        this.submissionsDataService
            .GetQuery()
            .Where(submission => submission.ParticipantId == participantId && submission.ProblemId == problemId)
            .OrderByDescending(submission => submission.CreatedOn)
            .Include(s => s.SubmissionType)
            .ToList();

    private IEnumerable<InMemoryFile> ZipParticipantsSolutions(
        IEnumerable<ParticipantModel> participants,
        ICollection<ProblemModel> problems,
        Func<int, int, Submission?>? getSubmission)
    {
        var files = new List<InMemoryFile>();

        foreach (var participant in participants)
        {
            // Create directory with the participants name
            var directoryName = GetParticipantName(participant).ToValidFilePath();
            files.Add(new InMemoryFile
            {
                FileName = directoryName + "/",
            });

            foreach (var problem in problems)
            {
                // All submissions
                if (getSubmission == null)
                {
                    var submissions = this.GetAllSubmissions(participant.Id, problem.Id);

                    files.AddRange(
                        submissions
                            .Select(submission => ZipSubmission(problem, submission, directoryName, true)));
                }
                else
                {
                    var submission = getSubmission(participant.Id, problem.Id);

                    if (submission != null)
                    {
                        files.Add(ZipSubmission(problem, submission, directoryName));
                    }
                }
            }
        }

        return files;
    }

    private async Task<bool> IsContestActive(Contest contest)
    {
        var isActive = await this.activityService.IsContestActive(contest.Map<ContestForActivityServiceModel>());

        return isActive;
    }
}