namespace OJS.Services.Ui.Data.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using OJS.Data;
    using OJS.Data.Models.Participants;
    using OJS.Services.Common.Data.Implementations;
    using System.Collections.Generic;
    using System.Linq;
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

        public IQueryable<Participant> GetAllByUser(string? userId)
            => this.GetQuery(p => p.UserId == userId);

        public IQueryable<Participant> GetAllByContestAndUser(int contestId, string userId) =>
            this.GetAllByContest(contestId)
                .Where(p => p.UserId == userId);

        public IQueryable<Participant> GetAllByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial)
            => this
                .GetAllByContestAndUser(contestId, userId)
                .Where(p => p.IsOfficial == isOfficial);

        public IQueryable<Participant> GetAllByUsernameAndContests(string username, IEnumerable<int> contestIds)
        {
            var distinctContestIds = contestIds.Distinct();

            return this.GetQuery(p => p.User.UserName == username)
                .Where(p => distinctContestIds.Contains(p.ContestId));
        }

        public IQueryable<Participant> GetAllByContest(int contestId)
            => this.GetQuery(p => p.ContestId == contestId);

        public IQueryable<Participant> GetAllOfficialByContest(int contestId)
            => this.GetAllByContest(contestId)
                .Where(p => p.IsOfficial);

        public Task<bool> ExistsByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial)
            => this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .AnyAsync();

        public async Task<IDictionary<int, int>> GetParticipantsCountInContests(IEnumerable<int> contestIds, bool isOfficial)
            => await this.GetQuery(p => contestIds.Contains(p.ContestId) && p.IsOfficial == isOfficial)
                .AsNoTracking()
                .GroupBy(p => p.ContestId)
                .Select(g => new { ContestId = g.Key, ParticipantsCount = g.Count() })
                .ToDictionaryAsync(p => p.ContestId, p => p.ParticipantsCount);
    }
}