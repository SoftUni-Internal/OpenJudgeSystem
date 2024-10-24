namespace OJS.Services.Ui.Business.Cache;

using OJS.Services.Common.Models.Cache;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure;
using OJS.Services.Ui.Models.Contests;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContestCategoryListViewModel = OJS.Services.Common.Models.Cache.ContestCategoryListViewModel;

public interface IContestCategoriesCacheService : IService
{
    Task<IEnumerable<ContestCategoryTreeViewModel>> GetContestSubCategoriesList(
        int categoryId,
        int? cacheSeconds = CacheConstants.OneDayInSeconds);

    Task<IEnumerable<ContestCategoryListViewModel>> GetContestCategoryParentsList(
        int categoryId,
        int? cacheSeconds = CacheConstants.OneDayInSeconds);

    Task<IEnumerable<ContestCategoryListViewModel>> GetMainContestCategories(
        int? cacheSeconds = CacheConstants.OneDayInSeconds);

    Task<IEnumerable<ContestCategoryTreeViewModel>> GetAllContestCategoriesTree(
        int? cacheSeconds = CacheConstants.OneHourInSeconds);

    Task<ContestCategoryServiceModel?> GetById(int? categoryId, int? cacheSeconds = CacheConstants.OneHourInSeconds);
}