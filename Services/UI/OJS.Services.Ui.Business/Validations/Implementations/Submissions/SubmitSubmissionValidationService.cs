namespace OJS.Services.Ui.Business.Validations.Implementations.Submissions;

using OJS.Common.Enumerations;
using OJS.Data.Models;
using System.Text;
using System.Threading.Tasks;
using OJS.Data.Models.Submissions;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Users;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Models;
using OJS.Services.Ui.Business.Cache;
using OJS.Services.Ui.Business.Validations.Implementations.Contests;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Cache;
using OJS.Services.Ui.Models.Participants;
using OJS.Services.Ui.Models.Submissions;
using System.Linq;

public class SubmitSubmissionValidationService : ISubmitSubmissionValidationService
{
    private readonly ILecturersInContestsCacheService lecturersInContestsCache;
    private readonly ISubmissionsDataService submissionsData;
    private readonly IContestsActivityService contestsActivity;
    private readonly IDataService<ProblemForParticipant> problemsForParticipantsData;
    private readonly IDatesService dates;

    public SubmitSubmissionValidationService(
        ILecturersInContestsCacheService lecturersInContestsCache,
        ISubmissionsDataService submissionsData,
        IContestsActivityService contestsActivity,
        IDataService<ProblemForParticipant> problemsForParticipantsData,
        IDatesService dates)
    {
        this.lecturersInContestsCache = lecturersInContestsCache;
        this.submissionsData = submissionsData;
        this.contestsActivity = contestsActivity;
        this.problemsForParticipantsData = problemsForParticipantsData;
        this.dates = dates;
    }

