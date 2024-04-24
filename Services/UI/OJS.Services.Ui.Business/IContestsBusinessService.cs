namespace OJS.Services.Ui.Business
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OJS.Data.Models.Contests;
    using OJS.Services.Common.Models;
    using OJS.Services.Common.Models.Contests;
    using OJS.Services.Ui.Models.Contests;
    using OJS.Services.Ui.Models.Search;
    using SoftUni.Common.Models;
    using SoftUni.Services.Infrastructure;

    public interface IContestsBusinessService : IService
    {
        Task<RegisterUserForContestServiceModel> RegisterUserForContest(int id, string? password, bool? hasConfirmedParticipation, bool isOfficial);

        Task ValidateContestPassword(int id, bool official, string password);

        Task<ContestDetailsServiceModel> GetContestDetails(int id);

        Task<ContestParticipationServiceModel> StartContestParticipation(StartContestParticipationServiceModel model);

        Task<ContestsForHomeIndexServiceModel> GetAllForHomeIndex();

        Task<IEnumerable<ContestForHomeIndexServiceModel>> GetAllCompetable();

        Task<IEnumerable<ContestForHomeIndexServiceModel>> GetAllPastContests();

        Task<ContestSearchServiceResultModel> GetSearchContestsByName(SearchServiceModel model);

        Task<ContestServiceModel> GetContestByProblem(int problemId);

        Task<bool> IsContestIpValidByContestAndIp(int contestId, string ip);

        Task DeleteById(int id);

        Task<PagedResult<ContestForListingServiceModel>> GetParticipatedByUserByFiltersAndSorting(
            string username,
            ContestFiltersServiceModel? sortAndFilterModel);

        Task<PagedResult<ContestForListingServiceModel>> GetAllByFiltersAndSorting(ContestFiltersServiceModel? model);
    }
}