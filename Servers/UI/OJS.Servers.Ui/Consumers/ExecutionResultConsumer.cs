namespace OJS.Servers.Ui.Consumers;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using OJS.Services.Infrastructure.Extensions;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Servers.Infrastructure.Telemetry;
using OJS.Services.Common.Models.Submissions;
using OJS.Services.Common.Telemetry;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Ui.Business;
using System;

public class ExecutionResultConsumer(
    ISubmissionsBusinessService submissionsBusinessService,
    ILogger<ExecutionResultConsumer> logger,
    ITracingService tracingService)
    : IConsumer<ProcessedSubmissionPubSubModel>
{
    public async Task Consume(ConsumeContext<ProcessedSubmissionPubSubModel> context)
        => await tracingService.TraceAsync(
            OjsActivitySources.submissions,
            OjsActivitySources.SubmissionActivities.ProcessingExecutionResult,
            async activity =>
            {
                var workerName = context.Message.WorkerName + $" ({context.Host.MachineName})";
                activity?.SetTag("worker.name", workerName);

                logger.LogReceivedExecutionResult(context.Message.Id, workerName);

                if (context.Message.Exception != null)
                {
                    logger.LogExceptionReturnedForSubmission(context.Message.Id, context.Message.Exception);
                }

                try
                {
                    var executionResult = context.Message.Map<SubmissionExecutionResult>();
                    logger.LogStartingProcessingExecutionResult(executionResult.SubmissionId, executionResult);
                    executionResult.WorkerName = workerName;

                    await submissionsBusinessService.ProcessExecutionResult(executionResult);
                    logger.LogProcessedExecutionResult(executionResult.SubmissionId, workerName);
                    activity?.SetTag("processing.result", "success");
                }
                catch (Exception ex)
                {
                    logger.LogErrorProcessingExecutionResult(context.Message.Id, workerName, ex);
                    throw;
                }
            },
            tags: null,
            BusinessContext.ForSubmission(context.Message.Id),
            continueFromMessageHeaders: context);
}