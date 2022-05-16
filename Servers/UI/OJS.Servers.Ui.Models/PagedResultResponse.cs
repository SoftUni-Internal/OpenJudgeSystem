namespace OJS.Servers.Ui.Models;

using SoftUni.AutoMapper.Infrastructure.Models;
using SoftUni.Common.Models;
using System.Collections.Generic;

public class PagedResultResponse<TItem> : IMapFrom<PagedResult<TItem>>
{
    public int TotalItemsCount { get; set; }

    public int ItemsPerPage { get; set; }

    public int PagesCount { get; set; }

    public int PageNumber { get; set; }

    public IEnumerable<TItem>? Items { get; set; }
}