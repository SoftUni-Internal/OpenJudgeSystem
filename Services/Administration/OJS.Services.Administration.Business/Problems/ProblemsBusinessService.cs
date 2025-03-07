namespace OJS.Services.Administration.Business.Problems;

using FluentExtensions.Extensions;
using Infrastructure.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OJS.Common;
using OJS.Common.Enumerations;
using OJS.Common.Helpers;
using OJS.Data.Models;
using OJS.Data.Models.Problems;
using OJS.Services.Administration.Business.ProblemGroups;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.Problems;
using OJS.Services.Common;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models;
using OJS.Services.Infrastructure.Extensions;
using Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using OJS.Workers.Common;
using IsolationLevel = System.Transactions.IsolationLevel;
using Resource = OJS.Common.Resources.ProblemsBusiness;
using SharedResource = OJS.Common.Resources.ContestsGeneral;

public class ProblemsBusinessService : AdministrationOperationService<Problem, int, ProblemAdministrationModel>, IProblemsBusinessService
{
    private readonly IContestsDataService contestsData;
    private readonly IParticipantScoresDataService participantScoresData;
    private readonly IProblemsDataService problemsData;
    private readonly IProblemResourcesDataService problemResourcesData;
    private readonly ISubmissionsDataService submissionsData;
    private readonly ISubmissionsForProcessingCommonDataService submissionsForProcessingData;
    private readonly ITestRunsDataService testRunsData;
    private readonly IProblemGroupsBusinessService problemGroupsBusiness;
    private readonly ISubmissionsCommonBusinessService submissionsCommonBusinessService;
    private readonly IProblemGroupsDataService problemGroupsDataService;
    private readonly IZippedTestsParserService zippedTestsParser;
    private readonly ITransactionsProvider transactionsProvider;
    private readonly IProblemsCacheService problemsCache;
    private readonly IUserProviderService userProviderService;
    private readonly ISettingsBusinessService settingsBusinessService;
    private readonly ISubmissionTypesDataService submissionTypesData;

    public ProblemsBusinessService(
        IContestsDataService contestsData,
        IParticipantScoresDataService participantScoresData,
        IProblemsDataService problemsData,
        IProblemResourcesDataService problemResourcesData,
        ISubmissionsDataService submissionsData,
        ISubmissionsForProcessingCommonDataService submissionsForProcessingData,
        ITestRunsDataService testRunsData,
        IProblemGroupsBusinessService problemGroupsBusiness,
        ISubmissionsCommonBusinessService submissionsCommonBusinessService,
        IProblemGroupsDataService problemGroupsDataService,
        IZippedTestsParserService zippedTestsParser,
        ITransactionsProvider transactionsProvider,
        IProblemsCacheService problemsCache,
        IUserProviderService userProviderService,
        ISettingsBusinessService settingsBusinessService,
        ISubmissionTypesDataService submissionTypesData)
    {
        this.contestsData = contestsData;
        this.participantScoresData = participantScoresData;
        this.problemsData = problemsData;
        this.problemResourcesData = problemResourcesData;
        this.submissionsData = submissionsData;
        this.submissionsForProcessingData = submissionsForProcessingData;
        this.testRunsData = testRunsData;
        this.problemGroupsBusiness = problemGroupsBusiness;
        this.submissionsCommonBusinessService = submissionsCommonBusinessService;
        this.problemGroupsDataService = problemGroupsDataService;
        this.zippedTestsParser = zippedTestsParser;
        this.transactionsProvider = transactionsProvider;
        this.problemsCache = problemsCache;
        this.userProviderService = userProviderService;
        this.settingsBusinessService = settingsBusinessService;
        this.submissionTypesData = submissionTypesData;
    }

