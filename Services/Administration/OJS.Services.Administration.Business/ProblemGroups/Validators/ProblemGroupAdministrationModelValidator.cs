﻿namespace OJS.Services.Administration.Business.ProblemGroups.Validators;

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OJS.Common;
using OJS.Common.Enumerations;
using OJS.Data.Models.Problems;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.ProblemGroups;
using OJS.Services.Common;
using OJS.Services.Common.Validation;
using System;
using System.Threading.Tasks;

public class ProblemGroupAdministrationModelValidator : BaseAdministrationModelValidator<ProblemGroupsAdministrationModel, int, ProblemGroup>
{
    private readonly IContestsActivityService contestsActivityService;
    private readonly IContestsDataService contestsDataService;
    private readonly IProblemGroupsDataService problemGroupsDataService;

    public ProblemGroupAdministrationModelValidator(
        IContestsActivityService contestsActivityService,
        IContestsDataService contestsDataService,
        IProblemGroupsDataService problemGroupsDataService)
        : base(problemGroupsDataService)
    {
        this.contestsActivityService = contestsActivityService;
        this.contestsDataService = contestsDataService;
        this.problemGroupsDataService = problemGroupsDataService;

        this.RuleFor(model => model.OrderBy)
            .NotNull()
            .GreaterThanOrEqualTo(0)
            .When(x => x.OperationType is CrudOperationType.Create or CrudOperationType.Update)
            .WithMessage("Order by must be greater or equal to 0");

        this.RuleFor(model => model)
            .MustAsync(async (model, _) => await this.NotBeActiveOrOnlineContest(model))
            .When(x => x.OperationType is CrudOperationType.Update or CrudOperationType.Delete)
            .WithMessage(
                $"{string.Format(Resources.ProblemGroupsControllers.CanEditOrderbyOnlyInOnlineContest, ContestType.OnlinePracticalExam.ToString())}");

        this.RuleFor(model => model)
            .MustAsync(async (model, _) => await this.IsOnline(model.Contest.Id) && !await this.contestsActivityService.IsContestActive(model.Contest.Id))
            .When(x => x.OperationType == CrudOperationType.Create)
            .WithMessage($"" +
                         $"{string.Format(Resources.ProblemGroupsControllers.CanCreateOnlyInOnlineContest, ContestType.OnlinePracticalExam.ToString())}" +
                         $" or " +
                         $"{Resources.ProblemGroupsControllers.ActiveContestCannotAddProblemGroup}");

        this.RuleFor(x => x.Contest.Id)
            .MustAsync(async (id, _) => !await this.contestsActivityService.IsContestActive(id))
            .WithMessage("Cannot delete problem group when the related contest is active")
            .When(x => x.OperationType is CrudOperationType.Update);
    }

    private async Task<bool> NotBeActiveOrOnlineContest(ProblemGroupsAdministrationModel model)
    {
        var problemGroup = await this.problemGroupsDataService.GetByIdQuery(model.Id).AsNoTracking().FirstOrDefaultAsync();

        var contestIdToCheck =
            model.OperationType is CrudOperationType.Update ? model.Contest.Id : problemGroup!.ContestId;

        if (Math.Abs(problemGroup!.OrderBy - model.OrderBy) > 0 && !await this.IsOnline(contestIdToCheck))
        {
            return false;
        }

        return true;
    }

    private async Task<bool> IsOnline(int id)
        => await this.contestsDataService.IsOnlineById(id);
}