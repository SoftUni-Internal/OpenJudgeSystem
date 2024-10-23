namespace OJS.Servers.Ui.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OJS.Servers.Infrastructure.Controllers;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Servers.Ui.Models;
using OJS.Servers.Ui.Models.Contests;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Ui.Business;
using OJS.Services.Ui.Models.Contests;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

public class ContestsController : BaseApiController
{
    private readonly IContestsBusinessService contestsBusinessService;

    public ContestsController(IContestsBusinessService contestsBusinessService)
        => this.contestsBusinessService = contestsBusinessService;

    /// <summary>
    /// Gets details of the current contest.
    /// </summary>
    /// <param name="id">ID of the contest.</param>
    /// <returns>Model containing information about the name, description and problems of the contest.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContestDetailsServiceModel), Status200OK)]
    public async Task<IActionResult> Details(int id)
        => await this.contestsBusinessService
            .GetContestDetails(id)
            .ToOkResult();

    // /// <summary>
    // /// Gets details of a contest by id.
    // /// </summary>
    // /// <param name="id">ID of the contest.</param>
    // /// <returns>Model containing all information about the name, description and problems of the contest.</returns>
    [HttpPost("{id:int}")]
    [ProducesResponseType(typeof(ContestLegacyExportServiceModel), Status200OK)]
    public async Task<IActionResult> Export(int id)
        => await this.contestsBusinessService
            .Export(id)
            .ToOkResult();

    [HttpPost]
    public async Task<IActionResult> GetExistingIds(ContestsGetExistingIdsRequestModel requestModel)
    {
        var existingContestIds = await this.contestsBusinessService.GetExistingIds(requestModel.Ids);

        return this.Content(JsonConvert.SerializeObject(existingContestIds));
    }

    /// <summary>
    /// Gets the contest that the problem is part of.
    /// </summary>
    /// <param name="problemId">The id of the problem that is used to find the contest.</param>
    /// <returns>Model containing information about the contest.</returns>
    [HttpGet("{problemId:int}")]
    [ProducesResponseType(typeof(ContestServiceModel), Status200OK)]
    [SuppressMessage(
        "Usage",
        "ASP0023:Route conflict detected between controller actions",
        Justification = "Base API controller Route handles different actions names.")]
    public async Task<IActionResult> GetByProblem(int problemId)
        => await this.contestsBusinessService
            .GetContestByProblem(problemId)
            .ToOkResult();

    /// <summary>
    /// Submits a password value from the user and validates it against contest configurations.
    /// </summary>
    /// <param name="id">ID of the contest.</param>
    /// <param name="official">Practice or compete mode of the contest.</param>
    /// <param name="model">The password the user has submitted.</param>
    /// <returns>Ok result if password is correct and an exception if otherwise.</returns>
    [HttpPost("{id:int}")]
    public async Task<IActionResult> SubmitContestPassword(
        int id,
        [FromQuery] bool official,
        [FromBody] SubmitContestPasswordRequestModel model)
        => await this.contestsBusinessService
            .ValidateContestPassword(id, official, model.Password)
            .ToOkResult();

    /// <summary>
    /// Gets contests summary with latest active and past contests for the home page.
    /// </summary>
    /// <returns>A collection of active and past contests.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ContestsForHomeIndexResponseModel), Status200OK)]
    public async Task<IActionResult> GetForHomeIndex()
        => await this.contestsBusinessService
            .GetAllForHomeIndex()
            .Map<ContestsForHomeIndexResponseModel>()
            .ToOkResult();

    /// <summary>
    /// Gets a page with visible contests, by applied filters.
    /// If no page options are provided, default values are applied.
    /// </summary>
    /// <param name="model">The filters by which the contests should be filtered and page options.</param>
    /// <returns>A page with contests, filtered by provided filters.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultResponse<ContestForListingResponseModel>), Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ContestFiltersRequestModel? model)
        => await this.contestsBusinessService
            .GetAllByFiltersAndSorting(model?.Map<ContestFiltersServiceModel>())
            .Map<PagedResultResponse<ContestForListingResponseModel>>()
            .ToOkResult();

    /// <summary>
    /// Gets all user contest participations.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="model">The filters by which the contests should be filtered and page options.</param>
    /// <returns>A collection of contest participations.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResultResponse<ContestForListingResponseModel>), Status200OK)]
    public async Task<IActionResult> GetParticipatedByUser(
        [FromQuery] string username,
        [FromQuery] ContestFiltersRequestModel? model)
        => await this.contestsBusinessService
            .GetParticipatedByUserByFiltersAndSorting(username, model?.Map<ContestFiltersServiceModel>())
            .Map<PagedResultResponse<ContestForListingResponseModel>>()
            .ToOkResult();
}