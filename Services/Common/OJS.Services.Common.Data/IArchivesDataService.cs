namespace OJS.Services.Common.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Infrastructure;

    public interface IArchivesDataService : IService
    {
        /// <summary>
        /// Creates the archive database if it doesn't exist.
        /// </summary>
        Task CreateDatabaseIfNotExists();

        /// <summary>
        /// Gets archived submissions that were created before a specific date.
        /// </summary>
        /// <param name="createdBeforeDate">The date to filter by.</param>
        /// <returns>Queryable of archived submissions.</returns>
        IQueryable<ArchivedSubmission> GetAllCreatedBeforeDate(DateTime createdBeforeDate);

        /// <summary>
        /// Gets archived submissions that are not hard-deleted from the main database.
        /// </summary>
        /// <returns>Queryable of archived submissions.</returns>
        IQueryable<ArchivedSubmission> GetAllNotHardDeletedFromMainDatabase();

        /// <summary>
        /// Marks archived submissions as hard-deleted from the main database.
        /// </summary>
        /// <param name="submissionIds">The IDs of submissions to mark.</param>
        Task<int> MarkAsHardDeletedFromMainDatabase(IEnumerable<int> submissionIds);

        Task<int> AddMany(IEnumerable<ArchivedSubmission> entities);

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        Task SaveChanges();
    }
}