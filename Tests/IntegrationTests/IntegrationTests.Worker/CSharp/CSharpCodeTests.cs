namespace IntegrationTests.Worker.CSharp;

using OJS.Workers.Common.Models;

public class CSharpCodeTests : BaseStrategyTest<CSharpCodeSubmissionFactory, CSharpCodeParameters>, IClassFixture<RabbitMqAndWorkerFixture>
{
    public CSharpCodeTests(RabbitMqAndWorkerFixture fixture)
        : base(fixture, new CSharpCodeSubmissionFactory()) { }

    [Fact]
    public async Task DotNet6_ShouldReturnCorrectAnswer_WhenCodeIsValid()
    {
        var submission = this.Factory.GetSubmission(new("Console.WriteLine(\"Hello SoftUni\");"));

        var receivedSubmission = await this.Fixture.ProcessSubmission(submission);

        // Assertions
        Assert.NotNull(receivedSubmission);
        Assert.Equal(submission.Id, receivedSubmission!.Id);
        Assert.NotNull(receivedSubmission.ExecutionResult);
        Assert.True(receivedSubmission.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.CorrectAnswer));
    }

    [Fact]
    public async Task DotNet6_ShouldFailCompilation_WhenCodeIsInvalid()
    {
        var submission = this.Factory.GetSubmission(new("This is not proper c# code!"));

        var receivedSubmission = await this.Fixture.ProcessSubmission(submission);

        // Assertions
        Assert.NotNull(receivedSubmission);
        Assert.Equal(submission.Id, receivedSubmission.Id);
        Assert.NotNull(receivedSubmission.ExecutionResult);
        Assert.False(receivedSubmission.ExecutionResult.IsCompiledSuccessfully);
    }

    [Fact]
    public async Task DotNet6_ShouldTimeout_WhenCodeRunsIndefinitely()
    {
        var submission = this.Factory.GetSubmission(new("while (true) { }"));

        var receivedSubmission = await this.Fixture.ProcessSubmission(submission);

        // Assertions
        Assert.NotNull(receivedSubmission);
        Assert.Equal(submission.Id, receivedSubmission!.Id);
        Assert.NotNull(receivedSubmission.ExecutionResult);
        Assert.True(receivedSubmission.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.TimeLimit));
    }
}