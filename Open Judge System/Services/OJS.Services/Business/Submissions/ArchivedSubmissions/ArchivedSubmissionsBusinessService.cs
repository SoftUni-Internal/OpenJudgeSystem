namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using System.Linq;
    using Hangfire.Server;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Data.Models;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.Submissions.ArchivedSubmissions;

    public class ArchivedSubmissionsBusinessService : IArchivedSubmissionsBusinessService
    {
        private readonly IArchivedSubmissionsDataService archivedSubmissionsData;
        private readonly ISubmissionsBusinessService submissionsBusiness;
        private readonly IHangfireBackgroundJobService backgroundJobs;

        public ArchivedSubmissionsBusinessService(
            IArchivedSubmissionsDataService archivedSubmissionsData,
            ISubmissionsBusinessService submissionsBusiness,
            IHangfireBackgroundJobService backgroundJobs)
        {
            this.archivedSubmissionsData = archivedSubmissionsData;
            this.submissionsBusiness = submissionsBusiness;
            this.backgroundJobs = backgroundJobs;
        }

        public int ArchiveOldSubmissionsDailyBatch(PerformContext context, int limit, int maxSubBatchSize)
        {
            var leftoverSubmissionsFromBatchSplitting = limit % maxSubBatchSize;
            var numberOfIterations = limit / maxSubBatchSize;
            if(leftoverSubmissionsFromBatchSplitting > 0)
            {
                numberOfIterations++;
            }
            
            var archived = 0;
            var deletedCount = 0;

            for (var i = 0; i < numberOfIterations; i++)
            {
                var curBatchSize = maxSubBatchSize;
                var isLastIteration = (i == (numberOfIterations - 1));
                if(leftoverSubmissionsFromBatchSplitting > 0 && isLastIteration)
                {
                    curBatchSize = leftoverSubmissionsFromBatchSplitting;
                }

                var allSubmissionsForArchive = this.submissionsBusiness
                                .GetAllForArchiving()
                                .OrderBy(x => x.Id)
                                .InBatches(GlobalConstants.BatchOperationsChunkSize, curBatchSize);

                foreach (var submissionsForArchiveBatch in allSubmissionsForArchive)
                {
                    var submissionsForArchives = submissionsForArchiveBatch
                        .Select(ArchivedSubmission.FromSubmission)
                        .ToList();

                    if(submissionsForArchives.Count == 0)
                    {
                        break;
                    }

                    archived += this.archivedSubmissionsData.Add(submissionsForArchives);
                }

                deletedCount += this.submissionsBusiness.HardDeleteArchived(curBatchSize);
            }

            return deletedCount;
        }

        public int ArchiveOldSubmissionsWithLimit(PerformContext context, int limit)
        {
            var archived = 0;
            this.archivedSubmissionsData.CreateDatabaseIfNotExists();

            var allSubmissionsForArchive = this.submissionsBusiness
                .GetAllForArchiving()
                .OrderBy(x => x.Id)
                .InBatches(GlobalConstants.BatchOperationsChunkSize, limit);

            foreach (var submissionsForArchiveBatch in allSubmissionsForArchive)
            {
                var submissionsForArchives = submissionsForArchiveBatch
                    .Select(ArchivedSubmission.FromSubmission)
                    .ToList();

                if(submissionsForArchives.Count == 0)
                {
                    break;
                }

                archived += this.archivedSubmissionsData.Add(submissionsForArchives);
            }

            return archived;
        }

        public int HardDeleteArchivedByLimit(PerformContext context, int limit)
            => this.submissionsBusiness.HardDeleteArchived(limit);
    }
}