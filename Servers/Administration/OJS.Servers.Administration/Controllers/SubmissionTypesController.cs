﻿namespace OJS.Servers.Administration.Controllers;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OJS.Data.Models;
using OJS.Data.Models.Submissions;
using OJS.Servers.Administration.Attributes;
using OJS.Services.Administration.Business.SubmissionTypes;
using OJS.Services.Administration.Business.SubmissionTypes.Validators;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.SubmissionTypes;
using OJS.Services.Common.Data;
using OJS.Workers.Common.Models;

using static OJS.Common.GlobalConstants.Roles;

public class SubmissionTypesController : BaseAdminApiController<SubmissionType, int, SubmissionTypeInListModel, SubmissionTypeAdministrationModel>
{
    private readonly ISubmissionTypesBusinessService submissionTypesBusinessService;

    public SubmissionTypesController(
        ISubmissionTypesBusinessService submissionTypesBusinessService,
        IGridDataService<SubmissionType> submissionTypesGridDataService,
        SubmissionTypeAdministrationModelValidator validator,
        IDataService<AccessLog> accessLogsData)
            : base(
                submissionTypesGridDataService,
                submissionTypesBusinessService,
                validator,
                accessLogsData) =>
        this.submissionTypesBusinessService = submissionTypesBusinessService;

    [HttpGet]
    public async Task<IActionResult> GetForProblem()
        => this.Ok(await this.submissionTypesBusinessService.GetForProblem());

    [HttpPost]
    [Authorize(Roles = Administrator)]
    [Authorize(Roles = Developer)]
    [ProtectedEntityAction(isRestricted: false)]
    public async Task<IActionResult> ReplaceSubmissionTypes(ReplaceSubmissionTypeServiceModel model)
    {
        var stringResult = await this.submissionTypesBusinessService.ReplaceSubmissionType(model);

        return this.Ok(stringResult);
    }

    [HttpGet]
    public async Task<IActionResult> GetForDocument()
        => this.Ok(await this.submissionTypesBusinessService.GetForDocument());

    [HttpGet]
    public IActionResult GetCompilers()
        => this.Ok(Enum.GetNames(typeof(CompilerType)).ToList());

    [HttpGet]
    public IActionResult GetExecutionStrategies()
        => this.Ok(Enum.GetNames(typeof(ExecutionStrategyType)).ToList());
}