    public override async Task<ProblemAdministrationModel> Create(ProblemAdministrationModel model)
    {
        if (model.AdditionalFiles != null)
        {
            this.ValidateAdditionalFiles(model.AdditionalFiles);
        }

        var contestId = model.ContestId;

        var problem = model.Map<Problem>();
        if (problem.ProblemGroupId == default)
        {
            var isValidGroupType = Enum.TryParse(model.ProblemGroupType, out ProblemGroupType problemGroupType);

            problem.ProblemGroup = new ProblemGroup
            {
                ContestId = contestId,
                OrderBy = problem.OrderBy,
                Type = isValidGroupType ? problemGroupType : null,
            };
        }

        await this.problemsData.Add(problem);

        await this.AddSubmissionTypes(problem, model);
        await this.TryAddTestsToProblem(problem, model.Tests);

        await this.problemsData.SaveChanges();

        await this.ReevaluateProblemsOrder(contestId);
        await this.problemsCache.ClearProblemCacheById(problem.Id);

        return model;
    }

    public override async Task Delete(int id)
    {
        var problem = this.problemsData
            .GetByIdQuery(id)
            .Select(p => new { p.ProblemGroupId, p.ProblemGroup.ContestId, p.IsDeleted, })
            .FirstOrDefault();

        if (problem == null || problem.IsDeleted)
        {
            return;
        }

        using var scope = TransactionsHelper.CreateTransactionScope(
            IsolationLevel.RepeatableRead,
            TransactionScopeAsyncFlowOption.Enabled);

        if (!await this.contestsData.IsWithRandomTasksById(problem.ContestId))
        {
            await this.problemGroupsBusiness.DeleteById(problem.ProblemGroupId);
        }

        await this.problemsCache.ClearProblemCacheById(id);

        await this.problemsData.DeleteById(id);
        await this.problemsData.SaveChanges();
        await this.testRunsData.DeleteByProblem(id);

        this.problemResourcesData.DeleteByProblem(id);

        this.submissionsData.DeleteByProblem(id);

        scope.Complete();
    }

    public async Task DeleteByContest(int contestId) =>
        await this.problemsData
            .GetAllByContest(contestId)
            .Select(p => p.Id)
            .ToList()
            .ForEachSequential(this.Delete);

    public async Task<ServiceResult> CopyToContestByIdByContestAndProblemGroup(int id, int contestId, int? problemGroupId)
    {
        var problem = await this.problemsData
            .GetByIdQuery(id)
            .AsNoTracking()
            .Include(p => p.Tests)
            .Include(p => p.Resources)
            .Include(p => p.ProblemGroup)
            .Include(p => p.SubmissionTypesInProblems)
            .SingleOrDefaultAsync();

        if (problem?.ProblemGroup.ContestId == contestId)
        {
            return new ServiceResult(Resource.CannotCopyProblemsIntoSameContest);
        }

        if (!await this.contestsData.ExistsById(contestId))
        {
            return new ServiceResult(SharedResource.ContestNotFound);
        }

        if (await this.contestsData.IsActiveById(contestId))
        {
            return new ServiceResult(Resource.CannotCopyProblemsIntoActiveContest);
        }

        await this.CopyProblemToContest(problem, contestId, problemGroupId);

        return ServiceResult.Success;
    }

    public Task ReevaluateProblemsOrder(int contestId)
        => this.problemGroupsBusiness.ReevaluateProblemsAndProblemGroupsOrder(contestId);

    public override async Task<ProblemAdministrationModel> Get(int id)
    {
        var problem = await this.problemsData
            .GetByIdQuery(id)
            .Include(stp => stp.SubmissionTypesInProblems)
            .ThenInclude(stp => stp.SubmissionType)
            .Include(p => p.ProblemGroup)
            .ThenInclude(pg => pg.Contest)
            .FirstOrDefaultAsync();

        if (problem is null)
        {
            throw new InvalidOperationException();
        }

        return problem.Map<ProblemAdministrationModel>();
    }

