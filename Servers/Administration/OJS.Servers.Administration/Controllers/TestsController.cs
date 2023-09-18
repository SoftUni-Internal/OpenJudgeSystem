namespace OJS.Servers.Administration.Controllers;

using OJS.Common.Utils;
using AutoCrudAdmin.Enumerations;
using AutoCrudAdmin.Extensions;
using AutoCrudAdmin.Models;
using AutoCrudAdmin.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OJS.Common.Helpers;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Tests;
using OJS.Servers.Administration.Infrastructure.Extensions;
using OJS.Servers.Administration.Models.Tests;
using OJS.Services.Administration.Business;
using OJS.Services.Administration.Business.Extensions;
using OJS.Services.Administration.Business.Validation.Helpers;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models;
using OJS.Services.Administration.Models.Contests.Problems;
using OJS.Services.Administration.Models.Tests;
using OJS.Services.Common;
using OJS.Services.Common.Models;
using OJS.Services.Infrastructure.Extensions;
using SoftUni.AutoMapper.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using static OJS.Common.GlobalConstants;
using Resource = OJS.Common.Resources.TestsControllers;

public class TestsController : BaseAutoCrudAdminController<Test>
{
    public const string ProblemIdKey = nameof(Test.ProblemId);

    private const int TestInputMaxLengthInGrid = 20;

    private readonly IProblemsDataService problemsData;
    private readonly IZipArchivesService zipArchives;
    private readonly IFileSystemService fileSystem;
    private readonly IZippedTestsParserService zippedTestsParser;
    private readonly ISubmissionsDataService submissionsData;
    private readonly ITestsDataService testsData;
    private readonly ITestRunsDataService testRunsData;
    private readonly IProblemsBusinessService problemsBusiness;
    private readonly IProblemsValidationHelper problemsValidationHelper;

    public TestsController(
        IProblemsDataService problemsData,
        IZipArchivesService zipArchives,
        IFileSystemService fileSystem,
        IZippedTestsParserService zippedTestsParser,
        ISubmissionsDataService submissionsData,
        ITestsDataService testsData,
        ITestRunsDataService testRunsData,
        IProblemsBusinessService problemsBusiness,
        IProblemsValidationHelper problemsValidationHelper)
    {
        this.problemsData = problemsData;
        this.zipArchives = zipArchives;
        this.fileSystem = fileSystem;
        this.zippedTestsParser = zippedTestsParser;
        this.submissionsData = submissionsData;
        this.testsData = testsData;
        this.testRunsData = testRunsData;
        this.problemsBusiness = problemsBusiness;
        this.problemsValidationHelper = problemsValidationHelper;
    }

    protected override Expression<Func<Test, bool>>? MasterGridFilter
        => this.TryGetEntityIdForNumberColumnFilter(ProblemIdKey, out var problemId)
            ? t => t.ProblemId == problemId
            : base.MasterGridFilter;

    protected override IEnumerable<AutoCrudAdminGridToolbarActionViewModel> CustomToolbarActions
        => this.TryGetEntityIdForNumberColumnFilter(ProblemIdKey, out var problemId)
            ? this.GetCustomToolbarActions(problemId)
            : base.CustomToolbarActions;

