namespace OJS.Services.Administration.Business.Tests;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Common;
using OJS.Common.Helpers;
using OJS.Data.Models.Tests;
using OJS.Services.Administration.Business.Problems;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.Tests;
using OJS.Services.Common;
using OJS.Services.Common.Models;
using OJS.Services.Common.Models.Files;
using OJS.Services.Infrastructure.Exceptions;
using SoftUni.AutoMapper.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

public class TestsBusinessService : AdministrationOperationService<Test, int, TestAdministrationModel>, ITestsBusinessService
{
    private readonly ITestsDataService testsDataService;
    private readonly ITestRunsDataService testRunsDataService;
    private readonly IProblemsDataService problemsDataService;
    private readonly IFileSystemService fileSystemService;
    private readonly IZippedTestsParserService zippedTestsParserService;
    private readonly ISubmissionsDataService submissionsDataService;
    private readonly IProblemsBusinessService problemsBusinessService;
    private readonly IZipArchivesService zipArchivesService;

    public TestsBusinessService(
        ITestsDataService testsDataService,
        ITestRunsDataService testRunsDataService,
        IProblemsDataService problemsDataService,
        IFileSystemService fileSystemService,
        IZippedTestsParserService zippedTestsParserService,
        ISubmissionsDataService submissionsDataService,
        IProblemsBusinessService problemsBusinessService,
        IZipArchivesService zipArchivesService)
    {
        this.testsDataService = testsDataService;
        this.testRunsDataService = testRunsDataService;
        this.problemsDataService = problemsDataService;
        this.fileSystemService = fileSystemService;
        this.zippedTestsParserService = zippedTestsParserService;
        this.submissionsDataService = submissionsDataService;
        this.problemsBusinessService = problemsBusinessService;
        this.zipArchivesService = zipArchivesService;
    }

    public override async Task<TestAdministrationModel> Get(int id)
    {
        var test = await this.testsDataService.GetByIdQuery(id)
            .MapCollection<TestAdministrationModel>()
            .FirstOrDefaultAsync();

        if (test is null)
        {
            throw new BusinessServiceException($"Test with id: {id} not found.");
        }

        return test.Map<TestAdministrationModel>();
    }

    public override async Task<TestAdministrationModel> Create(TestAdministrationModel model)
    {
        var test = model.Map<Test>();

        UpdateInputAndOutput(test, model);
        UpdateType(test, model);

        await this.testsDataService.Add(test);
        await this.testsDataService.SaveChanges();

        return model;
    }

    public override async Task<TestAdministrationModel> Edit(TestAdministrationModel model)
    {
        var test = await this.testsDataService.GetByIdQuery(model.Id)
            .Include(t => t.Problem)
            .Include(t => t.TestRuns).FirstOrDefaultAsync();

        if (test == null)
        {
            throw new BusinessServiceException("Test not found.");
        }

        test.MapFrom(model);

        UpdateInputAndOutput(test, model);
        UpdateType(test, model);

        this.testsDataService.Update(test);
        await this.testsDataService.SaveChanges();

        if (model.RetestProblem)
        {
            await this.problemsBusinessService.RetestById(model.ProblemId);
        }
        else
        {
            var submissionIds = await this.submissionsDataService.GetIdsByProblemId(test.ProblemId);

            if (submissionIds.Any())
            {
                await this.testRunsDataService.DeleteInBatchesBySubmissionIds(submissionIds);
            }
        }

        return model;
    }

    public override async Task Delete(int id)
    {
        await this.testsDataService.DeleteById(id);
        await this.testsDataService.SaveChanges();
    }

    public async Task DeleteAll(int problemId)
    {
        await this.testRunsDataService.DeleteByProblem(problemId);
        await this.testsDataService.DeleteByProblem(problemId);
    }

