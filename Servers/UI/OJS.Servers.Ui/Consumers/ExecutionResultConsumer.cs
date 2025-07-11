namespace OJS.Servers.Ui.Consumers;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using OJS.Services.Infrastructure.Extensions;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Models.Submissions;
using OJS.Services.Common.Telemetry;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Ui.Business;
using System;
using static OJS.Servers.Infrastructure.Telemetry.OjsActivitySources;

public class ExecutionResultConsumer(
    ISubmissionsBusinessService submissionsBusinessService,
    ILogger<ExecutionResultConsumer> logger,
    ITracingService tracingService)
    : IConsumer<ProcessedSubmissionPubSubModel>
{
    public async Task Consume(ConsumeContext<ProcessedSubmissionPubSubModel> context)
        => await tracingService.TraceAsync(
            submissions,
            SubmissionActivities.ProcessingExecutionResult,
            async activity =>
            {
                var workerName = context.Message.WorkerName + $" ({context.Host.MachineName})";
                activity?.SetTag(SubmissionTags.WorkerName, workerName);

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
                    activity?.SetTag(SubmissionTags.ProcessingSuccess, true);
                }
                catch (Exception ex)
                {
                    logger.LogErrorProcessingExecutionResult(context.Message.Id, workerName, ex);
                    activity?.SetTag(SubmissionTags.ProcessingSuccess, false);
                    throw;
                }
            },
            tags: null,
            BusinessContext.ForSubmission(context.Message.Id),
            continueFromMessageHeaders: context);
}