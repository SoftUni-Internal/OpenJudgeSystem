namespace OJS.Servers.Worker.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OJS.Services.Common;
using OJS.Services.Worker.Business;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Threading.Tasks;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Extensions;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Worker.Models.Configuration;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using ConfigurationException = OJS.Workers.Common.Exceptions.ConfigurationException;
using OJS.Servers.Infrastructure.Telemetry;
using OJS.Services.Common.Telemetry;

public class SubmissionsForProcessingConsumer(
    ISubmissionsBusinessService submissionsBusiness,
    IPublisherService publisher,
    IHostInfoService hostInfoService,
    ILogger<SubmissionsForProcessingConsumer> logger,
    IDatesService dates,
    IOptions<SubmissionExecutionConfig> executionConfigAccessor,
    ITracingService tracingService)
    : IConsumer<SubmissionForProcessingPubSubModel>
{
    private readonly SubmissionExecutionConfig executionConfig = executionConfigAccessor.Value;

    public Task Consume(ConsumeContext<SubmissionForProcessingPubSubModel> context)
        => tracingService.TraceAsync(
            OjsActivitySources.submissions,
            OjsActivitySources.SubmissionActivities.Execution,
            async activity =>
            {
                var startedExecutionOn = dates.GetUtcNowOffset();
                var workerName = hostInfoService.GetHostIp();

                activity?.SetTag("worker.name", workerName);
                activity?.SetTag("submission.verbosely", context.Message.Verbosely);
                activity?.SetTag("submission.strategy", context.Message.ExecutionStrategy);

                logger.LogStartingProcessingSubmission(context.Message.Id, workerName);

                var submissionStartedProcessingPubSubModel = new SubmissionStartedProcessingPubSubModel
                {
                    SubmissionId = context.Message.Id,
                    ProcessingStartedAt = startedExecutionOn,
                };

                await publisher.Publish(submissionStartedProcessingPubSubModel);

                var result = new ProcessedSubmissionPubSubModel(context.Message.Id)
                {
                    WorkerName = workerName,
                };

                try
                {
                    var submission = context.Message.Map<SubmissionServiceModel>();
                    logger.LogExecutingSubmission(submission.Id, submission.TrimDetails());

                    activity?.SetTag("submission.content_length", submission.FileContent?.Length ?? 0);

                    var executionResult = await submissionsBusiness.ExecuteSubmission(submission);

                    logger.LogProducedExecutionResult(submission.Id, executionResult);
                    result.SetExecutionResult(executionResult);

                    activity?.SetTag("execution.result", "success");
                    activity?.SetTag("execution.points", executionResult.TaskResult?.Points ?? 0);
                }
                catch (Exception exception)
                {
                    var exceptionType = exception switch
                    {
                        StrategyException => ExceptionType.Strategy,
                        SolutionException => ExceptionType.Solution,
                        ConfigurationException => ExceptionType.Configuration,
                        _ => ExceptionType.Other,
                    };

                    logger.LogErrorProcessingSubmission(context.Message.Id, result.WorkerName, exception);
                    result.SetException(exception, true, exceptionType);

                    // Mark processing as failed
                    tracingService.MarkAsFailed(activity!, exception, exceptionType.ToString());
                }
                finally
                {
                    result.SetStartedAndCompletedExecutionOn(startedExecutionOn.UtcDateTime, completedExecutionOn: DateTime.UtcNow);

                    if (context.Message.Verbosely)
                    {
                        // If the submission is marked as verbose, try to read the log file and attach it to the result
                        var logFilePath = FileHelpers.BuildSubmissionLogFilePath(context.Message.Id);
                        if (FileHelpers.FileExists(logFilePath))
                        {
                            result.VerboseLogFile = await FileHelpers.ReadFileUpToBytes(logFilePath, this.executionConfig.SubmissionVerboseLogFileMaxBytes);
                            FileHelpers.DeleteFile(logFilePath);
                        }
                    }

                    // Add final processing metrics
                    var processingDuration = DateTime.UtcNow - startedExecutionOn.UtcDateTime;
                    activity?.SetTag(OjsActivitySources.CommonTags.Duration, processingDuration.TotalMilliseconds);
                }

                await publisher.Publish(result);
                logger.LogPublishedProcessedSubmission(context.Message.Id, result.WorkerName);
            },
            tags: null,
            BusinessContext.ForSubmission(context.Message.Id, context.Message.TestsExecutionDetails?.TaskId),
            continueFromMessageHeaders: context);
}