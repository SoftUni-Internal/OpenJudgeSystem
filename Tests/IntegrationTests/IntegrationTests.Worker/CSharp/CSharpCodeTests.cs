namespace IntegrationTests.Worker.CSharp;

using OJS.Workers.Common.Models;

public class CSharpCodeTests : BaseStrategyTest<CSharpCodeSubmissionFactory, CSharpCodeParameters>, IClassFixture<RabbitMqAndWorkerFixture>
{
    public CSharpCodeTests(RabbitMqAndWorkerFixture fixture)
        : base(fixture, new CSharpCodeSubmissionFactory()) { }

    [Fact]
    public async Task TestCSharpCodeDotNet6CorrectResult()
    {
        var submission = this.Factory.GetSubmission(new("Console.WriteLine(\"Hello SoftUni\");"));

        await this.Fixture.PublishMessage(submission);

        var receivedSubmission = await this.Fixture.ConsumeMessage(CancellationToken.None, submission.Id);

        // Assertions
        Assert.NotNull(receivedSubmission);
        Assert.Equal(submission.Id, receivedSubmission!.Id);
        Assert.NotNull(receivedSubmission.ExecutionResult);
        Assert.True(receivedSubmission.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.CorrectAnswer));
    }

    [Fact]
    public async Task TestCSharpCodeDotNet6CompilationError()
    {
        var submission = this.Factory.GetSubmission(new("This is not proper c# code!"));

        await this.Fixture.PublishMessage(submission);

        var receivedSubmission = await this.Fixture.ConsumeMessage(CancellationToken.None, submission.Id);

        // Assertions
        Assert.NotNull(receivedSubmission);
        Assert.Equal(submission.Id, receivedSubmission.Id);
        Assert.NotNull(receivedSubmission.ExecutionResult);
        Assert.False(receivedSubmission.ExecutionResult.IsCompiledSuccessfully);
    }

    [Fact]
    public async Task TestCSharpCodeDotNet6TimeLimit()
    {
        var submission = this.Factory.GetSubmission(new("while (true) { }"));

        await this.Fixture.PublishMessage(submission);

        var receivedSubmission = await this.Fixture.ConsumeMessage(CancellationToken.None, submission.Id);

        // Assertions
        Assert.NotNull(receivedSubmission);
        Assert.Equal(submission.Id, receivedSubmission!.Id);
        Assert.NotNull(receivedSubmission.ExecutionResult);
        Assert.True(receivedSubmission.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.TimeLimit));
    }
}