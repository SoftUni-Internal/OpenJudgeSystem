namespace OJS.Services.Administration.Business.Contests.Permissions;

using System.Threading.Tasks;
using OJS.Data.Models.Contests;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.Contests;
using OJS.Services.Common.Models.Users;

public class ContestsBulkEditPermissionsService : IEntityPermissionsService<Contest, ContestsBulkEditModel>
{
    private readonly IContestCategoriesDataService contestCategoriesDataService;

    public ContestsBulkEditPermissionsService(
        IContestCategoriesDataService contestCategoriesDataService)
        => this.contestCategoriesDataService = contestCategoriesDataService;

    public Task<bool> HasPermission(UserInfoModel user, ContestsBulkEditModel value, string operation)
        => this.contestCategoriesDataService.UserHasContestCategoryPermissions(value.CategoryId, user.Id, user.IsAdmin);
}