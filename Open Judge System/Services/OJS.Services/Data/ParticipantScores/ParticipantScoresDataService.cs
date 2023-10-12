using System;
using OJS.Services.Business.ParticipantScores.Models;

namespace OJS.Services.Data.ParticipantScores
{
    using System.Collections.Generic;
    using System.Linq;
    using EntityFramework.Extensions;
    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Data.Participants;

    public class ParticipantScoresDataService : IParticipantScoresDataService
    {
        private readonly IEfGenericRepository<ParticipantScore> participantScores;
        private readonly IParticipantsDataService participantsData;

        public ParticipantScoresDataService(
            IEfGenericRepository<ParticipantScore> participantScores,
            IParticipantsDataService participantsData)
        {
            this.participantScores = participantScores;
            this.participantsData = participantsData;
        }

        public ParticipantScore GetByParticipantIdAndProblemId(int participantId, int problemId) =>
            this.participantScores
                .All()
                .FirstOrDefault(ps =>
                    ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId);

        public ParticipantScore GetByParticipantIdProblemIdAndIsOfficial(int participantId, int problemId,
            bool isOfficial) =>
            this.participantScores
                .All()
                .FirstOrDefault(ps =>
                    ps.ParticipantId == participantId &&
                    ps.ProblemId == problemId &&
                    ps.IsOfficial == isOfficial);

        public IQueryable<ParticipantScore> GetAllByParticipantIdAndIsOfficial(int participantId, bool isOfficial)
            => this.participantScores
                .All()
                .Where(ps => ps.ParticipantId == participantId)
                .Where(ps => ps.IsOfficial == isOfficial);

        public IQueryable<ParticipantScore> GetAll() =>
            this.participantScores.All();

        public IQueryable<ParticipantScore> GetAllByProblem(int problemId) =>
            this.GetAll()
                .Where(ps => ps.ProblemId == problemId);

        public IQueryable<ParticipantScore> GetAllHavingPointsExceedingLimit() =>
            this.GetAll()
                .Where(ps => ps.Points > ps.Problem.MaximumPoints);

        public IQueryable<ParticipantScore> GetAllByParticipants(IEnumerable<int> participantIds) =>
            this.GetAll()
                .Where(ps => !ps.Problem.IsDeleted && participantIds.Contains(ps.ParticipantId));

        public void ResetBySubmission(Submission submission)
        {
            if (submission.ParticipantId == null || submission.ProblemId == null)
            {
                return;
            }

            var participant = this.participantsData
                .GetByIdQuery(submission.ParticipantId.Value)
                .Select(ParticipantScoreDataModel.FromParticipant)
                .FirstOrDefault();

            if (participant == null)
            {
                return;
            }

            var existingScore = this.GetByParticipantIdProblemIdAndIsOfficial(
                submission.ParticipantId.Value,
                submission.ProblemId.Value,
                participant.IsOfficial);

            if (existingScore == null)
            {
                this.AddBySubmissionByUsernameAndIsOfficial(
                    submission,
                    participant.UserName,
                    participant.Participant);
            }
            else
            {
                this.UpdateBySubmissionAndPoints(
                    existingScore,
                    submission.Id,
                    submission.Points,
                    participant.Participant);
            }
        }

        public void DeleteAllByProblem(int problemId) =>
            this.participantScores
                .All()
                .Where(x => x.ProblemId == problemId)
                .Delete();

        public void DeleteForParticipantByProblem(int participantId, int problemId)
        {
            var isOfficial = this.participantsData.IsOfficialById(participantId);

            var existingScore = this.GetByParticipantIdProblemIdAndIsOfficial(participantId, problemId, isOfficial);

            if (existingScore != null)
            {
                this.participantScores.Delete(existingScore);
                this.participantScores.SaveChanges();
            }
        }

        public void Delete(IEnumerable<ParticipantScore> participantScoresEnumerable)
        {
            foreach (var participantScore in participantScoresEnumerable)
            {
                this.participantScores.Delete(participantScore);
            }

            this.participantScores.SaveChanges();
        }

        public void AddBySubmissionByUsernameAndIsOfficial(
            Submission submission,
            string userName,
            Participant participant)
        {
            participant.Scores.Add(new ParticipantScore
            {
                ParticipantId = submission.ParticipantId.Value,
                ProblemId = submission.ProblemId.Value,
                SubmissionId = submission.Id,
                ParticipantName = userName,
                Points = submission.Points,
                IsOfficial = participant.IsOfficial
            });
            UpdateTotalScoreSnapshot(participant, 0, submission.Points, true);
            this.participantsData.Update(participant);
        }

        public void UpdateBySubmissionAndPoints(
            ParticipantScore participantScore,
            int? submissionId,
            int submissionPoints,
            Participant participant)
        {
            //The submission TotalScoreSnapshotModifiedOn must be changed only if it is new submission in other way the results will not be ordered correctly.
            bool shouldUpdateTotalScoreDate = submissionId != null && submissionId != participantScore.SubmissionId;
            UpdateTotalScoreSnapshot(
                participant, 
                participantScore.Points, 
                submissionPoints,
                shouldUpdateTotalScoreDate);

            participantScore.SubmissionId = submissionId;
            participantScore.Points = submissionPoints;

            this.participantsData.Update(participant);
        }

        public void RemoveSubmissionIdsBySubmissionIds(IEnumerable<int> submissionIds) =>
            this.participantScores
                .Update(
                    ps => submissionIds.Cast<int?>().Contains(ps.SubmissionId),
                    ps => new ParticipantScore
                    {
                        SubmissionId = null
                    },
                    batchSize: GlobalConstants.BatchOperationsChunkSize);

        private void UpdateTotalScoreSnapshot(
            Participant participant, 
            int previousPoints, 
            int newPoints, 
            bool shouldUpdateDate)
        {
            participant.TotalScoreSnapshot = (participant.TotalScoreSnapshot - previousPoints) + newPoints;
            if (shouldUpdateDate)
            {
                participant.TotalScoreSnapshotModifiedOn = DateTime.Now;
            }
        }
    }
}