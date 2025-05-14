namespace IntegrationTests.Worker.TypeScript.TypeScriptCodeMochaUnitsTestsNodeJs20;

using OJS.Workers.Common.Models;

[Collection(nameof(WorkerTestsCollection))]
public class TypeScriptCodeMochaUnitsTestsNodeJs20Tests : BaseStrategyTest<TypeScriptCodeMochaUnitsTestsNodeJs20SubmissionFactory, TypeScriptCodeMochaUnitsTestsNodeJs20Parameters>
{
    public TypeScriptCodeMochaUnitsTestsNodeJs20Tests(RabbitMqAndWorkerFixture fixture)
        : base(fixture, new TypeScriptCodeMochaUnitsTestsNodeJs20SubmissionFactory()) { }

    [Fact]
    public async Task ShouldPass_WhenCodeIsValid()
    {
        // Arrange
        var code = @"class Oven {
            private readonly maxTemp;
            private _temp!: number;
            private _isOn: boolean;

            constructor(maxTemp: number) {
                this.maxTemp = maxTemp;
                this._isOn = false;
                this._temp = 0;
            }

            set temp(val: number) {
                if (!this._isOn) {
                    throw new Error('Oven is not turned on');
                }
                if (val < 0) {
                    throw new Error('Temperature cannot be set bellow 0');
                }
                if (val > this.maxTemp) {
                    throw new Error(`Temperature cannot be set over ${this.maxTemp}`);
                }

                this._temp = val;
            }

            get temp(): number {
                return this._temp;
            }

            turnOn(): boolean {
                if (this._isOn) {
                    return false;
                }

                this._isOn = true;
                return true;
            }

            turnOff(): boolean {
                if (this._isOn) {
                    this._isOn = false;
                    return true;
                }

                return false;
            }

            cook(recipe: string, temp: number) {
                this.temp = temp;
                return `Recipe ${recipe} was cooked successfully`;
            }
        }";

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
        var code = @"
        class Oven {
            private readonly maxTemp;
            private _temp!: any;
            private _isOn: boolean;

            constructor(maxTemp: number) {
                this.maxTemp = maxTemp;
                this._isOn = false;
                this._temp = 0;
            }

            set temp(val: number) {
                if (!this._isOn) {
                    throw new Error('Oven is not turned on');
                }
                if (val < 0) {
                    throw new Error('Temperature cannot be set bellow 0');
                }
                if (val > this.maxTemp) {
                    throw new Error(`Temperature cannot be set over ${this.maxTemp}`)
                }

                this._temp = val;
            }

            get temp(): number {
                return this._temp;
            }

            turnOn(): boolean {
                if (this._isOn) {
                    return false;
                }

                this._isOn = true;
                return true;
            }

            turnOff(): string {
                if (this._isOn) {
                    this._isOn = false;
                    return true;
                }

                return false;
            }

            cook(recipe: string, temp: number) {
                this.temp = temp;
                return `Recipe ${recipe} was cooked successfully`;

            }

        }";

        var submission = this.Factory.GetSubmission(new(code));

        // Act
        var result = await this.Fixture.ProcessSubmission(submission);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(submission.Id, result!.Id);
        Assert.NotNull(result.ExecutionResult);
        Assert.False(result.ExecutionResult.IsCompiledSuccessfully);
    }

    [Fact]
    public async Task ShouldFail_WhenErrorMessagesAreIncorrect()
    {
        // Arrange
        var code = @"class Oven {
            private readonly maxTemp;
            private _temp!: number;
            private _isOn: boolean;

            constructor(maxTemp: number) {
                this.maxTemp = maxTemp;
                this._isOn = false;
                this._temp = 0;
            }

            set temp(val: number) {
                if (!this._isOn) {
                    throw new Error('Oven is off');
                }
                if (val < 0) {
                    throw new Error('Temperature cannot be negative');
                }
                if (val > this.maxTemp) {
                    throw new Error(`Temperature too high`);
                }

                this._temp = val;
            }

            get temp(): number {
                return this._temp;
            }

            turnOn(): boolean {
                if (this._isOn) {
                    return false;
                }

                this._isOn = true;
                return true;
            }

            turnOff(): boolean {
                if (this._isOn) {
                    this._isOn = false;
                    return true;
                }

                return false;
            }

            cook(recipe: string, temp: number) {
                this.temp = temp;
                return `Recipe ${recipe} was cooked successfully`;
            }
        }";

        var submission = this.Factory.GetSubmission(new(code));

        // Act
        var result = await this.Fixture.ProcessSubmission(submission);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(submission.Id, result!.Id);
        Assert.NotNull(result.ExecutionResult);
        Assert.False(result.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.CorrectAnswer));
    }

    [Fact]
    public async Task ShouldFail_WhenCookMethodReturnsIncorrectMessage()
    {
        // Arrange
        var code = @"class Oven {
            private readonly maxTemp;
            private _temp!: number;
            private _isOn: boolean;

            constructor(maxTemp: number) {
                this.maxTemp = maxTemp;
                this._isOn = false;
                this._temp = 0;
            }

            set temp(val: number) {
                if (!this._isOn) {
                    throw new Error('Oven is not turned on');
                }
                if (val < 0) {
                    throw new Error('Temperature cannot be set bellow 0');
                }
                if (val > this.maxTemp) {
                    throw new Error(`Temperature cannot be set over ${this.maxTemp}`);
                }

                this._temp = val;
            }

            get temp(): number {
                return this._temp;
            }

            turnOn(): boolean {
                if (this._isOn) {
                    return false;
                }

                this._isOn = true;
                return true;
            }

            turnOff(): boolean {
                if (this._isOn) {
                    this._isOn = false;
                    return true;
                }

                return false;
            }

            cook(recipe: string, temp: number) {
                this.temp = temp;
                return `Successfully cooked ${recipe}`;
            }
        }";

        var submission = this.Factory.GetSubmission(new(code));

        // Act
        var result = await this.Fixture.ProcessSubmission(submission);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(submission.Id, result!.Id);
        Assert.NotNull(result.ExecutionResult);
        Assert.False(result.ExecutionResult.TaskResult!.TestResults.All(tr => tr.ResultType == TestRunResultType.CorrectAnswer));
    }

    [Fact]
    public async Task ShouldFail_WhenCodeHasSyntaxErrors()
    {
        // Arrange
        var code = @"class Oven {
            private readonly maxTemp;
            private _temp!: number;
            private _isOn: boolean;

            constructor(maxTemp: number) {
                this.maxTemp = maxTemp;
                this._isOn = false;
                this._temp = 0;
            }

            set temp(val: number) {
                if (!this._isOn) {
                    throw new Error('Oven is not turned on');
                }
                if (val < 0) {
                    throw new Error('Temperature cannot be set bellow 0');
                }
                if (val > this.maxTemp) {
                    throw new Error(`Temperature cannot be set over ${this.maxTemp}`);
                }

                this._temp = val;
            }

            get temp(): number {
                return this._temp;
            }

            turnOn(): boolean {
                if (this._isOn) {
                    return false;
                }

                this._isOn = true;
                return true;
            }

            turnOff(): boolean {
                if (this._isOn) {
                    this._isOn = false;
                    return true;
                }

                return false;
            }

            cook(recipe: string, temp: number) {
                this.temp = temp;
                return `Recipe ${recipe} was cooked successfully`;
            }
        "; // Missing closing brace

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