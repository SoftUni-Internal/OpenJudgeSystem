namespace OJS.Services.Ui.Data
{
    using OJS.Data.Models.Participants;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Common.Data;
    using OJS.Services.Ui.Models.Participations;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IParticipantScoresDataService : IDataService<ParticipantScore>
    {
        Task<IEnumerable<ParticipantScore>> GetWithSubmissionsAndTestsByParticipantId(int participantId);

        Task<ParticipantScore?> GetByParticipantIdAndProblemId(int participantId, int problemId);

        Task<IEnumerable<ParticipantScore>> GetByProblemIdAndParticipants(IEnumerable<int> participantIds, int problemId);

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

        Task<Dictionary<int, int?>> GetMaxByProblemIdsAndParticipation(
            IEnumerable<int> problemIds,
            IEnumerable<int> participantIds);
    }
}