    public override async Task<ProblemAdministrationModel> Edit(ProblemAdministrationModel model)
    {
        var problem = await this.problemsData.GetByIdQuery(model.Id)
            .Include(s => s.Checker)
            .Include(s => s.SubmissionTypesInProblems)
            .Include(s => s.ProblemGroup)
            .FirstOrDefaultAsync();

        if (problem is null)
        {
            throw new ArgumentNullException($"Problem with id {model.Id} not found");
        }

        if (model.AdditionalFiles != null)
        {
            this.ValidateAdditionalFiles(model.AdditionalFiles);
        }

        problem.MapFrom(model);

        problem.ProblemGroupId = model.ProblemGroupId;
        problem.ProblemGroup = await this.problemGroupsDataService
            .GetByIdQuery(model.ProblemGroupId)
            .Include(pg => pg.Contest)
            .FirstAsync();

        problem.ProblemGroup.Type = (ProblemGroupType)Enum.Parse(typeof(ProblemGroupType), model.ProblemGroupType!);

        if (!problem.ProblemGroup.Contest.IsWithRandomTasks)
        {
            problem.ProblemGroup.OrderBy = model.OrderBy;
        }

        await this.AddSubmissionTypes(problem, model);

        await this.problemsCache.ClearProblemCacheById(problem.Id);

        this.problemsData.Update(problem);

        await this.problemsData.SaveChanges();

        await this.ReevaluateProblemsOrder(problem.ProblemGroup.ContestId);

        return model;
    }

    public async Task<ProblemRetestValidationModel> ValidateRetest(int id)
    {
        var submissionsCount = await this.submissionsData.GetCountByProblem(id);

        if (submissionsCount == 0)
        {
            return new ProblemRetestValidationModel
            {
                SubmissionsCount = 0,
                AverageExecutionTime = 0,
                RetestAllowed = false,
                Message = "No submissions have been submitted for this problem."
            };
        }

        // Take 3 submissions with different results so an average execution time can be obtained
        var relevantSubmissions = this.submissionsData.GetAllByProblem(id)
            .Where(s => s.IsCompiledSuccessfully && s.StartedExecutionOn.HasValue && s.CompletedExecutionOn.HasValue)
            .GroupBy(s => s.Points)
            .Select(g => g.FirstOrDefault())
            .Take(3)
            .ToList();

        if (relevantSubmissions.Count == 0)
        {
            return new ProblemRetestValidationModel
            {
                SubmissionsCount = submissionsCount,
                AverageExecutionTime = 0,
                RetestAllowed = false,
                Message = "No submissions have been successfully compiled for this problem."
            };
        }

        var averageTimeDifferenceInSeconds = Math.Round(relevantSubmissions
            .Select(s => (s.CompletedExecutionOn.Value - s.StartedExecutionOn.Value).TotalSeconds)
            .Average());

        var maxWorkersWorkingTime =
            await this.settingsBusinessService.GetByKey(GlobalConstants.Settings.MaxWorkersWorkingTimeInSeconds);
        var maxSubmissionsCountAllowedForBatchRetest =
            await this.settingsBusinessService.GetByKey(GlobalConstants.Settings.MaxSubmissionsCountAllowedForBatchRetest);
        var maxSubmissionTimeToExecuteAllowedForBatchRetest =
            await this.settingsBusinessService.GetByKey(GlobalConstants.Settings.MaxSubmissionTimeToExecuteAllowedForBatchRetest);

        var allSubmissionsWorkingTime = submissionsCount * averageTimeDifferenceInSeconds;

        var allSubmissionsWorkingTimeExceedsMaxAllowedTime = allSubmissionsWorkingTime > double.Parse(maxWorkersWorkingTime.Value, CultureInfo.InvariantCulture);
        var submissionsCountExceedsMaxAllowedLimit = submissionsCount > int.Parse(maxSubmissionsCountAllowedForBatchRetest.Value!, CultureInfo.InvariantCulture);
        var submissionsTimeToExecuteExceedsMaxAllowedLimit = averageTimeDifferenceInSeconds < int.Parse(maxSubmissionTimeToExecuteAllowedForBatchRetest.Value!, CultureInfo.InvariantCulture);

        var validationModel = new ProblemRetestValidationModel
        {
            SubmissionsCount = submissionsCount,
            AverageExecutionTime = averageTimeDifferenceInSeconds,
            RetestAllowed = false,
        };

        var canRetest = !allSubmissionsWorkingTimeExceedsMaxAllowedTime || (!submissionsCountExceedsMaxAllowedLimit && !submissionsTimeToExecuteExceedsMaxAllowedLimit);

        if (this.userProviderService.GetCurrentUser().IsDeveloper && !canRetest)
        {
            // Developers can retest even if validations fail
            // Adding messages for developers for better understanding why retest fails
            if (allSubmissionsWorkingTimeExceedsMaxAllowedTime)
            {
                validationModel.Message = $"Submissions will take {Math.Round(allSubmissionsWorkingTime / 60)} minutes to execute.";
            }
            else if (submissionsCountExceedsMaxAllowedLimit)
            {
                validationModel.Message = $"Submissions count exceeds max allowed limit {maxSubmissionsCountAllowedForBatchRetest.Value}.";
            }
            else if (submissionsTimeToExecuteExceedsMaxAllowedLimit)
            {
                validationModel.Message =
                    $"Submissions time to execute ({averageTimeDifferenceInSeconds}) exceeds max allowed limit {maxSubmissionTimeToExecuteAllowedForBatchRetest.Value}";
            }
        }
        else if (!canRetest)
        {
            validationModel.Message =
                $"Retesting {submissionsCount} submissions will take {Math.Round(allSubmissionsWorkingTime / 60)} minutes to complete, exceeding the maximum allowed time limit ({Math.Round(double.Parse(maxWorkersWorkingTime.Value) / 60)} minutes). Contact a developer.";

            return validationModel;
        }

        validationModel.RetestAllowed = true;
        return validationModel;
    }

