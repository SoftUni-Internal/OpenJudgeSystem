namespace OJS.Services.Administration.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using OJS.Data.Models.Tests;
    using OJS.Services.Common.Data;

    public interface ITestRunsDataService : IDataService<TestRun>
    {
        Task DeleteByProblem(int problemId);

        Task DeleteBySubmission(int submissionId);

        Task DeleteInBatchesBySubmissionIds(IEnumerable<int> submissionIds);
    }
}