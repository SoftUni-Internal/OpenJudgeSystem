namespace OJS.Services.Common.Models.Pagination;

using System;
using OJS.Services.Common.Data.Pagination.Enums;
using System.Reflection;

public class FilteringModel(
    PropertyInfo property,
    OperatorType operatorType,
    string value,
    Type? sourceType,
    string[] propertyPath)
{
    public FilteringModel(PropertyInfo property, OperatorType operatorType, string value)
        : this(property, operatorType, value, null, [])
    {
    }

    public PropertyInfo Property { get; set; } = property;

    public OperatorType OperatorType { get; set; } = operatorType;

    public string Value { get; set; } = value;

    public Type? SourceType { get; set; } = sourceType;

    public string[] PropertyPath { get; set; } = propertyPath;

    public bool IsNestedProperty => this.PropertyPath.Length > 1;
}