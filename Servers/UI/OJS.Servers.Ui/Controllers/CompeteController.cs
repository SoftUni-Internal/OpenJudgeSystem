namespace OJS.Servers.Ui.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OJS.Servers.Infrastructure.Controllers;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Servers.Infrastructure.Telemetry;
using OJS.Servers.Ui.Models;
using OJS.Servers.Ui.Models.Submissions.Compete;
using OJS.Services.Common.Telemetry;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Ui.Business;
using OJS.Services.Ui.Models.Contests;
using OJS.Services.Ui.Models.Submissions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Http.StatusCodes;

[Authorize]
[Route("api/[controller]")]
public class CompeteController(
    IContestsBusinessService contestsBusiness,
    ISubmissionsBusinessService submissionsBusinessService,
    ITracingService tracing,
    ILogger<CompeteController> logger)
    : BaseApiController
{
    /// <summary>
    /// Starts a contest for the user. Creates participant and starts time counter.
    /// </summary>
    /// <param name="id">The id of the contest.</param>
    /// <param name="isOfficial">Is the contest compete or practice.</param>
    /// <returns>The new participant.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ContestParticipationServiceModel), Status200OK)]
    public async Task<IActionResult> Index(int id, [FromQuery] bool isOfficial)
        => await contestsBusiness
            .GetParticipationDetails(new StartContestParticipationServiceModel
            {
                ContestId = id,
                IsOfficial = isOfficial,
            })
            .ToActionResult(logger);

    /// <summary>
    /// This endpoint retrieves registration details for a specified contest and user.
    /// It considers whether the contest is an official entry and includes various checks and conditions
    /// based on the user's status and contest rules.
    /// </summary>
    /// <param name="id">Contest id.</param>
    /// <param name="isOfficial">Compete/practice.</param>
    /// <returns>Success status code.</returns>
    /// <returns>403 if user cannot compete contest.</returns>
    [HttpGet("{id:int}/register")]
    public async Task<IActionResult> Register(int id, [FromQuery] bool isOfficial)
        => await contestsBusiness
            .GetContestRegistrationDetails(id, isOfficial)
            .ToActionResult(logger);

    /// <summary>
    /// Registers user for contest. If a password is submitted it gets validated. This endpoint creates a participant.
    /// </summary>
    /// <param name="id">Contest id.</param>
    /// <param name="isOfficial">Compete/practice.</param>
    /// <param name="model">Contains contest password.</param>
    /// <returns>Success status code.</returns>
    /// <returns>401 for invalid password.</returns>
    /// <returns>403 if user cannot compete contest.</returns>
    [HttpPost("{id:int}/register")]
    public async Task<IActionResult> Register(
        int id,
        [FromQuery] bool isOfficial,
        [FromBody] ContestRegisterRequestModel model)
    {
        try
        {
            var isValidRegistration = await contestsBusiness
                .RegisterUserForContest(id, model.Password, model.HasConfirmedParticipation, isOfficial);

            return this.Ok(new { IsRegisteredSuccessFully = isValidRegistration });
        }
        catch (UnauthorizedAccessException uae)
        {
            return this.Unauthorized(uae.Message);
        }
        catch (BusinessServiceException be)
        {
            return this.StatusCode((int)HttpStatusCode.Forbidden, be.Message);
        }
    }

    /// <summary>
    /// Submits user's code for evaluation.
    /// </summary>
    /// <param name="model">The submission model containing the code and execution context.</param>
    /// <returns>Success status code.</returns>
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmissionRequestModel model)
        => await tracing.TraceAsync(
            OjsActivitySources.submissions,
            OjsActivitySources.SubmissionActivities.Received,
            async activity =>
            {
                var serviceModel = model.Map<SubmitSubmissionServiceModel>();

                tracing.AddTechnicalContext(activity!, "submit_code", "ui_controller");

                return await submissionsBusinessService
                    .Submit(serviceModel)
                    .ToActionResult(logger);
            },
            new Dictionary<string, object?>
            {
                ["submission.content_length"] = model.Content?.Length,
            },
            BusinessContext.ForSubmission(0, model.ProblemId, model.ContestId));

    /// <summary>
    /// Submits user's code in fle format (zip) for evaluation.
    /// </summary>
    /// <param name="model">The submission model containing the code and execution context.</param>
    /// <returns>Success status code.</returns>
    [HttpPost("submitfilesubmission")]
    public async Task<IActionResult> SubmitFileSubmission([FromForm] SubmitFileSubmissionRequestModel model)
        => await tracing.TraceAsync(
            OjsActivitySources.submissions,
            OjsActivitySources.SubmissionActivities.Received,
            async activity =>
            {
                var serviceModel = model.Map<SubmitSubmissionServiceModel>();

                tracing.AddTechnicalContext(activity!, "submit_file", "ui_controller");

                return await submissionsBusinessService
                    .Submit(serviceModel)
                    .ToActionResult(logger);
            },
            new Dictionary<string, object?>
            {
                ["submission.content_length"] = model.Content?.Length,
            },
            BusinessContext.ForSubmission(0, model.ProblemId, model.ContestId));

    /// <summary>
    /// Triggers a retest of a user's submission by putting a message on a queue for background processing.
    /// The retest permissions will be validated based on certain criteria:
    /// For administrator users or lecturers in a contest, the submission will always be retested.
    /// For regular users, if the user is the participant attached to the submission and if the tests have changed.
    /// </summary>
    /// <param name="id">The submission id to be retested.</param>
    /// <param name="verbosely">Whether to include verbose execution details.</param>
    /// <returns>Success status code.</returns>
    [HttpPost("retest/{id:int}")]
    public async Task<IActionResult> Retest(int id, bool verbosely = false)
        => await submissionsBusinessService
            .Retest(id, verbosely)
            .ToOkResult();
}
