namespace OJS.Services.Infrastructure.Models;

using OJS.Common.Constants;
using System.Collections.Generic;

public class ValidationResult
{
    protected ValidationResult()
    {
    }

    public bool IsValid { get; private set; }

    public virtual string Message { get; set; } = string.Empty;

    public string? ResourceType { get; set; }

    public string? PropertyName { get; set; }

    public string? ErrorCode { get; set; }

    public object? ErrorContext { get; set; }

    public IDictionary<string, string[]>? Errors { get; set; } = new Dictionary<string, string[]>();

    public static ValidationResult Valid()
        => new()
        {
            IsValid = true,
            Errors = null,
        };

    public static ValidationResult Invalid(string message)
        => new()
        {
            IsValid = false,
            Message = message,
        };

    public static ValidationResult Invalid(string message, string propertyName)
        => new()
        {
            IsValid = false,
            Message = message,
            PropertyName = propertyName,
        };

    public static ValidationResult Invalid(string message, string errorCode, object errorContext)
        => new()
        {
            IsValid = false,
            Message = message,
            ErrorCode = errorCode,
            ErrorContext = errorContext,
        };

    public static ValidationResult NotFound(string? message = null, string? resourceType = null, object? context = null)
        => new()
        {
            IsValid = false,
            Message = message ?? $"{resourceType} not found",
            ErrorCode = ServiceConstants.ErrorCodes.NotFound,
            ErrorContext = context,
        };

    public static ValidationResult AccessDenied(string? message = null, object? context = null)
        => new()
        {
            IsValid = false,
            Message = message ?? "Access denied",
            ErrorCode = ServiceConstants.ErrorCodes.AccessDenied,
            ErrorContext = context,
        };
}