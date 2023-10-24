using OJS.Data;

namespace OJS.Services.Data.Participants
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using EntityFramework.Extensions;
    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ParticipantsDataService : IParticipantsDataService
    {
        private readonly IEfGenericRepository<Participant> participants;
        private readonly IOjsDbContext db;

        public ParticipantsDataService(IEfGenericRepository<Participant> participants, IOjsDbContext db)
        {
            this.participants = participants;
            this.db = db;
        }

        public Participant GetById(int id) => this.participants.GetById(id);

        public Participant GetByContestByUserAndByIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .FirstOrDefault();

        public Participant GetWithContestByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .Include(p => p.Contest)
                .FirstOrDefault();

        public IQueryable<Participant> GetAll() => this.participants.All();

        public IQueryable<Participant> GetAllByUser(string userId) =>
            this.GetAll()
                .Where(p => p.UserId == userId);

        public IQueryable<Participant> GetAllByContest(int contestId) =>
            this.GetAll()
                .Where(p => p.ContestId == contestId);

        public IQueryable<Participant> GetAllByManyContestsAndUserId(int[] contestIds, string userId) =>
            this.GetAll()
                .Where(p => contestIds.Contains(p.ContestId) && p.UserId == userId);

        public IQueryable<Participant> GetByIdQuery(int id) =>
            this.GetAll()
                .Where(p => p.Id == id);

        public IQueryable<Participant> GetAllOfficialByContest(int contestId) =>
            this.GetAllByContest(contestId)
                .Where(p => p.IsOfficial);

        public IQueryable<Participant> GetAllByContestIdsAndIsOfficial(IEnumerable<int> contestIds, bool isOfficial)
            => this.GetAll()
                .AsNoTracking()
                .Where(p => contestIds.Contains(p.ContestId))
                .Where(p => p.IsOfficial == isOfficial);

        public IQueryable<Participant> GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
            int contestId,
            DateTime participationStartTimeRangeStart,
            DateTime participationStartTimeRangeEnd) =>
            this.GetAllOfficialByContest(contestId)
                .Where(p =>
                    p.ParticipationStartTime >= participationStartTimeRangeStart &&
                    p.ParticipationStartTime <= participationStartTimeRangeEnd &&
                    p.Contest.Type == ContestType.OnlinePracticalExam);

        // Not doing .ToDictionary directly after .GroupBy as it will generate slower query
        public IDictionary<int, int> GetContestParticipantsCount(IEnumerable<int> contestIds, bool isOfficial) =>
            this.GetAllByContestIdsAndIsOfficial(contestIds, isOfficial)
                .GroupBy(p => p.ContestId)
                .Select(g => new { ContestId = g.Key, ParticipantsCount = g.Count() })
                .ToDictionary(p => p.ContestId, p => p.ParticipantsCount);

        public bool ExistsByIdAndContest(int id, int contestId) =>
            this.GetByIdQuery(id)
                .Any(p => p.ContestId == contestId);

        public IQueryable<Participant> GetAllByContestAndIsOfficial(int contestId, bool isOfficial) =>
            this.GetAllByContest(contestId)
                .Where(p => p.IsOfficial == isOfficial);

        public IQueryable<Participant> GetAllByContestAndIsOfficialWithUserSubmissionsScoresAndProblems(int contestId,
            bool isOfficial) =>
            this.GetAllByContest(contestId)
                .AsQueryable()
                .Include(p => p.User)
                .Include(p => p.Submissions)
                .Include(p => p.Scores)
                .Include(p => p.Problems)
                .Where(p => p.IsOfficial == isOfficial);

        public bool ExistsByContestAndUser(int contestId, string userId) =>
            this.GetAllByContestAndUser(contestId, userId)
                .Any();

        public bool ExistsByContestByUserAndIsOfficial(int contestId, string userId, bool isOfficial) =>
            this.GetAllByContestByUserAndIsOfficial(contestId, userId, isOfficial)
                .Any();

        public bool IsOfficialById(int id) =>
            this.GetByIdQuery(id)
                .Select(p => p.IsOfficial)
                .FirstOrDefault();

        public void Add(Participant participant)
        {
            this.participants.Add(participant);
            this.participants.SaveChanges();
        }

        public void Update(Participant participant, bool withSaveChagnes)
        {
            if (withSaveChagnes)
            {
                this.Update(participant);
            }
            else
            {
                this.participants.Update(participant);
            }
        }
        
        public void Update(Participant participant)
        {
            this.participants.Update(participant);
            this.participants.SaveChanges();
        }

        public void Update(
            IQueryable<Participant> participantsQuery,
            Expression<Func<Participant, Participant>> updateExpression) =>
            participantsQuery.Update(updateExpression);

        public void Delete(IEnumerable<Participant> participantsForDeletion)
        {
            foreach (var participant in participantsForDeletion)
            {
                this.participants.Delete(participant);
            }

            this.participants.SaveChanges();
        }

        public void InvalidateByContestAndIsOfficial(int contestId, bool isOfficial) =>
            this.GetAllByContestAndIsOfficial(contestId, isOfficial)
                .Update(p => new Participant
                {
                    IsInvalidated = true
                });

        public void UpdateTotalScoreSnapshot()
        {
            var totalScoreSnapshot = nameof(Participant.TotalScoreSnapshot);
            var problemId = nameof(ParticipantScore.ProblemId);
            var points = nameof(ParticipantScore.Points);
            var participantId = nameof(Participant.Id);
            
            var command = $@"DECLARE @BatchSize INT = 5000;
               DECLARE @MaxId INT = (SELECT MAX(Id) FROM Participants);
               DECLARE @Offset INT = 0;

               WHILE @Offset <= @MaxId
               BEGIN
                   WITH OrderedParticipants AS (
                   SELECT TOP (@BatchSize) {participantId}
               FROM Participants
               WHERE {participantId} > @Offset
               ORDER BY {participantId}
                   )
               UPDATE p
               SET {totalScoreSnapshot} = ISNULL((
                   SELECT SUM(ps.{points})
               FROM ParticipantScores ps
                   JOIN Problems pr ON ps.{problemId} = pr.{nameof(ParticipantScore.Id)} AND pr.{nameof(Problem.IsDeleted)} = 0
               WHERE ps.{nameof(ParticipantScore.ParticipantId)} = p.{participantId}
                   ), 0)
               FROM OrderedParticipants op
                   JOIN Participants p ON op.{participantId} = p.{participantId};

               SET @Offset = @Offset + @BatchSize;
               END;";

            this.db.ExecuteSqlCommandWithTimeout(command, 0);
        }

        public void SaveChanges()
        {
            this.db.SaveChanges();
        }

        private IQueryable<Participant> GetAllByContestAndUser(int contestId, string userId) =>
            this.GetAllByContest(contestId)
                .Where(p => p.UserId == userId);

        private IQueryable<Participant> GetAllByContestByUserAndIsOfficial(
            int contestId,
            string userId,
            bool isOfficial) =>
            this.GetAllByContestAndUser(contestId, userId)
                .Where(p => p.IsOfficial == isOfficial);
    }
}