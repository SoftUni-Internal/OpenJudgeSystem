namespace OJS.Services.Ui.Business.Validations.Implementations.Contests;

using OJS.Services.Ui.Models.Contests;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Models;

public class ContestDetailsValidationService(IDatesService dates) : IContestDetailsValidationService
{
    public ValidationResult GetValidationResult((ContestDetailsServiceModel?, ContestCategoryServiceModel?, bool) item)
    {
        var (contest, contestCategory, isUserAdminOrLecturerInContest) = item;

        if (isUserAdminOrLecturerInContest)
        {
            return ValidationResult.Valid();
        }

        var contestIsVisible = contest?.IsVisible == true || contest?.VisibleFrom <= dates.GetUtcNow();

        return contest == null || contest.IsDeleted || !contestIsVisible || contestCategory is not { IsVisible: true }
            ? ValidationResult.Invalid(ValidationMessages.Contest.NotFound)
            : ValidationResult.Valid();
    }
}