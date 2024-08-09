namespace OJS.Services.Administration.Data;

using OJS.Data.Models;
using OJS.Services.Common.Data;
using OJS.Services.Infrastructure;
using System.Collections.Generic;
using System.Linq;

public interface ISubmissionTypesInProblemsDataService : IService, IDataService<SubmissionTypeInProblem>
{
    IQueryable<SubmissionTypeInProblem> GetAllByProblem(int problemId);

    IQueryable<SubmissionTypeInProblem> GetByProblemIds(IEnumerable<int> problemIds);
}