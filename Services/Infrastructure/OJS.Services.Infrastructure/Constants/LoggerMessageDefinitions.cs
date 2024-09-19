namespace OJS.Services.Infrastructure.Constants;

using Microsoft.Extensions.Logging;
using OJS.Workers.Common.Models;
using System;

/// <summary>
/// Define the logger messages used in the application for high performance logging (https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging).
/// The SkipEnabledCheck = true attribute is used to skip the check if the logger is enabled for the specified log level.
/// Usually Error level is always logged, so the check is not necessary for these messages.
/// EventIds are used to identify the message in the log. It has no specific meaning, but it should be unique for each message.
/// </summary>
public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(1, LogLevel.Error, "An error with code: {ErrorCode} and ID: {InstanceId} occurred.", SkipEnabledCheck = true)]
    public static partial void LogErrorWithCodeAndId(this ILogger logger, string? errorCode, string instanceId, Exception ex);

    [LoggerMessage(2, LogLevel.Error, "Failed to get host entry for host name: {HostName}", SkipEnabledCheck = true)]
    public static partial void LogFailedToGetHostEntryForHostName(this ILogger logger, string hostName, Exception ex);

    // Hosted services
    [LoggerMessage(20, LogLevel.Error, "Exception in {HostedServiceName}", SkipEnabledCheck = true)]
    public static partial void LogHostedServiceException(this ILogger logger, string hostedServiceName, Exception ex);

    [LoggerMessage(22, LogLevel.Error, "Message bus health check failed. Current status: {BusHealthStatus}. Please verify that the message bus server is running correctly.")]
    public static partial void LogMessageBusHealthCheckFailed(this ILogger logger, string? busHealthStatus);

    [LoggerMessage(51, LogLevel.Information, "Stopping {HostedServiceName}")]
    public static partial void LogStoppingHostedService(this ILogger logger, string hostedServiceName);

    [LoggerMessage(52, LogLevel.Information, "Background job for {JobDescription} is added or updated")]
    public static partial void LogBackgroundJobAddedOrUpdated(this ILogger logger, string jobDescription);

    // Resilience pipelines
    [LoggerMessage(100, LogLevel.Error, "Circuit breaker {CircuitBreakerState}. Total number of times {CircuitBreakerState}: {TimesChanged}. Event: {ResilienceEvent}. Outcome: [{ResilienceOutcome}]. Pipeline: {ResiliencePipeline}. Strategy: {ResilienceStrategy}.")]
    public static partial void LogCircuitBreakerStateChanged(this ILogger logger, string circuitBreakerState, int timesChanged, string resilienceEvent, string resilienceOutcome, string? resiliencePipeline, string? resilienceStrategy);

    [LoggerMessage(150, LogLevel.Information, "Circuit breaker's pipeline is being executed. Operation: {OperationKey}. Event: {ResilienceEvent}. Pipeline: {ResiliencePipeline}.")]
    public static partial void LogCircuitBreakerPipelineExecuting(this ILogger logger, string operationKey, string resilienceEvent, string? resiliencePipeline);

    [LoggerMessage(151, LogLevel.Information, "Circuit breaker's pipeline has been executed. Operation: {OperationKey}. Event: {ResilienceEvent}. Outcome: [{ResilienceOutcome}]. Pipeline: {ResiliencePipeline}.")]
    public static partial void LogCircuitBreakerPipelineExecuted(this ILogger logger, string operationKey, string resilienceEvent, string resilienceOutcome, string? resiliencePipeline);

    [LoggerMessage(152, LogLevel.Information, "Total number of retries: {ResilienceRetries}. Event: {ResilienceEvent}. Outcome: [{ResilienceOutcome}]. Pipeline: {ResiliencePipeline}. Strategy: {ResilienceStrategy}.")]
    public static partial void LogCircuitBreakerTotalRetries(this ILogger logger, int resilienceRetries, string resilienceEvent, string resilienceOutcome, string? resiliencePipeline, string? resilienceStrategy);

    [LoggerMessage(153, LogLevel.Warning, "Retry attempt #{RetryAttempt}. Operation: Retry_{OperationKey} Outcome: [{ResilienceOutcome}]. Duration: {RetryDuration}ms. Delay: {RetryDelay}ms.")]
    public static partial void LogCircuitBreakerRetryAttempt(this ILogger logger, int retryAttempt, string operationKey, string resilienceOutcome, int retryDuration, int retryDelay);

    // External http requests
    [LoggerMessage(201, LogLevel.Error, "Platform data for {UserName} not received. Error message: {PlatformCallErrorMessage}", SkipEnabledCheck = true)]
    public static partial void LogPlatformDataNotReceived(this ILogger logger, string userName, string? platformCallErrorMessage);

    [LoggerMessage(202, LogLevel.Error, "Error while trying to get platform data for {UserName}", SkipEnabledCheck = true)]
    public static partial void LogErrorGettingPlatformData(this ILogger logger, string userName, Exception ex);

    [LoggerMessage(203, LogLevel.Error, "Error while trying to get response from {RequestUrl}. Error message: {ErrorMessage}", SkipEnabledCheck = true)]
    public static partial void LogResponseNotSuccessfullyReceived(this ILogger logger, string requestUrl, string? errorMessage);

    [LoggerMessage(204, LogLevel.Error, "Request to {RequestUrl} failed.", SkipEnabledCheck = true)]
    public static partial void LogRequestFailed(this ILogger logger, string requestUrl, Exception ex);

    [LoggerMessage(250, LogLevel.Information, "Starting {HttpMethod} request to {RequestUrl}")]
    public static partial void LogStartingHttpRequest(this ILogger logger, string httpMethod, string requestUrl);

    [LoggerMessage(260, LogLevel.Debug, "Platform data for {UserName} successfully received.")]
    public static partial void LogPlatformDataReceived(this ILogger logger, string userName);

    // Submissions
    [LoggerMessage(1010, LogLevel.Error, "Exception in submitting submission #{SubmissionId}", SkipEnabledCheck = true)]
    public static partial void LogExceptionSubmittingSolution(this ILogger logger, int submissionId, Exception ex);

    [LoggerMessage(1020, LogLevel.Error, "Exception in submitting submissions batch.", SkipEnabledCheck = true)]
    public static partial void LogExceptionSubmittingSolutionsBatch(this ILogger logger, Exception ex);

    [LoggerMessage(1030, LogLevel.Error, "Exception returned for submission #{SubmissionId}: {@SubmissionException}", SkipEnabledCheck = true)]
    public static partial void LogExceptionReturnedForSubmission(this ILogger logger, int submissionId, ExceptionModel submissionException);

    [LoggerMessage(1040, LogLevel.Error, "Error processing execution result for submission #{SubmissionId} from worker {WorkerName}", SkipEnabledCheck = true)]
    public static partial void LogErrorProcessingExecutionResult(this ILogger logger, int submissionId, string? workerName, Exception ex);

    [LoggerMessage(1050, LogLevel.Error, "Error processing submission #{SubmissionId} on worker: {WorkerName}", SkipEnabledCheck = true)]
    public static partial void LogErrorProcessingSubmission(this ILogger logger, int submissionId, string? workerName, Exception ex);

    [LoggerMessage(1050, LogLevel.Error, "Submission for processing #{SubmissionForProcessingId} for Submission #{SubmissionId} not found in the database.", SkipEnabledCheck = true)]
    public static partial void LogSubmissionForProcessingNotFoundForSubmission(this ILogger logger, int submissionForProcessingId, int submissionId);

    [LoggerMessage(1060, LogLevel.Warning, "Submission for processing for Submission #{SubmissionId} is in state {CurrentProcessingState} state. Skipping updating it to {UpdateToProcessingState}.")]
    public static partial void LogSubmissionProcessingStateNotUpdated(this ILogger logger, int submissionId, string currentProcessingState, string updateToProcessingState);

    [LoggerMessage(1100, LogLevel.Information, "Result for submission #{SubmissionId} processed successfully with SubmissionForProcessing: {@SubmissionForProcessing}")]
    public static partial void LogSubmissionProcessedSuccessfully(this ILogger logger, int submissionId, object submissionForProcessing);

    [LoggerMessage(1160, LogLevel.Information, "Received retest submission #{SubmissionId}")]
    public static partial void LogReceivedRetestSubmission(this ILogger logger, int submissionId);

    [LoggerMessage(1170, LogLevel.Information, "Retested submission #{SubmissionId}")]
    public static partial void LogRetestedSubmission(this ILogger logger, int submissionId);

    [LoggerMessage(1180, LogLevel.Information, "Received execution result for submission #{SubmissionId} from worker {WorkerName}")]
    public static partial void LogReceivedExecutionResult(this ILogger logger, int submissionId, string? workerName);

    [LoggerMessage(1190, LogLevel.Information, "Starting processing execution result for submission #{SubmissionId}: {@ExecutionResult}")]
    public static partial void LogStartingProcessingExecutionResult(this ILogger logger, int submissionId, object executionResult);

    [LoggerMessage(1201, LogLevel.Information, "Processed execution result for submission #{SubmissionId} from worker {WorkerName}")]
    public static partial void LogProcessedExecutionResult(this ILogger logger, int submissionId, string? workerName);

    [LoggerMessage(1202, LogLevel.Information, "Starting processing submission #{SubmissionId} on worker {WorkerName}")]
    public static partial void LogStartingProcessingSubmission(this ILogger logger, int submissionId, string? workerName);

    [LoggerMessage(1203, LogLevel.Information, "Executing submission #{SubmissionId}: {@Submission}")]
    public static partial void LogExecutingSubmission(this ILogger logger, int submissionId, object submission);

    [LoggerMessage(1204, LogLevel.Information, "Produced execution result for submission #{SubmissionId}: {@ExecutionResult}")]
    public static partial void LogProducedExecutionResult(this ILogger logger, int submissionId, object executionResult);

    [LoggerMessage(1206, LogLevel.Information, "Published processed submission #{SubmissionId} from worker: {WorkerName}")]
    public static partial void LogPublishedProcessedSubmission(this ILogger logger, int submissionId, string? workerName);
}
