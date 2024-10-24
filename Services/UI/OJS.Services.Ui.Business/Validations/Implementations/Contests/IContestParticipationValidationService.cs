namespace OJS.Services.Ui.Business.Validations.Implementations.Contests;

using OJS.Services.Common.Models.Users;
using OJS.Services.Common.Validation;
using OJS.Services.Ui.Models.Contests;

public interface IContestParticipationValidationService : IValidationService<(ContestParticipationValidationServiceModel?, ContestCategoryServiceModel?, UserInfoModel?, bool)>
{
}