    public async Task<ValidationResult> GetValidationResult(
        (ProblemForSubmitCacheModel?,
        ParticipantSubmitServiceModel?,
        SubmitSubmissionServiceModel,
        SubmissionType?,
        UserInfoModel? user) item)
    {
        var (problem, participant, submitSubmissionServiceModel, submissionType, user) = item;

        if (problem == null)
        {
            return ValidationResult.Invalid(ValidationMessages.Problem.NotFound);
        }

        var participantActivity = this.contestsActivity.GetParticipantActivity(participant);

        if (participant == null || participantActivity == null)
        {
            return ValidationResult.Invalid(ValidationMessages.Participant.NotRegisteredForContest);
        }

        var isAdminOrLecturer = await this.lecturersInContestsCache
            .IsUserAdminOrLecturerInContest(participant.ContestId, participant.ContestCategoryId, user);

        if (participantActivity.IsInvalidated)
        {
            return ValidationResult.Invalid(ValidationMessages.Participant.ParticipantIsInvalidated);
        }

        if (!participantActivity.HasParticipationTimeLeft && !isAdminOrLecturer)
        {
            return ValidationResult.Invalid(ValidationMessages.Participant.ParticipationTimeEnded);
        }

        if (!participantActivity.IsActive && !isAdminOrLecturer)
        {
            return ValidationResult.Invalid(ValidationMessages.Participant.ParticipationNotActive);
        }

        var problemIdToString = problem.Id.ToString();

        var participantHasUnprocessedSubmissionForProblem =
            await this.submissionsData.HasParticipantNotProcessedSubmissionForProblem(problem.Id, participant.Id);

        if (participantHasUnprocessedSubmissionForProblem)
        {
            return ValidationResult.Invalid(
                ValidationMessages.Submission.UserHasNotProcessedSubmissionForProblem,
                problemIdToString);
        }

        if (!participant.ContestAllowParallelSubmissionsInTasks &&
            await this.submissionsData.HasParticipantNotProcessedSubmissionForContest(participant.ContestId, participant.Id))
        {
            return ValidationResult.Invalid(
                ValidationMessages.Submission.UserHasNotProcessedSubmissionForContest,
                problemIdToString);
        }

        if (string.IsNullOrWhiteSpace(submitSubmissionServiceModel.StringContent) &&
            (submitSubmissionServiceModel.ByteContent == null || submitSubmissionServiceModel.ByteContent.Length == 0))
        {
            return ValidationResult.Invalid(ValidationMessages.Submission.SubmissionEmpty, problemIdToString);
        }

        if (submitSubmissionServiceModel.Official &&
            participant.ContestType == ContestType.OnlinePracticalExam &&
            !isAdminOrLecturer)
        {
            var problemIsAssignedToParticipant = participant.Problems?.Any(pp => pp.ProblemId == problem.Id)
                ?? await this.problemsForParticipantsData
                    .Exists(pfp => pfp.ParticipantId == participant.Id && pfp.ProblemId == problem.Id);

            if (!problemIsAssignedToParticipant)
            {
                return ValidationResult.Invalid(ValidationMessages.Problem.ProblemNotAssignedToUser, problemIdToString);
            }
        }

        if (submissionType == null)
        {
            return ValidationResult.Invalid(ValidationMessages.Submission.SubmissionTypeNotFound, problemIdToString);
        }

        var isFileUpload = submitSubmissionServiceModel.StringContent == null || submitSubmissionServiceModel.ByteContent != null;

        if (isFileUpload && !submissionType.AllowedFileExtensions!.Contains(submitSubmissionServiceModel.FileExtension!))
        {
            return ValidationResult.Invalid(ValidationMessages.Submission.InvalidExtension, problemIdToString);
        }

        if (isFileUpload && !submissionType.AllowBinaryFilesUpload)
        {
            return ValidationResult.Invalid(ValidationMessages.Submission.BinaryFilesNotAllowed, problemIdToString);
        }

        if (!isFileUpload && submissionType.AllowBinaryFilesUpload)
        {
            return ValidationResult.Invalid(ValidationMessages.Submission.TextUploadNotAllowed, problemIdToString);
        }

        if (submitSubmissionServiceModel.ByteContent != null &&
            problem.SourceCodeSizeLimit < submitSubmissionServiceModel.ByteContent.Length)
        {
            return ValidationResult.Invalid(ValidationMessages.Submission.SubmissionFileTooBig, problemIdToString);
        }

        if (submitSubmissionServiceModel.StringContent != null &&
            problem.SourceCodeSizeLimit < Encoding.UTF8.GetBytes(submitSubmissionServiceModel.StringContent).Length)
        {
            return ValidationResult.Invalid(ValidationMessages.Submission.SubmissionTooLong, problemIdToString);
        }

        if (!isFileUpload && submitSubmissionServiceModel.StringContent!.Length < 5)
        {
            return ValidationResult.Invalid(ValidationMessages.Submission.SubmissionTooShort, problemIdToString);
        }

        var userSubmissionTimeLimit = this.GetUserSubmissionTimeLimit(participant);

        if (userSubmissionTimeLimit != 0)
        {
            return ValidationResult.Invalid(ValidationMessages.Submission.SubmissionWasSentTooSoon, problemIdToString);
        }

        return ValidationResult.Valid();
    }

    private int GetUserSubmissionTimeLimit(ParticipantSubmitServiceModel participant)
    {
        var limitBetweenSubmissions = participant.ContestLimitBetweenSubmissions;

        if (limitBetweenSubmissions <= 0)
        {
            return 0;
        }

        var lastSubmissionCreatedOn = participant.LastSubmissionTime ?? default;

        if (lastSubmissionCreatedOn == default)
        {
            return 0;
        }

        // check if the submission was sent after the submission time limit has passed
        var differenceBetweenSubmissions = this.dates.GetUtcNow() - lastSubmissionCreatedOn;
        // Adding 5 seconds to compensate for potential difference between server and client time
        return differenceBetweenSubmissions.TotalSeconds + 5 < limitBetweenSubmissions
            ? limitBetweenSubmissions - (int)differenceBetweenSubmissions.TotalSeconds
            : 0;
    }
}