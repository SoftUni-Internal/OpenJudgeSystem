namespace OJS.Services.Infrastructure.Models
{
    using OJS.Services.Infrastructure.Models.Mapping;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class ServiceResult<T> : IMapFrom<ServiceResult<T>>
    {
        public T? Data { get; private set; }

        public bool IsSuccess { get; private set; }

        public string? ErrorMessage { get; private set; }

        public string? ErrorCode { get; private set; }

        public string? ResourceType { get; private set; }

        public string? PropertyName { get; private set; }

        public object? ErrorContext { get; private set; }

        public string? InstanceId { get; private set; }

        public IDictionary<string, string[]>? Errors { get; private set; }

        internal ServiceResult(bool isSuccess, T? data, string? errorMessage, string? errorCode, string? resourceType, string? propertyName, object? errorContext, IDictionary<string, string[]>? errors = null)
        {
            this.IsSuccess = isSuccess;
            this.Data = data;
            this.ErrorMessage = errorMessage;
            this.ErrorCode = errorCode;
            this.ResourceType = resourceType;
            this.PropertyName = propertyName;
            this.ErrorContext = errorContext;
            this.Errors = errors;

            if (!isSuccess)
            {
                this.InstanceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();
            }
        }
    }
}