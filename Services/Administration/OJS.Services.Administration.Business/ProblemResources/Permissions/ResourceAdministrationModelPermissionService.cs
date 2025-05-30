namespace OJS.Services.Administration.Business.ProblemResources.Permissions;

using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Problems;
using OJS.Services.Administration.Business.Contests;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.ProblemResources;
using OJS.Services.Common.Models.Users;
using System.Linq;
using System.Threading.Tasks;
using OJS.Data.Models.Resources;

public class ResourceAdministrationModelPermissionService : IEntityPermissionsService<Resource, ResourceAdministrationModel>
{
    private readonly IProblemsDataService problemsDataService;
    private readonly IContestsBusinessService contestsBusinessService;

    public ResourceAdministrationModelPermissionService(
        IProblemsDataService problemsDataService,
        IContestsBusinessService contestsBusinessService)
    {
        this.problemsDataService = problemsDataService;
        this.contestsBusinessService = contestsBusinessService;
    }

    public async Task<bool> HasPermission(UserInfoModel user, ResourceAdministrationModel model, string operation)
    {
        var contestId = model.ResourceType == nameof(ProblemResource)
            ? await this.problemsDataService.GetByIdQuery(model.ParentId)
                .Select(p => p.ProblemGroup.ContestId)
                .FirstOrDefaultAsync()
            : model.ParentId;

        return await this.contestsBusinessService.UserHasContestPermissions(
            contestId,
            user.Id,
            user.IsAdmin);
    }
}