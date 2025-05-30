namespace OJS.Servers.Ui.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OJS.Servers.Infrastructure.Controllers;
using OJS.Services.Ui.Business;
using System;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static OJS.Common.GlobalConstants.HeaderKeys;
using static OJS.Common.GlobalConstants.HeaderValues;
using static OJS.Common.GlobalConstants.MimeTypes;

public class ResourcesController(IResourcesBusinessService resourcesBusinessService)
    : BaseApiController
{
    /// <summary>
    /// Gets problem resource file, by provided resource id.
    /// </summary>
    /// <param name="id">The problem resource id.</param>
    /// <returns>A file with the problem resource.</returns>
    [HttpGet("{id:int}")]
    [Authorize]
    [Produces(ApplicationOctetStream)]
    [ProducesResponseType(typeof(FileContentResult), Status200OK)]
    public async Task<IActionResult> GetResource(int id)
    {
        var resource = await resourcesBusinessService.GetResource(id);

        this.Response.Headers.Append(ContentDisposition, $"{ContentDispositionFileNameUtf8}{Uri.EscapeDataString(resource.Name)}.{resource.FileExtension}");
        return this.File(resource.File!, ApplicationOctetStream);
    }
}