namespace OJS.Services.Common.Telemetry;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

/// <summary>
/// Unified tracing service for creating and managing activities across the application.
/// Provides a single, standardized way to work with distributed tracing.
/// </summary>
public interface ITracingService
{
    /// <summary>
    /// Executes an operation within a traced activity with automatic error handling.
    /// This is the RECOMMENDED way to trace operations.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="activitySource">The ActivitySource to use.</param>
    /// <param name="activityName">The name of the activity.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="tags">Optional tags to add to the activity.</param>
    /// <param name="businessContext">Optional business context.</param>
    /// <returns>The result of the operation.</returns>
    Task<T> TraceAsync<T>(
        ActivitySource activitySource,
        string activityName,
        Func<Activity?, Task<T>> operation,
        IDictionary<string, object?>? tags = null,
        BusinessContext? businessContext = null);

    /// <summary>
    /// Executes an operation within a traced activity with automatic error handling (void async).
    /// This is the RECOMMENDED way to trace void operations.
    /// </summary>
    /// <param name="activitySource">The ActivitySource to use.</param>
    /// <param name="activityName">The name of the activity.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="tags">Optional tags to add to the activity.</param>
    /// <param name="businessContext">Optional business context.</param>
    Task TraceAsync(
        ActivitySource activitySource,
        string activityName,
        Func<Activity?, Task> operation,
        IDictionary<string, object?>? tags = null,
        BusinessContext? businessContext = null);

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
    Task<T> TraceAsync<T, TMessage>(
        ActivitySource activitySource,
        string activityName,
        Func<Activity?, Task<T>> operation,
        IDictionary<string, object?>? tags = null,
        BusinessContext? businessContext = null,
        MassTransit.ConsumeContext<TMessage>? continueFromMessageHeaders = null)
        where TMessage : class;

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
    Task TraceAsync<TMessage>(
        ActivitySource activitySource,
        string activityName,
        Func<Activity?, Task> operation,
        IDictionary<string, object?>? tags = null,
        BusinessContext? businessContext = null,
        MassTransit.ConsumeContext<TMessage>? continueFromMessageHeaders = null)
        where TMessage : class;

    /// <summary>
    /// Enriches an activity with common application context.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="user">The user principal (optional).</param>
    /// <param name="correlationId">The correlation ID (optional).</param>
    void EnrichWithCommonContext(
        Activity activity,
        ClaimsPrincipal? user = null,
        string? correlationId = null);

    /// <summary>
    /// Adds user context to an activity.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="user">The user principal.</param>
    void AddUserContext(Activity activity, ClaimsPrincipal user);

    /// <summary>
    /// Adds business context tags to an activity.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="context">The business context.</param>
    void AddBusinessContext(Activity activity, BusinessContext context);

    /// <summary>
    /// Adds technical context to an activity.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="operation">The operation being performed.</param>
    /// <param name="component">The component performing the operation.</param>
    /// <param name="itemCount">Number of items being processed (optional).</param>
    /// <param name="dataSize">Size of data being processed in bytes (optional).</param>
    void AddTechnicalContext(
        Activity? activity,
        string operation,
        string? component = null,
        int? itemCount = null,
        long? dataSize = null);

    // ========================================
    // ACTIVITY LIFECYCLE METHODS
    // ========================================

    /// <summary>
    /// Marks an activity as completed successfully.
    /// </summary>
    /// <param name="activity">The activity to mark as completed.</param>
    /// <param name="result">Optional result description.</param>
    /// <param name="duration">Optional duration override.</param>
    void MarkAsCompleted(Activity activity, string? result = null, TimeSpan? duration = null);

    /// <summary>
    /// Marks an activity as failed with error information.
    /// </summary>
    /// <param name="activity">The activity to mark as failed.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="errorType">Optional error type classification.</param>
    void MarkAsFailed(Activity activity, Exception exception, string? errorType = null);

    /// <summary>
    /// Gets or generates a correlation ID for the current context.
    /// </summary>
    /// <returns>A correlation ID.</returns>
    string GetOrGenerateCorrelationId();
}

/// <summary>
/// Business context for enriching activities.
/// </summary>
public class BusinessContext
{
    public int? SubmissionId { get; set; }
    public object? ProblemId { get; set; }
    public int? ContestId { get; set; }
    public int? ParticipantId { get; set; }
    public IDictionary<string, object?>? CustomTags { get; set; }

    /// <summary>
    /// Creates a business context for submission operations.
    /// </summary>
    public static BusinessContext ForSubmission(int submissionId,object? problemId = null, int? contestId = null)
        => new() { SubmissionId = submissionId, ProblemId = problemId, ContestId = contestId };

    /// <summary>
    /// Creates a business context for contest operations.
    /// </summary>
    public static BusinessContext ForContest(int contestId, int? participantId = null)
        => new() { ContestId = contestId, ParticipantId = participantId };

    /// <summary>
    /// Creates a business context for problem operations.
    /// </summary>
    public static BusinessContext ForProblem(int problemId, int? contestId = null)
        => new() { ProblemId = problemId, ContestId = contestId };
}
