namespace OJS.Servers.Administration.Controllers;

using Microsoft.AspNetCore.Mvc;
using OJS.Servers.Administration.Attributes;
using OJS.Services.Administration.Business.ProblemResources;
using OJS.Services.Administration.Business.ProblemResources.Permissions;
using OJS.Services.Administration.Business.ProblemResources.Validators;
using OJS.Services.Administration.Business.ProblemResources.GridData;
using OJS.Services.Administration.Models.ProblemResources;
using System.Threading.Tasks;
using OJS.Data.Models;
using OJS.Data.Models.Resources;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.Resources;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Pagination;
using OJS.Services.Common.Models.Users;
using OJS.Services.Infrastructure.Extensions;

public class ResourcesController : BaseAdminApiController<Resource, int, ResourceInListModel, ResourceAdministrationModel>
{
    private readonly IResourcesBusinessService resourcesBusinessService;
    private readonly IGridDataService<ContestResource> contestResourcesGridDataService;
    private readonly IGridDataService<ProblemResource> problemResourcesGridDataService;

    public ResourcesController(
        IResourcesGridDataService resourceGridService,
        IResourcesBusinessService resourcesBusinessService,
        ResourceAdministrationModelValidator modelValidator,
        IDataService<AccessLog> accessLogsData,
        IGridDataService<ContestResource> contestResourcesGridDataService,
        IGridDataService<ProblemResource> problemResourcesGridDataService)
        : base(resourceGridService, resourcesBusinessService, modelValidator, accessLogsData)
    {
        this.resourcesBusinessService = resourcesBusinessService;
        this.contestResourcesGridDataService = contestResourcesGridDataService;
        this.problemResourcesGridDataService = problemResourcesGridDataService;
    }

    [HttpGet("{id:int}")]
    [ProtectedEntityAction("id", typeof(ResourceIdPermissionService))]
    public async Task<IActionResult> Download(int id)
    {
        var model = await this.resourcesBusinessService.GetResourceFile(id);

        return this.File(model.Content!, model.MimeType!, model.FileName);
    }

    public override async Task<IActionResult> Create([FromForm] ResourceAdministrationModel model)
    {
        var response = await base.Create(model);
        return response;
    }

    public override async Task<IActionResult> Edit([FromForm] ResourceAdministrationModel model)
    {
        var response = await base.Edit(model);
        return response;
    }

    [HttpGet]
    [ProtectedEntityAction(false)]
    public async Task<IActionResult> GetAllContestResources([FromQuery] PaginationRequestModel model)
    {
        var user = this.User.Map<UserInfoModel>();


        if (!await this.contestResourcesGridDataService.UserHasAccessToGrid(user))
        {
            return this.Unauthorized();
        }

        return this.Ok(await this.contestResourcesGridDataService.GetAllForUser<ContestResourceInListModel>(model, user));
    }

    [HttpGet]
    [ProtectedEntityAction(false)]
    public async Task<IActionResult> GetAllProblemResources([FromQuery] PaginationRequestModel model)
    {
        var user = this.User.Map<UserInfoModel>();


        if (!await this.contestResourcesGridDataService.UserHasAccessToGrid(user))
        {
            return this.Unauthorized();
        }

        return this.Ok(await this.problemResourcesGridDataService.GetAllForUser<ProblemResourceInListModel>(model, user));
    }
}