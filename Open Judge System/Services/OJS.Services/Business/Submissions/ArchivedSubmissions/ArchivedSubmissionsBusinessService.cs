namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using System;
    using System.Linq;
    using Hangfire.Server;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Data.Models;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.Submissions.ArchivedSubmissions;

    public class ArchivedSubmissionsBusinessService : IArchivedSubmissionsBusinessService
    {
        private readonly IArchivedSubmissionsDataService archivedSubmissionsData;
        private readonly ISubmissionsBusinessService submissionsBusiness;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IHangfireBackgroundJobService backgroundJobs;

        public ArchivedSubmissionsBusinessService(
            IArchivedSubmissionsDataService archivedSubmissionsData,
            ISubmissionsBusinessService submissionsBusiness,
            ISubmissionsDataService submissionsData,
            IHangfireBackgroundJobService backgroundJobs)
        {
            this.archivedSubmissionsData = archivedSubmissionsData;
            this.submissionsBusiness = submissionsBusiness;
            this.submissionsData = submissionsData;
            this.backgroundJobs = backgroundJobs;
        }

        public void ArchiveOldSubmissions(PerformContext context)
        {
            this.archivedSubmissionsData.CreateDatabaseIfNotExists();

            var allSubmissionsForArchive = this.submissionsBusiness
                .GetAllForArchiving()
                .OrderBy(x => x.Id)
                .InBatches(GlobalConstants.BatchOperationsChunkSize);

            foreach (var submissionsForArchiveBatch in allSubmissionsForArchive)
            {
                var submissionsForArchives = submissionsForArchiveBatch
                    .Select(ArchivedSubmission.FromSubmission)
                    .ToList();

                this.archivedSubmissionsData.Add(submissionsForArchives);
            }

            this.backgroundJobs.OnSucceededStateContinueWith<IArchivedSubmissionsBusinessService>(
                context.BackgroundJob.Id,
                s => s.HardDeleteCurrentArchived(context));
        }

        public void HardDeleteCurrentArchived(PerformContext context)
        {
            var deletedCount = this.submissionsBusiness.HardDeleteAllArchived();
            if (deletedCount > 0)
            {
                this.backgroundJobs.AddFireAndForgetJob<IArchivedSubmissionsBusinessService>(
                    s => s.ArchiveOldSubmissions(context));
            }
        }

        public void ArchiveCleanOldSubmissions(PerformContext context)
        {
            var deleteLimit = DateTime.Now.AddYears(
                -GlobalConstants.BestSubmissionEligibleForArchiveAgeInYears);

            var allSubmissionsForArchive = this.submissionsData
                .GetAll()
                .Where(x => x.CreatedOn < deleteLimit);
        }
    }
}