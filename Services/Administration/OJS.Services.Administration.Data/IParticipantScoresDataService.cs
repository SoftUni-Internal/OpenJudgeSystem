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

        Task<ParticipantScore?> GetByParticipantIdProblemIdAndIsOfficial(int participantId, int problemId, bool isOfficial);

        IQueryable<ParticipantScore> GetAll();

        IQueryable<ParticipantScore> GetAllByProblem(int problemId);

        IQueryable<ParticipantScore> GetAllHavingPointsExceedingLimit();

        Task ResetBySubmission(Submission submission);

        Task DeleteAllByProblem(int problemId);

        Task DeleteForParticipantByProblem(int participantId, int problemId);

        Task Delete(IEnumerable<ParticipantScore> participantScores);

        Task AddBySubmissionByUsernameAndIsOfficial(Submission submission, string username, bool isOfficial, Participant participant);

        Task UpdateBySubmissionAndPoints(
            ParticipantScore participantScore,
            int? submissionId,
            int submissionPoints,
            Participant participant);

        Task RemoveSubmissionIdsBySubmissionIds(IEnumerable<int> submissionIds);
    }
}