namespace OJS.Services.Ui.Models.Contests;

using System.Collections.Generic;

public class ContestFiltersServiceModel
{
    public IEnumerable<int> CategoryIds { get; set; } = [];

    public IEnumerable<ContestStatus> Statuses { get; set; } = [];

    public IEnumerable<int> SubmissionTypeIds { get; set; } = [];

    public int? PageNumber { get; set; }

    public int? ItemsPerPage { get; set; }

    public ContestSortType? SortType { get; set; }

    public ContestSortTypeDirection SortTypeDirection { get; set; }

    public bool IncludeHidden { get; set; }
}