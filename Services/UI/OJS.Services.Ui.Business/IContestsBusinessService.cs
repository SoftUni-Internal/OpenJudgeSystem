namespace OJS.Services.Ui.Business
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OJS.Services.Ui.Models.Contests;
    using OJS.Services.Ui.Models.Search;
    using OJS.Services.Infrastructure.Models;
    using OJS.Services.Infrastructure;

    public interface IContestsBusinessService : IService
    {
        Task<ServiceResult<ContestRegistrationDetailsServiceModel>> GetContestRegistrationDetails(int id, bool isOfficial);

        Task<bool> RegisterUserForContest(
            int id,
            string? password,
            bool? hasConfirmedParticipation,
            bool isOfficial);

        Task<ServiceResult<VoidResult>> ValidateContestPassword(int id, bool official, string password);

        Task<ServiceResult<ContestDetailsServiceModel>> GetContestDetails(int id);

        Task<ServiceResult<ContestParticipationServiceModel>> GetParticipationDetails(StartContestParticipationServiceModel model);

        Task<ContestsForHomeIndexServiceModel> GetAllForHomeIndex();

        Task<ContestSearchServiceResultModel> GetSearchContestsByName(SearchServiceModel model);

        Task<PagedResult<ContestForListingServiceModel>> GetParticipatedByUserByFiltersAndSorting(
            string username,
            ContestFiltersServiceModel? sortAndFilterModel,
            int? contestId = null,
            int? categoryId = null);

        Task<PagedResult<ContestForListingServiceModel>> GetAllByFiltersAndSorting(ContestFiltersServiceModel? model);

        Task<PagedResult<ContestForListingServiceModel>> PrepareActivityAndResults(
            PagedResult<ContestForListingServiceModel> pagedContests,
            Dictionary<int, List<ParticipantResultServiceModel>> participantResultsByContest);

        Task<Dictionary<int, List<ParticipantResultServiceModel>>> GetUserParticipantResultsForContestInPage(
            ICollection<int> contestIds);

        Task<IEnumerable<string?>> GetEmailsOfParticipantsInContest(int contestId);

        Task<IEnumerable<ContestForListingServiceModel>> GetAllParticipatedContests(string username);
    }
}