    protected override IEnumerable<CustomGridColumn<Test>> CustomColumns
        => new CustomGridColumn<Test>[]
        {
            new ()
            {
                Name = AdditionalFormFields.Input.ToString(),
                ValueFunc = t => t.InputDataAsString.ToEllipsis(TestInputMaxLengthInGrid) !,
            },
            new ()
            {
                Name = AdditionalFormFields.Output.ToString(),
                ValueFunc = t => t.OutputDataAsString.ToEllipsis(TestInputMaxLengthInGrid) !,
            },
        };

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(TestsImportRequestModel model)
    {
        var problem = await this.problemsData.OneById(model.ProblemId);

        await this.problemsValidationHelper
            .ValidatePermissionsOfCurrentUser(problem?.Map<ProblemShortDetailsServiceModel>())
            .VerifyResult();

        var file = model.Tests;
        var problemId = model.ProblemId;

        if (file == null || file.Length == 0)
        {
            this.TempData.AddDangerMessage(Resource.NoEmptyFile);
            return this.RedirectToActionWithNumberFilter(nameof(TestsController), ProblemIdKey, problemId);
        }

        var extension = this.fileSystem.GetFileExtension(file);

        if (extension != FileExtensions.Zip)
        {
            this.TempData.AddDangerMessage(Resource.MustBeZip);
            return this.RedirectToActionWithNumberFilter(nameof(TestsController), ProblemIdKey, problemId);
        }

        TestsParseResult parsedTests;

        await using (var memory = new MemoryStream())
        {
            await file.CopyToAsync(memory);
            memory.Position = 0;

            try
            {
                parsedTests = await this.zippedTestsParser.Parse(memory);
            }
            catch
            {
                this.TempData.AddDangerMessage(Resource.ZipDamaged);
                return this.RedirectToActionWithNumberFilter(nameof(TestsController), ProblemIdKey, problemId);
            }
        }

        if (!this.zippedTestsParser.AreTestsParsedCorrectly(parsedTests))
        {
            this.TempData.AddDangerMessage(Resource.InvalidTests);
            return this.RedirectToActionWithNumberFilter(nameof(TestsController), ProblemIdKey, problemId);
        }

        int addedTestsCount;

        using (var scope = TransactionsHelper.CreateTransactionScope(
                   IsolationLevel.RepeatableRead,
                   TransactionScopeAsyncFlowOption.Enabled))
        {
            this.submissionsData.RemoveTestRunsCacheByProblem(problemId);

            if (model.DeleteOldTests)
            {
                await this.testRunsData.DeleteByProblem(problemId);
                await this.testsData.DeleteByProblem(problemId);
            }

            addedTestsCount = this.zippedTestsParser.AddTestsToProblem(problem!, parsedTests);

            this.problemsData.Update(problem!);
            await this.problemsData.SaveChanges();

            if (model.RetestProblem)
            {
                await this.problemsBusiness.RetestById(problemId);
            }

            scope.Complete();
        }

        this.TempData.AddSuccessMessage(string.Format(Resource.TestsAddedToProblem, addedTestsCount));
        return this.RedirectToActionWithNumberFilter(nameof(TestsController), ProblemIdKey, model.ProblemId);
    }

    public async Task<IActionResult> ExportZip(int problemId)
    {
        var problem = await this.problemsData.OneById(problemId);

        await this.problemsValidationHelper
            .ValidatePermissionsOfCurrentUser(problem?.Map<ProblemShortDetailsServiceModel>())
            .VerifyResult();

        var tests = problem!.Tests.OrderBy(x => x.OrderBy);

        var files = new List<InMemoryFile>();

        var trialTestCounter = 1;
        var openTestCounter = 1;
        var testCounter = 1;

        foreach (var test in tests)
        {
            var inputTestName = $"test.{testCounter:D3}{TestInputTxtFileExtension}";
            var outputTestName = $"test.{testCounter:D3}{TestOutputTxtFileExtension}";

            if (test.IsTrialTest)
            {
                inputTestName = $"test{ZeroTestStandardSignature}{trialTestCounter:D3}{TestInputTxtFileExtension}";
                outputTestName = $"test{ZeroTestStandardSignature}{trialTestCounter:D3}{TestOutputTxtFileExtension}";
                trialTestCounter++;
            }
            else if (test.IsOpenTest)
            {
                inputTestName = $"test{OpenTestStandardSignature}{openTestCounter:D3}{TestInputTxtFileExtension}";
                outputTestName = $"test{OpenTestStandardSignature}{openTestCounter:D3}{TestOutputTxtFileExtension}";
                openTestCounter++;
            }
            else
            {
                testCounter++;
            }

            files.Add(new InMemoryFile(inputTestName, test.InputDataAsString));
            files.Add(new InMemoryFile(outputTestName, test.OutputDataAsString));
        }

        var zipFile = await this.zipArchives.GetZipArchive(files);
        var zipFileName = $"{problem.Name}_Tests_{DateTime.Now}{FileExtensions.Zip}";

        return this.File(zipFile, MimeTypes.ApplicationZip, zipFileName);
    }

