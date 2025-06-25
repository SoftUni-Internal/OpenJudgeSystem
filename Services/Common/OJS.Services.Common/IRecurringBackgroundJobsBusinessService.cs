namespace OJS.Services.Common
{
    using Hangfire;
    using OJS.Services.Common.Filters;
    using OJS.Services.Infrastructure;
    using System.Threading.Tasks;
    using static OJS.Common.GlobalConstants.BackgroundJobs;

    public interface IRecurringBackgroundJobsBusinessService : IService
    {
        /// <summary>
        /// Enqueues all submissions that are pending (not added in the message queue, nor processing).
        /// </summary>
        [MessageBusExceptionFilter]
        [Queue(AdministrationQueueName)]
        Task<object> EnqueuePendingSubmissions();

        /// <summary>
        /// Deletes all processed (and not processing) submissions from the SubmissionsForProcessing table.
        /// </summary>
        [Queue(AdministrationQueueName)]
        Task<object> DeleteProcessedSubmissions();

        [Queue(AdministrationQueueName)]
        Task<object> UpdateTotalScoreSnapshotOfParticipants();

        [Queue(AdministrationQueueName)]
        Task<object> RemoveParticipantMultipleScores();

        [Queue(AdministrationQueueName)]
        Task<object> NormalizeAllPointsThatExceedAllowedLimit();

        /// <summary>
        /// Archives old submissions in batches for automatic nightly archiving.
        /// </summary>
        [Queue(AdministrationQueueName)]
        Task<object> ArchiveOldSubmissionsDailyBatch();

        /// <summary>
        /// Archives old submissions with a limit for yearly archiving.
        /// </summary>
        [Queue(AdministrationQueueName)]
        Task<object> ArchiveOldSubmissionsWithLimit();

        /// <summary>
        /// Hard deletes archived submissions that are no longer needed.
        /// </summary>
        [Queue(AdministrationQueueName)]
        Task<object> HardDeleteArchivedSubmissions();
    }
}