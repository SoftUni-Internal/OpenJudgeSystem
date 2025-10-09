namespace OJS.Servers.Ui.Consumers;

using FluentExtensions.Extensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Data;
using OJS.Services.Common.Telemetry;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Ui.Data;
using System.Threading.Tasks;
using static OJS.Servers.Infrastructure.Telemetry.OjsActivitySources;
using static OJS.Common.Enumerations.SubmissionProcessingState;

public class SubmissionForProcessingErrorConsumer(
    ILogger<SubmissionForProcessingErrorConsumer> logger,
    ISubmissionsDataService submissionsData,
    ISubmissionsForProcessingCommonDataService submissionsForProcessingCommonData,
    ITracingService tracingService,
    ITransactionsProvider transactionsProvider)
    : IConsumer<Fault<SubmissionForProcessingPubSubModel>>
{
    /// <summary>
    /// Updates the submission state to Processed and marks SubmissionForProcessing as Processed when the worker fails to consume the message.
    /// Uses a transaction to ensure both updates succeed or fail together.
    /// </summary>
    public async Task Consume(ConsumeContext<Fault<SubmissionForProcessingPubSubModel>> context)
        => await tracingService.TraceAsync(
            submissions,
            SubmissionActivities.ProcessingSubmissionError,
            async activity =>
            {
                // If we get here, the worker did not consume the submission for processing successfully.
                // This indicates an unexpected error occurred before the worker could even start processing or during publishing the result.
                var message = context.Message.Message;
                var submissionId = message.Id;

                logger.LogErrorConsumingSubmissionForProcessing(
                    submissionId,
                    context.Message.Exceptions.ToJson());

                var submissionForProcessing = await submissionsForProcessingCommonData.GetBySubmission(submissionId);
                var submission = await submissionsData
                    .GetByIdQuery(submissionId)
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync();

                if (submissionForProcessing is null)
                {
                    logger.LogSubmissionForProcessingNotFoundForSubmission(null, submissionId);
                }

                if (submission is null)
                {
                    logger.LogSubmissionNotFound(submissionId);
                    activity?.SetTag(SubmissionTags.Updated, false);
                    return;
                }

                // Check if already processed to ensure idempotency (in case this consumer is retried)
                if (submission.Processed && submissionForProcessing?.State is Processed or Faulted)
                {
                    activity?.SetTag(SubmissionTags.Updated, false);
                    activity?.SetTag("already_processed", true);
                    return;
                }

                await transactionsProvider.ExecuteInTransaction(async () =>
                {
                    if (submissionForProcessing is not null)
                    {
                        await submissionsForProcessingCommonData.SetProcessingState(submissionForProcessing, Faulted);
                        activity?.SetTag(SubmissionTags.SubmissionForProcessingStateUpdated, true);
                    }

                    submission.Processed = true;
                    submission.IsCompiledSuccessfully = false;
                    submission.TestRunsCache = null;
                    submission.CompilerComment = "Unexpected error occurred during processing on the worker. Please resubmit the solution or contact support if the problem persists.";
                    submission.ProcessingComment = context.Message.Exceptions[0].Message;
                    submissionsData.Update(submission);
                    await submissionsData.SaveChanges();
                    activity?.SetTag(SubmissionTags.Updated, true);
                });
            },
            tags: null,
            BusinessContext.ForSubmission(context.Message.Message.Id),
            continueFromMessageHeaders: context);
}