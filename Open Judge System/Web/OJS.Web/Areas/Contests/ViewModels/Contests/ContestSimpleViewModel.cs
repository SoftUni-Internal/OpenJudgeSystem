namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System;
    using System.Linq.Expressions;
    using OJS.Common.Extensions;
    using OJS.Data.Models;

    public class ContestSimpleViewModel
    {
        public static Expression<Func<Contest, ContestSimpleViewModel>> FromContest =>
            contest => new ContestSimpleViewModel
            {
                Id = contest.Id,
                Name = contest.Name,
            };
        
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string NameForUrl => this.Name.ToUrl();
    }
}