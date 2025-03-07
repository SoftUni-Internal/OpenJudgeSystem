namespace OJS.Services.Ui.Data
{
    using OJS.Data.Models.Contests;
    using OJS.Services.Common.Data;
    using OJS.Services.Ui.Models.Contests;
    using OJS.Services.Infrastructure.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IContestsDataService : IDataService<Contest>
    {
        Task<PagedResult<TServiceModel>> ApplyFiltersSortAndPagination<TServiceModel>(
            IQueryable<Contest> contests,
            ContestFiltersServiceModel model);

        Task<IEnumerable<TServiceModel>> GetAllCompetable<TServiceModel>();

        Task<IEnumerable<TServiceModel>> GetAllExpired<TServiceModel>();

        Task<PagedResult<TServiceModel>> GetAllAsPageByFiltersAndSorting<TServiceModel>(ContestFiltersServiceModel model);

        IQueryable<Contest> GetLatestForParticipantByUsername(string username);

        IQueryable<Contest> GetAllVisible();

        Task<int> GetMaxPointsForExportById(int id);

        Task<bool> IsUserInExamGroupByContestAndUser(int id, string userId);
    }
}