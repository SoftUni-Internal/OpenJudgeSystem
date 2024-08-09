namespace OJS.Services.Administration.Business.SubmissionTypes.Validators;

using System.Linq;
using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Submissions;
using OJS.Services.Administration.Data;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Models;
using static OJS.Common.GlobalConstants.Roles;

public class DeleteOrReplaceSubmissionTypeValidationService : IDeleteOrReplaceSubmissionTypeValidationService
{
    private readonly ISubmissionsDataService submissionsDataService;
    private readonly IDatesService datesService;

    public DeleteOrReplaceSubmissionTypeValidationService(
        ISubmissionsDataService submissionsDataService,
        IDatesService datesService)
    {
        this.submissionsDataService = submissionsDataService;
        this.datesService = datesService;
    }

    public ValidationResult GetValidationResult((SubmissionType?, SubmissionType?, bool) item)
    {
        var (submissionTypeToReplaceOrDelete, submissionTypeToReplaceWith, shouldDoSubmissionsDeletion) = item;

        if (submissionTypeToReplaceOrDelete == null)
        {
            return ValidationResult.Invalid("Submission type does not exist");
        }

        if (!shouldDoSubmissionsDeletion && submissionTypeToReplaceWith == null)
        {
            return ValidationResult.Invalid("Submission type to replace with not found");
        }

        var administratorRoles = new string[] { Administrator, Lecturer, Developer };

        var submissionsByRegularUsersInTheLastMonth = this.submissionsDataService
            .GetQuery()
            .Include(s => s.Participant)
            .ThenInclude(p => p.User)
            .ThenInclude(u => u.UsersInRoles)
            .Where(s => s.SubmissionTypeId == submissionTypeToReplaceOrDelete.Id &&
                        s.CreatedOn > this.datesService.GetUtcNow().AddMonths(-1) &&
                        !s.Participant.User.UsersInRoles.Any(ur => administratorRoles.Contains(ur.Role.Name)))
            .ToList();

        if (submissionsByRegularUsersInTheLastMonth.Count > 0)
        {
            return ValidationResult.Invalid("This submission type has been used in the last month and cannot be considered as deprecated. Try again later.");
        }

        return ValidationResult.Valid();
    }
}