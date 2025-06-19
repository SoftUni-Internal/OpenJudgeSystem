namespace OJS.Servers.Infrastructure.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OJS.Services.Common.Models;
using OJS.Services.Infrastructure.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static OJS.Common.Constants.ServiceConstants;

public static class ControllerResultExtensions
{
    public static IActionResult ToActionResult<T>(this ServiceResult<T> result, ILogger logger)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Data);
        }

        var activity = Activity.Current;
        activity?.SetStatus(ActivityStatusCode.Error, result.ErrorMessage);
        activity?.SetTag("service_result.instance_id", result.InstanceId);
        activity?.SetTag("service_result.error_code", result.ErrorCode);
        activity?.SetTag("service_result.error_context", result.ErrorContext);

        var traceId = activity?.TraceId.ToString();

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            [nameof(result.ErrorCode)] = result.ErrorCode ?? "Unknown",
            [nameof(result.ErrorContext)] = result.ErrorContext ?? "null",
            ["TraceId"] = traceId ?? "null",
        }))
        {
            return result.ErrorCode switch
            {
                ErrorCodes.AccessDenied => CreateResponse("Access denied", StatusCodes.Status403Forbidden, () =>
                    logger.LogServiceResultAccessDenied(result.InstanceId, result.ErrorMessage)),

                ErrorCodes.NotFound => CreateResponse("Not found", StatusCodes.Status404NotFound, () =>
                    logger.LogServiceResultNotFound(result.InstanceId, result.ErrorMessage)),

                ErrorCodes.BusinessRuleViolation => CreateResponse("Business rule violation", StatusCodes.Status422UnprocessableEntity, () =>
                    logger.LogServiceResultBusinessRuleViolation(result.InstanceId, result.ErrorMessage)),

                _ => CreateResponse("Bad request", StatusCodes.Status400BadRequest, () =>
                    logger.LogServiceResultError(result.InstanceId, result.ErrorMessage)),
            };
        }


        IActionResult CreateResponse(string title, int statusCode, Action logAction)
        {
            var details = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = result.ErrorMessage,
                Instance = result.InstanceId,
                Extensions =
                {
                    ["errorCode"] = result.ErrorCode,
                    ["errorContext"] = result.ErrorContext,
                    ["traceId"] = traceId,
                },
            };

            logAction();

            return statusCode switch
            {
                StatusCodes.Status404NotFound => new NotFoundObjectResult(details),
                StatusCodes.Status400BadRequest => new BadRequestObjectResult(details),
                _ => new ObjectResult(details) { StatusCode = statusCode },
            };
        }
    }
}