namespace OJS.Services.Common.Telemetry;

using Microsoft.AspNetCore.Http;
using OJS.Servers.Infrastructure.Telemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

/// <summary>
/// Unified tracing service implementation for creating and managing activities.
/// Provides a single, standardized way to work with distributed tracing.
/// </summary>
public class TracingService(IHttpContextAccessor httpContextAccessor) : ITracingService
{
    public async Task<T> TraceAsync<T>(
        ActivitySource activitySource,
        string activityName,
        Func<Activity?, Task<T>> operation,
        IDictionary<string, object?>? tags = null,
        BusinessContext? businessContext = null)
    {
        // ActivitySource automatically creates root or child activity based on Activity.Current
        using var activity = activitySource.StartActivity(activityName);

        if (activity != null)
        {
            // Add tags
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }
            }

            // Check if this is a root activity (no parent)
            var isRootActivity = activity.Parent == null;
            if (isRootActivity)
            {
                // Root activity: apply full context enrichment
                this.EnrichWithCommonContext(activity);
                if (businessContext != null)
                {
                    this.AddBusinessContext(activity, businessContext);
                }
            }
            else
            {
                // Child activity: inherit key tags from parent
                InheritParentTags(activity);
            }
        }

        try
        {
            var result = await operation(activity);
            this.MarkAsCompleted(activity);
            return result;
        }
        catch (Exception ex)
        {
            this.MarkAsFailed(activity, ex);
            throw;
        }
    }

    /// <summary>
    /// Unified tracing method that can handle both regular activities and distributed tracing from message headers.
    /// This is the RECOMMENDED way to trace operations in message consumers and regular code.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <typeparam name="TMessage">The message type (only used when continueFromMessageHeaders is provided).</typeparam>
    /// <param name="activitySource">The ActivitySource to use.</param>
    /// <param name="activityName">The name of the activity.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="tags">Optional tags to add to the activity.</param>
    /// <param name="businessContext">Optional business context.</param>
    /// <param name="continueFromMessageHeaders">Optional MassTransit consume context to continue distributed trace from message headers.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<T> TraceAsync<T, TMessage>(
        ActivitySource activitySource,
        string activityName,
        Func<Activity?, Task<T>> operation,
        IDictionary<string, object?>? tags = null,
        BusinessContext? businessContext = null,
        MassTransit.ConsumeContext<TMessage>? continueFromMessageHeaders = null)
        where TMessage : class
    {
        // Create activity - either from message headers or as regular activity
        using var activity = continueFromMessageHeaders != null
            ? SimpleTracePropagation.CreateActivityFromHeaders(continueFromMessageHeaders, activitySource, activityName)
            : activitySource.StartActivity(activityName);

        if (activity != null)
        {
            // Add tags
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }
            }

            // Check if this is a root activity (no parent) or if we're continuing from message headers
            var isRootActivity = activity.Parent == null;
            var isDistributedActivity = continueFromMessageHeaders != null;

            if (isRootActivity || isDistributedActivity)
            {
                // Root activity or distributed activity: apply full context enrichment
                this.EnrichWithCommonContext(activity);
                if (businessContext != null)
                {
                    this.AddBusinessContext(activity, businessContext);
                }
            }
            else
            {
                // Child activity: inherit key tags from parent
                InheritParentTags(activity);
            }
        }

        try
        {
            var result = await operation(activity);
            this.MarkAsCompleted(activity);
            return result;
        }
        catch (Exception ex)
        {
            this.MarkAsFailed(activity, ex);
            throw;
        }
    }

    public async Task TraceAsync(
        ActivitySource activitySource,
        string activityName,
        Func<Activity?, Task> operation,
        IDictionary<string, object?>? tags = null,
        BusinessContext? businessContext = null)
    {
        // ActivitySource automatically creates root or child activity based on Activity.Current
        using var activity = activitySource.StartActivity(activityName);

        if (activity != null)
        {
            // Add tags
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }
            }

            // Check if this is a root activity (no parent)
            var isRootActivity = activity.Parent == null;
            if (isRootActivity)
            {
                // Root activity: apply full context enrichment
                this.EnrichWithCommonContext(activity);
                if (businessContext != null)
                {
                    this.AddBusinessContext(activity, businessContext);
                }
            }
            else
            {
                // Child activity: inherit key tags from parent
                InheritParentTags(activity);
            }
        }

        try
        {
            await operation(activity);
            this.MarkAsCompleted(activity);
        }
        catch (Exception ex)
        {
            this.MarkAsFailed(activity, ex);
            throw;
        }
    }

    /// <summary>
    /// Unified tracing method that can handle both regular activities and distributed tracing from message headers (void async).
    /// This is the RECOMMENDED way to trace void operations in message consumers and regular code.
    /// </summary>
    /// <typeparam name="TMessage">The message type (only used when continueFromMessageHeaders is provided).</typeparam>
    /// <param name="activitySource">The ActivitySource to use.</param>
    /// <param name="activityName">The name of the activity.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="tags">Optional tags to add to the activity.</param>
    /// <param name="businessContext">Optional business context.</param>
    /// <param name="continueFromMessageHeaders">Optional MassTransit consume context to continue distributed trace from message headers.</param>
    public async Task TraceAsync<TMessage>(
        ActivitySource activitySource,
        string activityName,
        Func<Activity?, Task> operation,
        IDictionary<string, object?>? tags = null,
        BusinessContext? businessContext = null,
        MassTransit.ConsumeContext<TMessage>? continueFromMessageHeaders = null)
        where TMessage : class
    {
        // Create activity - either from message headers or as regular activity
        using var activity = continueFromMessageHeaders != null
            ? SimpleTracePropagation.CreateActivityFromHeaders(continueFromMessageHeaders, activitySource, activityName)
            : activitySource.StartActivity(activityName);

        if (activity != null)
        {
            // Add tags
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    activity.SetTag(tag.Key, tag.Value);
                }
            }

            // Check if this is a root activity (no parent) or if we're continuing from message headers
            var isRootActivity = activity.Parent == null;
            var isDistributedActivity = continueFromMessageHeaders != null;

            if (isRootActivity || isDistributedActivity)
            {
                // Root activity or distributed activity: apply full context enrichment
                this.EnrichWithCommonContext(activity);
                if (businessContext != null)
                {
                    this.AddBusinessContext(activity, businessContext);
                }
            }
            else
            {
                // Child activity: inherit key tags from parent
                InheritParentTags(activity);
            }
        }

        try
        {
            await operation(activity);
            this.MarkAsCompleted(activity);
        }
        catch (Exception ex)
        {
            this.MarkAsFailed(activity, ex);
            throw;
        }
    }

    public void EnrichWithCommonContext(
        Activity activity,
        ClaimsPrincipal? user = null,
        string? correlationId = null)
    {
        // Add service context
        activity.SetTag(OjsActivitySources.CommonTags.ServiceName, GetServiceName());
        activity.SetTag(OjsActivitySources.CommonTags.ServiceVersion, GetServiceVersion());
        activity.SetTag(OjsActivitySources.CommonTags.Environment, GetEnvironment());

        // Add user context if available
        var userPrincipal = user ?? httpContextAccessor.HttpContext?.User;
        if (userPrincipal?.Identity?.IsAuthenticated == true)
        {
            this.AddUserContext(activity, userPrincipal);
        }

        // Add correlation ID - prefer from OpenTelemetry baggage, then parameter, then generate
        var corrId = correlationId ?? this.GetOrGenerateCorrelationId();
        activity.SetTag(OjsActivitySources.CommonTags.CorrelationId, corrId);

        // Also add to baggage for cross-service propagation
        activity.SetBaggage(OjsActivitySources.CommonTags.CorrelationId, corrId);

        // Add request context if available
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            activity.SetTag(OjsActivitySources.CommonTags.RequestId, httpContext.TraceIdentifier);

            if (httpContext.Session.IsAvailable)
            {
                activity.SetTag(OjsActivitySources.CommonTags.SessionId, httpContext.Session.Id);
            }
        }
    }

    public void AddUserContext(Activity activity, ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value ??
                      user.FindFirst("preferred_username")?.Value;
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            activity.SetTag(OjsActivitySources.CommonTags.UserId, userId);
            activity.SetBaggage(OjsActivitySources.CommonTags.UserId, userId);
        }

        if (!string.IsNullOrEmpty(userName))
        {
            activity.SetTag(OjsActivitySources.CommonTags.UserName, userName);
            activity.SetBaggage(OjsActivitySources.CommonTags.UserName, userName);
        }

        if (!string.IsNullOrEmpty(userRole))
        {
            activity.SetTag(OjsActivitySources.CommonTags.UserRole, userRole);
            activity.SetBaggage(OjsActivitySources.CommonTags.UserRole, userRole);
        }
    }

    public void AddBusinessContext(Activity activity, BusinessContext context)
    {
        if (context.SubmissionId.HasValue)
        {
            var submissionId = context.SubmissionId.Value.ToString();
            activity.SetTag(OjsActivitySources.CommonTags.SubmissionId, submissionId);
            activity.SetBaggage(OjsActivitySources.CommonTags.SubmissionId, submissionId);
        }

        if (context.ProblemId != null)
        {
            activity.SetTag(OjsActivitySources.CommonTags.ProblemId, context.ProblemId);
            activity.SetBaggage(OjsActivitySources.CommonTags.ProblemId, context.ProblemId.ToString());
        }

        if (context.ContestId.HasValue)
        {
            var contestId = context.ContestId.Value.ToString();
            activity.SetTag(OjsActivitySources.CommonTags.ContestId, contestId);
            activity.SetBaggage(OjsActivitySources.CommonTags.ContestId, contestId);
        }

        if (context.ParticipantId.HasValue)
        {
            var participantId = context.ParticipantId.Value.ToString();
            activity.SetTag(OjsActivitySources.CommonTags.ParticipantId, participantId);
            activity.SetBaggage(OjsActivitySources.CommonTags.ParticipantId, participantId);
        }

        if (context.CustomTags != null)
        {
            foreach (var tag in context.CustomTags)
            {
                activity.SetTag(tag.Key, tag.Value);
                // Only add string values to baggage to avoid serialization issues
                if (tag.Value is string stringValue)
                {
                    activity.SetBaggage(tag.Key, stringValue);
                }
            }
        }
    }

    public void AddTechnicalContext(
        Activity? activity,
        string operation,
        string? component = null,
        int? itemCount = null,
        long? dataSize = null)
    {
        if (activity == null)
        {
            return;
        }

        activity.SetTag(OjsActivitySources.CommonTags.Operation, operation);

        if (!string.IsNullOrEmpty(component))
        {
            activity.SetTag(OjsActivitySources.CommonTags.Component, component);
        }

        if (itemCount.HasValue)
        {
            activity.SetTag(OjsActivitySources.CommonTags.ItemCount, itemCount.Value);
        }

        if (dataSize.HasValue)
        {
            activity.SetTag(OjsActivitySources.CommonTags.DataSize, dataSize.Value);
        }
    }

    public void MarkAsCompleted(Activity? activity, string? result = null, TimeSpan? duration = null)
    {
        if (activity == null)
        {
            return;
        }

        activity.SetStatus(ActivityStatusCode.Ok);

        if (!string.IsNullOrEmpty(result))
        {
            activity.SetTag("result", result);
        }

        if (duration.HasValue)
        {
            activity.SetTag(OjsActivitySources.CommonTags.Duration, duration.Value.TotalMilliseconds);
        }
    }

    public void MarkAsFailed(Activity? activity, Exception exception, string? errorType = null)
    {
        if (activity == null)
        {
            return;
        }

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("error", true);
        activity.SetTag("error.message", exception.Message);
        activity.SetTag("error.type", exception.GetType().Name);

        if (!string.IsNullOrEmpty(errorType))
        {
            activity.SetTag(OjsActivitySources.CommonTags.ErrorType, errorType);
        }

        // Add exception details as an event
        activity.AddEvent(new ActivityEvent("exception", DateTimeOffset.UtcNow, new ActivityTagsCollection
        {
            ["exception.type"] = exception.GetType().FullName,
            ["exception.message"] = exception.Message,
            ["exception.stacktrace"] = exception.StackTrace,
        }));
    }

    public string GetOrGenerateCorrelationId()
    {
        // Try to get correlation ID from OpenTelemetry baggage first (cross-service propagation)
        var currentActivity = Activity.Current;
        if (currentActivity != null)
        {
            var baggageCorrelationId = currentActivity.GetBaggageItem(OjsActivitySources.CommonTags.CorrelationId);
            if (!string.IsNullOrEmpty(baggageCorrelationId))
            {
                return baggageCorrelationId;
            }

            // Fallback to activity tag
            var existingCorrelationId = currentActivity.GetTagItem(OjsActivitySources.CommonTags.CorrelationId)?.ToString();
            if (!string.IsNullOrEmpty(existingCorrelationId))
            {
                return existingCorrelationId;
            }
        }

        // Try to get from HTTP context headers (for backward compatibility)
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue) == true)
        {
            var correlationId = headerValue.ToString();
            if (!string.IsNullOrEmpty(correlationId))
            {
                return correlationId;
            }
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString("N")[..16]; // Short correlation ID
    }

    private static void InheritParentTags(Activity childActivity)
    {
        var parentActivity = childActivity.Parent;
        if (parentActivity == null)
        {
            return;
        }

        var tagsToInherit = new[]
        {
            OjsActivitySources.CommonTags.CorrelationId,
            OjsActivitySources.CommonTags.UserId,
            OjsActivitySources.CommonTags.UserName,
            OjsActivitySources.CommonTags.SubmissionId,
            OjsActivitySources.CommonTags.ProblemId,
            OjsActivitySources.CommonTags.ContestId,
        };

        foreach (var tagName in tagsToInherit)
        {
            // First try to get from baggage (cross-service propagation)
            var baggageValue = childActivity.GetBaggageItem(tagName);
            if (!string.IsNullOrEmpty(baggageValue))
            {
                childActivity.SetTag(tagName, baggageValue);
                continue;
            }

            // Fallback to parent activity tags
            var tagValue = parentActivity.GetTagItem(tagName);
            if (tagValue != null)
            {
                childActivity.SetTag(tagName, tagValue);
            }
        }
    }

    private static string GetServiceName()
    {
        var assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
        return assemblyName?.Replace("OJS.Servers.", "").Replace("OJS.Services.", "") ?? "Unknown";
    }

    private static string GetServiceVersion()
    {
        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        return assembly?.GetName().Version?.ToString() ?? "1.0.0";
    }

    private static string GetEnvironment()
        => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
}
