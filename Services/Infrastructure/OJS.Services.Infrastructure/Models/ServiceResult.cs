namespace OJS.Services.Infrastructure.Models
{
    using OJS.Common.Constants;
    using System.Collections.Generic;

    public static class ServiceResult
    {
        public static ServiceResult<VoidResult> EmptySuccess => Success(VoidResult.instance);

        public static ServiceResult<T> Success<T>(T data)
            => new(true, data, null, null, null, null, null);

        public static ServiceResult<T> NotFound<T>(string resourceType, string? message = null, object? context = null)
            => new(false, default, message ?? $"{resourceType} not found", ServiceConstants.ErrorCodes.NotFound, resourceType, null, context);

        public static ServiceResult<T> AccessDenied<T>(string? message = null, object? context = null)
            => new(false, default, message ?? "Access denied", ServiceConstants.ErrorCodes.Forbidden, null, null, context);

        public static ServiceResult<T> BusinessRuleViolation<T>(string message, object? context = null)
            => new(false, default, message, ServiceConstants.ErrorCodes.BusinessRuleViolation, null, null, context);

        public static ServiceResult<T> Failure<T>(string errorCode, string errorMessage, string? resourceType = null, string? propertyName = null, object? context = null, IDictionary<string, string[]>? errors = null)
            => new(false, default, errorMessage, errorCode, resourceType, propertyName, context, errors);
    }
}