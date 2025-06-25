namespace OJS.Services.Administration.Business.ProblemResources.Permissions;

using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Problems;
using OJS.Services.Administration.Business.Contests;
using OJS.Services.Administration.Data;
using OJS.Services.Common.Models.Users;
using System.Linq;
using System.Threading.Tasks;
using OJS.Data.Models.Resources;

public class ResourceIdPermissionService : IEntityPermissionsService<Resource, int>
{
    private readonly IProblemResourcesDataService problemResourcesDataService;
    private readonly IContestsBusinessService contestsBusinessService;

    public ResourceIdPermissionService(
        IProblemResourcesDataService problemResourcesDataService,
        IContestsBusinessService contestsBusinessService)
    {
        this.problemResourcesDataService = problemResourcesDataService;
        this.contestsBusinessService = contestsBusinessService;
    }

    public async Task<bool> HasPermission(UserInfoModel user, int value, string operation)
    {
        var contestId = await this.problemResourcesDataService
            .GetByIdQuery(value).Select(pr => pr.Problem.ProblemGroup.ContestId)
            .FirstOrDefaultAsync();

        return await this.contestsBusinessService.UserHasContestPermissions(
            contestId,
            user.Id,
            user.IsAdmin);
    }
}