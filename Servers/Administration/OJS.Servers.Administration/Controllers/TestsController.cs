﻿namespace OJS.Servers.Administration.Controllers;

using Microsoft.AspNetCore.Mvc;
using OJS.Common;
using OJS.Data.Models.Tests;
using OJS.Servers.Administration.Attributes;
using OJS.Services.Administration.Business.Problems.Permissions;
using OJS.Services.Administration.Business.Tests;
using OJS.Services.Administration.Business.Tests.GridData;
using OJS.Services.Administration.Business.Tests.Permissions;
using OJS.Services.Administration.Business.Tests.Validators;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models;
using OJS.Services.Administration.Models.TestRuns;
using OJS.Services.Administration.Models.Tests;
using OJS.Services.Common.Models.Pagination;
using System.Threading.Tasks;
using OJS.Data.Models;
using OJS.Services.Common.Data;

public class TestsController : BaseAdminApiController<Test, int, TestsInListModel, TestAdministrationModel>
{
    private readonly IGridDataService<Test> testGridDataService;
    private readonly ITestsBusinessService testsBusinessService;
    private readonly IGridDataService<TestRun> testRunsGridDataService;

    public TestsController(
        ITestsGridDataService testGridDataService,
        ITestsBusinessService testsBusinessService,
        TestAdministrationModelValidator validator,
        IGridDataService<TestRun> testRunsGridDataService,
        IDataService<AccessLog> accessLogsData)
        : base(
            testGridDataService,
            testsBusinessService,
            validator,
            accessLogsData)
    {
        this.testGridDataService = testGridDataService;
        this.testsBusinessService = testsBusinessService;
        this.testRunsGridDataService = testRunsGridDataService;
    }

    [HttpGet("{problemId:int}")]
    [ProtectedEntityAction("problemId", typeof(ProblemIdPermissionsService))]
    public async Task<IActionResult> GetByProblemId([FromQuery] PaginationRequestModel model, [FromRoute] int problemId)
        => this.Ok(
            await this.testGridDataService.GetAll<TestsInListModel>(
                model,
                test => test.Problem.Id == problemId));

    [HttpDelete("{problemId:int}")]
    [ProtectedEntityAction("problemId", typeof(ProblemIdPermissionsService))]
    public async Task<IActionResult> DeleteAll(int problemId)
    {
        await this.testsBusinessService.DeleteAll(problemId);
        return this.Ok($"Successfully deleted tests.");
    }

    [HttpPost]
    [ProtectedEntityAction("model", typeof(TestsImportPermissionService))]
    public async Task<IActionResult> Import([FromForm]TestsImportRequestModel model)
        => this.Ok(await this.testsBusinessService.Import(model));

    [HttpGet("{problemId:int}")]
    [ProtectedEntityAction("problemId", typeof(ProblemIdPermissionsService))]
    public async Task<IActionResult> ExportZip(int problemId)
    {
        var file = await this.testsBusinessService.ExportZip(problemId);
        return this.File(file.Content!, GlobalConstants.MimeTypes.ApplicationOctetStream, file.FileName);
    }

    [HttpGet("{id:int}")]
    [ProtectedEntityAction("id", AdministrationConstants.AdministrationOperations.Read)]
    public async Task<IActionResult> GetTestRunsByTestId([FromQuery] PaginationRequestModel model, [FromRoute] int id)
        => this.Ok(await this.testRunsGridDataService.GetAll<TestRunInListModel>(model, tr => tr.TestId == id));
}