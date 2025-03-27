namespace OJS.Services.Common.Data.Pagination;

using OJS.Services.Common.Models.Pagination;
using System.Collections.Generic;
using System.Linq;
using OJS.Services.Infrastructure;

public interface IFilteringService : IService
{
    IQueryable<TModel> ApplyFiltering<TEntity, TModel>(IQueryable<TEntity> query, List<FilteringModel> filters);

    IEnumerable<FilteringModel> MapFilterStringToCollection<TModel>(PaginationRequestModel paginationRequestModel);
}
