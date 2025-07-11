namespace OJS.Services.Administration.Business.SubmissionTypes.Validators;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Submissions;
using OJS.Services.Administration.Data;
using OJS.Services.Common.Models.Users;
using OJS.Services.Infrastructure.Models;

public class DeleteOrReplaceSubmissionTypeValidationService(ISubmissionsDataService submissionsDataService)
    : IDeleteOrReplaceSubmissionTypeValidationService
{
    public async Task<ValidationResult> GetValidationResult((int, int?, SubmissionType?, SubmissionType?, bool, UserInfoModel) item)
    {
        var (
            requestSubmissionTypeToReplaceValue,
            requestSubmissionTypeToReplaceWithValue,
            submissionTypeToReplaceOrDelete,
            submissionTypeToReplaceWith,
            isReplacingSubmissionType,
            user) = item;

        if (requestSubmissionTypeToReplaceValue == requestSubmissionTypeToReplaceWithValue)
        {
            return ValidationResult.Invalid("Cannot replace submission type with identical submission type");
        }

        if (submissionTypeToReplaceOrDelete == null)
        {
            return ValidationResult.Invalid("Submission type does not exist");
        }

        if (isReplacingSubmissionType && submissionTypeToReplaceWith == null)
        {
            return ValidationResult.Invalid("Submission type to replace with not found");
        }

        var submissionsByRegularUsersInTheLastMonthCount = await submissionsDataService
            .GetAllBySubmissionTypeSentByRegularUsersInTheLastNMonths(submissionTypeToReplaceOrDelete.Id, 1)
            .CountAsync();

        if (submissionsByRegularUsersInTheLastMonthCount > 0 && (!isReplacingSubmissionType || !user.IsDeveloper))
        {
            return ValidationResult.Invalid("This submission type has been used in the last month and cannot be considered as deprecated. Try again later.");
        }

        return ValidationResult.Valid();
    }
}