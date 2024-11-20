﻿namespace OJS.Services.Administration.Data.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using OJS.Data;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Common.Models.Users;
    using OJS.Services.Infrastructure;
    using OJS.Services.Infrastructure.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using static OJS.Common.GlobalConstants.Roles;

    public class SubmissionsDataService : AdministrationDataService<Submission>, ISubmissionsDataService
    {
        private readonly IDatesService datesService;
        public SubmissionsDataService(OjsDbContext submissions, IDatesService datesService)
            : base(submissions)
            => this.datesService = datesService;

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

        public Task<Submission?> GetWithProblemTestsAndSubmissionTypes(int id)
        {
            var queryable = this.GetByIdQuery(id);

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

        protected override Expression<Func<Submission, bool>> GetUserFilter(UserInfoModel user)
            => submission => user.IsAdmin ||
                             submission.Problem.ProblemGroup.Contest.Category!.LecturersInContestCategories.Any(cc => cc.LecturerId == user.Id) ||
                             submission.Problem.ProblemGroup.Contest.LecturersInContests.Any(l => l.LecturerId == user.Id);

        private static IQueryable<Submission> IncludeProblemTestsAndSubmissionTypes(IQueryable<Submission> queryable)
            => queryable
                .AsNoTracking()
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