namespace OJS.Services.Ui.Business
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OJS.Data.Models.Contests;
    using OJS.Services.Common.Models;
    using OJS.Services.Common.Models.Contests;
    using OJS.Services.Ui.Models.Contests;
    using OJS.Services.Ui.Models.Search;
    using OJS.Services.Infrastructure.Models;
    using OJS.Services.Infrastructure;

    public interface IContestsBusinessService : IService
    {
        Task<ContestRegistrationDetailsServiceModel> GetContestRegistrationDetails(int id, bool isOfficial);

        Task<bool> RegisterUserForContest(
            int id,
            string? password,
            bool? hasConfirmedParticipation,
            bool isOfficial);

        Task ValidateContestPassword(int id, bool official, string password);

        Task<ContestDetailsServiceModel> GetContestDetails(int id);

        Task<ContestLegacyExportServiceModel> Export(int id);

        Task<IEnumerable<int>> GetExistingIds(IEnumerable<int> ids);

        Task<ContestParticipationServiceModel> GetParticipationDetails(StartContestParticipationServiceModel model);

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

        Task<PagedResult<ContestForListingServiceModel>> PrepareActivityAndResults(
            PagedResult<ContestForListingServiceModel> pagedContests,
            Dictionary<int, List<ParticipantResultServiceModel>> participantResultsByContest);

        Task<Dictionary<int, List<ParticipantResultServiceModel>>> GetUserParticipantResultsForContestInPage(
            ICollection<int> contestIds);
    }
}