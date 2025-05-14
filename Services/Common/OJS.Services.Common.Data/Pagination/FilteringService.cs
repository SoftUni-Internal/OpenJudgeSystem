namespace OJS.Services.Common.Data.Pagination;

using OJS.Services.Common.Data.Pagination.Enums;
using System;
using System.Linq;
using System.Linq.Expressions;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Common.Models.Pagination;
using System.Collections.Generic;
using System.Reflection;

public class FilteringService : IFilteringService
{
    private static readonly string NullValue = "null";

    public virtual IQueryable<TModel> ApplyFiltering<TEntity, TModel>(IQueryable<TEntity> query, List<FilteringModel> filters)
    {
        if (filters.Count == 0)
        {
            return query.MapCollection<TModel>();
        }

        var mappedQuery = query.MapCollection<TModel>();

        foreach (var filter in filters)
        {
            var expression = BuildFilteringExpression<TModel>(filter);
            mappedQuery = mappedQuery.Where(expression);
        }

        return mappedQuery;
    }

    public IEnumerable<FilteringModel> MapFilterStringToCollection<T>(PaginationRequestModel paginationRequestModel)
    {
        var filteringCollection = new List<FilteringModel>();
        if (string.IsNullOrEmpty(paginationRequestModel.Filter))
        {
            return filteringCollection;
        }

        var conditions = paginationRequestModel.Filter!.Split("&&;", StringSplitOptions.RemoveEmptyEntries);

        foreach (var condition in conditions)
        {
            var filterParts = condition.Split('~', StringSplitOptions.RemoveEmptyEntries);
            if (filterParts.Length != 3)
            {
                throw new ArgumentOutOfRangeException($"Filter {condition} must contain key, operator and value");
            }

            var key = filterParts[0];
            var operatorTypeAsString = filterParts[1];
            var value = filterParts[2];

            var isParsed = Enum.TryParse(operatorTypeAsString, true, out OperatorType operatorType);

            if (!isParsed)
            {
                throw new ArgumentException($"Operator with type {operatorTypeAsString} is not supported.");
            }

            var (finalProperty, sourceType, propertyPath) = PropertyInfoCache.GetPropertyDetails<T>(key);

            filteringCollection.Add(new FilteringModel(finalProperty, operatorType, value, sourceType, propertyPath));
        }

        return filteringCollection;
    }

    private static Expression<Func<T, bool>> BuildFilteringExpression<T>(FilteringModel filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        Expression propertyAccess;

        if (filter.IsNestedProperty)
        {
            // Build nested property access
            propertyAccess = parameter;
            foreach (var prop in filter.PropertyPath)
            {
                var propInfo = propertyAccess.Type.GetProperty(prop, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
                if (propInfo == null)
                {
                    throw new InvalidOperationException($"Property {prop} not found in type {propertyAccess.Type.Name}");
                }

                propertyAccess = Expression.Property(propertyAccess, propInfo);
            }
        }
        else
        {
            // Single property access
            propertyAccess = Expression.Property(parameter, filter.Property);
        }

        // Cast to MemberExpression for compatibility with existing methods
        if (propertyAccess is not MemberExpression memberExpression)
        {
            throw new InvalidOperationException("Property access could not be converted to MemberExpression");
        }

        // Build the appropriate expression based on property type
        Expression? expression = null;
        var propertyType = filter.Property.PropertyType;

        if (propertyType.IsEnum)
        {
            expression = BuildEnumExpression(filter.Value, propertyType, memberExpression);
        }
        else if (propertyType == typeof(string))
        {
            expression = BuildStringExpression(filter.OperatorType, filter.Value, memberExpression);
        }
        else if (propertyType == typeof(bool))
        {
            expression = BuildBooleanExpression(filter.OperatorType, filter.Value, memberExpression);
        }
        else if (propertyType == typeof(int) ||
                 Nullable.GetUnderlyingType(propertyType) == typeof(int) ||
                 propertyType == typeof(double) ||
                 Nullable.GetUnderlyingType(propertyType) == typeof(double))
        {
            expression = BuildNumberExpression(filter.OperatorType, filter.Value, memberExpression, propertyType);
        }
        else if (propertyType == typeof(DateTime) ||
                 Nullable.GetUnderlyingType(propertyType) == typeof(DateTime))
        {
            expression = BuildDateTimeExpression<DateTime>(
                filter.OperatorType,
                filter.Value,
                memberExpression,
                DateTime.TryParse,
                nameof(DateTime));
        }
        else if (propertyType == typeof(DateTimeOffset) ||
                 Nullable.GetUnderlyingType(propertyType) == typeof(DateTimeOffset))
        {
            expression = BuildDateTimeExpression<DateTimeOffset>(
                filter.OperatorType,
                filter.Value,
                memberExpression,
                DateTimeOffset.TryParse,
                nameof(DateTimeOffset));
        }

        if (expression == null)
        {
            throw new InvalidOperationException("Expression cannot be built");
        }

        return Expression.Lambda<Func<T, bool>>(expression, parameter);
    }

    private static Expression BuildEnumExpression(string filterValue, Type propertyType, MemberExpression property)
    {
        if (!Enum.TryParse(propertyType, filterValue, ignoreCase: true, out object? enumValue))
        {
            throw new ArgumentException($"Invalid enum value '{filterValue}' for enum type {propertyType.Name}");
        }

        var constant = Expression.Constant(enumValue, propertyType);
        return Expression.Equal(property, constant);
    }

    private static Expression? BuildNumberExpression(OperatorType operatorType, string? value, MemberExpression property, Type? propertyType)
    {
        Expression? expression;

        if (value == null || value.Equals(NullValue, StringComparison.OrdinalIgnoreCase))
        {
            if (!IsNullableType(property.Type))
            {
                throw new ArgumentException($"Cannot assign null to a non-nullable integer property: {property.Member.Name}");
            }

            expression = GetNullableTypesOperation(property, operatorType);
        }
        else if (int.TryParse(value, out var intValue) && (propertyType == typeof(int) || propertyType == typeof(int?)))
        {
            var constant = Expression.Constant(intValue, IsNullableType(property.Type) ? typeof(int?) : typeof(int));
            expression = GetNumberOperation(property, constant, operatorType);
        }
        else if (double.TryParse(value, out var doubleValue) && propertyType == typeof(double))
        {
            var constant = Expression.Constant(doubleValue, IsNullableType(property.Type) ? typeof(double?) : typeof(double));
            expression = GetNumberOperation(property, constant, operatorType);
        }
        else
        {
            throw new ArgumentException($"Invalid value for integer property: {value}");
        }

        return expression;
    }

    private delegate bool TryParseDelegate<T>(string input, out T result);

    private static Expression BuildDateTimeExpression<T>(
        OperatorType operatorType,
        string? value,
        MemberExpression property,
        TryParseDelegate<T> tryParse,
        string typeName)
        where T : struct
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals(NullValue, StringComparison.OrdinalIgnoreCase))
        {
            return !IsNullableType(property.Type)
                ? throw new ArgumentException($"Cannot assign null to a non-nullable {typeName} property: {property.Member.Name}")
                : GetNullableTypesOperation(property, operatorType);
        }

