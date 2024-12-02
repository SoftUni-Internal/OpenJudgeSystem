namespace OJS.Services.Administration.Business.SubmissionTypes.Validators;

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Submissions;
using OJS.Services.Administration.Data;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Models;

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

    public async Task<ValidationResult> GetValidationResult((int, int?, SubmissionType?, SubmissionType?, bool) item)
    {
        var (
            requestSubmissionTypeToReplaceValue,
            requestSubmissionTypeToReplaceWithValue,
            submissionTypeToReplaceOrDelete,
            submissionTypeToReplaceWith,
            shouldDoSubmissionsDeletion) = item;

        if (requestSubmissionTypeToReplaceWithValue.HasValue && requestSubmissionTypeToReplaceValue == requestSubmissionTypeToReplaceWithValue)
        {
            return ValidationResult.Invalid("Cannot replace submission type with identical submission type");
        }

        if (submissionTypeToReplaceOrDelete == null)
        {
            return ValidationResult.Invalid("Submission type does not exist");
        }

        if (!shouldDoSubmissionsDeletion && submissionTypeToReplaceWith == null)
        {
            return ValidationResult.Invalid("Submission type to replace with not found");
        }

        var submissionsByRegularUsersInTheLastMonth = await this.submissionsDataService
            .GetAllBySubmissionTypeSentByRegularUsersInTheLastNMonths(submissionTypeToReplaceOrDelete.Id, 1)
            .ToListAsync();

        if (submissionsByRegularUsersInTheLastMonth.Count > 0)
        {
            return ValidationResult.Invalid("This submission type has been used in the last month and cannot be considered as deprecated. Try again later.");
        }

        return ValidationResult.Valid();
    }
}