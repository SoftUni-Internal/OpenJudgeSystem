namespace OJS.Services.Administration.Business.Implementations;

using System.Linq;
using System.Threading.Tasks;
using OJS.Common;
using OJS.Data.Models.Submissions;
using OJS.Services.Administration.Data;
using OJS.Services.Common;
using OJS.Services.Common.Data;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Extensions;

public class ArchivedSubmissionsBusinessService : IArchivedSubmissionsBusinessService
{
    private readonly ISubmissionsDataService submissionsData;
    private readonly IArchivesDataService archivesData;
    private readonly IDatesService dates;

    public ArchivedSubmissionsBusinessService(
        ISubmissionsDataService submissionsData,
        IArchivesDataService archivesData,
        IDatesService dates)
    {
        this.submissionsData = submissionsData;
        this.archivesData = archivesData;
        this.dates = dates;
    }

    public async Task<int> ArchiveOldSubmissionsDailyBatch(int limit, int maxSubBatchSize)
    {
        await this.archivesData.CreateDatabaseIfNotExists();

        var leftoverSubmissionsFromBatchSplitting = limit % maxSubBatchSize;
        var numberOfIterations = limit / maxSubBatchSize;
        if(leftoverSubmissionsFromBatchSplitting > 0)
        {
            numberOfIterations++;
        }

        var archived = 0;

        for (var i = 0; i < numberOfIterations; i++)
        {
            var curBatchSize = maxSubBatchSize;
            var isLastIteration = i == (numberOfIterations - 1);
            if(leftoverSubmissionsFromBatchSplitting > 0 && isLastIteration)
            {
                curBatchSize = leftoverSubmissionsFromBatchSplitting;
            }

            var allSubmissionsForArchive = this
                .GetSubmissionsForArchiving()
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

                archived += await this.archivesData.AddMany(submissionsForArchives);
                await this.archivesData.SaveChanges();
            }

            await this.submissionsData.HardDeleteArchived(curBatchSize);
        }

        return archived;
    }

    public async Task<int> ArchiveOldSubmissionsWithLimit(int limit)
    {
        var archived = 0;
        await this.archivesData.CreateDatabaseIfNotExists();

        var allSubmissionsForArchive = this
            .GetSubmissionsForArchiving()
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

            archived += await this.archivesData.AddMany(submissionsForArchives);
            await this.archivesData.SaveChanges();
        }

        return archived;
    }

    public async Task<int> HardDeleteArchivedByLimit(int limit)
        => await this.submissionsData.HardDeleteArchived(limit);

    private IQueryable<Submission> GetSubmissionsForArchiving()
    {
        var now = this.dates.GetUtcNow();
        var bestSubmissionCutoffDate = now.AddYears(-GlobalConstants.BestSubmissionEligibleForArchiveAgeInYears);
        var nonBestSubmissionCutoffDate = now.AddYears(-GlobalConstants.NonBestSubmissionEligibleForArchiveAgeInYears);

        return this.submissionsData
            .GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
                bestSubmissionCutoffDate,
                nonBestSubmissionCutoffDate);
    }
}