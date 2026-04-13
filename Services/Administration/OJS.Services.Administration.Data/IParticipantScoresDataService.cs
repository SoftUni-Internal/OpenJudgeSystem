namespace OJS.Services.Administration.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using OJS.Data.Models.Participants;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Common.Data;

    public interface IParticipantScoresDataService : IDataService<ParticipantScore>
    {
        Task<ParticipantScore?> GetByParticipantIdAndProblemId(int participantId, int problemId);

        IQueryable<ParticipantScore> GetAllByProblem(int problemId);

        IQueryable<ParticipantScore> GetAllHavingPointsExceedingLimit();

        Task ResetBySubmission(Submission submission);

        Task DeleteAllByProblem(int problemId);

        Task DeleteForParticipantByProblem(int participantId, int problemId);

        Task Delete(IEnumerable<ParticipantScore> participantScores);

        Task UpdateBySubmissionAndPoints(
            ParticipantScore participantScore,
            int? submissionId,
            int submissionPoints,
            Participant participant,
            bool shouldSaveChanges = true);

        Task RemoveSubmissionIdsBySubmissionIds(IEnumerable<int> submissionIds);
    }
}