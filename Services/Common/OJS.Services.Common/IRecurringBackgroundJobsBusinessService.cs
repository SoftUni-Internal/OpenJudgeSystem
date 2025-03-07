﻿namespace OJS.Services.Common
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
    }
}