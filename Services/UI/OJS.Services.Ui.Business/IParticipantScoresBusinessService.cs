namespace OJS.Services.Ui.Business
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Ui.Models.Problems;
    using OJS.Services.Ui.Models.Submissions;
    using SoftUni.Services.Infrastructure;
    using OJS.Services.Ui.Models.Participations;

    public interface IParticipantScoresBusinessService : IService
    {
        Task RecalculateForParticipantByProblem(int participantId, int problemId);

        Task NormalizeAllPointsThatExceedAllowedLimit();

        Task SaveForSubmission(Submission submission);

        Task<IEnumerable<ProblemResultServiceModel>> GetParticipantScoresByProblemForUser(int problemId, bool isOfficial);

        Task<IEnumerable<ParticipantScoreModel>> GetByProblemForParticipants(IEnumerable<int> participantIds, int problemId);

        Task<IEnumerable<ParticipationForProblemMaxScoreServiceModel>> GetAllForParticipant(int participantId);
    }
}