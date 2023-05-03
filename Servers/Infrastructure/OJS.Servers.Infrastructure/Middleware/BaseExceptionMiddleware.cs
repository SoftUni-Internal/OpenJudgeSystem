﻿namespace OJS.Servers.Infrastructure.Middleware
{
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using OJS.Servers.Infrastructure.Extensions;
    using OJS.Services.Infrastructure.Exceptions;
    using System;
    using System.Net;
    using System.Reflection;
    using static OJS.Servers.Infrastructure.ServerConstants.ExceptionHandling;

    public abstract class BaseExceptionMiddleware
    {
        private const string StatusCodePropertyName = "StatusCode";
        private const string ExtensionDictionaryKey = "Data";

        public RequestDelegate Get =>
            async httpContext =>
            {
                var errorFeature = httpContext.Features.Get<IExceptionHandlerFeature>();
                var exception = errorFeature
                    ?.Error ?? new Exception($"Cannot get exception from {nameof(IExceptionHandlerFeature)}");

                var instanceId = Guid.NewGuid();
                var problemDetails = new ProblemDetails { Instance = instanceId.ToString() };

                switch (exception)
                {
                    case BadHttpRequestException badHttpRequestException:
                        this.HandleBadHttpRequest(httpContext, problemDetails, badHttpRequestException);
                        break;
                    case BusinessServiceException businessException:
                        this.HandleValidationException(problemDetails, businessException);
                        break;
                    default:
                        this.HandleException(httpContext, problemDetails, exception);
                        break;
                }

                httpContext.Response.StatusCode = problemDetails.Status ?? 500;
                await httpContext.Response.WriteJson(problemDetails);
            };

        protected virtual void HandleBadHttpRequest(
            HttpContext httpContext,
            ProblemDetails problemDetails,
            BadHttpRequestException exception)
        {
            var status = (int?)typeof(BadHttpRequestException)
                .GetProperty(
                    StatusCodePropertyName,
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(exception);

            problemDetails.Title = BadHttpRequestExceptionTitle;
            problemDetails.Status = status;
        }

        protected virtual void HandleValidationException(
            ProblemDetails problemDetails,
            BusinessServiceException exception)
        {
            problemDetails.Title = ValidationExceptionTitle;
            problemDetails.Detail = exception.Message;
            problemDetails.Status = 422;
            problemDetails.Extensions[ExtensionDictionaryKey] = exception.ParameterName;
        }

        protected virtual void HandleException(
            HttpContext httpContext,
            ProblemDetails problemDetails,
            Exception exception)
        {
            problemDetails.Title = UnhandledExceptionTitle;
            problemDetails.Status = (int)HttpStatusCode.InternalServerError;
        }
    }
}