namespace OJS.Services.Common.Data;

using System;
using System.Collections.Concurrent;
using System.Reflection;

public static class PropertyInfoCache
{
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo> Cache = new();

    public static PropertyInfo GetPropertyInfo<T>(string propertyName)
        => GetPropertyInfo(typeof(T), propertyName);

    public static (PropertyInfo FinalProperty, Type SourceType, string[] PropertyPath) GetPropertyDetails<T>(string propertyPath)
    {
        if (!propertyPath.Contains('.'))
        {
            return (GetPropertyInfo<T>(propertyPath), typeof(T), [propertyPath]);
        }

        var pathParts = propertyPath.Split('.');

        if (pathParts.Length <= 0)
        {
            throw new ArgumentException($"Invalid property path: {propertyPath}");
        }

        var currentType = typeof(T);
        PropertyInfo? propertyInfo = null;

        foreach (var pathPart in pathParts)
        {
            propertyInfo = GetPropertyInfo(currentType, pathPart);
            currentType = propertyInfo!.PropertyType;
        }

        return (propertyInfo!, typeof(T), pathParts);
    }

    public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        => Cache.GetOrAdd((type, propertyName), key =>
        {
            var (targetType, propName) = key;
            var propertyInfo = targetType.GetProperty(propName, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                throw new InvalidOperationException($"Property '{propName}' not found on type '{targetType}'.");
            }

            return propertyInfo;
        });
}