namespace OJS.Services.Ui.Business.Validations.Implementations.Submissions;

using OJS.Services.Common.Models;
using OJS.Services.Common.Models.Users;
using OJS.Services.Ui.Business.Validations.Implementations.Contests;
using OJS.Services.Ui.Models.Participants;
using OJS.Services.Ui.Models.Submissions;

public class SubmissionResultsValidationService : ISubmissionResultsValidationService
{
    public ValidationResult GetValidationResult(
        (UserInfoModel, ProblemForSubmissionDetailsServiceModel?, ParticipantServiceModel?, bool) validationInput)
    {
        var (userInfoModel, problem, participant, isOfficial) = validationInput;

        if (problem == null)
        {
            return ValidationResult.Invalid(ValidationMessages.Problem.NotFound);
        }

        if (participant == null && !userInfoModel.IsAdminOrLecturer && isOfficial)
        {
            return ValidationResult.Invalid(ValidationMessages.Participant.NotRegisteredForExam);
        }

        if (!problem.ShowResults)
        {
            return ValidationResult.Invalid(ValidationMessages.Problem.ProblemSubmissionsNotAvailable);
        }

        return ValidationResult.Valid();
    }
}