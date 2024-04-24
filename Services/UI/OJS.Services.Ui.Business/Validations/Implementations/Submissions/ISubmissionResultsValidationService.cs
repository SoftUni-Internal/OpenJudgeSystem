namespace OJS.Services.Ui.Business.Validations.Implementations.Submissions;

using OJS.Services.Common.Models.Users;
using OJS.Services.Common.Validation;
using OJS.Services.Ui.Models.Participants;
using OJS.Services.Ui.Models.Submissions;

public interface ISubmissionResultsValidationService : IValidationService<(UserInfoModel, ProblemForSubmissionDetailsServiceModel?, ParticipantServiceModel?, bool)>
{
}