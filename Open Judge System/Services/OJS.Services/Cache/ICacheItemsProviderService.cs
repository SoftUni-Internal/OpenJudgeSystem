namespace OJS.Services.Cache
{
    using System.Collections.Generic;

    using OJS.Common.Constants;
    using OJS.Services.Cache.Models;
    using OJS.Services.Common;

    public interface ICacheItemsProviderService : IService
    {
        IEnumerable<ContestCategoryListViewModel> GetContestSubCategoriesList(
            int? categoryId,
            int? cacheSeconds = CacheConstants.OneDayInSeconds);

        IEnumerable<ContestCategoryListViewModel> GetContestCategoryParentsList(
            int categoryId,
            int? cacheSeconds = CacheConstants.OneDayInSeconds);

        IEnumerable<CategoryMenuItemViewModel> GetMainContestCategories(
            int? cacheSeconds = CacheConstants.OneDayInSeconds);

        string GetContestCategoryName(
            int categoryId,
            int? cacheSeconds = CacheConstants.OneDayInSeconds);

        void ClearContestCategory(int categoryId);

        IEnumerable<HomeContestViewModel> GetActiveContests();

        IEnumerable<HomeContestViewModel> GetPastContests();

        /// <summary>
        /// Gets the participants count for contests in category page (for Compete and Practice mode).
        /// <para>
        /// Even a contest does not have any participants or a contest does not exist,
        /// it will be included in the result with 0 participants.
        /// </para>
        /// </summary>
        IDictionary<int, ParticipantsCountCacheModel> GetParticipantsCountForContestsInCategoryPage(
            IReadOnlyCollection<int> contestIds,
            int contestCategoryId,
            int? page);
        
        /// <summary>
        /// Gets the participants count for a contest (for Compete and Practice mode).
        /// <para>
        /// Even a contest does not have any participants or a contest does not exist,
        /// it will be included in the result with 0 participants.
        /// </para>
        /// </summary>
        ParticipantsCountCacheModel GetParticipantsCountForContest(int contestId);

        void ClearContests();
    }
}