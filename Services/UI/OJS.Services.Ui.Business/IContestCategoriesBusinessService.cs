namespace OJS.Services.Ui.Business;

using OJS.Services.Common.Models.Cache;
using OJS.Services.Infrastructure;
using OJS.Services.Ui.Models.Contests;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContestCategoryListViewModel = OJS.Services.Common.Models.Cache.ContestCategoryListViewModel;

public interface IContestCategoriesBusinessService : IService
{
    Task<IEnumerable<ContestCategoryTreeViewModel>> GetTree();

    Task<IEnumerable<ContestCategoryListViewModel>> GetAllMain();

    Task<IEnumerable<ContestCategoryTreeViewModel>> GetAllSubcategories(int categoryId);

    Task<IEnumerable<ContestCategoryListViewModel>> GetAllParentCategories(int categoryId);

    Task<ContestCategoryServiceModel?> GetById(int categoryId);
}