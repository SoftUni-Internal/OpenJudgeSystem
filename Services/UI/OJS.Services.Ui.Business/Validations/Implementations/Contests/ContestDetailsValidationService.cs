namespace OJS.Services.Ui.Business.Validations.Implementations.Contests;

using OJS.Services.Ui.Models.Contests;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Models;

public class ContestDetailsValidationService : IContestDetailsValidationService
{
    private readonly IContestCategoriesBusinessService categoriesService;
    private readonly IDatesService dates;

    public ContestDetailsValidationService(
        IContestCategoriesBusinessService categoriesService,
        IDatesService dates)
    {
        this.categoriesService = categoriesService;
        this.dates = dates;
    }

    public ValidationResult GetValidationResult((ContestDetailsServiceModel?, bool) item)
    {
        var (contest, isUserAdminOrLecturerInContest) = item;

        var contestIsVisible = contest?.IsVisible == true || contest?.VisibleFrom <= this.dates.GetUtcNow();

        if (contest == null ||
            contest.IsDeleted ||
            ((contest.Category == null || !contest.Category!.IsVisible || contest.Category!.IsDeleted ||
              !contestIsVisible ||
              this.categoriesService.IsCategoryChildOfInvisibleParentRecursive(contest.CategoryId)) && !isUserAdminOrLecturerInContest))
        {
            return ValidationResult.Invalid(ValidationMessages.Contest.NotFound);
        }

        return ValidationResult.Valid();
    }
}