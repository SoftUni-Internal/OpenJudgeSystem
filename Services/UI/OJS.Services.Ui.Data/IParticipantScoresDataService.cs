namespace OJS.Services.Ui.Data
{
    using OJS.Data.Models.Participants;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Common.Data;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IParticipantScoresDataService : IDataService<ParticipantScore>
    {
        Task<IEnumerable<ParticipantScore>> GetByProblemIdAndParticipants(IEnumerable<int> participantIds, int problemId);

        Task<ParticipantScore?> GetByParticipantIdProblemIdAndIsOfficial(int participantId, int problemId, bool isOfficial);

        IQueryable<ParticipantScore> GetAll();

        IQueryable<ParticipantScore> GetAllHavingPointsExceedingLimit();

        Task ResetBySubmission(Submission submission);

        Task DeleteForParticipantByProblem(int participantId, int problemId);

        Task AddBySubmissionByUsernameAndIsOfficial(Submission submission, string username, bool isOfficial, Participant participant);

        Task UpdateBySubmissionAndPoints(
            ParticipantScore participantScore,
            int? submissionId,
            int submissionPoints,
            Participant participant);
    }
}