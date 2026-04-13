namespace OJS.Services.Common
{
    using System.Threading.Tasks;
    using OJS.Services.Infrastructure;

    public interface IArchivedSubmissionsBusinessService : IService
    {
        /// <summary>
        /// Archives old submissions in batches, splitting the work into sub-batches for efficiency.
        /// This is the main method used for automatic nightly archiving.
        /// </summary>
        /// <param name="limit">Maximum number of submissions to archive in this batch.</param>
        /// <param name="maxSubBatchSize">Maximum size of each sub-batch.</param>
        /// <returns>The number of submissions that were archived.</returns>
        Task<int> ArchiveOldSubmissionsDailyBatch(int limit, int maxSubBatchSize);

        /// <summary>
        /// Archives up to a specified number of old submissions in one go.
        /// </summary>
        /// <param name="limit">Maximum number of submissions to archive.</param>
        /// <returns>The number of submissions that were archived.</returns>
        Task<int> ArchiveOldSubmissionsWithLimit(int limit);

        /// <summary>
        /// Hard deletes archived submissions from the main database, up to a limit.
        /// </summary>
        /// <param name="limit">Maximum number of submissions to hard delete.</param>
        /// <returns>The number of submissions that were hard deleted.</returns>
        Task<int> HardDeleteArchivedByLimit(int limit);
    }
}