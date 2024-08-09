namespace OJS.Services.Administration.Data.Implementations;

using OJS.Data;
using OJS.Data.Models;
using System.Collections.Generic;
using System.Linq;

public class SubmissionTypesInProblemsDataService : AdministrationDataService<SubmissionTypeInProblem>, ISubmissionTypesInProblemsDataService
{
    public SubmissionTypesInProblemsDataService(OjsDbContext submissionTypesInProblems)
        : base(submissionTypesInProblems)
    {
    }

    public IQueryable<SubmissionTypeInProblem> GetAllByProblem(int problemId)
        => this.GetQuery(stp => stp.ProblemId == problemId);

    public IQueryable<SubmissionTypeInProblem> GetByProblemIds(IEnumerable<int> problemIds)
        => this.GetQuery(stp => problemIds.Contains(stp.ProblemId));
}