using Kendo.Mvc.Infrastructure;

namespace OJS.Web.Infrastructure.Filters
{
    using System;
    using System.Web.Mvc;
    using MissingFeatures;
    using OJS.Common.Constants;
    using OJS.Services.Cache;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    // <summary>
    //   Filter that will remove contest from the cache if it is changed or deleted.
    // </summary> 
    public class ClearContestCacheFilter : IActionFilter<ClearContestAttribute>
    {
        private readonly ICacheService cacheService;

        public ClearContestCacheFilter(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        public void OnActionExecuting(ClearContestAttribute attribute, ActionExecutingContext filterContext)
        {
            var model = filterContext.ActionParameters["model"] as ContestAdministrationViewModel;
            var contestId = model?.Id;

            if (contestId == null || string.IsNullOrEmpty(contestId.ToString()))
            {
                throw new ArgumentNullException("The id is not presenting in the model");
            }

            this.cacheService.Remove(string.Format(CacheConstants.ContestView, contestId.ToString()));
        }

        public void OnActionExecuted(ClearContestAttribute attribute, ActionExecutedContext filterContext)
        {
            // Not used
        }
    }
}