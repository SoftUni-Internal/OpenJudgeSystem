namespace OJS.Services.Infrastructure.Extensions;

using Microsoft.EntityFrameworkCore;
using OJS.Services.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public static class QueryableExtensions
{
    public static async Task<IEnumerable<T>> ToEnumerableAsync<T>(this IQueryable<T> queryable)
        => await queryable
            .ToListAsync();

    public static PagedResult<T> ToPagedResult<T>(
        this IQueryable<T> queryable,
        int? itemsPerPage,
        int? pageNumber)
    {
        var page = GetPagedResult<T>(queryable.Count(), itemsPerPage, pageNumber);

        page.Items = [.. queryable.GetItemsPageQuery(itemsPerPage!.Value, pageNumber!.Value)];

        return page;
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> queryable,
        int? itemsPerPage,
        int? pageNumber)
    {
        var page = GetPagedResult<T>(await queryable.CountAsync(), itemsPerPage, pageNumber);

        page.Items = await queryable
            .GetItemsPageQuery(itemsPerPage!.Value, pageNumber!.Value)
            .ToListAsync();

        return page;
    }

    private static PagedResult<T> GetPagedResult<T>(
        int totalItemsCount,
        int? itemsPerPage,
        int? pageNumber)
    {
        if (itemsPerPage <= 0 || pageNumber <= 0)
        {
            var parameterName = itemsPerPage <= 0 ? nameof(itemsPerPage) : nameof(pageNumber);
            throw new ArgumentException("Value cannot be less than or equal to zero", parameterName);
        }

        itemsPerPage ??= totalItemsCount;
        pageNumber ??= 1;

        var pagesCount = CalculatePagesCount(totalItemsCount, itemsPerPage.Value);

        return new PagedResult<T>
        {
            TotalItemsCount = totalItemsCount,
            ItemsPerPage = itemsPerPage.Value,
            PagesCount = pagesCount,
            PageNumber = pageNumber.Value,
        };
    }

    private static int CalculatePagesCount(int totalItemsCount, int itemsPerPage)
        => totalItemsCount > itemsPerPage
            ? (int)Math.Ceiling((double)totalItemsCount / itemsPerPage)
            : 1;

    private static IQueryable<T> GetItemsPageQuery<T>(this IQueryable<T> queryable, int itemsPerPage, int pageNumber)
        => queryable
            .Skip(itemsPerPage * (pageNumber - 1))
            .Take(itemsPerPage);

    /// <summary>
    /// Extension method for splitting query into batches. NOTE: USE THIS ONLY IF THE
    /// OPERATION WILL NOT CHANGE THE SELECTED QUERY SET ITSELF. Explanation:
    /// The InBatches Extension will essentially modify the collection while iterating over it
    /// leading to only half the entries actually being modified
    /// (essentially behaving like deleting elements from a List while iterating it). For example if we select
    /// all IsDeleted = 0 entries and modify them to IsDeleted = 1 using this extension method
    /// after executing on the first batch, a new select is ran with OFFSET equal to batch size,
    /// but it will get a modified version of the data
    /// (where the original batch is missing since it was already modified) leading to skipping OFFSET amount
    /// of entries each execution which leads to half the entries being skipped.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="size">Size of a single batch</param>
    /// <param name="limit">Limits the query to a max amount, the sub queries will execute (limit / size) number of times,
    /// regardless of amount of entries returned. Consumer should decide whether to cancel early, based on number of elements returned.</param>
    /// <returns></returns>
    public static IEnumerable<IQueryable<T>> InBatches<T>(this IOrderedQueryable<T> queryable, int size, int limit = 0)
    {
        IQueryable<T> current = queryable;

        if (limit > 0)
        {
            var currentAmount = 0;
            while (currentAmount < limit)
            {
                var batch = current.Take(size);
                currentAmount += size;
                yield return batch;
                current = current.Skip(size);
            }
        }
        else
        {
            while (current.Any())
            {
                var batch = current.Take(size);
                yield return batch;
                current = current.Skip(size);
            }
        }
    }

    /// <summary>
    /// Extension to split query into batches, if the query you use will modify the elements such that they
    /// no longer match the selection criteria of the original query, use this extension method instead.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="queryable"></param>
    /// <param name="size">Size of a single batch</param>
    /// <param name="limit">Limits the query to a max amount, the sub queries will execute (limit / size) number of times,
    /// regardless of amount of entries returned. Consumer should decide whether to cancel early, based on number of elements returned.</param>
    /// <returns></returns>
    public static IEnumerable<IQueryable<T>> InSelfModifyingBatches<T>(this IOrderedQueryable<T> queryable, int size, int limit = 0)
    {
        IQueryable<T> current = queryable;

        if (limit > 0)
        {
            var currentAmount = 0;
            while (currentAmount < limit)
            {
                var batch = current.Take(size);
                currentAmount += size;
                yield return batch;
            }
        }
        else
        {
            while (current.Any())
            {
                var batch = current.Take(size);
                yield return batch;
            }
        }
    }
}
