namespace OJS.Services.Ui.Data.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using OJS.Data.Models.Tests;
    using OJS.Services.Common.Data.Implementations;
    using System.Linq;
    using System.Threading.Tasks;

    public class TestRunsDataService : DataService<TestRun>, ITestRunsDataService
    {
        public TestRunsDataService(DbContext testRuns) : base(testRuns)
        {
        }

        public IQueryable<TestRun> GetAllByTest(int testId) =>
            this.DbSet
                .Where(tr => tr.TestId == testId);

        public async Task DeleteByProblem(int problemId)
        {
            this.Delete(tr => tr.Submission.ProblemId == problemId);
            await this.SaveChanges();
        }

        public async Task DeleteByTest(int testId)
        {
            this.Delete(tr => tr.TestId == testId);
            await this.SaveChanges();
        }

        public async Task DeleteBySubmission(int submissionId)
        {
            this.Delete(tr => tr.SubmissionId == submissionId);
            await this.SaveChanges();
        }
    }
}