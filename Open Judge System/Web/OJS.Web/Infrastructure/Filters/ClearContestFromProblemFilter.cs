namespace OJS.Web.Infrastructure.Filters
{
    using System;
    using System.Web.Mvc;
    using OJS.Common.Constants;
    using OJS.Services.Cache;
    using OJS.Web.Areas.Administration.ViewModels.Contest;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.Infrastructure.Filters.Contracts;

    // <summary>
    //   Filter that will remove specific contest from the cache if any of it's problems is changed or deleted, also if a new problem is added to the contest.
    // </summary
    public class ClearContestFromProblemFilter : IActionFilter<ClearContestFromProblemAttribute>
    {
        private readonly ICacheService cacheService;

        public ClearContestFromProblemFilter(ICacheService cacheService)
        {
            this.cacheService = cacheService;
        }

        public void OnActionExecuted(ClearContestFromProblemAttribute attribute, ActionExecutedContext filterContext)
        {
            // Not used
        }

        public void OnActionExecuting(ClearContestFromProblemAttribute attribute, ActionExecutingContext filterContext)
        {
            var contestId = filterContext.HttpContext.Request.Params[attribute.QueryKeyForContestId];

            if (string.IsNullOrEmpty(contestId))
            {
                throw new ArgumentNullException("The id is not presenting in the model");
            }

            this.cacheService.Remove(string.Format(CacheConstants.ContestView, contestId));
        }
    }
}