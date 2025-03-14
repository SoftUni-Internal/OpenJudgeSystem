namespace OJS.Workers.ExecutionStrategies.Golang;

using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Compilers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

public class GolangCompileExecuteAndCheckExecutionStrategy<TSettings> : BaseCompiledCodeExecutionStrategy<TSettings>
    where TSettings : GolangCompileExecuteAndCheckExecutionStrategySettings
{
    private const string CodeSaveFileName = "main.go";

    public GolangCompileExecuteAndCheckExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        ICompilerFactory compilerFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, compilerFactory, settingsProvider, logger)
    {
    }

    protected override Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
        => this.CompileExecuteAndCheck(
            executionContext,
            result,
                this.CreateRestrictedExecutor(),
            useSystemEncoding: true,
            dependOnExitCodeForRunTimeError: false,
            useWorkingDirectoryForProcess: true);

    protected override string SaveCodeToTempFile<TInput>(IExecutionContext<TInput> executionContext)
        => FileHelpers.SaveStringToFile(
            executionContext.Code,
            FileHelpers.BuildPath(this.WorkingDirectory, CodeSaveFileName));
}

public record GolangCompileExecuteAndCheckExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed)
    : BaseCompiledCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);