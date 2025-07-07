namespace OJS.Services.Administration.Business.SubmissionTypes.Validators;

using OJS.Data.Models.Submissions;
using OJS.Services.Common.Models.Users;
using OJS.Services.Common.Validation;

public interface IDeleteOrReplaceSubmissionTypeValidationService : IValidationServiceAsync<(
    int,
    int?,
    SubmissionType?,
    SubmissionType?,
    bool shouldDoSubmissionsDeletion,
    UserInfoModel user)>
{
}