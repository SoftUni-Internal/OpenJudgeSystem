namespace OJS.Common.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize) =>    
            source
                .Select((x, i) => new
                {
                    Index = i,
                    Value = x
                })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value));

        public static IEnumerable<IEnumerable<T>> CrossProduct<T>(
            this IEnumerable<IEnumerable<T>> source) =>
            source.Aggregate(
                (IEnumerable<IEnumerable<T>>) new[] { Enumerable.Empty<T>() },
                (acc, src) => src.SelectMany(x => acc.Select(a => a.Concat(new[] { x }))));
        
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

        public static ISet<T> ToSet<T>(this IEnumerable<T> enumerable)
            => new HashSet<T>(enumerable);
    }
}