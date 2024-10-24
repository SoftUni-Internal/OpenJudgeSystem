namespace OJS.Services.Administration.Business.Implementations;

using System;
using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Services.Administration.Data;
using OJS.Services.Infrastructure.Cache;
using OJS.Services.Infrastructure.Constants;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using OJS.Data.Models.Contests;

public class ContestCategoriesCacheService : IContestCategoriesCacheService
{
    private readonly ICacheService cache;
    private readonly IContestCategoriesDataService contestCategoriesData;

    public ContestCategoriesCacheService(
        ICacheService cache,
        IContestCategoriesDataService contestCategoriesData)
    {
        this.cache = cache;
        this.contestCategoriesData = contestCategoriesData;
    }

    public async Task ClearMainContestCategoriesCache()
        => await this.RemoveCaches([
            CacheConstants.MainContestCategoriesDropDown,
            CacheConstants.ContestCategoriesTree ]);

    public async Task ClearContestCategoryParentsAndChildren(int categoryId)
    {
        var contestCategory = await this.contestCategoriesData
            .GetByIdQuery(categoryId)
            .FirstOrDefaultAsync();

        if (contestCategory == null)
        {
            return;
        }

        var allCategoryIds = new HashSet<int>();

        // Retrieve the IDs of all children
        await this.BreadthFirstSearch(
            contestCategory,
            childCategory => allCategoryIds.Add(childCategory.Id));

        // Traverse upwards and retrieve the IDs of all parents
        while (contestCategory is { ParentId: not null })
        {
            allCategoryIds.Add(contestCategory.ParentId.Value);
            contestCategory = await this.contestCategoriesData
                .GetByIdQuery(contestCategory.ParentId.Value)
                .FirstOrDefaultAsync();
        }

        // Remove cache for all collected category IDs
        await allCategoryIds.ForEachAsync(this.RemoveCacheFromCategory);
    }

    /// <summary>
    /// Performs a breadth-first traversal of contest categories starting from the specified parent category.
    /// Executes a provided action on each visited category.
    /// </summary>
    /// <param name="parent">The root category from which the traversal begins.</param>
    /// <param name="action">The action to execute on each visited category ( including the parent category ).</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// <para>Warning: The contest category's children are loaded inside
    /// the method, do not <c>.Include(cc =&gt; cc.Children)</c> for the parent category.</para>
    /// </remarks>
    private async Task BreadthFirstSearch(ContestCategory parent, Action<ContestCategory> action)
    {
        // Initialize a queue to manage the order of category traversal
        var queue = new Queue<ContestCategory>();

        // Enqueue the root category to start the traversal
        queue.Enqueue(parent);

        // Continue processing until there are no more categories in the queue
        while (queue.Count > 0)
        {
            // Dequeue the next category to visit
            var currentCategory = queue.Dequeue();

            // Asynchronously load all child categories of the current category
            await this.contestCategoriesData.LoadChildrenRecursively(currentCategory);

            // Execute the specified action on the current category
            action(currentCategory);

            // Enqueue all immediate child categories to be visited next
            foreach (var child in currentCategory.Children)
            {
                queue.Enqueue(child);
            }
        }
    }

    private async Task RemoveCacheFromCategory(int contestCategoryId)
    {
        var categoryNameCacheId = string.Format(
            CacheConstants.ContestCategoryNameFormat,
            contestCategoryId);

        var subCategoriesCacheId = string.Format(
            CacheConstants.ContestSubCategoriesFormat,
            contestCategoryId);

        var parentCategoriesCacheId = string.Format(
            CacheConstants.ContestParentCategoriesFormat,
            contestCategoryId);

        await this.RemoveCaches([
            categoryNameCacheId,
            subCategoriesCacheId,
            parentCategoriesCacheId,
        ]);
    }

    private async Task RemoveCaches(IEnumerable<string> keys)
        => await keys.ForEachAsync(this.cache.Remove);
}