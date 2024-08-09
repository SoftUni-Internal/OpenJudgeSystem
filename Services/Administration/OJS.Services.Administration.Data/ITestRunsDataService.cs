namespace OJS.Services.Administration.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using OJS.Data.Models.Tests;
    using OJS.Services.Common.Data;

    public interface ITestRunsDataService : IDataService<TestRun>
    {
        IQueryable<TestRun> GetAllByTest(int testId);

        Task DeleteByProblem(int problemId);

        Task DeleteByTest(int testId);

        Task DeleteBySubmission(int submissionId);

        Task DeleteBySubmissions(IEnumerable<int> submissionIds);

        Task DeleteInBatchesBySubmissionIds(IEnumerable<int> submissionIds);
    }
}