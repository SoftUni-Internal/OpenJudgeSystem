namespace OJS.Services.Common;

using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OJS.Services.Infrastructure.BackgroundJobs;
using OJS.Services.Infrastructure.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;
using static OJS.Common.GlobalConstants.BackgroundJobs;

/// <summary>
/// Service that runs in the background on each app startup and registers or updates all hangfire background jobs.
/// Exits when finished registering the jobs.
/// </summary>
public class BackgroundJobsHostedService : IHostedService
{
    private const string EnqueuePendingSubmissionsCronExpression = "*/3 * * * *";
    private readonly string deleteProcessedSubmissionsCronExpression = Cron.Daily(2);
    private readonly string updatingParticipantTotalScoreSnapshotCronExpression = Cron.Daily(4);
    private readonly string removingMultipleParticipantScoresForProblemCronExpression = Cron.Daily(3);
    private readonly string normalizingAllPointsThatExceedAllowedLimitCronExpression = Cron.Daily(1);
    private readonly string archiveOldSubmissionsDailyBatchCronExpression = Cron.Daily(1, 30);
    private readonly string hardDeleteArchivedSubmissionsCronExpression = Cron.Yearly(1, 1, 2, 30);

    private readonly IHangfireBackgroundJobsService hangfireBackgroundJobs;
    private readonly ILogger<BackgroundJobsHostedService> logger;

    public BackgroundJobsHostedService(
        IHangfireBackgroundJobsService hangfireBackgroundJobs,
        ILogger<BackgroundJobsHostedService> logger)
    {
        this.hangfireBackgroundJobs = hangfireBackgroundJobs;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            this.AddOrUpdateRecurringJobs();
        }
        catch (Exception ex)
        {
            this.logger.LogHostedServiceException(nameof(BackgroundJobsHostedService), ex);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.logger.LogStoppingHostedService(nameof(BackgroundJobsHostedService));

        return Task.CompletedTask;
    }

    private void AddOrUpdateRecurringJobs()
    {
        this.hangfireBackgroundJobs
            .AddOrUpdateRecurringJob<IRecurringBackgroundJobsBusinessService>(
                nameof(IRecurringBackgroundJobsBusinessService.EnqueuePendingSubmissions),
                m => m.EnqueuePendingSubmissions(),
                EnqueuePendingSubmissionsCronExpression,
                AdministrationQueueName);

        this.logger.LogBackgroundJobAddedOrUpdated("enqueueing pending submissions");

        this.hangfireBackgroundJobs
            .AddOrUpdateRecurringJob<IRecurringBackgroundJobsBusinessService>(
                nameof(IRecurringBackgroundJobsBusinessService.DeleteProcessedSubmissions),
                m => m.DeleteProcessedSubmissions(),
                this.deleteProcessedSubmissionsCronExpression,
                AdministrationQueueName);

        this.logger.LogBackgroundJobAddedOrUpdated("deleting processed submissions");

        this.hangfireBackgroundJobs
            .AddOrUpdateRecurringJob<IRecurringBackgroundJobsBusinessService>(
                nameof(IRecurringBackgroundJobsBusinessService.UpdateTotalScoreSnapshotOfParticipants),
                m => m.UpdateTotalScoreSnapshotOfParticipants(),
                this.updatingParticipantTotalScoreSnapshotCronExpression,
                AdministrationQueueName);

        this.logger.LogBackgroundJobAddedOrUpdated("updating total score snapshot of participants");

        this.hangfireBackgroundJobs
            .AddOrUpdateRecurringJob<IRecurringBackgroundJobsBusinessService>(
                nameof(IRecurringBackgroundJobsBusinessService.RemoveParticipantMultipleScores),
                m => m.RemoveParticipantMultipleScores(),
                this.removingMultipleParticipantScoresForProblemCronExpression,
                AdministrationQueueName);

        this.logger.LogBackgroundJobAddedOrUpdated("removing participant multiple scores");

        this.hangfireBackgroundJobs
            .AddOrUpdateRecurringJob<IRecurringBackgroundJobsBusinessService>(
                nameof(IRecurringBackgroundJobsBusinessService.NormalizeAllPointsThatExceedAllowedLimit),
                m => m.NormalizeAllPointsThatExceedAllowedLimit(),
                this.normalizingAllPointsThatExceedAllowedLimitCronExpression,
                AdministrationQueueName);

        this.logger.LogBackgroundJobAddedOrUpdated("normalizing all points that exceed allowed limit");

        this.hangfireBackgroundJobs
            .AddOrUpdateRecurringJob<IRecurringBackgroundJobsBusinessService>(
                nameof(IRecurringBackgroundJobsBusinessService.ArchiveOldSubmissionsDailyBatch),
                m => m.ArchiveOldSubmissionsDailyBatch(),
                this.archiveOldSubmissionsDailyBatchCronExpression,
                AdministrationQueueName);

        this.logger.LogBackgroundJobAddedOrUpdated("archiving submissions - daily");

        this.hangfireBackgroundJobs
            .AddOrUpdateRecurringJob<IRecurringBackgroundJobsBusinessService>(
                nameof(IRecurringBackgroundJobsBusinessService.HardDeleteArchivedSubmissions),
                m => m.HardDeleteArchivedSubmissions(),
                this.hardDeleteArchivedSubmissionsCronExpression,
                AdministrationQueueName);

        this.logger.LogBackgroundJobAddedOrUpdated("hard deleting archived submissions");
    }
}