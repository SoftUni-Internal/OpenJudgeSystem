namespace OJS.Services.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OJS.Common.Constants;
    using OJS.Common.Extensions;
    using OJS.Services.Cache.Models;
    using OJS.Services.Data.ContestCategories;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;

    public class CacheItemsProviderService : ICacheItemsProviderService
    {
        private readonly IContestCategoriesDataService contestCategoriesData;
        private readonly IContestsDataService contestsData;
        private readonly ICacheService redisCache;
        private readonly IParticipantsDataService participantsData;

        private static readonly TimeSpan TimeToCacheParticipantsCount = TimeSpan.FromMinutes(5);

        public CacheItemsProviderService(
            IContestCategoriesDataService contestCategoriesData,
            IContestsDataService contestsData,
            ICacheService redisCache,
            IParticipantsDataService participantsData)
        {
            this.contestCategoriesData = contestCategoriesData;
            this.contestsData = contestsData;
            this.redisCache = redisCache;
            this.participantsData = participantsData;
        }

        public IEnumerable<ContestCategoryListViewModel> GetContestSubCategoriesList(
            int? categoryId,
            int? cacheSeconds)
        {
            var cacheId = categoryId.HasValue
                ? string.Format(CacheConstants.ContestSubCategoriesFormat, categoryId.Value)
                : CacheConstants.ContestCategoriesTree;

            if (cacheSeconds.HasValue)
            {
                return this.redisCache.GetOrSet(cacheId, GetSubCategories, TimeSpan.FromSeconds(cacheSeconds.Value));
            }

            return this.redisCache.GetOrSet(cacheId, GetSubCategories);


            IEnumerable<ContestCategoryListViewModel> GetSubCategories() =>
                this.contestCategoriesData
                    .GetAllVisible()
                    .Where(cc => categoryId.HasValue ? cc.ParentId == categoryId : cc.ParentId == null)
                    .OrderBy(cc => cc.OrderBy)
                    .Select(ContestCategoryListViewModel.FromCategory)
                    .ToList();
        }

        public IEnumerable<ContestCategoryListViewModel> GetContestCategoryParentsList(
            int categoryId,
            int? cacheSeconds = CacheConstants.OneDayInSeconds)
        {
            var cacheId = string.Format(CacheConstants.ContestParentCategoriesFormat, categoryId);

            var contestCategories =
                this.redisCache.GetOrSet(cacheId, GetParentCategories, TimeSpan.FromSeconds(cacheSeconds.Value));

            IEnumerable<ContestCategoryListViewModel> GetParentCategories()
            {
                var categories = new List<ContestCategoryListViewModel>();
                var category = this.contestCategoriesData.GetById(categoryId);

                while (category != null)
                {
                    categories.Add(new ContestCategoryListViewModel
                    {
                        Id = category.Id,
                        Name = category.Name
                    });

                    category = category.Parent;
                }

                categories.Reverse();

                return categories;
            }

            return contestCategories;
        }

        public IEnumerable<CategoryMenuItemViewModel> GetMainContestCategories(int? cacheSeconds) =>
            this.redisCache.GetOrSet(
                CacheConstants.MainContestCategoriesDropDown,
                () =>
                    this.contestCategoriesData
                        .GetAllVisible()
                        .Where(x => !x.ParentId.HasValue)
                        .OrderBy(x => x.OrderBy)
                        .Select(CategoryMenuItemViewModel.FromCategory)
                        .ToList(),
                TimeSpan.FromSeconds(cacheSeconds.Value));


        public string GetContestCategoryName(int categoryId, int? cacheSeconds) =>
            this.redisCache.GetOrSet(
                string.Format(CacheConstants.ContestCategoryNameFormat, categoryId),
                () => this.contestCategoriesData.GetNameById(categoryId),
                TimeSpan.FromSeconds(cacheSeconds.Value));

        public void ClearContestCategory(int categoryId)
        {
            var contestCategory = this.contestCategoriesData.GetById(categoryId);

            if (contestCategory == null)
            {
                return;
            }

            contestCategory.Children
                .Select(cc => cc.Id)
                .ToList()
                .ForEach(RemoveCacheFromCategory);

            while (contestCategory != null)
            {
                RemoveCacheFromCategory(contestCategory.Id);

                contestCategory = contestCategory.Parent;
            }

            void RemoveCacheFromCategory(int contestCategoryId)
            {
                var categoryNameCacheId = string.Format(
                    CacheConstants.ContestCategoryNameFormat,
                    contestCategoryId);

                var subCategoriesCacheId = string.Format(
                    CacheConstants.ContestSubCategoriesFormat,
                    contestCategoryId);

                var parentCategoriesCacheId = string.Format(
                    CacheConstants.ContestParentCategoriesFormat,
                    contestCategoryId);

                this.redisCache.Remove(categoryNameCacheId);
                this.redisCache.Remove(subCategoriesCacheId);
                this.redisCache.Remove(parentCategoriesCacheId);
            }
        }

        public IEnumerable<HomeContestViewModel> GetActiveContests()
        {
            var upcomingMaxTime = DateTime.Now.AddHours(2);

            var cachedResult = this.redisCache.GetOrSet(
                CacheConstants.ActiveContests,
                () => this.contestsData
                    .GetAllUpcoming()
                    .Where(x => x.StartTime.HasValue && x.StartTime <= upcomingMaxTime)
                    .Select(HomeContestViewModel.FromContest)
                    .Concat(
                        this.contestsData.GetAllCompetable()
                            .Select(HomeContestViewModel.FromContest))
                    .OrderBy(c => c.EndTime)
                    .ToList(),
                TimeSpan.FromHours(DateTime.UtcNow.AddHours(1).Hour));

            return cachedResult.Where(
                    c =>
                        c.StartTime <= DateTime.Now &&
                        c.EndTime.HasValue &&
                        c.EndTime >= DateTime.Now)
                .OrderBy(c => c.EndTime);
        }

        public IEnumerable<HomeContestViewModel> GetPastContests() =>
            this.redisCache.GetOrSet(
                CacheConstants.PastContests,
                () => this.contestsData
                    .GetAllPast()
                    .OrderByDescending(pc => pc.EndTime)
                    .Select(HomeContestViewModel.FromContest)
                    .Take(CacheConstants.DefaultPastContestsToTake)
                    .ToList(),
                TimeSpan.FromSeconds(CacheConstants.OneHourInSeconds));

        public IDictionary<int, ParticipantsCountCacheModel> GetParticipantsCountForContestsInCategoryPage(
            IReadOnlyCollection<int> contestIds,
            int contestCategoryId,
            int? page) =>
            this.redisCache.GetOrSet(
                string.Format(CacheConstants.ParticipantsCountByContest, contestCategoryId, page),
                () => this.GetContestsParticipantsCount(contestIds),
                TimeToCacheParticipantsCount);

        public ParticipantsCountCacheModel GetParticipantsCountForContest(int contestId)
            => this.redisCache.GetOrSet(
                string.Format(CacheConstants.ParticipantsCountByContest, contestId),
                () => this.GetContestsParticipantsCount(new[] { contestId })[contestId],
                TimeToCacheParticipantsCount);

        public void ClearContests()
        {
            this.ClearActiveContests();
            this.ClearPastContests();
        }

        private void ClearActiveContests() =>
            this.redisCache.Remove(CacheConstants.ActiveContests);

        private void ClearPastContests() =>
            this.redisCache.Remove(CacheConstants.PastContests);

        /// <summary>
        /// Gets a dictionary with all provided contests (Id as Key) and their corresponding
        /// participants count, separated by Compete and Practice mode.
        /// <para>
        /// Even if a contest has no participants or the contest with the provided id does not exist,
        /// it will be included in the result with 0 participants.
        /// </para>
        /// </summary>
        /// <param name="contestIds">The contests for which to gather participants count</param>
        /// <returns>Dictionary with all the provided <see cref="contestIds"/> as keys</returns>
        private IDictionary<int, ParticipantsCountCacheModel> GetContestsParticipantsCount(
            IReadOnlyCollection<int> contestIds)
        {
            var officialParticipants = this.participantsData.GetContestParticipantsCount(contestIds, true);
            var practiceParticipants = this.participantsData.GetContestParticipantsCount(contestIds, false);

            return contestIds.ToDictionary(
                id => id,
                id => new ParticipantsCountCacheModel
                {
                    Official = officialParticipants.GetValuerOrDefault(id),
                    Practice = practiceParticipants.GetValuerOrDefault(id),
                });
        }
    }
}