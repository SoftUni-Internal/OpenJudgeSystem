namespace OJS.Services.Ui.Business.Implementations;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Services.Common.Models.Cache;
using OJS.Services.Ui.Data;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Ui.Models.Contests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using ContestCategoryListViewModel = OJS.Services.Common.Models.Cache.ContestCategoryListViewModel;

public class ContestCategoriesBusinessService(IContestCategoriesDataService contestCategoriesData)
    : IContestCategoriesBusinessService
{
    public async Task<IEnumerable<ContestCategoryTreeViewModel>> GetTree()
    {
        var allCategories = await contestCategoriesData
            .GetAllVisible<ContestCategoryTreeViewModel>()
            .OrderByAsync(x => x.OrderBy)
            .ToListAsync();

        var categoriesWithChildren = FillChildren(allCategories);

        var mainCategories = await categoriesWithChildren
            .Where(c => !c.ParentId.HasValue)
            .OrderBy(c => c.OrderBy)
            .ToListAsync();

        foreach (var category in mainCategories)
        {
            await this.FillAllowedStrategyTypes(category);
        }

        return mainCategories;
    }

    public async Task<IEnumerable<ContestCategoryListViewModel>> GetAllMain()
        => await contestCategoriesData
            .GetAllVisibleMainOrdered<ContestCategoryListViewModel>()
            .ToListAsync();

    public async Task<IEnumerable<ContestCategoryTreeViewModel>> GetAllSubcategories(int categoryId)
    {
        var allCategories = await contestCategoriesData
            .GetAllVisible<ContestCategoryTreeViewModel>()
            .ToListAsync();

        var result = new List<ContestCategoryTreeViewModel>();

        var directSubcategories = allCategories
            .Where(c => c.ParentId == categoryId)
            .OrderBy(c => c.OrderBy)
            .ToList();

        directSubcategories
            .ForEach(c => GetWithChildren(c, c.Children, allCategories, result));

        return result;
    }

    public async Task<IEnumerable<ContestCategoryListViewModel>> GetAllParentCategories(int categoryId)
    {
        var categories = new List<ContestCategoryListViewModel>();
        var category = await contestCategoriesData.OneById(categoryId);

        while (category != null)
        {
            categories.Add(category.Map<ContestCategoryListViewModel>());

            category = category.Parent;
        }

        categories.Reverse();

        return categories;
    }

    public async Task<ContestCategoryServiceModel?> GetById(int categoryId)
    {
        var category = await contestCategoriesData.OneByIdTo<ContestCategoryServiceModel>(categoryId);

        if (category == null)
        {
            return null;
        }

        category.IsVisible = category.IsVisible && !await this.IsCategoryChildOfInvisibleParentRecursive(categoryId);

        return category;
    }

    private async Task<bool> IsCategoryChildOfInvisibleParentRecursive(int? categoryId)
    {
        if (categoryId == null)
        {
            return false;
        }

        var categoryWithParent = await contestCategoriesData
            .GetByIdQuery(categoryId.Value)
            .Include(c => c.Parent)
            .FirstOrDefaultAsync();

        if (categoryWithParent?.Parent != null)
        {
            if (categoryWithParent.Parent.IsVisible == false)
            {
                return true;
            }

            return await this.IsCategoryChildOfInvisibleParentRecursive(categoryWithParent.Parent.Id);
        }

        return false;
    }

    private static IEnumerable<ContestCategoryTreeViewModel> FillChildren(
        IEnumerable<ContestCategoryTreeViewModel> allCategories)
    {
        var categoriesList = allCategories.ToList();

        return categoriesList
            .Mutate(category =>
                category.Children = categoriesList.Where(x => x.ParentId == category.Id));
    }

    private static void GetWithChildren(
        ContestCategoryTreeViewModel category,
        IEnumerable<ContestCategoryTreeViewModel> children,
        ICollection<ContestCategoryTreeViewModel> allCategories,
        ICollection<ContestCategoryTreeViewModel> result)
    {
        result.Add(category);

        children
            .OrderBy(c => c.OrderBy)
            .ForEach(childNode =>
            {
                var grandChildren = allCategories
                    .Where(x => x.ParentId == childNode.Id)
                    .ToList();

                result.AddRange(grandChildren);

                GetWithChildren(childNode, grandChildren, allCategories, result);
            });
    }

    private async Task FillAllowedStrategyTypes(ContestCategoryTreeViewModel category)
    {
        foreach (var child in category.Children)
        {
            await this.FillAllowedStrategyTypes(child);
        }

        category.AllowedStrategyTypes = await contestCategoriesData.GetAllowedStrategyTypesById<AllowedContestStrategiesServiceModel>(category.Id);

        category.AllowedStrategyTypes = await category.AllowedStrategyTypes.Concat(
                category.Children.SelectMany(c => c.AllowedStrategyTypes))
                    .DistinctBy(x => x.Id)
                    .ToListAsync();
    }
}