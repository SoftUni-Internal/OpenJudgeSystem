﻿namespace OJS.Servers.Ui.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OJS.Common.Enumerations;
using OJS.Servers.Infrastructure.Controllers;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Servers.Ui.Models;
using OJS.Servers.Ui.Models.Submissions.Details;
using OJS.Services.Common;
using OJS.Services.Common.Models.Submissions;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Ui.Business;
using OJS.Services.Ui.Models.Submissions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using OJS.Services.Common.Models.Pagination;
using OJS.Services.Infrastructure.Models;
using static OJS.Common.GlobalConstants.MimeTypes;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static OJS.Common.GlobalConstants.Roles;

public class SubmissionsController(
    ISubmissionsBusinessService submissionsBusiness,
    IFileSystemService fileSystem,
    ILogger<SubmissionsController> logger)
    : BaseApiController
{
    /// <summary>
    /// Gets all user submissions. Prepared for the user's profile page.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FullDetailsPublicSubmissionsResponseModel>), Status200OK)]
    public async Task<IActionResult> GetUserSubmissions(
        [FromQuery] string username,
        [FromQuery] PaginationRequestModel requestModel)
        => await submissionsBusiness
            .GetByUsername<FullDetailsPublicSubmissionsServiceModel>(username, requestModel)
            .Map<PagedResultResponse<FullDetailsPublicSubmissionsResponseModel>>()
            .ToOkResult();

    /// <summary>
    /// Gets the submitted file.
    /// </summary>
    /// <param name="id">Id of the submission.</param>
    /// <returns>The file to download.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FileContentResult), Status200OK)]
    [SuppressMessage(
        "Usage",
        "ASP0023:Route conflict detected between controller actions",
        Justification = "Base API controller Route handles different actions names.")]
    public async Task<IActionResult> Download(int id)
    {
        var submissionDownloadServiceModel = await submissionsBusiness.GetSubmissionFile(id);

        return this.File(
            submissionDownloadServiceModel.Content!,
            submissionDownloadServiceModel.MimeType!,
            submissionDownloadServiceModel.FileName);
    }

    /// <summary>
    /// Gets a subset of submissions by specific problem.
    /// </summary>
    /// <param name="problemId">The id of the problem.</param>
    /// <param name="isOfficial">Should the submissions be only from compete mode.</param>
    /// <param name="requestModel">Contains information regarding the current request - filter, sorter, page, itemsPerPage, etc.</param>
    /// <returns>A collection of submissions for a specific problem.</returns>
    [HttpGet("{problemId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResultResponse<FullDetailsPublicSubmissionsResponseModel>), Status200OK)]
    [SuppressMessage(
        "Usage",
        "ASP0023:Route conflict detected between controller actions",
        Justification = "Base API controller Route handles different actions names.")]
    public async Task<IActionResult> GetUserSubmissionsByProblem(
        int problemId,
        [FromQuery] bool isOfficial,
        [FromQuery] PaginationRequestModel requestModel)
        => await submissionsBusiness
            .GetUserSubmissionsByProblem(problemId, isOfficial, requestModel)
            .Map<ServiceResult<PagedResultResponse<FullDetailsPublicSubmissionsResponseModel>>>()
            .ToActionResult(logger);

    /// <summary>
    /// Gets submission details by provided submission id.
    /// </summary>
    /// <param name="id">The id of the submission.</param>
    /// <returns>Submission details model.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SubmissionDetailsResponseModel), Status200OK)]
    public async Task<IActionResult> Details(int id)
        => await submissionsBusiness
            .GetDetailsById(id)
            .Map<SubmissionDetailsResponseModel>()
            .ToOkResult();

    /// <summary>
    /// Saves/updates the provided execution result for the given submission in the database.
    /// </summary>
    /// <param name="submissionExecutionResult">The submission execution result.</param>
    /// <returns>Success model.</returns>
    /// <remarks>
    /// The submission comes from the RabbitMQ execution result queue.
    /// It sends it to here after executing it on a remote worker.
    /// </remarks>
    [HttpPost("/Submissions/SaveExecutionResult")]
    [ProducesResponseType(typeof(SaveExecutionResultResponseModel), Status200OK)]
    public async Task<IActionResult> SaveExecutionResult([FromBody] SubmissionExecutionResult submissionExecutionResult)
    {
        await submissionsBusiness.ProcessExecutionResult(submissionExecutionResult);

        var result = new SaveExecutionResultResponseModel { SubmissionId = submissionExecutionResult.SubmissionId, };

        return this.Ok(result);
    }

    /// <summary>
    /// Gets the count of all submissions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(int), Status200OK)]
    public async Task<IActionResult> TotalCount()
        => await submissionsBusiness
            .GetTotalCount()
            .ToOkResult();

    /// <summary>
    /// Gets the count of all unprocessed submissions.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Administrator)]
    [ProducesResponseType(typeof(Dictionary<SubmissionProcessingState, int>), Status200OK)]
    public async Task<IActionResult> UnprocessedTotalCount()
        => await submissionsBusiness
            .GetAllUnprocessedCount()
            .ToOkResult();

    // Unify (Public, GetProcessingSubmissions, GetPendingSubmissions) endpoints for Submissions into single one.
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultResponse<PublicSubmissionsResponseModel>), Status200OK)]
    public async Task<IActionResult> GetSubmissions([FromQuery] SubmissionStatus status, [FromQuery] PaginationRequestModel requestModel)
         => await submissionsBusiness.GetSubmissions<PublicSubmissionsServiceModel>(status, requestModel)
             .Map<PagedResultResponse<PublicSubmissionsResponseModel>>()
             .ToOkResult();

    [HttpGet]
    [Authorize(Roles = AdministratorOrLecturer)]
    [ProducesResponseType(typeof(PagedResultResponse<FullDetailsPublicSubmissionsResponseModel>), Status200OK)]
    public async Task<IActionResult> GetSubmissionsForUserInRole(
        [FromQuery] SubmissionStatus status,
        [FromQuery] PaginationRequestModel requestModel)
        => await submissionsBusiness.GetSubmissions<FullDetailsPublicSubmissionsServiceModel>(
                status,
                requestModel)
            .Map<PagedResultResponse<FullDetailsPublicSubmissionsResponseModel>>()
            .ToOkResult();

    /// <summary>
    /// Downloads the logs for a specific submission, if they exist.
    /// </summary>
    /// <param name="id">The id of the submission</param>
    /// <returns>The log file for the submission</returns>
    [HttpGet("{id:int}")]
    [Authorize(Roles = AdministratorOrLecturer)]
    [ProducesResponseType(typeof(FileContentResult), Status200OK)]
    public IActionResult DownloadLogs(int id)
    {
        var filePath = submissionsBusiness.GetLogFilePath(id);

        return fileSystem.FileExists(filePath)
            ? this.PhysicalFile(filePath, ApplicationOctetStream)
            : this.NotFound(
                $"Logs for submission #{id} not found. " +
                $"Execute or retest the submission in verbose mode to generate logs, " +
                $"then download them immediately, as they might be deleted soon.");
    }
}