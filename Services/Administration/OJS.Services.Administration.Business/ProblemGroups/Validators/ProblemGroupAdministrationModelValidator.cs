namespace OJS.Services.Administration.Business.ProblemGroups.Validators;

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OJS.Common;
using OJS.Common.Enumerations;
using OJS.Data.Models.Problems;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.ProblemGroups;
using OJS.Services.Common.Data;
using OJS.Services.Common.Data.Validation;
using System;
using System.Threading.Tasks;

public class ProblemGroupAdministrationModelValidator : BaseAdministrationModelValidator<ProblemGroupsAdministrationModel, int, ProblemGroup>
{
    private readonly IContestsDataService contestsDataService;
    private readonly IProblemGroupsDataService problemGroupsDataService;

    public ProblemGroupAdministrationModelValidator(
        IContestsActivityService contestsActivityService,
        IContestsDataService contestsDataService,
        IProblemGroupsDataService problemGroupsDataService)
        : base(problemGroupsDataService)
    {
        this.contestsDataService = contestsDataService;
        this.problemGroupsDataService = problemGroupsDataService;

        this.RuleFor(model => model.OrderBy)
            .NotNull()
            .GreaterThanOrEqualTo(0)
            .WithMessage("Order by must be greater or equal to 0")
            .When(x => x.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model)
            .MustAsync(async (model, _) => await this.NotBeActiveOrOnlineContest(model))
            .WithMessage($"{Resources.ProblemGroupsControllers.CanEditOrderByOnlyInContestWithRandomTasks}")
            .When(x => x.OperationType is CrudOperationType.Update or CrudOperationType.Delete);

        this.RuleFor(model => model)
            .MustAsync(async (model, _) => await this.contestsDataService.IsWithRandomTasksById(model.Contest.Id) &&
                                           !await contestsActivityService.IsContestActive(model.Contest.Id))
            .WithMessage($"" +
                         $"{Resources.ProblemGroupsControllers.CanCreateOnlyInContestWithRandomTasks}" +
                         $" or " +
                         $"{Resources.ProblemGroupsControllers.ActiveContestCannotAddProblemGroup}")
            .When(x => x.OperationType == CrudOperationType.Create);

        this.RuleFor(x => x.Contest.Id)
            .MustAsync(async (id, _) => !await contestsActivityService.IsContestActive(id))
            .WithMessage("Cannot delete problem group when the related contest is active")
            .When(x => x.OperationType is CrudOperationType.Update);
    }

    private async Task<bool> NotBeActiveOrOnlineContest(ProblemGroupsAdministrationModel model)
    {
        var problemGroup = await this.problemGroupsDataService.GetByIdQuery(model.Id).AsNoTracking().FirstOrDefaultAsync();

        var contestIdToCheck =
            model.OperationType is CrudOperationType.Update ? model.Contest.Id : problemGroup!.ContestId;

        return Math.Abs(problemGroup!.OrderBy - model.OrderBy) <= 0 ||
               await this.contestsDataService.IsWithRandomTasksById(contestIdToCheck);
    }
}