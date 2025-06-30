namespace OJS.Common.Extensions;

using System.Collections.Generic;
using System.Linq;

public static class EnumerableExtensions
{
    public static IEnumerable<IEnumerable<T>> InBatches<T>(this IEnumerable<T> queryable, int size)
    {
        var current = queryable.AsQueryable();
        while (current.Any())
        {
            var batch = current.Take(size);
            yield return batch;
            current = current.Skip(size);
        }
    }
}