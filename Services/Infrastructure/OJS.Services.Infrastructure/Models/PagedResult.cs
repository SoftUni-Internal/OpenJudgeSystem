namespace OJS.Services.Infrastructure.Models;

using System.Collections.Generic;
using System.Linq;

public class PagedResult<TItem>
{
    public int TotalItemsCount { get; set; }

    public int ItemsPerPage { get; set; }

    public int PagesCount { get; set; }

    public int PageNumber { get; set; }

    public IEnumerable<TItem> Items { get; set; } = [];
}