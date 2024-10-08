﻿namespace OJS.Services.Ui.Business.Validations.Implementations.Submissions;

using OJS.Services.Common.Models.Users;
using OJS.Services.Common.Validation;
using OJS.Services.Ui.Models.Submissions;

public interface ISubmissionDetailsValidationService : IValidationService<(SubmissionDetailsServiceModel, UserInfoModel, bool)>
{
}