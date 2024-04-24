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

    public class ParticipantsDataService : DataService<Participant>, IParticipantsDataService
    {
        public ParticipantsDataService(OjsDbContext db)
            : base(db)
        {
        }

        public Task<Participant?> GetByContestByUserAndByIsOfficial(
            int contestId,
            string userId,
            bool isOfficial)
            => this
                .GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .FirstOrDefaultAsync();

        public Task<Participant?> GetWithContestAndSubmissionDetailsByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial)
            => this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .Include(p => p.Contest)
                    .ThenInclude(c => c.Category)
                .Include(p => p.Contest)
                        .ThenInclude(c => c.ProblemGroups)
                            .ThenInclude(pg => pg.Problems)
                                .ThenInclude(p => p.SubmissionTypesInProblems)
                                    .ThenInclude(sp => sp.SubmissionType)
                .Include(p => p.ProblemsForParticipants)
                .FirstOrDefaultAsync();

        public Task<Participant?> GetWithProblemsForParticipantsByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial)
            => this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .Include(p => p.ProblemsForParticipants)
                    .ThenInclude(pfp => pfp.Problem)
                .FirstOrDefaultAsync();

        public IQueryable<Participant> GetAllByUser(string? userId)
            => this.GetQuery(p => p.UserId == userId);

        public IQueryable<Participant> GetAllByUsername(string username)
            => this.GetQuery(p => p.User.UserName == username);

        public IQueryable<Participant> GetAllByUsernameAndContests(string username, IEnumerable<int> contestIds)
            => this.GetQuery(p => p.User.UserName == username)
                .Where(p => contestIds.Contains(p.ContestId));

        public IQueryable<Participant> GetAllByContest(int contestId)
            => this.GetQuery(p => p.ContestId == contestId);

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

        public Task<bool> ExistsByIdAndContest(int id, int contestId)
            => this.GetByIdQuery(id)
                .AnyAsync(p => p.ContestId == contestId);

        public IQueryable<Participant> GetAllByContestAndIsOfficial(int contestId, bool isOfficial)
            => this.GetAllByContest(contestId)
                .Where(p => p.IsOfficial == isOfficial);

        public Task<bool> ExistsByContestAndUser(int contestId, string userId)
            => this.GetAllByContestAndUser(contestId, userId)
                .AnyAsync();

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

        public async Task Delete(IEnumerable<Participant> participantsForDeletion)
        {
            foreach (var participant in participantsForDeletion)
            {
                this.Delete(participant);
            }

            await this.SaveChanges();
        }

        public Task InvalidateByContestAndIsOfficial(int contestId, bool isOfficial)
            => this.GetAllByContestAndIsOfficial(contestId, isOfficial)
                .UpdateFromQueryAsync(p => new Participant
                {
                    IsInvalidated = true,
                });

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