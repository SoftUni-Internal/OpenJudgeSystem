namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Contests;
    using OJS.Common.Models;
    using X.PagedList;

    public class ContestResultsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<ContestProblemListViewModel> Problems { get; set; }

        public List<ParticipantResultViewModel> Results { get; set; }

        public IPagedList<ParticipantResultViewModel> PagedResults { get; private set; }

        public bool ContestCanBeCompeted { get; set; }

        public bool ContestCanBePracticed { get; set; }

        public bool UserHasContestRights { get; set; }

        public ContestType ContestType { get; set; }

        public bool IsCompete { get; set; }

        public int TotalCount { get; set; }

        public ContestResultsViewModel ToPagedResults(int page, int pageSize)
        {
            this.PagedResults =
                new StaticPagedList<ParticipantResultViewModel>(this.Results, page, pageSize, this.TotalCount);

            return this;
        }
    }
}