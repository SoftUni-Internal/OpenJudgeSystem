namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using Hangfire;
    using Hangfire.Server;

    using OJS.Services.Common;

    public interface IArchivedSubmissionsBusinessService : IService
    {
        [AutomaticRetry(Attempts = 0)]
        int ArchiveOldSubmissionsDailyBatch(PerformContext context, int limit, int maxSubBatchSize);

        int ArchiveOldSubmissionsWithLimit(PerformContext context, int limit);

        int HardDeleteArchivedByLimit(PerformContext context, int limit);
    }
}