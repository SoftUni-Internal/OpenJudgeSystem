namespace OJS.Web.Infrastructure.Filters.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class ClearContestAttribute : Attribute
    {
        public ClearContestAttribute(string queryKeyForContestId) => this.QueryKeyForContestId = queryKeyForContestId;

        public string QueryKeyForContestId { get; set; }
    }
}