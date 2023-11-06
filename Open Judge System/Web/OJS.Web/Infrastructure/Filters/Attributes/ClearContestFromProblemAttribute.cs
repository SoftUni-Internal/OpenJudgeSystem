namespace OJS.Web.Infrastructure.Filters.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class ClearContestFromProblemAttribute : Attribute
    {

        public ClearContestFromProblemAttribute(string queryKeyForContestId)
            => this.QueryKeyForContestId = queryKeyForContestId;

        public string QueryKeyForContestId { get; set; }
    }
}
