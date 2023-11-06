namespace OJS.Services.Data.Submissions.ArchivedSubmissions
{
    using System.Collections.Generic;
    using System.Linq;
    using MissingFeatures;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Data.Archives.Repositories.Contracts;
    using OJS.Data.Models;

    public class ArchivedSubmissionsDataService : IArchivedSubmissionsDataService
    {
        private readonly IArchivesGenericRepository<ArchivedSubmission> archivedSubmissions;

        public ArchivedSubmissionsDataService(
            IArchivesGenericRepository<ArchivedSubmission> archivedSubmissions) =>
                this.archivedSubmissions = archivedSubmissions;

        public IQueryable<ArchivedSubmission> GetAllUndeletedFromMainDatabase() =>
            this.archivedSubmissions
                .All()
                .Where(s => !s.IsHardDeletedFromMainDatabase);

        public int Add(IEnumerable<ArchivedSubmission> entities)
        {
            var entitiesList = entities.ToList();
            var ids = entitiesList
                .Select(x => x.Id)
                .ToSet();
            
            var existingEntities = this.archivedSubmissions.All()
                .Where(x => ids.Contains(x.Id))
                .Select(x => x.Id)
                .ToSet();
            
            var entitiesToAdd = entitiesList
                .Where(x => !existingEntities.Contains(x.Id))
                .ToList();

            this.archivedSubmissions.Add(entitiesToAdd);
            return entitiesToAdd.Count;
        }

        public void SetToHardDeletedFromMainDatabaseByIds(IEnumerable<int> ids)
            => this.archivedSubmissions.Update(
                s => ids.Contains(s.Id),
                s => new ArchivedSubmission
                {
                    IsHardDeletedFromMainDatabase = true
                },
                batchSize: GlobalConstants.BatchOperationsChunkSize);

        public void CreateDatabaseIfNotExists() =>
            this.archivedSubmissions.CreateDatabaseIfNotExists();
    }
}