namespace OJS.Data.Models.Tests;

using OJS.Data.Models.Submissions;
using OJS.Data.Models.Common;
using OJS.Workers.Common.Models;

public class TestRun : Entity<int>
{
    public int SubmissionId { get; set; }

    public virtual Submission Submission { get; set; } = null!;

    public int TestId { get; set; }

    public virtual Test Test { get; set; } = null!;

    public int TimeUsed { get; set; }

    public long MemoryUsed { get; set; }

    public TestRunResultType ResultType { get; set; }

    public bool IsTrialTest { get; set; }

    public string? ExecutionComment { get; set; }

    public string? CheckerComment { get; set; }

    public string? ExpectedOutputFragment { get; set; }

    public string? UserOutputFragment { get; set; }
}