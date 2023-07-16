namespace OJS.Services.Ui.Business
{
    using System.Threading.Tasks;
    using OJS.Data.Models.Contests;
    using OJS.Data.Models.Participants;
    using OJS.Services.Common.Models;
    using OJS.Services.Ui.Models.Search;
    using SoftUni.Services.Infrastructure;

    public interface IProblemsBusinessService : IService
    {
        Task RetestById(int id);

        Task DeleteById(int id);

        Task DeleteByContest(int contestId);

        Task<ServiceResult> CopyToContestByIdByContestAndProblemGroup(int id, int contestId, int? problemGroupId);

        Task<ProblemSearchServiceResultModel> GetSearchProblemsByName(SearchServiceModel model);

        void ValidateProblemForParticipant(
            Participant participant,
            Contest contest,
            int problemId,
            bool isOfficial);
    }
}