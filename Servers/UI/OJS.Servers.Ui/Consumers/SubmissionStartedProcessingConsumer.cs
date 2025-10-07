namespace OJS.Servers.Ui.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using OJS.Common.Enumerations;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Data;
using OJS.Services.Common.Telemetry;
using OJS.Services.Infrastructure.Constants;
using System.Threading.Tasks;
using static OJS.Servers.Infrastructure.Telemetry.OjsActivitySources;

public class SubmissionStartedProcessingConsumer(
    ISubmissionsForProcessingCommonDataService submissionsForProcessingCommonData,
    ISubmissionsCommonDataService submissionsData,
    ILogger<SubmissionStartedProcessingConsumer> logger,
    ITracingService tracingService)
    : IConsumer<SubmissionStartedProcessingPubSubModel>
{
    public async Task Consume(ConsumeContext<SubmissionStartedProcessingPubSubModel> context)
        => await tracingService.TraceAsync(
            submissions,
            SubmissionActivities.ProcessingStarted,
            async activity =>
            {
                var submissionId = context.Message.SubmissionId;
                var workerName = context.Message.WorkerName;
                var processingStartedAt = context.Message.ProcessingStartedAt;

                var submissionForProcessing = await submissionsForProcessingCommonData.GetBySubmission(submissionId);
                var submission = await submissionsData.Find(submissionId);

                if (submission == null)
                {
                    logger.LogSubmissionNotFound(submissionId);
                    return;
                }

                submission.WorkerName = workerName;
                submission.StartedExecutionOn = processingStartedAt.UtcDateTime;

                if (submissionForProcessing == null)
                {
                    logger.LogSubmissionForProcessingNotFoundForSubmission(null, submissionId);
                }
                else if (submissionForProcessing.State == SubmissionProcessingState.Enqueued)
                {
                    // Update the processing state only if the submission is still in the enqueued state.
                    // It's possible that the submission is already being processed and marked as processed.
                    // In this case, we don't want to revert the state to processing.
                    await submissionsForProcessingCommonData.SetProcessingState(
                        submissionForProcessing,
                        SubmissionProcessingState.Processing,
                        processingStartedAt,
                        saveChanges: false);
                }

                await submissionsData.SaveChanges();
            },
            tags: null,
            BusinessContext.ForSubmission(context.Message.SubmissionId),
            continueFromMessageHeaders: context);
}