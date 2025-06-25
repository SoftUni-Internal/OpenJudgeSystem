namespace OJS.Services.Common.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using OJS.Data;
    using OJS.Data.Models.Submissions;
    using OJS.Common;

    using efplus = Z.EntityFramework.Plus;

    public class ArchivesDataService : IArchivesDataService
    {
        private readonly ArchivesDbContext archivesDbContext;
        private readonly DbSet<ArchivedSubmission> dbSet;

        public ArchivesDataService(ArchivesDbContext archivesDbContext)
        {
            this.archivesDbContext = archivesDbContext;
            this.dbSet = archivesDbContext.Set<ArchivedSubmission>();
        }

        public IQueryable<ArchivedSubmission> GetQuery(
            System.Linq.Expressions.Expression<Func<ArchivedSubmission, bool>>? filter = null,
            System.Linq.Expressions.Expression<Func<ArchivedSubmission, object>>? orderBy = null,
            bool descending = false,
            int? skip = null,
            int? take = null)
        {
            var query = this.dbSet.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = descending
                    ? query.OrderByDescending(orderBy)
                    : query.OrderBy(orderBy);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query;
        }

        public async Task CreateDatabaseIfNotExists()
        {
            try
            {
                await this.archivesDbContext.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create archive database", ex);
            }
        }

        public IQueryable<ArchivedSubmission> GetAllCreatedBeforeDate(DateTime createdBeforeDate)
            => this.GetQuery(s => s.CreatedOn < createdBeforeDate);

        public IQueryable<ArchivedSubmission> GetAllNotHardDeletedFromMainDatabase()
            => this.GetQuery(s => !s.IsHardDeletedFromMainDatabase);

        public async Task<int> MarkAsHardDeletedFromMainDatabase(IEnumerable<int> submissionIds)
            => await this.dbSet
                .Where(s => submissionIds.Contains(s.Id))
                .UpdateFromQueryAsync(s => new ArchivedSubmission
                {
                    IsHardDeletedFromMainDatabase = true,
                    ModifiedOn = DateTime.UtcNow,
                },
                bub => bub.BatchSize = GlobalConstants.BatchOperationsChunkSize);

        public async Task<int> AddMany(IEnumerable<ArchivedSubmission> entities)
        {
            var entitiesList = entities.ToList();
            var ids = entitiesList
                .Select(x => x.Id)
                .ToHashSet();

            var existingEntities = this.dbSet
                .Where(x => ids.Contains(x.Id))
                .Select(x => x.Id)
                .ToHashSet();

            var entitiesToAdd = entitiesList
                .Where(x => !existingEntities.Contains(x.Id))
                .ToList();

            await this.dbSet.AddRangeAsync(entitiesToAdd);
            return entitiesToAdd.Count;
        }

        public async Task SaveChanges()
            => await this.archivesDbContext.SaveChangesAsync();
    }
}