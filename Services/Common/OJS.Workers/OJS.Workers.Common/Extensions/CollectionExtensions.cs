namespace OJS.Workers.Common.Extensions;

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
    {
        foreach (T obj in source)
        {
            destination.Add(obj);
        }
    }
}