namespace OJS.Services.Administration.Business.Contests.Validators;

using System;
using FluentValidation;
using OJS.Common.Enumerations;
using OJS.Services.Administration.Models.Contests;
using OJS.Services.Common.Validation;

public class ContestsBulkEditModelValidator : BaseValidator<ContestsBulkEditModel>
{
    public ContestsBulkEditModelValidator()
    {
        this.RuleFor(model => model.EndTime)
            .GreaterThan(model => model.StartTime)
            .WithMessage("End Time must be greater than Start Time.")
            .When(model => model.StartTime.HasValue);

        this.RuleFor(model => model.PracticeEndTime)
            .GreaterThan(model => model.PracticeStartTime)
            .WithMessage("Practice end time must be greater than Practice start time.")
            .When(model => model.PracticeStartTime.HasValue);

        this.RuleFor(model => model.Type)
            .Must(BeAValidContestType)
            .WithMessage("There is no contest type with this value.")
            .When(model => model.Type != null);

        this.RuleFor(model => model.LimitBetweenSubmissions)
            .GreaterThanOrEqualTo(0)
            .WithMessage("The limit between submissions should be a positive integer.")
            .When(model => model.LimitBetweenSubmissions.HasValue);
    }

    private static bool BeAValidContestType(string? type)
    {
        var isValid = Enum.TryParse<ContestType>(type, true, out _);
        return isValid;
    }
}