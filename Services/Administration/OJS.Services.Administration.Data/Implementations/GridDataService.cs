namespace OJS.Services.Administration.Data.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Services.Administration.Data.Excel;
using OJS.Services.Common.Data;
using OJS.Services.Common.Data.Pagination;
using OJS.Services.Common.Data.Pagination.Enums;
using OJS.Services.Common.Models.Files;
using OJS.Services.Common.Models.Pagination;
using OJS.Services.Common.Models.Users;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Infrastructure.Models;
using OJS.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class GridDataService<TEntity>
    : IGridDataService<TEntity>
    where TEntity : class, IEntity
{
    private readonly IDataService<TEntity> dataService;
    private readonly ISortingService sortingService;
    private readonly IFilteringService filteringService;
    private readonly IExcelService excelService;

    public GridDataService(
        IDataService<TEntity> dataService,
        ISortingService sortingService,
        IFilteringService filteringService,
        IExcelService excelService)
    {
        this.dataService = dataService;
        this.sortingService = sortingService;
        this.filteringService = filteringService;
        this.excelService = excelService;
    }

    // TODO: Mark entities with attributes that are not allowed for lecturers
    // and use reflection to filter out the grids for the current user.
    public Task<bool> UserHasAccessToGrid(UserInfoModel user) => Task.FromResult(true);

    public virtual Task<PagedResult<TModel>> GetAll<TModel>(
        PaginationRequestModel paginationRequestModel,
        Expression<Func<TEntity, bool>>? filter = null)
        => this.GetPagedResultFromQuery<TModel>(paginationRequestModel, this.dataService.GetQuery(filter));

    public virtual Task<PagedResult<TModel>> GetAll<TModel>(
        PaginationRequestModel paginationRequestModel,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, bool>>? filter = null,
        bool descending = false)
        => this.GetPagedResultFromQuery<TModel>(paginationRequestModel, this.dataService.GetQuery(filter, orderBy, descending));

    public virtual Task<PagedResult<TModel>> GetAllForUser<TModel>(
        PaginationRequestModel paginationRequestModel,
        UserInfoModel user,
        Expression<Func<TEntity, bool>>? filter = null)
        => this.GetPagedResultFromQuery<TModel>(paginationRequestModel, this.dataService.GetQueryForUser(user, filter));

    public async Task<FileResponseModel> GetExcelResults<TModel>(
        PaginationRequestModel paginationRequestModel,
        Expression<Func<TEntity, bool>>? filter = null)
    {
        var results =
            await this.GetNonPagedResultFromQuery<TModel>(paginationRequestModel, this.dataService.GetQuery(filter));

        return this.excelService.ExportResults<TModel?>(new Dictionary<string, IEnumerable<TModel?>> { ["Results"] = results });
    }

    public async Task<FileResponseModel> GetExcelResults<TModel>(
        PaginationRequestModel paginationRequestModel,
        Expression<Func<TEntity, object>> orderBy,
        Expression<Func<TEntity, bool>>? filter = null,
        bool descendingOrder = false)
    {
        var results =
            await this.GetNonPagedResultFromQuery<TModel>(paginationRequestModel, this.dataService.GetQuery(filter, orderBy, descendingOrder));

        return this.excelService.ExportResults<TModel?>(new Dictionary<string, IEnumerable<TModel?>> { ["Results"] = results });
    }

    public async Task<FileResponseModel> GetExcelResultsForUser<TModel>(
        PaginationRequestModel paginationRequestModel,
        UserInfoModel user,
        Expression<Func<TEntity, bool>>? filter = null)
    {
        var results =
            await this.GetNonPagedResultFromQuery<TModel>(paginationRequestModel, this.dataService.GetQueryForUser(user, filter));

        return this.excelService.ExportResults<TModel?>(new Dictionary<string, IEnumerable<TModel?>> { ["Results"] = results });
    }

    private async Task<PagedResult<TModel>> GetPagedResultFromQuery<TModel>(PaginationRequestModel paginationRequestModel, IQueryable<TEntity> query)
        => await this.ApplyFiltersAndSorters<TModel>(paginationRequestModel, query)
            .ToPagedResultAsync(paginationRequestModel.ItemsPerPage, paginationRequestModel.Page);

    private async Task<ICollection<TModel>> GetNonPagedResultFromQuery<TModel>(
        PaginationRequestModel paginationRequestModel, IQueryable<TEntity> query) =>
        await this.ApplyFiltersAndSorters<TModel>(paginationRequestModel, query)
            .ToListAsync();

    private IQueryable<TModel> ApplyFiltersAndSorters<TModel>(PaginationRequestModel paginationRequestModel, IQueryable<TEntity> query)
    {
        var filterAsCollection = this.filteringService.MapFilterStringToCollection<TModel>(paginationRequestModel).ToList();

        var mappedQuery = this.filteringService.ApplyFiltering<TEntity, TModel>(query.AsNoTracking(), filterAsCollection);

        return this.sortingService
            .ApplySorting(mappedQuery, paginationRequestModel.Sorting);
    }
}