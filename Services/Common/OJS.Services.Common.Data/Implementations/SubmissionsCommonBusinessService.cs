namespace OJS.Services.Common.Data.Implementations;

using Microsoft.Extensions.Logging;
using OJS.Common.Enumerations;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Submissions;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Common.Telemetry;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static OJS.Servers.Infrastructure.Telemetry.OjsActivitySources;

public class SubmissionsCommonBusinessService(
    IPublisherService publisher,
    ISubmissionsCommonDataService submissionsCommonDataService,
    ISubmissionsForProcessingCommonDataService submissionForProcessingData,
    ILogger<SubmissionsCommonBusinessService> logger,
    IDatesService dates,
    ITracingService tracingService)
    : ISubmissionsCommonBusinessService
{
    public SubmissionServiceModel BuildSubmissionForProcessing(
        Submission submission,
        Problem problem,
        SubmissionType submissionType,
        bool executeVerbosely = false)
    {
        // We detach the existing entity, in order to avoid tracking exception on Update.
        submissionsCommonDataService.Detach(submission);

        // Needed to map execution details
        submission.Problem = problem;
        submission.SubmissionType = submissionType;

        var serviceModel = submission.Map<SubmissionServiceModel>();
        serviceModel.Verbosely = executeVerbosely;

        serviceModel.TestsExecutionDetails!.TaskSkeleton = problem.SubmissionTypesInProblems
            .Where(x => x.SubmissionTypeId == submission.SubmissionTypeId)
            .Select(x => x.SolutionSkeleton)
            .FirstOrDefault();

        return serviceModel;
    }

    public SubmissionServiceModel BuildSubmissionForProcessing(Submission submission, bool executeVerbosely = false)
        => this.BuildSubmissionForProcessing(submission, submission.Problem, submission.SubmissionType!, executeVerbosely);

    public async Task PublishSubmissionForProcessing(SubmissionServiceModel submission, SubmissionForProcessing submissionForProcessing)
        => await tracingService.TraceAsync(
            submissions,
            SubmissionActivities.Queued,
            async activity =>
            {
                // Add technical context
                tracingService.AddTechnicalContext(activity!, "publish_to_queue", "message_queue");

                var pubSubModel = submission.Map<SubmissionForProcessingPubSubModel>();

                // Trace context is automatically injected into RabbitMQ headers by MassTransit filters
                // No need to manually add trace context to the message model

                var enqueuedAt = dates.GetUtcNowOffset();

                try
                {
                    await publisher.Publish(pubSubModel);

                    // Add success metrics
                    activity?.SetTag("publish.success", true);
                    activity?.SetTag("enqueued_at", enqueuedAt.UtcDateTime.ToString("o"));
                }
                catch (Exception ex)
                {
                    // We log the exception and return. The submission will be retried later by the background job for Pending submissions.
                    logger.LogExceptionPublishingSubmission(submission.Id, ex);
                    activity?.SetTag("publish.success", false);
                    tracingService.MarkAsFailed(activity!, ex);
                    return;
                }

                // We detach the entity to ensure we get fresh data from the database.
                submissionForProcessingData.Detach(submissionForProcessing);
                var freshSubmissionForProcessing = await submissionForProcessingData.Find(submissionForProcessing.Id);

                if (freshSubmissionForProcessing == null || freshSubmissionForProcessing.SubmissionId != submission.Id)
                {
                    logger.LogSubmissionForProcessingNotFoundForSubmission(submissionForProcessing.Id, submission.Id);
                    activity?.SetTag(SubmissionTags.SubmissionForProcessingStateUpdated, false);
                    return;
                }

                await submissionForProcessingData
                    .SetProcessingState(freshSubmissionForProcessing, SubmissionProcessingState.Enqueued, enqueuedAt);

                activity?.SetTag(SubmissionTags.SubmissionForProcessingStateUpdated, true);
            },
            new Dictionary<string, object?>
            {
                [SubmissionTags.Strategy] = submission.ExecutionStrategy.ToString(),
                [SubmissionTags.Verbosely] = submission.Verbosely,
            },
            BusinessContext.ForSubmission(submission.Id, submission.TestsExecutionDetails?.TaskId));

    public async Task<int> PublishSubmissionsForProcessing(ICollection<SubmissionServiceModel> submissions)
    {
        var pubSubModels = submissions.MapCollection<SubmissionForProcessingPubSubModel>().ToList();
        var enqueuedAt = dates.GetUtcNowOffset();

        try
        {
            await publisher.PublishBatch(pubSubModels);
        }
        catch (Exception ex)
        {
            logger.LogExceptionPublishingSubmissionsBatch(ex);
            throw;
        }

        var submissionsIds = submissions.Select(s => s.Id).ToList();

        await submissionForProcessingData.MarkMultipleEnqueued(submissionsIds, enqueuedAt);
        return pubSubModels.Count;
    }
}