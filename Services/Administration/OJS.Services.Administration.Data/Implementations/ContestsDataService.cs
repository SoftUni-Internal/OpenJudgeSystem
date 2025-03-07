namespace OJS.Services.Administration.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using OJS.Common.Enumerations;
    using OJS.Data;
    using OJS.Data.Models.Contests;
    using OJS.Data.Models.Problems;
    using OJS.Services.Common.Data;
    using OJS.Services.Common.Models.Contests;
    using OJS.Services.Common.Models.Users;
    using OJS.Services.Infrastructure.Extensions;

    public class ContestsDataService(OjsDbContext db, IContestsActivityService activityService)
        : AdministrationDataService<Contest>(db), IContestsDataService
    {
        public Task<Contest?> GetByIdWithProblems(int id)
            => this.GetByIdQuery(id)
                .Include(c => c.ProblemGroups)
                    .ThenInclude(pg => pg.Problems)
                .FirstOrDefaultAsync();

        public Task<Contest?> GetByIdWithProblemsAndParticipants(int id)
            => this.GetByIdQuery(id)
                .Include(c => c.ProblemGroups)
                    .ThenInclude(pg => pg.Problems)
                .Include(c => c.Participants)
                .FirstOrDefaultAsync();

        public Task<Contest?> GetByIdWithParticipantsScoresAndSubmissions(int id)
            => this.GetByIdQuery(id)
                .Include(c => c.Participants)
                    .ThenInclude(p => p.Scores)
                .Include(p => p.Participants)
                    .ThenInclude(p => p.Submissions)
                .FirstOrDefaultAsync();

        public IQueryable<Contest> GetAllVisible()
            => this.GetQuery(c => c.IsVisible);

        public IQueryable<Contest> GetAllByLecturer(string? lecturerId)
            => this.GetQuery(c =>
                    c.LecturersInContests.Any(l => l.LecturerId == lecturerId) ||
                    c.Category!.LecturersInContestCategories.Any(l => l.LecturerId == lecturerId));

        public Task<int> GetMaxPointsForExportById(int id)
            => this.GetMaxPointsByIdAndProblemGroupsFilter(id, pg => pg.Type != ProblemGroupType.ExcludedFromHomework);

        public async Task<string?> GetNameById(int id)
            => await this.GetByIdQuery(id)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

        public async Task<bool> IsActiveById(int id)
        {
            var contest = await this.OneById(id);

            return contest != null && await activityService.IsContestActive(contest.Map<ContestForActivityServiceModel>());
        }

        public async Task<bool> IsWithRandomTasksById(int id)
        {
            var type = await this.GetByIdQuery(id)
                .Select(c => c.Type)
                .FirstOrDefaultAsync();

            return type is ContestType.OnlinePracticalExam or ContestType.OnsitePracticalExamWithRandomTasks;
        }

        public Task<bool> IsUserLecturerInContestByContestAndUser(int id, string? userId)
            => this.GetByIdQuery(id)
                .AnyAsync(c =>
                    c.LecturersInContests.Any(l => l.LecturerId == userId) ||
                    c.Category!.LecturersInContestCategories.Any(l => l.LecturerId == userId));

        public async Task<IEnumerable<string>> GetProblemNamesById(int id)
            => await this.GetByIdQuery(id)
                .SelectMany(c => c.ProblemGroups)
                .SelectMany(pg => pg.Problems)
                .Select(p => p.Name)
                .ToListAsync();

        // Lecturers can see contests in which they are lecturers or contests that are in a category for which they are lecturers.
        protected override Expression<Func<Contest, bool>> GetUserFilter(UserInfoModel user)
            => contest => user.IsAdmin ||
                contest.Category!.LecturersInContestCategories.Any(cc => cc.LecturerId == user.Id) ||
                contest.LecturersInContests.Any(l => l.LecturerId == user.Id);

        private async Task<int> GetMaxPointsByIdAndProblemGroupsFilter(int id, Expression<Func<ProblemGroup, bool>> filter)
        {
            var problemsMaxPoints = await this.GetByIdQuery(id)
                .SelectMany(c => c.ProblemGroups)
                .Where(filter)
                .Select(pg => pg.Problems
                    .Where(p => !p.IsDeleted)
                    .Select(p => (int)p.MaximumPoints)
                    .FirstOrDefault())
                .ToListAsync();

            return problemsMaxPoints.Sum();
        }
    }
}