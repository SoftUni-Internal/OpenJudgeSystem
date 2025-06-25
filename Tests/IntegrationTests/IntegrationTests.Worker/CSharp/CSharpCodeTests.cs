namespace IntegrationTests.Worker.CSharp;

using OJS.Workers.Common.Models;

[Collection(nameof(WorkerTestsCollection))]
public class CSharpCodeTests : BaseStrategyTest<CSharpCodeSubmissionFactory, CSharpCodeParameters>
{
    public CSharpCodeTests(RabbitMqAndWorkerFixture fixture)
        : base(fixture, new CSharpCodeSubmissionFactory()) { }

    [Fact]
    public async Task DotNet6_ShouldReturnCorrectAnswer_WhenCodeIsValid()
    {
        // Arrange
        var submission = this.Factory.GetSubmission(new("Console.WriteLine(\"Hello SoftUni\");"));

        // Act
        var result = await this.Fixture.ProcessSubmission(submission);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(submission.Id, result!.Id);
        Assert.NotNull(result.ExecutionResult);
        Assert.True(result.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.CorrectAnswer));
    }

    [Fact]
    public async Task DotNet6_ShouldFailCompilation_WhenCodeIsInvalid()
    {
        // Arrange
        var submission = this.Factory.GetSubmission(new("This is not proper c# code!"));

        // Act
        var result = await this.Fixture.ProcessSubmission(submission);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(submission.Id, result.Id);
        Assert.NotNull(result.ExecutionResult);
        Assert.False(result.ExecutionResult.IsCompiledSuccessfully);
    }

    [Fact]
    public async Task DotNet6_ShouldTimeout_WhenCodeRunsIndefinitely()
    {
        // Arrange
        var submission = this.Factory.GetSubmission(new("while (true) { }"));

        // Act
        var result = await this.Fixture.ProcessSubmission(submission);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(submission.Id, result!.Id);
        Assert.NotNull(result.ExecutionResult);
        Assert.True(result.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.TimeLimit));
    }
}