namespace OJS.Common.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class QueryableExtensions
    {
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
        public static IEnumerable<IQueryable<T>> InBatches<T>(this IQueryable<T> queryable, int size, int limit = 0)
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
        public static IEnumerable<IQueryable<T>> InSelfModifyingBatches<T>(this IQueryable<T> queryable, int size, int limit = 0)
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
}