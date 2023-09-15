using System.Diagnostics;
using X.PagedList;

namespace OJS.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using EntityFramework.Extensions;
    using Hangfire;
    using MissingFeatures;
    using OJS.Common;
    using OJS.Common.Helpers;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Services.Business.ParticipantScores;
    using OJS.Services.Business.Submissions.ArchivedSubmissions;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Common.HttpRequester;
    using OJS.Services.Common.HttpRequester.Models;
    using OJS.Services.Common.HttpRequester.Models.Users;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Services.Data.Problems;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Common.Helpers;
    using OJS.Workers.Common.Extensions;
    using OJS.Workers.Common.Helpers;

    [AuthorizeRoles(SystemRole.Administrator)]
    public class TempController : BaseController
    {
        private readonly IHangfireBackgroundJobService backgroundJobs;
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IProblemsDataService problemsDataService;
        private readonly IParticipantsDataService participantsData;
        private readonly IHttpRequesterService httpRequester;
        private readonly IOjsDbContext db;

        public TempController(
            IOjsData data,
            IHangfireBackgroundJobService backgroundJobs,
            IProblemGroupsDataService problemGroupsData,
            IParticipantsDataService participantsData,
            IHttpRequesterService httpRequester,
            IProblemsDataService problemsDataService,
            IOjsDbContext db)
            : base(data)
        {
            this.backgroundJobs = backgroundJobs;
            this.problemGroupsData = problemGroupsData;
            this.participantsData = participantsData;
            this.httpRequester = httpRequester;
            this.problemsDataService = problemsDataService;
            this.db = db;
        }

        public ActionResult RegisterJobForCleaningSubmissionsForProcessingTable()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<ISubmissionsForProcessingDataService>(
                "CleanSubmissionsForProcessingTable",
                s => s.Clean(),
                Cron.Daily());

            return null;
        }

        public ActionResult RegisterJobForDeletingLeftOverFilesInTempFolder()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob(
                "DeleteLeftOverFoldersInTempFolder",
                () => DirectoryHelpers.DeleteExecutionStrategyWorkingDirectories(),
                Cron.Daily(1));

            return null;
        }

        public ActionResult RegisterJobForArchiveOldSubmissionsWithLimit()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<IArchivedSubmissionsBusinessService>(
                "ArchiveOldSubmissionsWithLimit",
                s => s.ArchiveOldSubmissionsWithLimit(null, Settings.ArchiveSingleBatchLimit),
                Cron.Yearly(1, 1, 2, 30));

            return null;
        }

        public ActionResult RegisterJobForHardDeleteArchivedByLimit()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<IArchivedSubmissionsBusinessService>(
                "HardDeleteArchivedByLimit",
                s => s.HardDeleteArchivedByLimit(null, Settings.ArchiveSingleBatchLimit),
                Cron.Yearly(1, 1, 2, 30));

            return null;
        }

        public ActionResult RegisterJobForArchiveOldSubmissionsDailyBatch()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<IArchivedSubmissionsBusinessService>(
                "ArchiveOldSubmissionsDailyBatch",
                s => s.ArchiveOldSubmissionsDailyBatch(null, Settings.ArchiveDailyBatchSize,
                    Settings.ArchiveMaxSubBatchSize),
                Cron.Daily(1, 30));

            return null;
        }

        public ActionResult RegisterJobForNormalizingSubmissionAndParticipantScorePoints()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<IParticipantScoresBusinessService>(
                "NormalizePointsExceedingMaxPointsForProblem",
                x => x.NormalizeAllPointsThatExceedAllowedLimit(),
                Cron.Daily(2));

            return null;
        }

        public ActionResult NormalizeParticipantsWithDuplicatedParticipantScores()
        {
            var result = new StringBuilder("<p>Done! Deleted Participant scores:</p><ol>");

            var problemIds = this.Data.Problems.AllWithDeleted().Select(pr => pr.Id).ToList();
            foreach (var problemId in problemIds)
            {
                var participantScoresRepository = new EfGenericRepository<ParticipantScore>(new OjsDbContext());
                var scoresMarkedForDeletion = new List<ParticipantScore>();

                participantScoresRepository
                    .All()
                    .Where(ps => ps.ProblemId == problemId)
                    .GroupBy(p => new { p.ProblemId, p.ParticipantId })
                    .Where(participantScoreGroup => participantScoreGroup.Count() > 1)
                    .ForEach(participantScoreGroup =>
                    {
                        participantScoreGroup
                            .OrderByDescending(ps => ps.Points)
                            .ThenByDescending(ps => ps.Id)
                            .Skip(1)
                            .ForEach(ps => scoresMarkedForDeletion.Add(ps));
                    });

                if (scoresMarkedForDeletion.Any())
                {
                    foreach (var participantScoreForDeletion in scoresMarkedForDeletion)
                    {
                        participantScoresRepository.Delete(participantScoreForDeletion);
                        result.Append($@"<li>ParticipantScore with
                            ParticipantId: {participantScoreForDeletion.ParticipantId} and
                            ProblemId: {participantScoreForDeletion.ProblemId}</li>");
                    }

                    participantScoresRepository.SaveChanges();
                }
            }

            result.Append("</ol>");
            return this.Content(result.ToString());
        }

        public ActionResult DeleteEmptyProblemGroups()
        {
            var softDeleted = this.problemGroupsData
                .GetAll()
                .Where(pg => pg.Problems.All(p => p.IsDeleted))
                .Update(pg => new ProblemGroup
                {
                    IsDeleted = true
                });

            var hardDeleted = this.problemGroupsData
                .GetAllWithDeleted()
                .Where(pg => !pg.Problems.Any())
                .Delete();

            return this.Content($"Done! ProblemGroups set to deleted: {softDeleted}" +
                                $"<br/> ProblemGroups hard deleted: {hardDeleted}");
        }

        public ActionResult DeleteDuplicatedParticipantsInSameContest()
        {
            var deletedParticipantsCount = this.participantsData
                .GetAll()
                .GroupBy(p => new { p.UserId, p.IsOfficial, p.ContestId })
                .Where(gr => gr.Count() > 1)
                .SelectMany(gr => gr)
                .Where(p => !p.Scores.Any())
                .Delete();

            return this.Content($"Done! Deleted Participants count: {deletedParticipantsCount}");
        }

        public async Task<ActionResult> EqualizeUserIdFromSulsAndJudgeByUserName(string userName)
        {
            var userInfoResponse = await this.httpRequester.GetAsync<ExternalUserInfoModel>(
                new { userName },
                string.Format(UrlConstants.GetUserInfoByUsernameApiFormat, Settings.SulsPlatformBaseUrl),
                Settings.SulsApiKey);

            if (!userInfoResponse.IsSuccess || userInfoResponse.Data == null)
            {
                return this.Content($"Cannot get user info from SoftUni Platform for user \"{userName}\"");
            }

            var correctUserId = userInfoResponse.Data.Id;

            var user = this.Data.Users.GetByUsername(userName);

            if (user == null)
            {
                return this.Content($"Cannot find user with UserName: \"{userName}\" in the database");
            }

            if (user.Id == correctUserId)
            {
                return this.Content($"User \"{userName}\" has the same Id as in SoftUni Platform");
            }

            var tempUserIdToStoreParticipants = this.Data.Users.GetByUsername("gogo4ds")?.Id;

            if (string.IsNullOrEmpty(tempUserIdToStoreParticipants))
            {
                return this.Content("Invalid temp UserId to store participants");
            }

            var participantsForUser = this.participantsData.GetAllByUser(user.Id);

            var participantIdsForUser = participantsForUser.Select(x => x.Id).ToList();
            var examGroupsForUser = user.ExamGroups.ToList();

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                this.participantsData.Update(
                    participantsForUser,
                    _ => new Participant
                    {
                        UserId = tempUserIdToStoreParticipants,
                    });

                user.ExamGroups.Clear();
                this.Data.SaveChanges();

                this.Data.Users.Update(
                    u => u.UserName == userName,
                    _ => new UserProfile
                    {
                        Id = correctUserId,
                    });

                if (participantIdsForUser.Any())
                {
                    participantsForUser = this.participantsData
                        .GetAll()
                        .Where(p => participantIdsForUser.Contains(p.Id));

                    this.participantsData.Update(
                        participantsForUser,
                        _ => new Participant
                        {
                            UserId = correctUserId,
                        });
                }

                if (examGroupsForUser.Any())
                {
                    var newUser = this.Data.Users.GetById(correctUserId);

                    newUser.ExamGroups = examGroupsForUser;
                    this.Data.SaveChanges();
                }

                scope.Complete();
            }

            return this.Content(
                $@"Done. Changed Id of User ""{userName}"" to match the Id from Suls that is ""{correctUserId}""
                and modified his {participantIdsForUser.Count} Participants and {examGroupsForUser.Count} ExamGroups
                to point to the new Id");
        }

        public ActionResult OptimizeMysqlProblemSkeletonAndTestInputQueries()
        {
            var changedSkeletonsCount = 0;
            var changedTestsCount = 0;

            var problems = this.Data.Problems.All()
                .Include(p => p.Tests)
                .Where(
                    p => p.ProblemSubmissionTypeExecutionDetails.Any(
                        st => MySqlStrategiesHelper.ExecutionStrategyTypesForOptimization
                            .Any(x => x == st.SubmissionType.ExecutionStrategyType)))
                .ToList();

            foreach (var problem in problems)
            {
                var skeletons = problem
                    .ProblemSubmissionTypeExecutionDetails
                    .Where(
                        pst => MySqlStrategiesHelper
                            .ExecutionStrategyTypesForOptimization
                            .Any(x => x == pst.SubmissionType.ExecutionStrategyType))
                    .Where(pst => pst.SolutionSkeleton != null && pst.SolutionSkeleton.Any())
                    .ToList();

                foreach (var skeleton in skeletons)
                {
                    var skeletonAsString = skeleton.SolutionSkeleton.Decompress();

                    if (!string.IsNullOrWhiteSpace(skeletonAsString))
                    {
                        if (MySqlStrategiesHelper.TryOptimizeQuery(skeletonAsString, out var newSkeleton))
                        {
                            skeleton.SolutionSkeleton = newSkeleton.Compress();

                            this.Data.Problems.Update(problem);
                            changedSkeletonsCount++;
                        }
                    }
                }

                foreach (var test in problem.Tests)
                {
                    if (MySqlStrategiesHelper.TryOptimizeQuery(test.InputDataAsString, out var newInput))
                    {
                        test.InputData = newInput.Compress();

                        this.Data.Tests.Update(test);
                        changedTestsCount++;
                    }
                }
            }

            this.Data.SaveChanges();

            return this.Content(
                $"Done. Processed {problems.Count} Problems. <br/>" +
                $"Updated {changedSkeletonsCount} solution skeletons. <br/>" +
                $"Updated {changedTestsCount} test inputs.");
        }

        public async Task<ActionResult> MigrateContestCategoryFromRemoteJudge(string id)
        {
            try
            {
                var contestCategoryResponse = await this.FetchContestCategory(int.Parse(id));

                var checkers = this.Data.Checkers.All().ToList();
                var submissionTypes = this.Data.SubmissionTypes.All().ToList();

                await this.LoadContestCategoryAndAssignCheckerAndSubmissionTypes(contestCategoryResponse.Data, checkers,
                    submissionTypes);

                using (var scope = TransactionsHelper.CreateTransactionScope())
                {
                    this.Data.ContestCategories.Add(contestCategoryResponse.Data);
                    this.Data.SaveChanges();
                    scope.Complete();
                    return this.Content($"The contest category is added!");
                }
            }
            catch (Exception e)
            {
                return this.Content(
                    $"Contest categories can't be migrated and exception {e}");
            }
        }

        public ActionResult UpdateParticipantsTotalScore()
        {
            var query = @"DECLARE @BatchSize INT = 5000;
           DECLARE @MaxId INT = (SELECT MAX(Id) FROM Participants);
           DECLARE @Offset INT = 0;

           WHILE @Offset <= @MaxId
           BEGIN
               WITH OrderedParticipants AS (
               SELECT TOP (@BatchSize) Id
           FROM Participants
           WHERE Id > @Offset
           ORDER BY Id
               )
           UPDATE p
           SET TotalScoreSnapshot = ISNULL((
               SELECT SUM(ps.Points)
           FROM ParticipantScores ps
               JOIN Problems pr ON ps.ProblemId = pr.Id AND pr.IsDeleted = 0
           WHERE ps.ParticipantId = p.Id
               ), 0)
           FROM OrderedParticipants op
               JOIN Participants p ON op.Id = p.Id;

           SET @Offset = @Offset + @BatchSize;
           END;";

            this.db.ExecuteSqlCommandWithTimeout(query, 0);

            return this.Content("UPDATED");
        }

        private async Task LoadContestCategoryAndAssignCheckerAndSubmissionTypes(
            ContestCategory contestCategory,
            IEnumerable<Checker> checkers,
            IEnumerable<SubmissionType> submissionTypes)
        {
            if (contestCategory == null)
            {
                return;
            }

            foreach (var contest in contestCategory.Contests)
            {
                foreach (var problemGroup in contest.ProblemGroups)
                {
                    this.AssignCheckerAndSubmissionTypes(problemGroup.Problems, checkers, submissionTypes);
                }
            }

            var ids = contestCategory.Children.Select(x => x.Id).ToList();
            contestCategory.Children.Clear();

            foreach (var id in ids)
            {
                var contestCategoryResponse = await this.FetchContestCategory(id);

                contestCategory.Children.Add(contestCategoryResponse.Data);
                await this.LoadContestCategoryAndAssignCheckerAndSubmissionTypes(contestCategoryResponse.Data,
                    checkers,
                    submissionTypes);
            }
        }

        private async Task<ExternalDataRetrievalResult<ContestCategory>> FetchContestCategory(int id)
        {
            var contestCategoryResponse = await this.httpRequester.GetAsync<ContestCategory>(
                new { id },
                string.Format(UrlConstants.GetContestCategoryExportDataApiFormat, Settings.JudgeBaseUrl),
                Settings.ApiKey);

            if (!contestCategoryResponse.IsSuccess || contestCategoryResponse.Data == null)
            {
                throw new InvalidOperationException(contestCategoryResponse.ErrorMessage);
            }

            return contestCategoryResponse;
        }

        private void AssignCheckerAndSubmissionTypes(
            IEnumerable<Problem> problems,
            IEnumerable<Checker> checkersFromDb,
            IEnumerable<SubmissionType> submissionTypesFromDb)
        {
            foreach (var problem in problems)
            {
                var checker = checkersFromDb?.FirstOrDefault(x => x.Name == problem.Checker.Name);

                problem.Checker = checker;

                if (problem.ProblemSubmissionTypeExecutionDetails.Any())
                {
                    problem.SubmissionTypes = new List<SubmissionType>();

                    foreach (var problemSubmissionType in problem.ProblemSubmissionTypeExecutionDetails)
                    {
                        var submissionType =
                            submissionTypesFromDb.FirstOrDefault(x =>
                                x.Name == problemSubmissionType.SubmissionType.Name);

                        if (submissionType != null)
                        {
                            problemSubmissionType.SubmissionType = submissionType;
                            problemSubmissionType.SubmissionTypeId = submissionType.Id;
                            problem.SubmissionTypes.Add(submissionType);
                        }
                    }
                }
                else
                {
                    var submissionTypes = new List<SubmissionType>();

                    foreach (var problemSubmissionType in problem.SubmissionTypes)
                    {
                        var submissionType =
                            submissionTypesFromDb.FirstOrDefault(x => x.Name == problemSubmissionType.Name);
                        submissionTypes.Add(submissionType);
                    }

                    problem.SubmissionTypes = submissionTypes;
                }
            }
        }
    }
}