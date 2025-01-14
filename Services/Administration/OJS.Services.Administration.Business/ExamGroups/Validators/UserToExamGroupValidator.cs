﻿namespace OJS.Services.Administration.Business.ExamGroups.Validators;

using FluentValidation;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.ExamGroups;
using OJS.Services.Common.Validation;
using System.Threading.Tasks;

public class UserToExamGroupValidator : BaseValidator<UserToExamGroupModel>
{
    private readonly IExamGroupsDataService examGroupsDataService;

    public UserToExamGroupValidator(IExamGroupsDataService examGroupsDataService)
    {
        this.examGroupsDataService = examGroupsDataService;
        this.ClassLevelCascadeMode = CascadeMode.Stop;
        this.RuleFor(x => x.UserId)
            .NotEmpty();

        this.RuleFor(x => x.ExamGroupId)
            .GreaterThan(0)
            .MustAsync(async (model, _) => await this.BeValidAndWithContest(model))
            .WithMessage("The problem group does not exists or does not have a contest connected to it.");
    }

    private async Task<bool> BeValidAndWithContest(int examGroupId) =>
        await this.examGroupsDataService
            .Exists(eg => eg.Id == examGroupId && eg.ContestId != null);
}