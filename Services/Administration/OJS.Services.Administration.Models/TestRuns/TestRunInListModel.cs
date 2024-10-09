namespace OJS.Services.Administration.Models.TestRuns;

using OJS.Data.Models.Tests;
using OJS.Services.Infrastructure.Models.Mapping;
using OJS.Workers.Common.Models;

public class TestRunInListModel : IMapFrom<TestRun>
{
    public int Id { get; set; }

    public int TimeUsed { get; set; }

    public long MemoryUsed { get; set; }

    public string? ExecutionComment { get; set; }

    public string? CheckerComment { get; set; }

    public int SubmissionId { get; set; }

    public TestRunResultType ResultType { get; set; }
}