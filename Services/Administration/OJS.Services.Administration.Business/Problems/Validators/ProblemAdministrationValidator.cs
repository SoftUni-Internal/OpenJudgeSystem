namespace OJS.Services.Administration.Business.Problems.Validators;

using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using OJS.Common.Enumerations;
using OJS.Data.Models.Problems;
using OJS.Data.Validation;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.Problems;
using OJS.Services.Common.Data;
using OJS.Services.Common.Data.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static OJS.Workers.Common.Constants;

public class ProblemAdministrationValidator : BaseAdministrationModelValidator<ProblemAdministrationModel, int, Problem>
{
    private const int MaximumMemoryLimitInBytes = int.MaxValue;
    private readonly IProblemsDataService problemsDataService;
    private readonly IContestsActivityService contestsActivityService;
    private readonly ISubmissionTypesDataService submissionTypesDataService;
    private readonly IProblemGroupsDataService problemGroupsDataService;

    public ProblemAdministrationValidator(
        IProblemsDataService problemsDataService,
        IContestsActivityService contestsActivityService,
        ISubmissionTypesDataService submissionTypesDataService,
        IProblemGroupsDataService problemGroupsDataService,
        IContestsDataService contestsDataService)
        : base(problemsDataService)
    {
        this.problemsDataService = problemsDataService;
        this.contestsActivityService = contestsActivityService;
        this.submissionTypesDataService = submissionTypesDataService;
        this.problemGroupsDataService = problemGroupsDataService;

        this.RuleFor(model => model.Name)
                .Length(1, ConstraintConstants.Problem.NameMaxLength)
                .When(x => x.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model.SourceCodeSizeLimit)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Source code size limit cannot be zero or less.")
                .When(x => x.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model.MaximumPoints)
                .GreaterThanOrEqualTo((short)0)
                .WithMessage("Maximum points cannot be zero or less.")
                .When(x => x.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model.SubmissionTypes.Count)
                .GreaterThanOrEqualTo(1)
                .WithMessage("There must be at least one submission type.")
                .When(x => x.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model.ContestId)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Must select valid contest.")
                .When(x => x.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model.CheckerId)
                .NotNull()
                .WithMessage("Checker cannot be null")
                .GreaterThanOrEqualTo(1)
                .WithMessage("Checker must be valid type")
                .When(x => x.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model.Id)
                .MustAsync(async (id, _) => await this.ContestMustNotBeActive(id))
                .When(x => x.OperationType is CrudOperationType.Delete);

        this.RuleFor(model => model)
                .CustomAsync(this.MustHaveValidTimeAndMemoryLimits)
                .When(x => x.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model)
            .MustAsync(async (model, _) => await this.MustHaveValidProblemGroupId(model))
            .WithMessage("Invalid value for \"Problem Group Order By\" has been provided.")
            .WhenAsync(async (x, _) => x.OperationType is CrudOperationType.Create or CrudOperationType.Update &&
                                      await contestsDataService.IsWithRandomTasksById(x.ContestId));
    }

    private async Task<bool> ContestMustNotBeActive(int problemId)
    {
        var contestId = await this.problemsDataService
            .GetByIdQuery(problemId)
            .Select(x => x.ProblemGroup.ContestId)
            .FirstAsync();

        return !await this.contestsActivityService.IsContestActive(contestId);
    }

    private async Task MustHaveValidTimeAndMemoryLimits(
        ProblemAdministrationModel model,
        ValidationContext<ProblemAdministrationModel> context,
        CancellationToken cancellationToken)
    {
        var problemSubmissionTypes = model.SubmissionTypes;
        var submissionTypeIds = problemSubmissionTypes.Select(st => st.Id).ToList();
        var selectedSubmissionTypes =
            await this.submissionTypesDataService
                .GetQuery(st => submissionTypeIds.Contains(st.Id))
                .ToListAsync(cancellationToken);

        foreach (var problemSubmissionType in problemSubmissionTypes)
        {
            var submissionType = selectedSubmissionTypes.Single(st => st.Id == problemSubmissionType.Id);

            var timeLimit = problemSubmissionType.TimeLimit ?? model.TimeLimit;
            var maxTimeLimit = submissionType.MaxAllowedTimeLimitInMilliseconds ?? MaxTimeLimitInMilliseconds;
            if (timeLimit < MinimumTimeLimitInMilliseconds ||
                timeLimit > maxTimeLimit)
            {
                context.AddFailure(new ValidationFailure(
                    nameof(ProblemAdministrationModel.SubmissionTypes),
                    $"Time limit for {problemSubmissionType.Name} must be between {MinimumTimeLimitInMilliseconds} and {maxTimeLimit}."));
            }

            var memoryLimit = problemSubmissionType.MemoryLimit ?? model.MemoryLimit;
            var maxMemoryLimit = submissionType.MaxAllowedMemoryLimitInBytes ?? MaximumMemoryLimitInBytes;
            if (memoryLimit < MinimumMemoryLimitInBytes ||
                memoryLimit > maxMemoryLimit)
            {
                context.AddFailure(new ValidationFailure(
                    nameof(ProblemAdministrationModel.SubmissionTypes),
                    $"Memory limit for {problemSubmissionType.Name} must be between {MinimumMemoryLimitInBytes} and {maxMemoryLimit}."));
            }
        }
    }

    private async Task<bool> MustHaveValidProblemGroupId(ProblemAdministrationModel model)
    {
        var problemGroups = await this.problemGroupsDataService
            .GetAllByContest(model.ContestId)
            .Select(pg => pg.Id)
            .ToListAsync();

        return new HashSet<int>(problemGroups).Contains(model.ProblemGroupId);
    }
}