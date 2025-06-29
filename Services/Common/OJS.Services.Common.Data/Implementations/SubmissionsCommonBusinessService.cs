namespace OJS.Services.Common.Data.Implementations;

using Microsoft.Extensions.Logging;
using OJS.Common.Enumerations;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Submissions;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Servers.Infrastructure.Telemetry;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Common.Telemetry;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SubmissionsCommonBusinessService : ISubmissionsCommonBusinessService
{
    private readonly IPublisherService publisher;
    private readonly ISubmissionsCommonDataService submissionsCommonDataService;
    private readonly ISubmissionsForProcessingCommonDataService submissionForProcessingData;
    private readonly ILogger<SubmissionsCommonBusinessService> logger;
    private readonly IDatesService dates;
    private readonly ITracingService tracingService;

    public SubmissionsCommonBusinessService(
        IPublisherService publisher,
        ISubmissionsCommonDataService submissionsCommonDataService,
        ISubmissionsForProcessingCommonDataService submissionForProcessingData,
        ILogger<SubmissionsCommonBusinessService> logger,
        IDatesService dates,
        ITracingService tracingService)
    {
        this.publisher = publisher;
        this.submissionsCommonDataService = submissionsCommonDataService;
        this.submissionForProcessingData = submissionForProcessingData;
        this.logger = logger;
        this.dates = dates;
        this.tracingService = tracingService;
    }

    public SubmissionServiceModel BuildSubmissionForProcessing(
        Submission submission,
        Problem problem,
        SubmissionType submissionType,
        bool executeVerbosely = false)
    {
        // We detach the existing entity, in order to avoid tracking exception on Update.
        this.submissionsCommonDataService.Detach(submission);

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
        => await this.tracingService.TraceAsync(
            OjsActivitySources.submissions,
            OjsActivitySources.SubmissionActivities.Queued,
            async activity =>
            {
                // Add technical context
                this.tracingService.AddTechnicalContext(activity!, "publish_to_queue", "message_queue");

                var pubSubModel = submission.Map<SubmissionForProcessingPubSubModel>();

                // Trace context is automatically injected into RabbitMQ headers by MassTransit filters
                // No need to manually add trace context to the message model

                var enqueuedAt = this.dates.GetUtcNowOffset();

                try
                {
                    await this.publisher.Publish(pubSubModel);

                    // Add success metrics
                    activity?.SetTag("publish.success", true);
                    activity?.SetTag("enqueued_at", enqueuedAt.UtcDateTime.ToString("o"));
                }
                catch (Exception ex)
                {
                    // We log the exception and return. The submission will be retried later by the background job for Pending submissions.
                    this.logger.LogExceptionPublishingSubmission(submission.Id, ex);
                    activity?.SetTag("publish.success", false);
                    this.tracingService.MarkAsFailed(activity!, ex);
                    return;
                }

                // We detach the entity to ensure we get fresh data from the database.
                this.submissionForProcessingData.Detach(submissionForProcessing);
                var freshSubmissionForProcessing = await this.submissionForProcessingData.Find(submissionForProcessing.Id);

                if (freshSubmissionForProcessing == null || freshSubmissionForProcessing.SubmissionId != submission.Id)
                {
                    this.logger.LogSubmissionForProcessingNotFoundForSubmission(submissionForProcessing.Id, submission.Id);
                    return;
                }

                await this.submissionForProcessingData
                    .SetProcessingState(freshSubmissionForProcessing, SubmissionProcessingState.Enqueued, enqueuedAt);

                // Add final success metrics
                activity?.SetTag("submission.submission_for_processing_state_updated", true);
            },
            new Dictionary<string, object?>
            {
                ["submission.execution_strategy"] = submission.ExecutionStrategy.ToString(),
                ["submission.verbosely"] = submission.Verbosely,
            },
            BusinessContext.ForSubmission(submission.Id, submission.TestsExecutionDetails?.TaskId));

    public async Task<int> PublishSubmissionsForProcessing(ICollection<SubmissionServiceModel> submissions)
    {
        var pubSubModels = submissions.MapCollection<SubmissionForProcessingPubSubModel>().ToList();
        var enqueuedAt = this.dates.GetUtcNowOffset();

        try
        {
            await this.publisher.PublishBatch(pubSubModels);
        }
        catch (Exception ex)
        {
            this.logger.LogExceptionPublishingSubmissionsBatch(ex);
            throw;
        }

        var submissionsIds = submissions.Select(s => s.Id).ToList();

        await this.submissionForProcessingData.MarkMultipleEnqueued(submissionsIds, enqueuedAt);
        return pubSubModels.Count;
    }
}