    public async Task<string> Import(TestsImportRequestModel model)
    {
        var problem = this.problemsDataService.GetWithTestsSubmissionTypesAndProblemGroupById(model.ProblemId);

        var file = model.Tests;
        var problemId = model.ProblemId;

        if (file == null || file.Length == 0)
        {
            throw new BusinessServiceException(Resources.TestsControllers.NoEmptyFile);
        }

        var extension = this.fileSystemService.GetFileExtension(file);

        if (extension != GlobalConstants.FileExtensions.Zip)
        {
            throw new BusinessServiceException(Resources.TestsControllers.MustBeZip);
        }

        TestsParseResult parsedTests;

        await using (var memory = new MemoryStream())
        {
            await file.CopyToAsync(memory);
            memory.Position = 0;

            try
            {
                parsedTests = await this.zippedTestsParserService.Parse(memory);
            }
            catch
            {
                throw new BusinessServiceException(Resources.TestsControllers.ZipDamaged);
            }
        }

        if (!this.zippedTestsParserService.AreTestsParsedCorrectly(parsedTests))
        {
            throw new BusinessServiceException(Resources.TestsControllers.InvalidTests);
        }

        int addedTestsCount;

        using (var scope = TransactionsHelper.CreateTransactionScope(
                   IsolationLevel.RepeatableRead,
                   TransactionScopeAsyncFlowOption.Enabled))
        {
            this.submissionsDataService.RemoveTestRunsCacheByProblem(problemId);

            if (model.DeleteOldTests)
            {
                await this.testRunsDataService.DeleteByProblem(problemId);
                await this.testsDataService.DeleteByProblem(problemId);
            }

            addedTestsCount = this.zippedTestsParserService.AddTestsToProblem(problem!, parsedTests);

            this.problemsDataService.Update(problem!);
            await this.problemsDataService.SaveChanges();

            if (model.RetestProblem)
            {
                await this.problemsBusinessService.RetestById(problemId);
            }

            scope.Complete();
        }

        return string.Format(Resources.TestsControllers.TestsAddedToProblem, addedTestsCount);
    }

    public async Task<FileResponseModel> ExportZip(int problemId)
    {
        var problem = this.problemsDataService.GetWithTestsAndProblemGroupById(problemId);

        var tests = problem!.Tests.OrderBy(x => x.OrderBy);

        var files = new List<InMemoryFile>();

        var trialTestCounter = 1;
        var openTestCounter = 1;
        var testCounter = 1;

        foreach (var test in tests)
        {
            var inputTestName = $"test.{testCounter:D3}{GlobalConstants.TestInputTxtFileExtension}";
            var outputTestName = $"test.{testCounter:D3}{GlobalConstants.TestOutputTxtFileExtension}";

            if (test.IsTrialTest)
            {
                inputTestName = $"test{GlobalConstants.ZeroTestStandardSignature}{trialTestCounter:D3}{GlobalConstants.TestInputTxtFileExtension}";
                outputTestName = $"test{GlobalConstants.ZeroTestStandardSignature}{trialTestCounter:D3}{GlobalConstants.TestOutputTxtFileExtension}";
                trialTestCounter++;
            }
            else if (test.IsOpenTest)
            {
                inputTestName = $"test{GlobalConstants.OpenTestStandardSignature}{openTestCounter:D3}{GlobalConstants.TestInputTxtFileExtension}";
                outputTestName = $"test{GlobalConstants.OpenTestStandardSignature}{openTestCounter:D3}{GlobalConstants.TestOutputTxtFileExtension}";
                openTestCounter++;
            }
            else
            {
                testCounter++;
            }

            files.Add(new InMemoryFile(inputTestName, test.InputDataAsString));
            files.Add(new InMemoryFile(outputTestName, test.OutputDataAsString));
        }

        var zipFile = await this.zipArchivesService.GetZipArchive(files);
        var zipFileName = $"{problem.Name}_Tests_{DateTime.Now}{GlobalConstants.FileExtensions.Zip}";

        return new FileResponseModel
        {
            Content = zipFile, FileName = zipFileName, MimeType = GlobalConstants.MimeTypes.ApplicationZip,
        };
    }

    private static void UpdateInputAndOutput(Test entity, TestAdministrationModel model)
    {
        entity.InputData = model.Input!.Compress();
        entity.OutputData = model.Output!.Compress();
    }

    private static void UpdateType(Test entity, TestAdministrationModel model)
    {
        Enum.TryParse<TestTypeEnum>(model.Type, out var testType);
        switch (testType)
        {
            case TestTypeEnum.Trial:
                entity.IsTrialTest = true;
                entity.IsOpenTest = false;
                break;
            case TestTypeEnum.Open:
                entity.IsTrialTest = false;
                entity.IsOpenTest = true;
                break;
            case TestTypeEnum.Standard:
                entity.IsTrialTest = false;
                entity.IsOpenTest = false;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(testType), testType, null);
        }
    }
}