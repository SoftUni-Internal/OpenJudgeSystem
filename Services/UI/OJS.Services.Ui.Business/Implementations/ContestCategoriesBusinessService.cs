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

        return mainCategories;
    }

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

    public async Task<ContestCategoryServiceModel?> GetById(int categoryId)
    {
        var category = await contestCategoriesData.OneByIdTo<ContestCategoryServiceModel>(categoryId);

        if (category == null)
        {
            return null;
        }

        category.IsVisible = await this.IsCategoryVisible(category);

        return category;
    }

    private async Task<bool> IsCategoryVisible(ContestCategoryServiceModel category)
    {
        if (!category.IsVisible)
        {
            return false;
        }

        // Category is visible, check if all its parents are visible.
        var parentId = category.ParentId;
        while (parentId != null)
        {
            var parent = await contestCategoriesData
                .GetByIdQuery(parentId)
                .AsNoTracking()
                .MapCollection<ContestCategoryServiceModel>()
                .FirstOrDefaultAsync();

            if (parent is { IsVisible: false })
            {
                return false;
            }

            parentId = parent?.ParentId;
        }

        return true;
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
}