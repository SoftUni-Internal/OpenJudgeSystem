namespace OJS.Services.Administration.Business.Problems;

using FluentExtensions.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OJS.Common.Enumerations;
using OJS.Common.Helpers;
using OJS.Data;
using OJS.Data.Models;
using OJS.Data.Models.Problems;
using OJS.Services.Administration.Business.Contests;
using OJS.Services.Administration.Business.ProblemGroups;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.Contests.Problems;
using OJS.Services.Administration.Models.Problems;
using OJS.Services.Common;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
    private readonly IContestsBusinessService contestsBusiness;
    private readonly ISubmissionsCommonBusinessService submissionsCommonBusinessService;
    private readonly IProblemGroupsDataService problemGroupsDataService;
    private readonly IZippedTestsParserService zippedTestsParser;
    private readonly ITransactionsProvider transactionsProvider;

    public ProblemsBusinessService(
        IContestsDataService contestsData,
        IParticipantScoresDataService participantScoresData,
        IProblemsDataService problemsData,
        IProblemResourcesDataService problemResourcesData,
        ISubmissionsDataService submissionsData,
        ISubmissionsForProcessingCommonDataService submissionsForProcessingData,
        ITestRunsDataService testRunsData,
        IProblemGroupsBusinessService problemGroupsBusiness,
        IContestsBusinessService contestsBusiness,
        ISubmissionsCommonBusinessService submissionsCommonBusinessService,
        IProblemGroupsDataService problemGroupsDataService,
        IZippedTestsParserService zippedTestsParser,
        ITransactionsProvider transactionsProvider)
    {
        this.contestsData = contestsData;
        this.participantScoresData = participantScoresData;
        this.problemsData = problemsData;
        this.problemResourcesData = problemResourcesData;
        this.submissionsData = submissionsData;
        this.submissionsForProcessingData = submissionsForProcessingData;
        this.testRunsData = testRunsData;
        this.problemGroupsBusiness = problemGroupsBusiness;
        this.contestsBusiness = contestsBusiness;
        this.submissionsCommonBusinessService = submissionsCommonBusinessService;
        this.problemGroupsDataService = problemGroupsDataService;
        this.zippedTestsParser = zippedTestsParser;
        this.transactionsProvider = transactionsProvider;
    }

    public override async Task<ProblemAdministrationModel> Create(ProblemAdministrationModel model)
    {
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

        AddSubmissionTypes(problem, model);
        await this.TryAddTestsToProblem(problem, model.Tests);

        await this.problemsData.SaveChanges();

        await this.ReevaluateProblemsOrder(contestId);

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

        if (!await this.contestsData.IsOnlineById(problem.ContestId))
        {
            await this.problemGroupsBusiness.DeleteById(problem.ProblemGroupId);
        }

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

    public async Task<bool> UserHasProblemPermissions(int problemId, string? userId, bool isUserAdmin)
    {
        var problem = await this.problemsData.OneByIdTo<ProblemShortDetailsServiceModel>(problemId);

        if (problem == null)
        {
            throw new BusinessServiceException("Problem cannot be null");
        }

        return await this.contestsBusiness.UserHasContestPermissions(problem.ContestId, userId, isUserAdmin);
    }

    public Task ReevaluateProblemsOrder(int contestId)
        => this.problemGroupsBusiness.ReevaluateProblemsAndProblemGroupsOrder(contestId);

    public override async Task<ProblemAdministrationModel> Get(int id)
    {
        var problem = await this.problemsData.GetByIdQuery(id)
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
            .Include(s => s.SubmissionTypesInProblems)
            .Include(s => s.ProblemGroup)
            .Include(s => s.Checker)
            .FirstOrDefaultAsync();

        if (problem is null)
        {
            throw new ArgumentNullException($"Problem with id {model.Id} not found");
        }

        problem.MapFrom(model);

        problem.ProblemGroupId = model.ProblemGroupId;
        problem.ProblemGroup = await this.problemGroupsDataService
            .GetByIdQuery(model.ProblemGroupId)
            .Include(pg => pg.Contest)
            .FirstAsync();

        problem.ProblemGroup.Type = (ProblemGroupType)Enum.Parse(typeof(ProblemGroupType), model.ProblemGroupType!);

        if (!problem.ProblemGroup.Contest.IsOnlineExam)
        {
            problem.ProblemGroup.OrderBy = model.OrderBy;
        }

        AddSubmissionTypes(problem, model);

        this.problemsData.Update(problem);

        await this.problemsData.SaveChanges();

        await this.ReevaluateProblemsOrder(problem.ProblemGroup.ContestId);

        return model;
    }

    public async Task RetestById(int id)
    {
        var submissions = (await this.submissionsData.GetAllNonDeletedByProblemId<SubmissionServiceModel>(id)).ToList();

        var submissionIds = submissions.Select(s => s.Id).ToList();

        await this.transactionsProvider.ExecuteInTransaction(async () =>
        {
            await this.testRunsData.DeleteInBatchesBySubmissionIds(submissionIds);

            await this.participantScoresData.DeleteAllByProblem(id);

            await this.submissionsData.SetAllToUnprocessedByProblem(id);

            await this.submissionsForProcessingData.AddOrUpdateMany(submissionIds);
        });

        await this.submissionsCommonBusinessService.PublishSubmissionsForProcessing(submissions);
    }

    private static void AddSubmissionTypes(Problem problem, ProblemAdministrationModel model)
    {
        var newSubmissionTypes = model.SubmissionTypes
            .Select(x => new SubmissionTypeInProblem
            {
                ProblemId = problem.Id,
                SubmissionTypeId = x.Id,
                SolutionSkeleton = x.SolutionSkeleton?.ToString().Compress(),
                TimeLimit = x.TimeLimit,
                MemoryLimit = x.MemoryLimit,
            });

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

            problem.ProblemGroup = new ProblemGroup
            {
                ContestId = contestId,
                OrderBy = orderBy,
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