    protected override IEnumerable<FormControlViewModel> GenerateFormControls(
        Test entity,
        EntityAction action,
        IDictionary<string, string> entityDict,
        IDictionary<string, Expression<Func<object, bool>>> complexOptionFilters,
        Type autocompleteType)
    {
        var formControls = base.GenerateFormControls(entity, action, entityDict, complexOptionFilters, autocompleteType)
            .ToList();

        var problemId = entityDict.GetEntityIdOrDefault<Problem>();

        if (problemId != null)
        {
            var problemInput = formControls.First(fc => fc.Name == nameof(Test.Problem));
            problemInput.Value = problemId;
            problemInput.IsReadOnly = true;
        }

        var isTrialFormControl = formControls.First(x => x.Name == nameof(entity.IsTrialTest));
        var isOpenFormControl = formControls.First(x => x.Name == nameof(entity.IsOpenTest));

        isTrialFormControl.IsHidden = true;
        isOpenFormControl.IsHidden = true;
        formControls.Add(new FormControlViewModel
        {
            Name = AdditionalFormFields.Input.ToString(),
            Type = typeof(string),
            Value = entity.InputDataAsString,
            FormControlType = FormControlType.TextArea,
        });

        formControls.Add(new FormControlViewModel
        {
            Name = AdditionalFormFields.Output.ToString(),
            Type = typeof(string),
            Value = entity.OutputDataAsString,
            FormControlType = FormControlType.TextArea,
        });

        formControls.Add(new FormControlViewModel
        {
            Name = AdditionalFormFields.Type.ToString(),
            Type = typeof(TestTypeEnum),
            Options = EnumUtils.GetValuesFrom<TestTypeEnum>().Cast<object>(),
            Value = entity.IsTrialTest
                ? TestTypeEnum.TrialTest
                : TestTypeEnum.OpenTest,
        });
        formControls.Add(new FormControlViewModel
        {
            Name = AdditionalFormFields.RetestProblem.ToString(),
            Type = typeof(bool),
            Value = false,
        });
        return formControls;
    }

    protected override async Task BeforeEntitySaveAsync(Test entity, AdminActionContext actionContext)
    {
        await base.BeforeEntitySaveAsync(entity, actionContext);
        UpdateInputAndOutput(entity, actionContext);
        UpdateType(entity, actionContext);
    }

    protected override Task BeforeEntitySaveOnEditAsync(
        Test existingEntity,
        Test newEntity,
        AdminActionContext actionContext)
    {
        base.BeforeEntitySaveOnEditAsync(existingEntity, newEntity, actionContext);
        var retestProblem = bool.Parse(actionContext.GetFormValue(AdditionalFormFields.RetestProblem));

        if (retestProblem)
        {
            this.problemsBusiness.RetestById(newEntity.ProblemId);
        }

        return Task.CompletedTask;
    }

    private static void UpdateType(Test entity, AdminActionContext actionContext)
    {
        var testType = Enum.Parse<TestTypeEnum>(actionContext.GetFormValue(AdditionalFormFields.Type));
        switch (testType)
        {
            case TestTypeEnum.TrialTest:
                entity.IsTrialTest = true;
                entity.IsOpenTest = false;
                break;
            case TestTypeEnum.OpenTest:
                entity.IsTrialTest = false;
                entity.IsOpenTest = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(testType), testType, null);
        }
    }

    private static void UpdateInputAndOutput(Test entity, AdminActionContext actionContext)
    {
        var inputData = actionContext.GetByteArrayFromStringInput(AdditionalFormFields.Input);
        var outputData = actionContext.GetByteArrayFromStringInput(AdditionalFormFields.Output);

        entity.InputData = inputData;
        entity.OutputData = outputData;
    }

    private static IEnumerable<FormControlViewModel> GetFormControlsForImportTests(int problemId)
        => new FormControlViewModel[]
        {
            new ()
            {
                Name = nameof(TestsImportRequestModel.ProblemId),
                Type = typeof(int),
                IsHidden = true,
                Value = problemId,
            },
            new () { Name = nameof(TestsImportRequestModel.Tests), Type = typeof(IFormFile), },
            new () { Name = nameof(TestsImportRequestModel.RetestProblem), Type = typeof(bool), Value = false, },
            new () { Name = nameof(TestsImportRequestModel.DeleteOldTests), Type = typeof(bool), Value = true, },
        };

    private IEnumerable<AutoCrudAdminGridToolbarActionViewModel> GetCustomToolbarActions(int problemId)
    {
        var routeValues = new Dictionary<string, string> { { nameof(problemId), problemId.ToString() }, };

        return new AutoCrudAdminGridToolbarActionViewModel[]
        {
            new () { Name = "Add new", Action = nameof(this.Create), RouteValues = routeValues, },
            new () { Name = "Export Zip", Action = nameof(this.ExportZip), RouteValues = routeValues, },
            new ()
            {
                Name = "Import tests",
                Action = nameof(this.Import),
                FormControls = GetFormControlsForImportTests(problemId),
            },
        };
    }
}