﻿namespace OJS.Services.Administration.Business.SubmissionTypes;

using OJS.Data.Models.Submissions;
using OJS.Services.Administration.Models.SubmissionTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISubmissionTypesBusinessService : IAdministrationOperationService<SubmissionType, int, SubmissionTypesAdministrationModel>
{
    Task<List<SubmissionTypesInProblemView>> GetForProblem();
}