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
using OJS.Services.Common.Data;

public class ResourcesController : BaseAdminApiController<Resource, int, ResourceInListModel, ResourceAdministrationModel>
{
    private readonly IResourcesBusinessService resourcesBusinessService;

    public ResourcesController(
        IResourcesGridDataService resourceGridService,
        IResourcesBusinessService resourcesBusinessService,
        ResourceAdministrationModelValidator modelValidator,
        IDataService<AccessLog> accessLogsData)
        : base(resourceGridService, resourcesBusinessService, modelValidator, accessLogsData) =>
        this.resourcesBusinessService = resourcesBusinessService;

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
}