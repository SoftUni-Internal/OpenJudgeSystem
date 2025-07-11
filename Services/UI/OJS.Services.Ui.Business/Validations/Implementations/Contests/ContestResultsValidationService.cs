namespace OJS.Services.Ui.Business.Validations.Implementations.Contests;

using OJS.Services.Common.Models.Contests;
using OJS.Services.Common.Validation.Helpers;
using OJS.Services.Infrastructure.Models;

public class ContestResultsValidationService(INotDefaultValueValidationHelper notDefaultValueValidationHelper)
    : IContestResultsValidationService
{
    public ValidationResult GetValidationResult((IContestActivityServiceModel?, bool, bool, bool) item)
    {
        var (contest, fullResults, isOfficial, isUserAdminOrLecturerInContest) = item;

        var contestNullValidationResult = notDefaultValueValidationHelper.ValidateValueIsNotDefault(contest, nameof(contest));

        if (!contestNullValidationResult.IsValid)
        {
            return contestNullValidationResult;
        }

        if (isUserAdminOrLecturerInContest)
        {
            return ValidationResult.Valid();
        }

        if (fullResults && !isUserAdminOrLecturerInContest)
        {
            return ValidationResult.AccessDenied(ValidationMessages.Participant.NoPrivilegesForContestResults);
        }

        if (!contest!.IsVisible)
        {
            return ValidationResult.Invalid(ValidationMessages.Participant.ResultsNotVisibleForContest);
        }

        if (isOfficial && contest is { CanBeCompeted: false, CompeteUserActivity: null })
        {
            return ValidationResult.Invalid(ValidationMessages.Participant.ResultsNotVisibleForContest);
        }

        if (!isOfficial && contest is { CanBePracticed: false, PracticeUserActivity: null })
        {
            return ValidationResult.Invalid(ValidationMessages.Participant.ResultsNotVisibleForContest);
        }

        return ValidationResult.Valid();
    }
}