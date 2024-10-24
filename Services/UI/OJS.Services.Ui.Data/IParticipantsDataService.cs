namespace OJS.Services.Ui.Data
{
    using OJS.Data.Models.Participants;
    using OJS.Services.Common.Data;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IParticipantsDataService : IDataService<Participant>
    {
        Task<Participant?> GetByContestByUserAndByIsOfficial(int contestId, string userId, bool isOfficial);

        Task<Participant?> GetWithContestAndProblemsForParticipantByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial);

        IQueryable<Participant> GetWithProblemsForParticipantsByContestAndUser(int contestId, string userId);

        IQueryable<Participant> GetAllByUser(string? userId);

        IQueryable<Participant> GetAllByUsernameAndContests(string username, IEnumerable<int> contestIds);

        IQueryable<Participant> GetAllByContestWithScoresAndProblems(int contestId);

        IQueryable<Participant> GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
            int contestId,
            DateTime participationStartTimeRangeStart,
            DateTime participationStartTimeRangeEnd);

        Task<bool> ExistsByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial);

        Task<bool> IsOfficialById(int id);

        Task<IDictionary<int, int>> GetParticipantsCountInContests(IEnumerable<int> contestIds, bool isOfficial);

        Task Update(
            IQueryable<Participant> participantsQuery,
            Expression<Func<Participant, Participant>> updateExpression);
    }
}