namespace OJS.Services.Administration.Business.SubmissionTypes;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Data;
using OJS.Data.Models;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Submissions;
using OJS.Services.Administration.Business.SubmissionTypes.Validators;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.SubmissionTypes;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SubmissionTypesBusinessService : AdministrationOperationService<SubmissionType, int, SubmissionTypeAdministrationModel>, ISubmissionTypesBusinessService
{
    private readonly ISubmissionTypesDataService submissionTypesDataService;
    private readonly IContestsDataService contestsDataService;
    private readonly ISubmissionsDataService submissionsDataService;
    private readonly IParticipantScoresDataService participantScoresData;
    private readonly ITestRunsDataService testRunsData;
    private readonly ISubmissionTypesInProblemsDataService submissionTypesInProblemsDataService;
    private readonly IProblemsDataService problemsDataService;
    private readonly IDeleteOrReplaceSubmissionTypeValidationService deleteOrReplaceSubmissionTypeValidationService;
    private readonly IDatesService datesService;
    private readonly ITransactionsProvider transactionsProvider;

    public SubmissionTypesBusinessService(
        ISubmissionTypesDataService submissionTypesDataService,
        IContestsDataService contestsDataService,
        ISubmissionsDataService submissionsDataService,
        IParticipantScoresDataService participantScoresData,
        ITestRunsDataService testRunsData,
        ISubmissionTypesInProblemsDataService submissionTypesInProblemsDataService,
        IProblemsDataService problemsDataService,
        IDeleteOrReplaceSubmissionTypeValidationService deleteOrReplaceSubmissionTypeValidationService,
        IDatesService datesService,
        ITransactionsProvider transactionsProvider)
    {
        this.submissionTypesDataService = submissionTypesDataService;
        this.contestsDataService = contestsDataService;
        this.submissionsDataService = submissionsDataService;
        this.participantScoresData = participantScoresData;
        this.testRunsData = testRunsData;
        this.submissionTypesInProblemsDataService = submissionTypesInProblemsDataService;
        this.deleteOrReplaceSubmissionTypeValidationService = deleteOrReplaceSubmissionTypeValidationService;
        this.datesService = datesService;
        this.transactionsProvider = transactionsProvider;
        this.problemsDataService = problemsDataService;
    }

    public async Task<List<SubmissionTypesInProblemView>> GetForProblem() =>
        await this.submissionTypesDataService.GetAll().MapCollection<SubmissionTypesInProblemView>().ToListAsync();

    public async Task<string> ReplaceSubmissionType(ReplaceSubmissionTypeServiceModel model)
    {
        var stringBuilder = new StringBuilder();

        if (model.SubmissionTypeToReplaceWith.HasValue && model.SubmissionTypeToReplace == model.SubmissionTypeToReplaceWith.Value)
        {
            throw new BusinessServiceException("Cannot replace submission type with identical submission type");
        }

        var submissionTypeToReplaceOrDelete = await this.submissionTypesDataService
            .GetByIdQuery(model.SubmissionTypeToReplace)
            .FirstOrDefaultAsync();

        SubmissionType? submissionTypeToReplaceWith = null;

        if (model.SubmissionTypeToReplaceWith.HasValue)
        {
            submissionTypeToReplaceWith = await this.submissionTypesDataService
                .GetByIdQuery(model.SubmissionTypeToReplaceWith!.Value)
                .FirstOrDefaultAsync();
        }

        bool shouldDoSubmissionsDeletion = !model.SubmissionTypeToReplaceWith.HasValue;

        var validationResult = this.deleteOrReplaceSubmissionTypeValidationService.GetValidationResult(
            (submissionTypeToReplaceOrDelete, submissionTypeToReplaceWith, shouldDoSubmissionsDeletion));

        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        var stopWatch = Stopwatch.StartNew();
        var problems = await this.problemsDataService
            .GetQuery(p => p.SubmissionTypesInProblems
                .Any(st => st.SubmissionTypeId == submissionTypeToReplaceOrDelete!.Id))
            .Include(p => p.SubmissionTypesInProblems)
            .ToListAsync();

        if (shouldDoSubmissionsDeletion)
        {
            stringBuilder.Append(
                $"Submission type \"{submissionTypeToReplaceOrDelete!.Name}\" is deleted and all submissions associated with it");
            stringBuilder.AppendLine();

            // Must be called before delete so problems with 1 submission type are calculated correctly
            await this.AppendMessageForProblemsThatWillBeLeftWithNoSubmissionType(stringBuilder, problems);
        }
        else
        {
            stringBuilder.Append(
                $"Submission type \"{submissionTypeToReplaceOrDelete!.Name}\" is deleted and replaced with \"{submissionTypeToReplaceWith!.Name}\"");
        }

        await problems.Chunk(100).ForEachSequential(async problemChunk =>
        {
            var problemIdsInChunk = problemChunk.Select(p => p.Id).ToList();

            var submissionsInChunk = this.submissionsDataService
                .GetAllByProblems(problemIdsInChunk)
                .Where(s => s.SubmissionTypeId == submissionTypeToReplaceOrDelete!.Id);

            if (shouldDoSubmissionsDeletion)
            {
                await this.DeleteSubmissions(submissionsInChunk);
            }
            else
            {
                await this.ReplaceSubmissionTypeInSubmissionsAndProblems(
                    problemChunk,
                    submissionsInChunk,
                    submissionTypeToReplaceOrDelete!,
                    submissionTypeToReplaceWith!);
            }

            await this.submissionTypesDataService.SaveChanges();
            Console.WriteLine(stopWatch.ElapsedMilliseconds);
            stopWatch.Restart();

            this.submissionTypesInProblemsDataService
                .Delete(stp =>
                    stp.SubmissionTypeId == submissionTypeToReplaceOrDelete!.Id &&
                    problemIdsInChunk.Contains(stp.ProblemId));
        });

        this.submissionTypesDataService.Delete(submissionTypeToReplaceOrDelete!);

        await this.submissionTypesDataService.SaveChanges();

        return stringBuilder.ToString();
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

    private async Task ReplaceSubmissionTypeInSubmissionsAndProblems(
        Problem[] problems,
        IQueryable<Submission> submissionsQuery,
        SubmissionType submissionTypeToReplaceOrDelete,
        SubmissionType submissionTypeToReplaceWith)
    {
        await submissionsQuery.UpdateFromQueryAsync(s =>
            new Submission
            {
                SubmissionTypeId = submissionTypeToReplaceWith!.Id,
                ProcessingComment = $"{s.ProcessingComment}{Environment.NewLine}The submission type of this submission was updated from {submissionTypeToReplaceOrDelete.Name} to {submissionTypeToReplaceWith.Name} and changes to the problem or submission might be needed for correct execution.",
            });

        var problemsWithoutSubmissionType = problems
            .Where(p => p.SubmissionTypesInProblems.All(s => s.SubmissionTypeId != submissionTypeToReplaceWith.Id));

        foreach (var problem in problemsWithoutSubmissionType)
        {
            await this.submissionTypesInProblemsDataService.Add(new SubmissionTypeInProblem
            {
                Problem = problem, SubmissionType = submissionTypeToReplaceWith!,
            });
        }
    }

    private async Task DeleteSubmissions(IQueryable<Submission> submissionsQuery)
    {
        var submissions = submissionsQuery.ToList();
        await this.testRunsData.DeleteBySubmissions(submissions.Select(s => s.Id).ToList());

        this.submissionsDataService.DeleteMany(submissions);
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