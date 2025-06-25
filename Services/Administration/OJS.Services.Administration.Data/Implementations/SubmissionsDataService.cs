namespace OJS.Services.Administration.Data.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using OJS.Data;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Common.Models.Users;
    using OJS.Services.Infrastructure;
    using OJS.Services.Infrastructure.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Services.Common.Data;
    using static OJS.Common.GlobalConstants.Roles;

    public class SubmissionsDataService : AdministrationDataService<Submission>, ISubmissionsDataService
    {
        private readonly IDatesService datesService;
        private readonly IArchivesDataService archivesDataService;
        private readonly ITestRunsDataService testRunsDataService;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly ITransactionsProvider transactions;

        public SubmissionsDataService(
            OjsDbContext submissions,
            IDatesService datesService,
            IArchivesDataService archivesDataService,
            ITestRunsDataService testRunsDataService,
            IParticipantScoresDataService participantScoresData,
            ITransactionsProvider transactions)
            : base(submissions)
        {
            this.datesService = datesService;
            this.archivesDataService = archivesDataService;
            this.testRunsDataService = testRunsDataService;
            this.participantScoresData = participantScoresData;
            this.transactions = transactions;
        }

        public Submission? GetBestForParticipantByProblem(int participantId, int problemId)
            => this.GetAllByProblemAndParticipant(problemId, participantId)
                .Where(s => s.Processed)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.Id)
                .FirstOrDefault();

        public IQueryable<Submission> GetByIdQuery(int id)
            => this.GetQuery(s => s.Id == id);

        public IQueryable<Submission> GetAllByProblem(int problemId)
            => this.GetQuery(s => s.ProblemId == problemId);

        public Task<int> GetCountByProblem(int problemId) => this.GetAllByProblem(problemId).CountAsync();

        public IQueryable<Submission> GetAllByProblems(IEnumerable<int> problemIds)
            => this.GetQuery(s => problemIds.Contains(s.ProblemId));

        public Task<Submission?> GetNonDeletedWithNonDeletedProblemTestsAndSubmissionTypes(int id)
        {
            var queryable = this.GetByIdQuery(id)
                .Where(s => !s.IsDeleted && !s.Problem.IsDeleted);

            queryable = IncludeProblemTestsAndSubmissionTypes(queryable);

            return queryable.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Submission>> GetAllNonDeletedByProblemWithProblemTestsAndSubmissionTypes(int problemId)
        {
            var queryable = this.GetAllByProblem(problemId).Where(s => !s.IsDeleted);

            queryable = IncludeProblemTestsAndSubmissionTypes(queryable);

            return await queryable.ToListAsync();
        }

        public IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId)
            => this.GetQuery(s => s.ParticipantId == participantId && s.ProblemId == problemId);

        public IQueryable<Submission> GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
            DateTime createdBeforeDate,
            DateTime nonBestCreatedBeforeDate)
            => this.GetQuery(s => s.CreatedOn < createdBeforeDate ||
                                  (s.CreatedOn < nonBestCreatedBeforeDate &&
                                   s.Participant!.Scores.All(ps => ps.SubmissionId != s.Id)));

        public IQueryable<Submission> GetAllHavingPointsExceedingLimit()
            => this.GetQuery(s => s.Points > s.Problem.MaximumPoints)
                .Include(s => s.Problem);

        public IQueryable<Submission> GetAllBySubmissionTypeSentByRegularUsersInTheLastNMonths(int submissionTypeId, int monthsCount)
            => this.GetQuery()
                .Include(s => s.Participant)
                .ThenInclude(p => p.User)
                .ThenInclude(u => u.UsersInRoles)
                .Where(s => s.SubmissionTypeId == submissionTypeId &&
                            s.CreatedOn > this.datesService.GetUtcNow().AddMonths(-monthsCount) &&
                            !s.Participant.User.UsersInRoles.Any(ur => new List<string>
                            {
                                Administrator, Lecturer, Developer,
                            }.Contains(ur.Role.Name!)));

        public async Task SetAllToUnprocessedByProblem(int problemId)
            => await this.GetAllByProblem(problemId)
                .UpdateFromQueryAsync(s => new Submission { Processed = false });

        public void DeleteByProblem(int problemId)
            => this.Delete(s => s.ProblemId == problemId);

        public Task RemoveTestRunsCacheByProblem(int problemId)
            => this.GetAllByProblem(problemId)
                .UpdateFromQueryAsync(s => new Submission { TestRunsCache = null });

        public async Task<IEnumerable<int>> GetIdsByProblemId(int problemId)
            => await this.GetAllByProblem(problemId)
                .Select(s => s.Id)
                .ToListAsync();

        /// <summary>
        /// Deletes archived submissions from OnlineJudgeSystem Db and marks them as Hard Deleted in OnlineJudgeSystemArchives.
        /// </summary>
        /// <param name="deleteCountLimit">Specifies a limit to the number of submissions deleted, if omitted or 0 is passed, delete all available, without limits.</param>
        /// <returns></returns>
        public async Task<int> HardDeleteArchived(int deleteCountLimit = 0)
        {
            var hardDeletedCount = 0;

            var submissionBatches = deleteCountLimit > 0
                ? this.archivesDataService
                 .GetAllNotHardDeletedFromMainDatabase()
                 .Distinct()
                 .OrderBy(x => x.Id)
                 .InSelfModifyingBatches(GlobalConstants.BatchOperationsChunkSize, deleteCountLimit)
                : this.archivesDataService
                .GetAllNotHardDeletedFromMainDatabase()
                .Distinct()
                .OrderBy(x => x.Id)
                .InSelfModifyingBatches(GlobalConstants.BatchOperationsChunkSize);

            foreach (var submissionIdsBatch in submissionBatches)
            {
                var archivedIds = submissionIdsBatch
                    .Select(s => s.Id)
                    .ToHashSet();

                if(archivedIds.Count == 0)
                {
                    break;
                }

                var idsSet = await this.GetQuery()
                    .Where(s => archivedIds.Contains(s.Id))
                    .Select(x => x.Id)
                    .ToListAsync();

                if(idsSet.Count > 0)
                {
                    await this.transactions.ExecuteInTransaction(async () =>
                    {
                        await this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(idsSet);

                        await this.testRunsDataService.Delete(
                            tr => idsSet.Contains(tr.SubmissionId),
                            batchSize: GlobalConstants.BatchOperationsChunkSize);

                        await this.GetQuery(s => idsSet.Contains(s.Id)).DeleteFromQueryAsync();
                    }, IsolationLevel.ReadCommitted);
                }

                foreach (var archivedIdsBatch in archivedIds.InBatches(GlobalConstants.BatchOperationsChunkSize / 10))
                {
                    await this.archivesDataService.MarkAsHardDeletedFromMainDatabase(archivedIdsBatch);
                }

                hardDeletedCount += idsSet.Count;
            }

            return hardDeletedCount;
        }

        protected override Expression<Func<Submission, bool>> GetUserFilter(UserInfoModel user)
            => submission => user.IsAdmin ||
                             submission.Problem.ProblemGroup.Contest.Category!.LecturersInContestCategories.Any(cc => cc.LecturerId == user.Id) ||
                             submission.Problem.ProblemGroup.Contest.LecturersInContests.Any(l => l.LecturerId == user.Id);

        private static IQueryable<Submission> IncludeProblemTestsAndSubmissionTypes(IQueryable<Submission> queryable)
            => queryable
                .AsSplitQuery()
                .Include(s => s.SubmissionType)
                .Include(s => s.Problem)
                    .ThenInclude(p => p.Checker)
                .Include(s => s.Problem)
                    .ThenInclude(p => p.Tests)
                .Include(s => s.Problem)
                    .ThenInclude(p => p.SubmissionTypesInProblems);
    }
}