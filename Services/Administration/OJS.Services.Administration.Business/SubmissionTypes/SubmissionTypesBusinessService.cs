namespace OJS.Services.Administration.Business.SubmissionTypes;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Data.Models;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Submissions;
using OJS.Services.Administration.Business.SubmissionTypes.Validators;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.SubmissionTypes;
using OJS.Services.Administration.Models.SubmissionTypesInSubmissionDocuments;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SubmissionTypesBusinessService : AdministrationOperationService<SubmissionType, int, SubmissionTypeAdministrationModel>, ISubmissionTypesBusinessService
{
    private readonly ISubmissionTypesDataService submissionTypesDataService;
    private readonly IContestsDataService contestsDataService;
    private readonly ISubmissionsDataService submissionsDataService;
    private readonly ITestRunsDataService testRunsData;
    private readonly ISubmissionTypesInProblemsDataService submissionTypesInProblemsDataService;
    private readonly IProblemsDataService problemsDataService;
    private readonly IDeleteOrReplaceSubmissionTypeValidationService deleteOrReplaceSubmissionTypeValidationService;
    private readonly IUserProviderService userProvider;

    public SubmissionTypesBusinessService(
        ISubmissionTypesDataService submissionTypesDataService,
        IContestsDataService contestsDataService,
        ISubmissionsDataService submissionsDataService,
        ITestRunsDataService testRunsData,
        ISubmissionTypesInProblemsDataService submissionTypesInProblemsDataService,
        IProblemsDataService problemsDataService,
        IDeleteOrReplaceSubmissionTypeValidationService deleteOrReplaceSubmissionTypeValidationService,
        IUserProviderService userProvider)
    {
        this.submissionTypesDataService = submissionTypesDataService;
        this.contestsDataService = contestsDataService;
        this.submissionsDataService = submissionsDataService;
        this.testRunsData = testRunsData;
        this.submissionTypesInProblemsDataService = submissionTypesInProblemsDataService;
        this.deleteOrReplaceSubmissionTypeValidationService = deleteOrReplaceSubmissionTypeValidationService;
        this.userProvider = userProvider;
        this.problemsDataService = problemsDataService;
    }

    public async Task<List<SubmissionTypesInProblemView>> GetForProblem() =>
        await this.submissionTypesDataService.GetAll().MapCollection<SubmissionTypesInProblemView>().ToListAsync();

    public async Task<List<SubmissionTypeInDocument>> GetForDocument()
        => await this.submissionTypesDataService.GetAll().MapCollection<SubmissionTypeInDocument>().ToListAsync();

    public async Task<bool> AllExist(IEnumerable<SubmissionTypeInSubmissionDocumentAdministrationModel> submissionTypes)
    {
        var idsToCheck = submissionTypes.Select(st => st.SubmissionTypeId).ToHashSet();

        var matchingEntities = await this.submissionTypesDataService
            .All(st => idsToCheck.Contains(st.Id));

        return matchingEntities.Count() == idsToCheck.Count;
    }

    public async Task<bool> ExistsById(int submissionTypeId)
        => await this.submissionTypesDataService
            .ExistsById(submissionTypeId);

    public async Task<string> ReplaceSubmissionType(ReplaceSubmissionTypeServiceModel model)
    {
        var stringBuilder = new StringBuilder();

        var user = this.userProvider.GetCurrentUser();

        var submissionTypeToReplaceOrDelete = (await this.submissionTypesDataService
            .GetByIdQuery(model.SubmissionTypeToReplace)
            .FirstOrDefaultAsync())
            ?? throw new BusinessServiceException("Submission type to replace or delete not found.");

        SubmissionType? submissionTypeToReplaceWith = null;

        if (model.SubmissionTypeToReplaceWith.HasValue)
        {
            submissionTypeToReplaceWith = await this.submissionTypesDataService
                .GetByIdQuery(model.SubmissionTypeToReplaceWith)
                .FirstOrDefaultAsync();
        }

        var isReplacingSubmissionType = model.SubmissionTypeToReplaceWith.HasValue;

        var validationResult = await this.deleteOrReplaceSubmissionTypeValidationService.GetValidationResult(
            (
                model.SubmissionTypeToReplace,
                model.SubmissionTypeToReplaceWith,
                submissionTypeToReplaceOrDelete,
                submissionTypeToReplaceWith,
                isReplacingSubmissionType,
                user));

        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        var problemsQuery = this.problemsDataService
            .GetQuery(p => p.SubmissionTypesInProblems
                .Any(st => st.SubmissionTypeId == submissionTypeToReplaceOrDelete.Id))
            .Include(p => p.SubmissionTypesInProblems);
        List<Problem>? problems = null;

        if (isReplacingSubmissionType)
        {
            stringBuilder.Append(
                $"Submission type \"{submissionTypeToReplaceOrDelete!.Name}\" is replaced with \"{submissionTypeToReplaceWith!.Name}\"");
        }
        else
        {
            stringBuilder.Append(
                $"Submission type \"{submissionTypeToReplaceOrDelete!.Name}\" is deleted along with all submissions associated with it");
            stringBuilder.AppendLine();

            problems = await problemsQuery.ToListAsync();

            // Must be called before delete so problems with 1 submission type are calculated correctly
            await this.AppendMessageForProblemsThatWillBeLeftWithNoSubmissionType(stringBuilder, problems);
        }

        var problemIds = problems?.Select(p => p.Id).ToList()
            ?? await problemsQuery.Select(p => p.Id).ToListAsync();

        var submissionIds = this.submissionsDataService
            .GetAllByProblems(problemIds)
            .Where(s => s.SubmissionTypeId == submissionTypeToReplaceOrDelete.Id)
            .Select(s => s.Id)
            .ToList();

        // Update the default submission type of associated problems
        await this.problemsDataService
            .Update(
                p => problemIds.Contains(p.Id),
                setters => setters
                    .SetProperty(p => p.DefaultSubmissionTypeId,
                        p => p.DefaultSubmissionTypeId == submissionTypeToReplaceOrDelete.Id
                            ? submissionTypeToReplaceWith != null
                                ? submissionTypeToReplaceWith.Id
                                : null
                            : p.DefaultSubmissionTypeId));

        switch (isReplacingSubmissionType)
        {
            case true when submissionTypeToReplaceWith != null:
            {
                var problemsWithoutSubmissionTypeIds = problemsQuery
                    .Where(p => p.SubmissionTypesInProblems.All(s => s.SubmissionTypeId != submissionTypeToReplaceWith.Id))
                    .Select(p => p.Id)
                    .ToList();

                // First, we add the new submission type to problems that don't have it, then we delete the old one
                await this.submissionTypesInProblemsDataService
                    .AddMany(problemsWithoutSubmissionTypeIds.Select(pId => new SubmissionTypeInProblem
                    {
                        ProblemId = pId,
                        SubmissionTypeId = submissionTypeToReplaceWith.Id,
                    }));

                await this.submissionTypesInProblemsDataService.SaveChanges();
                await DeleteOldSubmissionTypeFromProblems();

                submissionTypeToReplaceOrDelete.Name += " / Deprecated";

                break;
            }
            case false:
                await this.testRunsData.ExecuteDelete(tr => submissionIds.Contains(tr.SubmissionId));
                await this.submissionsDataService.ExecuteDelete(s => submissionIds.Contains(s.Id));
                await DeleteOldSubmissionTypeFromProblems();
                this.submissionTypesDataService.Delete(submissionTypeToReplaceOrDelete);
                break;
            default:
                throw new InvalidOperationException("Submission type to replace with is null. Cannot replace.");
        }

        await this.submissionTypesDataService.SaveChanges();

        return stringBuilder.ToString();

        async Task DeleteOldSubmissionTypeFromProblems()
        {
            await this.submissionTypesInProblemsDataService
                .ExecuteDelete(stp =>
                    stp.SubmissionTypeId == submissionTypeToReplaceOrDelete.Id &&
                    problemIds.Contains(stp.ProblemId));
        }
    }

    public override async Task<SubmissionTypeAdministrationModel> Get(int id) =>
         await this.submissionTypesDataService
             .GetByIdQuery(id)
             .MapCollection<SubmissionTypeAdministrationModel>()
             .FirstAsync();

    public override async Task<SubmissionTypeAdministrationModel> Create(SubmissionTypeAdministrationModel model)
    {
        var submissionType = model.Map<SubmissionType>();
        await this.submissionTypesDataService.Add(submissionType);
        await this.submissionTypesDataService.SaveChanges();

        return model;
    }

    public override async Task<SubmissionTypeAdministrationModel> Edit(SubmissionTypeAdministrationModel model)
    {
        var submissionType =
            await this.submissionTypesDataService
                .GetByIdQuery(model.Id)
                .Include(st => st.SubmissionTypesInProblems)
                .FirstOrDefaultAsync();

        if (submissionType == null)
        {
            throw new BusinessServiceException("Submission type not found.");
        }

        submissionType.MapFrom(model);
        this.submissionTypesDataService.Update(submissionType);
        await this.submissionTypesDataService.SaveChanges();

        return model;
    }

    public override async Task Delete(int id)
    {
        await this.submissionTypesDataService.DeleteById(id);
        await this.submissionTypesDataService.SaveChanges();
    }

    private async Task AppendMessageForProblemsThatWillBeLeftWithNoSubmissionType(
        StringBuilder stringBuilder,
        List<Problem> problems)
    {
        var problemIds = problems.Select(p => p.Id);

        var problemsWithOneSubmissionType = await this.contestsDataService
            .GetAllVisible()
            .Where(c => c.ProblemGroups
                .Any(pg => pg.Problems.Any(p => problemIds.Contains(p.Id) && p.SubmissionTypesInProblems.Count == 1)))
            .Select(c => new
            {
                ContestKey = new { c.Id, c.Name },
                Problems = c.ProblemGroups.SelectMany(pg => pg.Problems.Where(p => p.SubmissionTypesInProblems.Count == 1)),
            })
            .ToDictionaryAsync(
                g => g.ContestKey,
                g => g.Problems.ToList());

        if (problemsWithOneSubmissionType.Count >= 1)
        {
            stringBuilder.Append("The following Contests are left with Problems without a submission type:");
            stringBuilder.AppendLine();
        }

        foreach (var group in problemsWithOneSubmissionType)
        {
            stringBuilder.Append($"Contest #{group.Key.Id}: {group.Key.Name}");
            stringBuilder.AppendLine();

            foreach (var problem in group.Value)
            {
                stringBuilder.Append($"- Problem: {problem.Name}");
                stringBuilder.AppendLine();
            }

            stringBuilder.AppendLine();
        }
    }
}