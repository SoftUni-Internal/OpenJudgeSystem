namespace OJS.Services.Administration.Business.Implementations
{
    using MassTransit;
    using Microsoft.Extensions.Logging;
    using OJS.Services.Administration.Business.Participants;
    using OJS.Services.Administration.Business.SubmissionsForProcessing;
    using OJS.Services.Common;
    using OJS.Services.Common.Exceptions;
    using OJS.Services.Infrastructure.Constants;
    using System;
    using System.Threading.Tasks;

    public class RecurringBackgroundJobsBusinessService : IRecurringBackgroundJobsBusinessService
    {
        private readonly ISubmissionsForProcessingBusinessService submissionsForProcessing;
        private readonly IParticipantsBusinessService participantsBusinessService;
        private readonly IParticipantScoresBusinessService participantScoresBusiness;
        private readonly IArchivedSubmissionsBusinessService archivedSubmissionsBusiness;
        private readonly IBusControl bus;
        private readonly ILogger<RecurringBackgroundJobsBusinessService> logger;

        public RecurringBackgroundJobsBusinessService(
            ISubmissionsForProcessingBusinessService submissionsForProcessing,
            IParticipantsBusinessService participantsBusinessService,
            IParticipantScoresBusinessService participantScoresBusiness,
            IArchivedSubmissionsBusinessService archivedSubmissionsBusiness,
            IBusControl bus,
            ILogger<RecurringBackgroundJobsBusinessService> logger)
        {
            this.submissionsForProcessing = submissionsForProcessing;
            this.participantsBusinessService = participantsBusinessService;
            this.participantScoresBusiness = participantScoresBusiness;
            this.archivedSubmissionsBusiness = archivedSubmissionsBusiness;
            this.bus = bus;
            this.logger = logger;
        }

        public async Task<object> EnqueuePendingSubmissions()
        {
            var busHealth = this.bus.CheckHealth();

            if (busHealth.Status != BusHealthStatus.Healthy)
            {
                this.logger.LogMessageBusHealthCheckFailed(Enum.GetName(typeof(BusHealthStatus), busHealth.Status));
                throw new MessageBusNotHealthyException("The message bus is not in a healthy state. Cannot enqueue pending submissions.");
            }

            const int fromMinutesAgo = 3;
            var enqueuedCount = await this.submissionsForProcessing.EnqueuePendingSubmissions(fromMinutesAgo);

            return $"Successfully enqueued {enqueuedCount} pending (more than {fromMinutesAgo} minutes ago) submissions.";
        }

        public async Task<object> DeleteProcessedSubmissions()
        {
            const int fromMinutesAgo = 60;
            var deletedCount = await this.submissionsForProcessing.DeleteProcessedSubmissions(fromMinutesAgo);

            return $"Successfully deleted {deletedCount} processed (more than {fromMinutesAgo} minutes ago) " +
                   $"submissions from SubmissionsForProcessing table";
        }

        public async Task<object> UpdateTotalScoreSnapshotOfParticipants()
        {
           await this.participantsBusinessService.UpdateTotalScoreSnapshotOfParticipants();

           return "Successfully updated total score snapshot of participants";
        }

        public async Task<object> RemoveParticipantMultipleScores()
        {
            await this.participantsBusinessService.RemoveDuplicateParticipantScores();

            return "Successfully removed participant multiple scores";
        }

        public async Task<object> NormalizeAllPointsThatExceedAllowedLimit()
        {
            await this.participantScoresBusiness.NormalizeAllPointsThatExceedAllowedLimit();

            return "Successfully normalized all points that exceed allowed limit";
        }

        public async Task<object> ArchiveOldSubmissionsDailyBatch()
        {
            const int archiveDailyBatchLimit = 500_000;
            const int archiveMaxSubBatchSize = 10_000;

            var archivedCount = await this.archivedSubmissionsBusiness.ArchiveOldSubmissionsDailyBatch(
                archiveDailyBatchLimit,
                archiveMaxSubBatchSize);

            return $"Successfully archived {archivedCount} submissions.";
        }

        public async Task<object> ArchiveOldSubmissionsWithLimit()
        {
            const int archiveYearlyBatchLimit = 25_000;

            var archivedCount = await this.archivedSubmissionsBusiness.ArchiveOldSubmissionsWithLimit(
                archiveYearlyBatchLimit);

            return $"Successfully archived {archivedCount} submissions in yearly batch.";
        }

        public async Task<object> HardDeleteArchivedSubmissions()
        {
            const int archiveSingleBatchLimit = 25_000;

            var hardDeletedCount = await this.archivedSubmissionsBusiness.HardDeleteArchivedByLimit(
                archiveSingleBatchLimit);

            return $"Successfully hard deleted {hardDeletedCount} archived submissions.";
        }
    }
}