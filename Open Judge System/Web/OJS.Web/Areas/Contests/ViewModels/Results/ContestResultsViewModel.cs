namespace OJS.Web.Areas.Contests.ViewModels.Results
{
    using System.Collections.Generic;
    using Contests;
    using OJS.Common.Models;
    using X.PagedList;

    public class ContestResultsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<ContestProblemListViewModel> Problems { get; set; }

        public IPagedList<ParticipantResultViewModel> Results { get; set; }

        public bool ContestCanBeCompeted { get; set; }

        public bool ContestCanBePracticed { get; set; }

        public bool UserHasContestRights { get; set; }

        public ContestType ContestType { get; set; }

        public bool IsCompete { get; set; }
    }
}