namespace OJS.Services.Ui.Data.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using OJS.Common.Enumerations;
    using OJS.Data;
    using OJS.Data.Models.Participants;
    using OJS.Services.Common.Data.Implementations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class ParticipantsDataService(OjsDbContext db) : DataService<Participant>(db), IParticipantsDataService
    {
        public Task<Participant?> GetByContestByUserAndByIsOfficial(
            int contestId,
            string userId,
            bool isOfficial)
            => this
                .GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .FirstOrDefaultAsync();

        public Task<Participant?> GetWithContestAndProblemsForParticipantByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial)
            => this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .Include(p => p.User)
                .Include(p => p.Contest)
                .Include(p => p.ProblemsForParticipants)
                .FirstOrDefaultAsync();

        public IQueryable<Participant> GetWithProblemsForParticipantsByContestAndUser(int contestId,
            string userId)
            => this.GetAllByContestAndUser(contestId, userId)
                .Include(p => p.ProblemsForParticipants)
                .ThenInclude(pfp => pfp.Problem);

        public IQueryable<Participant> GetAllByUser(string? userId)
            => this.GetQuery(p => p.UserId == userId);

        public IQueryable<Participant> GetAllByUsernameAndContests(string username, IEnumerable<int> contestIds)
            => this.GetQuery(p => p.User.UserName == username)
                .Where(p => contestIds.Contains(p.ContestId));

        public IQueryable<Participant> GetAllByContest(int contestId)
            => this.GetQuery(p => p.ContestId == contestId);

        public IQueryable<Participant> GetAllByContestWithScoresAndProblems(int contestId)
            => this.GetAllByContest(contestId)
                .Include(p => p.Scores)
                .ThenInclude(s => s.Problem)
                .ThenInclude(p => p.ProblemGroup);

        public IQueryable<Participant> GetAllOfficialByContest(int contestId)
            => this.GetAllByContest(contestId)
                .Where(p => p.IsOfficial);

        public IQueryable<Participant> GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
            int contestId,
            DateTime participationStartTimeRangeStart,
            DateTime participationStartTimeRangeEnd)
            => this.GetAllOfficialByContest(contestId)
                    .Where(p =>
                        p.ParticipationStartTime >= participationStartTimeRangeStart &&
                        p.ParticipationStartTime <= participationStartTimeRangeEnd &&
                        p.Contest.Type == ContestType.OnlinePracticalExam);

        public Task<bool> ExistsByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial)
            => this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .AnyAsync();

        public Task<bool> IsOfficialById(int id)
            => this.GetByIdQuery(id)
                .Select(p => p.IsOfficial)
                .FirstOrDefaultAsync();

        public async Task<IDictionary<int, int>> GetParticipantsCountInContests(IEnumerable<int> contestIds, bool isOfficial)
            => await this.GetQuery(p => contestIds.Contains(p.ContestId) && p.IsOfficial == isOfficial)
                .AsNoTracking()
                .GroupBy(p => p.ContestId)
                .Select(g => new { ContestId = g.Key, ParticipantsCount = g.Count() })
                .ToDictionaryAsync(p => p.ContestId, p => p.ParticipantsCount);

        public Task Update(
            IQueryable<Participant> participantsQuery,
            Expression<Func<Participant, Participant>> updateExpression)
            => participantsQuery.UpdateFromQueryAsync(updateExpression);

        private IQueryable<Participant> GetAllByContestAndUser(int contestId, string userId) =>
            this.GetAllByContest(contestId)
                .Where(p => p.UserId == userId);

        private IQueryable<Participant> GetAllByContestByUserAndIsOfficial(
            int contestId,
            string userId,
            bool isOfficial)
            => this
                .GetAllByContestAndUser(contestId, userId)
                .Where(p => p.IsOfficial == isOfficial);
    }
}