namespace OJS.Services.Administration.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using OJS.Data.Models.Contests;
    using OJS.Services.Common.Data;

    public interface IContestCategoriesDataService : IDataService<ContestCategory>
    {
        IQueryable<ContestCategory> GetAllVisible();

        IQueryable<ContestCategory> GetAllVisibleAndNotDeleted();

        IQueryable<ContestCategory> GetAllVisibleByLecturer(string? lecturerId);

        Task<ContestCategory> GetById(int? id);

        Task<ContestCategory?> GetByIdWithParent(int? id);

        Task<string?> GetNameById(int id);

        Task<bool> HasContestsById(int id);

        Task<IEnumerable<ContestCategory>> GetContestCategoriesByParentId(int? parentId);

        Task<IEnumerable<ContestCategory>> GetContestCategoriesWithoutParent();

        Task<bool> UserHasContestCategoryPermissions(int categoryId, string? userId, bool isAdmin);

        Task LoadChildrenRecursively(ContestCategory category);
    }
}