    public async Task RetestById(int id)
    {
        var submissions = await this.submissionsData.GetAllNonDeletedByProblemWithProblemTestsAndSubmissionTypes(id);

        var submissionIds = submissions.Select(s => s.Id).ToList();

        await this.transactionsProvider.ExecuteInTransaction(async () =>
        {
            await this.testRunsData.DeleteInBatchesBySubmissionIds(submissionIds);

            await this.participantScoresData.DeleteAllByProblem(id);

            await this.submissionsData.SetAllToUnprocessedByProblem(id);

            await this.submissionsData.RemoveTestRunsCacheByProblem(id);

            await this.submissionsForProcessingData.AddOrUpdateMany(submissionIds);
        });

        var serviceModels = submissions
                .Select(s => this.submissionsCommonBusinessService.BuildSubmissionForProcessing(s))
                .ToList();

        await this.submissionsCommonBusinessService.PublishSubmissionsForProcessing(serviceModels);
    }

    public async Task<(MemoryStream outputStream, string zipFileName)> DownloadAdditionalFiles(int problemId)
    {
        var problem = await this.problemsData
            .GetByIdQuery(problemId)
            .FirstOrDefaultAsync();

        if (problem is null)
        {
            throw new BusinessServiceException($"Problem with id {problemId} does not exist.");
        }

        var additionalFiles = problem.AdditionalFiles;
        if (additionalFiles == null || additionalFiles.Length == 0)
        {
            throw new BusinessServiceException($"Problem with id {problemId} does not have any additional files.");
        }

        var zipFileName = $"{problem.Name}_AdditionalFiles_{DateTime.Now:yyyyMMddHHmmss}{Constants.ZipFileExtension}";

        var outputStream = new MemoryStream();
        try
        {
            await outputStream.WriteAsync(additionalFiles);

            outputStream.Position = 0;

            return (outputStream, zipFileName);
        }
        catch
        {
            await outputStream.DisposeAsync();
            throw;
        }
    }

    private void ValidateAdditionalFiles(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new BusinessServiceException("The additional files should not be empty.");
        }

