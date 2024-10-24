namespace OJS.Services.Administration.Business.ContestCategories;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Data.Models;
using System.Collections.Generic;
using OJS.Services.Administration.Data;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using System.Linq;
using OJS.Data.Models.Contests;
using System.Threading.Tasks;
using OJS.Services.Administration.Models.ContestCategories;
using OJS.Services.Common.Data;

public class ContestCategoriesBusinessService : AdministrationOperationService<ContestCategory, int, ContestCategoryAdministrationModel>, IContestCategoriesBusinessService
{
    private readonly IContestCategoriesDataService categoriesDataService;
    private readonly IOrderableService<ContestCategory> contestCategoriesOrderableService;
    private readonly IContestCategoriesCacheService contestCategoriesCache;
    private readonly IDataService<LecturerInContestCategory> lecturerInContestCategoryDataService;

    public ContestCategoriesBusinessService(
        IContestCategoriesDataService categoriesDataService,
        IOrderableService<ContestCategory> contestCategoriesOrderableService,
        IContestCategoriesCacheService contestCategoriesCache,
        IDataService<LecturerInContestCategory> lecturerInContestCategoryDataService)
    {
        this.categoriesDataService = categoriesDataService;
        this.contestCategoriesCache = contestCategoriesCache;
        this.contestCategoriesOrderableService = contestCategoriesOrderableService;
        this.lecturerInContestCategoryDataService = lecturerInContestCategoryDataService;
    }

    public async Task<IEnumerable<ContestCategoriesHierarchyModel>> GetHierarchy()
    {
        var allCategories = await this.categoriesDataService
            .GetAllVisibleAndNotDeleted()
            .OrderBy(cc => cc.OrderBy)
            .MapCollection<ContestCategoriesHierarchyModel>()
            .ToListAsync();

        await allCategories.ForEachAsync(cch => cch.Children = allCategories.Where(c => c.ParentId == cch.Id));

        var mainCategories = allCategories
            .Where(c => !c.ParentId.HasValue)
            .OrderBy(c => c.OrderBy)
            .ToList();

        return mainCategories;
    }

    public async Task EditHierarchy(Dictionary<int, ContestCategoriesHierarchyEditModel> categoriesToUpdate)
    {
        var childCategoriesIds = new HashSet<int>();
        var parentCategoriesIds = new HashSet<int>();

        foreach (var category in categoriesToUpdate.Values)
        {
            childCategoriesIds.Add(category.Id);

            if (category.ParentId.HasValue)
            {
                parentCategoriesIds.Add((int)category.ParentId);
            }
        }

        if (!await this.AllExist(childCategoriesIds) || !await this.AllExist(parentCategoriesIds))
        {
            throw new BusinessServiceException("Invalid categories have been provided.");
        }

        var categories = await this.categoriesDataService
            .All(cc => childCategoriesIds.Contains(cc.Id))
            .ToListAsync();
        foreach (var category in categories)
        {
            category.ParentId = categoriesToUpdate[category.Id].ParentId;
        }

        await this.categoriesDataService.SaveChanges();
        await this.contestCategoriesCache.ClearMainContestCategoriesCache();
    }

    public async Task<IEnumerable<LecturerInContestCategoryActionsModel>> GetForLecturerInContestCategory(string userId)
        => await this.lecturerInContestCategoryDataService
            .GetQuery(licc => licc.LecturerId == userId)
            .MapCollection<LecturerInContestCategoryActionsModel>()
            .ToListAsync();

    public IQueryable<ContestCategory> GetAllVisible() => this.categoriesDataService.GetAllVisible();

    public override async Task<ContestCategoryAdministrationModel> Get(int id)
        => await this.categoriesDataService.GetByIdWithParent(id).Map<ContestCategoryAdministrationModel>();

    public override async Task<ContestCategoryAdministrationModel> Create(ContestCategoryAdministrationModel model)
    {
        var contestCategory = model.Map<ContestCategory>();

        if (model.ParentId == 0)
        {
            contestCategory.ParentId = null;
            contestCategory.Parent = null;
        }

        await this.categoriesDataService.Add(contestCategory);
        await this.categoriesDataService.SaveChanges();

        await this.ReevaluateContestCategoriesOrder(contestCategory);
        await this.contestCategoriesCache.ClearMainContestCategoriesCache();
        return model;
    }

    public override async Task<ContestCategoryAdministrationModel> Edit(ContestCategoryAdministrationModel model)
    {
        var contestCategory = await this.categoriesDataService.GetByIdQuery(model.Id).FirstOrDefaultAsync();
        contestCategory.MapFrom(model);

        if (model.ParentId is null or 0)
        {
            contestCategory!.ParentId = null;
            contestCategory!.Parent = null;
        }
        else
        {
            contestCategory!.Parent = await this.categoriesDataService.GetById(model.ParentId);
        }

        await this.contestCategoriesCache.ClearContestCategoryParentsAndChildren(contestCategory.Id);

        this.categoriesDataService.Update(contestCategory!);
        await this.categoriesDataService.SaveChanges();

        await this.ReevaluateContestCategoriesOrder(contestCategory);
        await this.contestCategoriesCache.ClearMainContestCategoriesCache();
        return model;
    }

    public override async Task Delete(int id)
    {
        var contestCategory = await this.categoriesDataService.GetByIdQuery(id).FirstOrDefaultAsync();

        if (contestCategory is null)
        {
            throw new BusinessServiceException($"Contest Category with Id:{id} not found.");
        }

        await this.categoriesDataService.LoadChildrenRecursively(contestCategory);

        await Task.WhenAll(
            this.contestCategoriesCache.ClearContestCategoryParentsAndChildren(contestCategory.Id),
            this.CascadeDeleteCategories(contestCategory));

        this.categoriesDataService.Delete(contestCategory);
        await this.contestCategoriesCache.ClearMainContestCategoriesCache();
        await this.categoriesDataService.SaveChanges();
    }

    private async Task ReevaluateContestCategoriesOrder(ContestCategory contestCategory)
    {
        IEnumerable<ContestCategory> contestCategories;

        if (contestCategory.ParentId.HasValue)
        {
            contestCategories = await this.categoriesDataService.GetContestCategoriesByParentId(contestCategory.ParentId);
        }
        else
        {
            contestCategories = await this.categoriesDataService.GetContestCategoriesWithoutParent();
        }

        await this.contestCategoriesOrderableService.ReevaluateOrder(contestCategories);
    }

    private async Task CascadeDeleteCategories(ContestCategory contestCategory)
    {
        foreach (var children in contestCategory.Children.ToList())
        {
            await this.CascadeDeleteCategories(children);
        }

        this.categoriesDataService.Delete(contestCategory);
    }

    private async Task<bool> AllExist(HashSet<int> ids)
    {
        var matchingCategories = await this.categoriesDataService
            .All(cc => ids.Contains(cc.Id));

        return matchingCategories.Count() == ids.Count;
    }
}