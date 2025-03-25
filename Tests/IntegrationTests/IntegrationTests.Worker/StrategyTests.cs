namespace IntegrationTests.Worker;

using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionDetails;
using OJS.Workers.Common.Models;

// TODO: Simplify the creation of submissions for testing
public class StrategyTests(RabbitMqAndWorkerFixture fixture) : IClassFixture<RabbitMqAndWorkerFixture>
{
    [Fact]
    public async Task TestCSharpCodeDotNet6CorrectResult()
    {
        var submission = new SubmissionForProcessingPubSubModel
        {
            Id = 543002,
            ExecutionType = ExecutionType.TestsExecution,
            ExecutionStrategy = ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck,
            CompilerType = CompilerType.CSharpDotNetCore,
            FileContent = null,
            AdditionalFiles = null,
            Code = "Console.WriteLine(\"Hello SoftUni\");",
            TimeLimit = 100,
            ExecutionStrategyBaseTimeLimit = null,
            MemoryLimit = 16777216,
            ExecutionStrategyBaseMemoryLimit = null,
            Verbosely = false,
            SimpleExecutionDetails = null,
            TestsExecutionDetails = new TestsExecutionDetailsServiceModel
            {
                MaxPoints = 100,
                TaskId = null,
                CheckerType = "trim",
                CheckerParameter = null,
                Tests =
                [
                    new TestContext { Id = 172459, Input = "0", Output = "Hello SoftUni", IsTrialTest = true, OrderBy = 0 },
                    new TestContext { Id = 172460, Input = "0", Output = "Hello SoftUni", IsTrialTest = false, OrderBy = 0 }
                ],
                TaskSkeleton = null,
                TaskSkeletonAsString = null
            }
        };

        await fixture.PublishMessage(submission);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
        var receivedSubmission = await fixture.ConsumeMessage(cts.Token, submission.Id);

        // Assertions
        Assert.NotNull(receivedSubmission);
        Assert.Equal(submission.Id, receivedSubmission!.Id);
        Assert.NotNull(receivedSubmission.ExecutionResult);
        Assert.True(receivedSubmission.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.CorrectAnswer), "All tests should pass");
    }

    [Fact]
    public async Task TestCSharpCodeDotNet6CompilationError()
    {
        var submission = new SubmissionForProcessingPubSubModel
        {
            Id = 543003,
            ExecutionType = ExecutionType.TestsExecution,
            ExecutionStrategy = ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck,
            CompilerType = CompilerType.CSharpDotNetCore,
            FileContent = null,
            AdditionalFiles = null,
            Code = "This is not proper c# code!",
            TimeLimit = 100,
            ExecutionStrategyBaseTimeLimit = null,
            MemoryLimit = 16777216,
            ExecutionStrategyBaseMemoryLimit = null,
            Verbosely = false,
            SimpleExecutionDetails = null,
            TestsExecutionDetails = new TestsExecutionDetailsServiceModel
            {
                MaxPoints = 100,
                TaskId = null,
                CheckerType = "trim",
                CheckerParameter = null,
                Tests =
                [
                    new TestContext { Id = 172459, Input = "0", Output = "Hello SoftUni", IsTrialTest = true, OrderBy = 0 },
                    new TestContext { Id = 172460, Input = "0", Output = "Hello SoftUni", IsTrialTest = false, OrderBy = 0 }
                ],
                TaskSkeleton = null,
                TaskSkeletonAsString = null
            }
        };

        await fixture.PublishMessage(submission);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
        var receivedSubmission = await fixture.ConsumeMessage(cts.Token, submission.Id);

        // Assertions
        Assert.NotNull(receivedSubmission);
        Assert.Equal(submission.Id, receivedSubmission.Id);
        Assert.NotNull(receivedSubmission.ExecutionResult);
        Assert.False(receivedSubmission.ExecutionResult.IsCompiledSuccessfully, "The submission should not have been compiled successfully.");
    }

    [Fact]
    public async Task TestCSharpCodeDotNet6TimeLimit()
    {
        var submission = new SubmissionForProcessingPubSubModel
        {
            Id = 543004,
            ExecutionType = ExecutionType.TestsExecution,
            ExecutionStrategy = ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck,
            CompilerType = CompilerType.CSharpDotNetCore,
            FileContent = null,
            AdditionalFiles = null,
            Code = "while (true) { }",
            TimeLimit = 100,
            ExecutionStrategyBaseTimeLimit = null,
            MemoryLimit = 16777216,
            ExecutionStrategyBaseMemoryLimit = null,
            Verbosely = false,
            SimpleExecutionDetails = null,
            TestsExecutionDetails = new TestsExecutionDetailsServiceModel
            {
                MaxPoints = 100,
                TaskId = null,
                CheckerType = "trim",
                CheckerParameter = null,
                Tests =
                [
                    new TestContext { Id = 172459, Input = "0", Output = "Hello SoftUni", IsTrialTest = true, OrderBy = 0 },
                    new TestContext { Id = 172460, Input = "0", Output = "Hello SoftUni", IsTrialTest = false, OrderBy = 0 }
                ],
                TaskSkeleton = null,
                TaskSkeletonAsString = null
            }
        };

        await fixture.PublishMessage(submission);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
        var receivedSubmission = await fixture.ConsumeMessage(cts.Token, submission.Id);

        // Assertions
        Assert.NotNull(receivedSubmission);
        Assert.Equal(submission.Id, receivedSubmission!.Id);
        Assert.NotNull(receivedSubmission.ExecutionResult);
        Assert.True(receivedSubmission.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.TimeLimit), "All tests should time out.");
    }
}