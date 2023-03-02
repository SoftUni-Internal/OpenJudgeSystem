namespace OJS.Common.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class QueryableExtensions
    {
        public static IEnumerable<IQueryable<T>> InBatches<T>(this IOrderedQueryable<T> queryable, int size)
        {
            IQueryable<T> current = queryable;
            while (current.Any())
            {
                var batch = current.Take(size);
                yield return batch;
                current = current.Skip(size);
            }
        }
    }
}