namespace OJS.Services.Ui.Models.Contests;

using System.Collections.Generic;
using System.Linq;

public class ContestFiltersAndSortingServiceModel
{
    public IEnumerable<int> CategoryIds { get; set; } = Enumerable.Empty<int>();

    public IEnumerable<ContestStatus> Statuses { get; set; } = Enumerable.Empty<ContestStatus>();

    public IEnumerable<int> SubmissionTypeIds { get; set; } =
        Enumerable.Empty<int>();

    public int? PageNumber { get; set; }

    public int? ItemsPerPage { get; set; }

    public string SortType { get; set; }
}