        if (!tryParse(value, out var parsedValue))
        {
            throw new ArgumentException($"Invalid value for {typeName} property: {value}");
        }

        var targetType = IsNullableType(property.Type) ? typeof(T?) : typeof(T);
        var constant = Expression.Constant(parsedValue, targetType);

        return operatorType switch
        {
            OperatorType.Equals => Expression.Equal(property, constant),
            OperatorType.GreaterThan => Expression.GreaterThan(property, constant),
            OperatorType.LessThan => Expression.LessThan(property, constant),
            OperatorType.LessThanOrEqual => Expression.LessThanOrEqual(property, constant),
            OperatorType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(property, constant),
            _ => throw new ArgumentOutOfRangeException(
                nameof(operatorType),
                $"Property of type {typeName} cannot have {operatorType} operator"),
        };
    }

    private static Expression? BuildBooleanExpression(OperatorType operatorType, string value, MemberExpression property)
    {
        Expression? expression;
        if (!bool.TryParse(value, out var boolValue))
        {
            throw new ArgumentException($"Invalid value for boolean property: {value}");
        }

        var constant = Expression.Constant(boolValue);
        switch (operatorType)
        {
            case OperatorType.Equals:
                expression = Expression.Equal(property, constant);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    $"Property of type bool cannot have {operatorType} operator");
        }

        return expression;
    }

    private static Expression? BuildStringExpression(OperatorType operatorType, string value, MemberExpression property)
    {
        Expression? expression;
        var constant = Expression.Constant(value, typeof(string));
        switch (operatorType)
        {
            case OperatorType.Equals:
                expression = Expression.Equal(property, constant);
                break;
            case OperatorType.Contains:
                expression = Expression.Call(
                    property,
                    typeof(string).GetMethod("Contains", new[] { typeof(string) })!,
                    constant);
                break;
            case OperatorType.StartsWith:
                expression = Expression.Call(
                    property,
                    typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!,
                    constant);
                break;
            case OperatorType.EndsWith:
                expression = Expression.Call(
                    property,
                    typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!,
                    constant);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    $"Property of type string: cannot have {operatorType} operator");
        }

        return expression;
    }

    private static Expression GetNumberOperation(MemberExpression property,  ConstantExpression constant,  OperatorType operatorType)
    {
        switch (operatorType)
        {
            case OperatorType.Equals:
              return Expression.Equal(property, constant);
            case OperatorType.GreaterThan:
              return Expression.GreaterThan(property, constant);
            case OperatorType.LessThan:
              return Expression.LessThan(property, constant);
            case OperatorType.LessThanOrEqual:
              return Expression.LessThanOrEqual(property, constant);
            case OperatorType.GreaterThanOrEqual:
              return Expression.GreaterThanOrEqual(property, constant);
            case OperatorType.NotEquals:
              return Expression.NotEqual(property, constant);
            default:
              throw new ArgumentOutOfRangeException(
                    $"Property of type int cannot have {operatorType} operator");
        }
    }

    private static Expression GetNullableTypesOperation(MemberExpression property,  OperatorType operatorType)
    {
        switch (operatorType)
            {
                case OperatorType.Equals:
                    return Expression.Equal(property, Expression.Constant(null, property.Type));
                case OperatorType.NotEquals:
                    return Expression.NotEqual(property, Expression.Constant(null, property.Type));
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Property of type int? cannot have {operatorType} operator");
            }
    }

    private static bool IsNullableType(Type type)
        => Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
}