namespace OJS.Services.Administration.Data.Implementations;

using FluentExtensions.Extensions;
using OJS.Common;
using OJS.Data;
using OJS.Data.Models.Tests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class TestRunsDataService : AdministrationDataService<TestRun>, ITestRunsDataService
{
    public TestRunsDataService(OjsDbContext testRuns)
        : base(testRuns)
    {
    }

    public IQueryable<TestRun> GetAllByTest(int testId) =>
        this.GetQuery(tr => tr.TestId == testId);

    public async Task DeleteByProblem(int problemId)
    {
        this.Delete(tr => tr.Test.ProblemId == problemId);
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

    public Task DeleteBySubmissions(IEnumerable<int> submissionIds)
        => this.GetQuery(tr => submissionIds.Contains(tr.SubmissionId))
            .DeleteFromQueryAsync();

    public async Task DeleteInBatchesBySubmissionIds(IEnumerable<int> submissionIds)
        => await submissionIds
            .Chunk(GlobalConstants.BatchOperationsChunkSize)
            .ForEachSequential(async chunk =>
            {
                this.Delete(t => chunk.Contains(t.SubmissionId));

                await this.SaveChanges();
            });
}