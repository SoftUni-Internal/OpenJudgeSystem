namespace IntegrationTests.Worker.TypeScript.TypeScriptCodeNodeJs20;

using OJS.Workers.Common.Models;

[Collection(nameof(WorkerTestsCollection))]
public class TypeScriptCodeNodeJs20Tests : BaseStrategyTest<TypeScriptCodeNodeJs20SubmissionFactory, TypeScriptCodeNodeJs20Parameters>
{
    public TypeScriptCodeNodeJs20Tests(RabbitMqAndWorkerFixture fixture)
        : base(fixture, new TypeScriptCodeNodeJs20SubmissionFactory()) { }

    [Fact]
    public async Task ShouldPass_WhenCodeIsValid()
    {
        // Arrange
        var code = @$"
        function pointsValidation(arr: number[]): void {{
            const x1: number = arr[0];
            const y1: number = arr[1];
            const x2: number = arr[2];
            const y2: number = arr[3];

            const checkFirstPoint: number = firstPoint();
            const checkSecondPoint: number = secondPoint();
            const checkThirdPoint: number = thirdPoint();

            if (checkFirstPoint === Math.trunc(checkFirstPoint)) {{
                console.log(`{{${{x1}}, ${{y1}}}} to {{0, 0}} is valid`);
            }} else {{
                console.log(`{{${{x1}}, ${{y1}}}} to {{0, 0}} is invalid`);
            }}

            if (checkSecondPoint === Math.trunc(checkSecondPoint)) {{
                console.log(`{{${{x2}}, ${{y2}}}} to {{0, 0}} is valid`);
            }} else {{
                console.log(`{{${{x2}}, ${{y2}}}} to {{0, 0}} is invalid`);
            }}

            if (checkThirdPoint === Math.trunc(checkThirdPoint)) {{
                console.log(`{{${{x1}}, ${{y1}}}} to {{${{x2}}, ${{y2}}}} is valid`);
            }} else {{
                console.log(`{{${{x1}}, ${{y1}}}} to {{${{x2}}, ${{y2}}}} is invalid`);
            }}

            function firstPoint(): number {{
                return Math.sqrt(Math.pow(x1, 2) + Math.pow(y1, 2));
            }}

            function secondPoint(): number {{
                return Math.sqrt(Math.pow(x2, 2) + Math.pow(y2, 2));
            }}

            function thirdPoint(): number {{
                return Math.sqrt(Math.pow(x2 - x1, 2) + Math.pow(y2 - y1, 2));
            }}
        }}";


        var submission = this.Factory.GetSubmission(new(code));

        // Act
        var result = await this.Fixture.ProcessSubmission(submission);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(submission.Id, result!.Id);
        Assert.NotNull(result.ExecutionResult);
        Assert.True(result.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.CorrectAnswer));
    }

    [Fact]
    public async Task ShouldFail_WhenCodeHasTypeErrors()
    {
        // Arrange
        var code = @$"
        function pointsValidation(arr: number[]): void {{
            const x1: number = arr[0];
            const y1: number = arr[1];
            const x2: number = arr[2];
            const y2: number = arr[3];

            const checkFirstPoint: string = firstPoint();
            const checkSecondPoint: number = secondPoint();
            const checkThirdPoint: number = thirdPoint();

            if (checkFirstPoint === Math.trunc(checkFirstPoint)) {{
                console.log(`{{${{x1}}, ${{y1}}}} to {{0, 0}} is valid`);
            }} else {{
                console.log(`{{${{x1}}, ${{y1}}}} to {{0, 0}} is invalid`);
            }}

            if (checkSecondPoint === Math.trunc(checkSecondPoint)) {{
                console.log(`{{${{x2}}, ${{y2}}}} to {{0, 0}} is valid`);
            }} else {{
                console.log(`{{${{x2}}, ${{y2}}}} to {{0, 0}} is invalid`);
            }}

            if (checkThirdPoint === Math.trunc(checkThirdPoint)) {{
                console.log(`{{${{x1}}, ${{y1}}}} to {{${{x2}}, ${{y2}}}} is valid`);
            }} else {{
                console.log(`{{${{x1}}, ${{y1}}}} to {{${{x2}}, ${{y2}}}} is invalid`);
            }}

            function firstPoint(): number {{
                return Math.sqrt(Math.pow(x1, 2) + Math.pow(y1, 2));
            }}

            function secondPoint(): number {{
                return Math.sqrt(Math.pow(x2, 2) + Math.pow(y2, 2));
            }}

            function thirdPoint(): number {{
                return Math.sqrt(Math.pow(x2 - x1, 2) + Math.pow(y2 - y1, 2));
            }}
        }}";


        var submission = this.Factory.GetSubmission(new(code));

        // Act
        var result = await this.Fixture.ProcessSubmission(submission);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(submission.Id, result!.Id);
        Assert.NotNull(result.ExecutionResult);
        Assert.False(result.ExecutionResult.IsCompiledSuccessfully);
    }
}