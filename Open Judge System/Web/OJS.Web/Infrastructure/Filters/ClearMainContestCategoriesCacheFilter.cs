namespace OJS.Web.Infrastructure.Filters
{
    using System.Web.Mvc;

    using OJS.Common.Constants;
    using OJS.Services.Cache;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    public class ClearMainContestCategoriesCacheFilter : IActionFilter<ClearMainContestCategoriesCacheAttribute>
    {
        private readonly ICacheService cacheService;

        public ClearMainContestCategoriesCacheFilter(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        public void OnActionExecuting(
            ClearMainContestCategoriesCacheAttribute attribute,
            ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(
            ClearMainContestCategoriesCacheAttribute attribute,
            ActionExecutedContext filterContext)
        {
            this.cacheService.Remove(CacheConstants.MainContestCategoriesDropDown);
            this.cacheService.Remove(CacheConstants.ContestCategoriesTree);
        }
    }
}