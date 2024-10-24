namespace OJS.Services.Ui.Business.Validations.Implementations.Contests;

using OJS.Services.Ui.Models.Contests;
using OJS.Services.Common.Validation;

public interface IContestDetailsValidationService : IValidationService<(ContestDetailsServiceModel?, ContestCategoryServiceModel?, bool)>
{
}