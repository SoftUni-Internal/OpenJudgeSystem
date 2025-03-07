namespace OJS.Services.Administration.Business.Contests.Validators;

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OJS.Common.Enumerations;
using OJS.Data.Models.Contests;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.Contests;
using OJS.Services.Common.Data;
using OJS.Services.Common.Data.Validation;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Threading.Tasks;

public class ContestAdministrationModelValidator : BaseAdministrationModelValidator<ContestAdministrationModel, int, Contest>
{
    private const int ProblemGroupsCountLimit = 40;
    private readonly IContestsActivityService activityService;
    private readonly IContestsDataService contestService;

    public ContestAdministrationModelValidator(
        IContestsActivityService activityService,
        IContestsDataService contestService)
        : base(contestService)
    {
        this.activityService = activityService;
        this.contestService = contestService;

        this.RuleFor(model => model.Name)
            .Length(4, 100)
            .When(model => model.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model.EndTime)
            .GreaterThan(model => model.StartTime)
            .WithMessage("End Time must be greater than Start Time")
            .When(model => model.StartTime.HasValue)
            .When(model => model.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model.PracticeEndTime)
            .GreaterThan(model => model.PracticeStartTime)
            .WithMessage("Practice end time must be greater than Practice start time")
            .When(model => model.PracticeStartTime.HasValue)
            .When(model => model.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model.Type)
            .NotNull()
            .NotEmpty()
            .Must(BeAValidContestType)
            .WithMessage("There is no contest type with this value")
            .When(model => model.OperationType is CrudOperationType.Create or CrudOperationType.Update);

        this.RuleFor(model => model)
            .MustAsync(async (model, _)
                => await this.ValidateActiveContestCannotEditDurationTypeOnEdit(model))
            .WithMessage("Cannot change duration or type in an active contest.")
            .WithName("Duration")
            .NotNull()
            .When(model => model.OperationType is CrudOperationType.Update);

        this.RuleFor(model => model)
            .Must(ValidateContestProblemGroups)
            .WithMessage($"The number of problem groups cannot be less than 0 and more than {ProblemGroupsCountLimit}")
            .WithName("Number of problem groups")
            .NotNull()
            .When(model => model.OperationType == CrudOperationType.Create);

        this.RuleFor(model => model.Id)
            .MustAsync(async (x, _) => !await this.activityService.IsContestActive(x))
            .WithMessage("Cannot delete active contest")
            .NotNull()
            .When(model => model.OperationType is CrudOperationType.Delete);

        this.RuleFor(model => model.Duration)
            .NotNull()
            .Must((model, _) => model.Duration!.Value != TimeSpan.Zero)
            .WithMessage("The duration's value must be set when the contest is of type Online Exam.")
            .When(model => model.OperationType is CrudOperationType.Create or CrudOperationType.Update &&
                           model.Type == ContestType.OnlinePracticalExam.ToString());
    }

    private static bool BeAValidContestType(string? type)
    {
        var isValid = Enum.TryParse<ContestType>(type, true, out _);
        return isValid;
    }

    private static bool ValidateContestProblemGroups(ContestAdministrationModel model)
    {
        if (model.IsWithRandomTasks)
        {
            if (model.NumberOfProblemGroups <= 0)
            {
                return false;
            }

            if (model.NumberOfProblemGroups > ProblemGroupsCountLimit)
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> ValidateActiveContestCannotEditDurationTypeOnEdit(ContestAdministrationModel model)
    {
        var contest = await this.contestService.GetByIdQuery(model.Id).FirstOrDefaultAsync();

        if (contest is null)
        {
            return false;
        }

        var isActive = await this.activityService.IsContestActive(contest.Map<ContestForActivityServiceModel>());

        if (string.IsNullOrEmpty(model.Type))
        {
            return false;
        }

        var isValid = Enum.TryParse(model.Type, true, out ContestType contestType);
        if (!isValid)
        {
            return false;
        }

        return !isActive ||
               (contest.Duration == model.Duration &&
                contest.Type == contestType);
    }
}