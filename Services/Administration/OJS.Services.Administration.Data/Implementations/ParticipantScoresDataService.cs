namespace OJS.Services.Administration.Data.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using OJS.Data;
    using OJS.Data.Models.Participants;
    using OJS.Data.Models.Submissions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using OJS.Common;

    public class ParticipantScoresDataService : AdministrationDataService<ParticipantScore>, IParticipantScoresDataService
    {
        private readonly IParticipantsDataService participantsData;

        public ParticipantScoresDataService(
            OjsDbContext db,
            IParticipantsDataService participantsData)
            : base(db)
            => this.participantsData = participantsData;

        public Task<ParticipantScore?> GetByParticipantIdAndProblemId(int participantId, int problemId) =>
            this.One(ps =>
                    ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId);

        public Task<ParticipantScore?> GetByParticipantIdProblemIdAndIsOfficial(int participantId, int problemId, bool isOfficial) =>
            this.One(ps =>
                    ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId &&
                    ps.IsOfficial == isOfficial);

        public IQueryable<ParticipantScore> GetAllByProblem(int problemId) =>
            this.GetQuery(ps => ps.ProblemId == problemId);

        public IQueryable<ParticipantScore> GetAllHavingPointsExceedingLimit() =>
            this.GetQuery(ps => ps.Points > ps.Problem.MaximumPoints)
                .Include(ps => ps.Participant)
                .Include(ps => ps.Problem);

        public async Task ResetBySubmission(Submission submission)
        {
            var participant = await this.participantsData
                .GetByIdQuery(submission.ParticipantId)
                .Select(p => new
                {
                    p.IsOfficial,
                    p.User.UserName,
                    Participant = p,
                })
                .FirstOrDefaultAsync();

            if (participant == null)
            {
                return;
            }

            var existingScore = await this.GetByParticipantIdProblemIdAndIsOfficial(
                submission.ParticipantId,
                submission.ProblemId,
                participant.IsOfficial);

            if (existingScore == null)
            {
                await this.AddBySubmissionByUsernameAndIsOfficial(submission, participant.UserName!, participant.IsOfficial, participant.Participant);
            }
            else
            {
                await this.UpdateBySubmissionAndPoints(existingScore, submission.Id, submission.Points, participant.Participant);
            }
        }

        public Task DeleteAllByProblem(int problemId)
            => this.GetQuery(x => x.ProblemId == problemId)
                .DeleteFromQueryAsync();

        public async Task DeleteForParticipantByProblem(int participantId, int problemId)
        {
            var isOfficial = await this.participantsData.IsOfficialById(participantId);

            var existingScore = await this.GetByParticipantIdProblemIdAndIsOfficial(participantId, problemId, isOfficial);

            if (existingScore != null)
            {
                this.Delete(existingScore);
                await this.SaveChanges();
            }
        }

        public async Task Delete(IEnumerable<ParticipantScore> participantScores)
        {
            this.DeleteMany(participantScores);

            await this.SaveChanges();
        }

        public async Task AddBySubmissionByUsernameAndIsOfficial(Submission submission, string username, bool isOfficial, Participant participant)
        {
            await this.Add(new ParticipantScore
            {
                ParticipantId = submission.ParticipantId,
                ProblemId = submission.ProblemId,
                SubmissionId = submission.Id,
                ParticipantName = username,
                Points = submission.Points,
                IsOfficial = isOfficial,
            });

            UpdateTotalScoreSnapshot(participant, 0, submission.Points, true);
            this.participantsData.Update(participant);

            await this.SaveChanges();
        }

        public async Task UpdateBySubmissionAndPoints(
            ParticipantScore participantScore,
            int? submissionId,
            int submissionPoints,
            Participant participant,
            bool shouldSaveChanges = true)
        {
            // The submission TotalScoreSnapshotModifiedOn must be changed only if it is new submission in other way the results will not be ordered correctly.
            var shouldUpdateTotalScoreDate = submissionId != null && submissionId != participantScore.SubmissionId;

            participantScore.SubmissionId = submissionId;
            participantScore.Points = submissionPoints;

            UpdateTotalScoreSnapshot(
                participant,
                participantScore.Points,
                submissionPoints,
                shouldUpdateTotalScoreDate);

            if (shouldSaveChanges)
            {
                this.Update(participantScore);
                this.participantsData.Update(participant);

                await this.SaveChanges();
            }
        }

        public async Task RemoveSubmissionIdsBySubmissionIds(IEnumerable<int> submissionIds) =>
            await this
                .Update(
                    ps => submissionIds.Cast<int?>().Contains(ps.SubmissionId),
                    ps => new ParticipantScore
                    {
                        SubmissionId = null
                    },
                    batchSize: GlobalConstants.BatchOperationsChunkSize);

        private static void UpdateTotalScoreSnapshot(
            Participant participant,
            int previousPoints,
            int newPoints,
            bool shouldUpdateDate)
        {
            participant.TotalScoreSnapshot = participant.TotalScoreSnapshot - previousPoints + newPoints;
            if (shouldUpdateDate)
            {
                participant.TotalScoreSnapshotModifiedOn = DateTime.Now;
            }
        }
    }
}