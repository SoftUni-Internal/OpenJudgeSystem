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
using System;
using System.Threading.Tasks;
using static OJS.Servers.Infrastructure.Telemetry.OjsActivitySources;
using static OJS.Common.Enumerations.SubmissionProcessingState;

public class ExecutionResultErrorConsumer(
    ILogger<ExecutionResultErrorConsumer> logger,
    ISubmissionsDataService submissionsData,
    ISubmissionsForProcessingCommonDataService submissionsForProcessingData,
    ITracingService tracingService)
    : IConsumer<Fault<ProcessedSubmissionPubSubModel>>
{
    /// <summary>
    /// Updates the submission state to Faulted and logs the error.
    /// No need for a transaction here, we just want to safely update everything we can.
    /// </summary>
    public async Task Consume(ConsumeContext<Fault<ProcessedSubmissionPubSubModel>> context)
        => await tracingService.TraceAsync(
            submissions,
            SubmissionActivities.ProcessingExecutionResult,
            async activity =>
            {
                // If we get here, the submission result was not processed successfully, and there is an unexpected error.
                // Further investigation is always needed to determine if the error is recoverable.
                var message = context.Message.Message;
                var submissionId = message.Id;

                logger.LogErrorProcessingExecutionResultForSubmission(
                    submissionId,
                    context.Message.Message.WorkerName,
                    context.Message.Exceptions.ToJson());

                if (message.Exception is not null)
                {
                    var workerException = message.Exception?.Message + Environment.NewLine + message.Exception?.StackTrace;
                    logger.LogExceptionFromWorker(message.Id, message.WorkerName, workerException);
                    activity?.SetTag(SubmissionTags.WorkerException, workerException);
                }

                var submissionForProcessing = await submissionsForProcessingData.GetBySubmission(submissionId);

                if (submissionForProcessing is not null)
                {
                    await submissionsForProcessingData.SetProcessingState(submissionForProcessing, Faulted);
                    activity?.SetTag(SubmissionTags.SubmissionForProcessingStateUpdated, true);
                }
                else
                {
                    logger.LogSubmissionForProcessingNotFoundForSubmission(null, submissionId);
                }

                var submission = await submissionsData
                    .GetByIdQuery(submissionId)
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync();

                if (submission is not null)
                {
                    submission.Processed = true;
                    submission.IsCompiledSuccessfully = false;
                    submission.TestRunsCache = null;
                    submission.StartedExecutionOn = message.StartedExecutionOn;
                    submission.CompletedExecutionOn = message.CompletedExecutionOn;
                    submission.WorkerName = message.WorkerName;
                    submission.CompilerComment = "Unexpected error occurred during processing. Please contact support.";
                    submission.ProcessingComment = context.Message.Exceptions[0].Message;
                    submissionsData.Update(submission);
                    await submissionsData.SaveChanges();
                    activity?.SetTag(SubmissionTags.Updated, true);
                }
                else
                {
                    logger.LogSubmissionNotFound(submissionId);
                    activity?.SetTag(SubmissionTags.Updated, false);
                }
            },
            tags: null,
            BusinessContext.ForSubmission(context.Message.Message.Id),
            continueFromMessageHeaders: context);
}