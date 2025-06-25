namespace OJS.Servers.Ui.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OJS.Servers.Infrastructure.Controllers;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Servers.Ui.Models;
using OJS.Servers.Ui.Models.Contests;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Ui.Business;
using OJS.Services.Ui.Models.Contests;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static OJS.Servers.Infrastructure.ServerConstants.Authorization;

public class ContestsController(
    IContestsBusinessService contestsBusinessService,
    ILogger<ContestsController> logger)
    : BaseApiController
{
    /// <summary>
    /// Gets details of the current contest.
    /// </summary>
    /// <param name="id">ID of the contest.</param>
    /// <returns>Model containing information about the name, description and problems of the contest.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContestDetailsServiceModel), Status200OK)]
    public async Task<IActionResult> Details(int id)
        => await contestsBusinessService
            .GetContestDetails(id)
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
        => await contestsBusinessService
            .ValidateContestPassword(id, official, model.Password)
            .ToActionResult(logger);

    /// <summary>
    /// Gets contests summary with latest active and past contests for the home page.
    /// </summary>
    /// <returns>A collection of active and past contests.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ContestsForHomeIndexResponseModel), Status200OK)]
    public async Task<IActionResult> GetForHomeIndex()
        => await contestsBusinessService
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
        => await contestsBusinessService
            .GetAllByFiltersAndSorting(model?.Map<ContestFiltersServiceModel>())
            .Map<PagedResultResponse<ContestForListingResponseModel>>()
            .ToOkResult();

    /// <summary>
    /// Gets all user contest participations.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="model">The filters by which the contests should be filtered and page options.</param>
    /// <param name="contestId">The contest based on which the result will be filtered.</param>
    /// <param name="categoryId">The category based on which the result will be filtered.</param>
    /// <returns>A collection of contest participations.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResultResponse<ContestForListingResponseModel>), Status200OK)]
    public async Task<IActionResult> GetParticipatedByUser(
        [FromQuery] string username,
        [FromQuery] ContestFiltersRequestModel? model,
        [FromQuery] int? contestId,
        [FromQuery] int? categoryId)
        => await contestsBusinessService
            .GetParticipatedByUserByFiltersAndSorting(username, model?.Map<ContestFiltersServiceModel>(), contestId, categoryId)
            .Map<PagedResultResponse<ContestForListingResponseModel>>()
            .ToOkResult();

    /// <summary>
    /// Gets the emails of all participants in a given contest.
    /// </summary>
    /// <param name="contestId">The id of the contest.</param>
    /// <returns>Results in json format.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<string?>), Status200OK)]
    [Authorize(ApiKeyPolicyName)]
    public async Task<IActionResult> GetEmailsOfParticipantsInContest([Required] int contestId)
        => await contestsBusinessService
            .GetEmailsOfParticipantsInContest(contestId)
            .ToOkResult();

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ContestForListingResponseModel>), Status200OK)]
    public async Task<IActionResult> GetAllParticipatedContests(
        [FromQuery] string username)
        => await contestsBusinessService
            .GetAllParticipatedContests(username)
            .Map<IEnumerable<ContestForListingResponseModel>>()
            .ToOkResult();
}