        var isExtensionInvalid = Path.GetExtension(file.FileName) != Constants.ZipFileExtension;

        if (isExtensionInvalid)
        {
            throw new BusinessServiceException("The additional files should be in a .zip format.");
        }

        using var stream = file.OpenReadStream();
        var buffer = new byte[2];
        var bytesRead = stream.Read(buffer, 0, 2);

        if (bytesRead < 2)
        {
            throw new BusinessServiceException("The additional files are too small to be a valid .zip file.");
        }

        /*
         *  Validate if the file is .zip, its magic number ( the first two bytes ), should be 'P' and 'K'.
         *  Where '0x50' and '0x4B' are the byte representations of 'P' and 'K' respectively.
         */
        if (buffer[0] != 0x50 || buffer[1] != 0x4B)
        {
            throw new BusinessServiceException("The additional files should be in a .zip format.");
        }
    }

    private async Task AddSubmissionTypes(Problem problem, ProblemAdministrationModel model)
    {
        var submissionTypeIds = model.SubmissionTypes.Select(st => st.Id).ToList();

        var submissionTypes = await this.submissionTypesData
            .GetQuery(st => submissionTypeIds.Contains(st.Id))
            .Select(st => st.Id)
            .ToListAsync();

        var newSubmissionTypes = new List<SubmissionTypeInProblem>();

        foreach (var newSubmissionType in model.SubmissionTypes)
        {
            if (!submissionTypes.Contains(newSubmissionType.Id))
            {
                throw new BusinessServiceException($"Submission type with Id #{newSubmissionType.Id} does not exist.");
            }

            newSubmissionTypes.Add(new SubmissionTypeInProblem
            {
                ProblemId = problem.Id,
                SubmissionTypeId = newSubmissionType.Id,
                SolutionSkeleton = newSubmissionType.SolutionSkeleton?.ToString().Compress(),
                TimeLimit = newSubmissionType.TimeLimit,
                MemoryLimit = newSubmissionType.MemoryLimit,
            });
        }

        problem.SubmissionTypesInProblems = new HashSet<SubmissionTypeInProblem>(newSubmissionTypes);
    }


    private async Task CopyProblemToContest(Problem? problem, int contestId, int? problemGroupId)
    {
        double orderBy;

        if (problem == null)
        {
            return;
        }

        if (problemGroupId.HasValue)
        {
            orderBy = await this.problemsData.GetNewOrderByProblemGroup(problemGroupId.Value);

            problem.ProblemGroup = null!;
            problem.ProblemGroupId = problemGroupId.Value;
            problem.ProblemGroup = this.problemGroupsDataService.GetByIdQuery(problemGroupId.Value).First();
        }
        else
        {
            orderBy = await this.problemsData.GetNewOrderByContest(contestId);
            var problemGroupOrderBy = await this.problemGroupsBusiness.GetNewLatestOrderByContest(contestId);

            problem.ProblemGroup = new ProblemGroup
            {
                ContestId = contestId,
                OrderBy = problemGroupOrderBy,
            };
        }

        problem.OrderBy = orderBy;

        await this.problemGroupsBusiness.GenerateNewProblem(problem, problem.ProblemGroup, new List<Problem>());
        await this.problemsData.SaveChanges();
    }

    private async Task TryAddTestsToProblem(Problem problem, IFormFile? tests)
    {
        if (tests == null)
        {
            return;
        }

        try
        {
            await this.AddTestsToProblem(problem, tests);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(string.Format(ex.Message), ex);
        }
    }

    private async Task AddTestsToProblem(Problem problem, IFormFile testsFile)
    {
        await using var memoryStream = new MemoryStream();
        await testsFile.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        var parsedTests = await this.zippedTestsParser.Parse(memoryStream);

        if (!this.zippedTestsParser.AreTestsParsedCorrectly(parsedTests))
        {
            throw new ArgumentException("Invalid tests");
        }

        this.zippedTestsParser.AddTestsToProblem(problem, parsedTests);
    }
}