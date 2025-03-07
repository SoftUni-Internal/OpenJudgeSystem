namespace OJS.Workers.ExecutionStrategies.CPlusPlus;

using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.Compilers;
using OJS.Workers.ExecutionStrategies.Extensions;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

public class CPlusPlusCompileExecuteAndCheckExecutionStrategy<TSettings> : CompileExecuteAndCheckExecutionStrategy<TSettings>
    where TSettings : CPlusPlusCompileExecuteAndCheckExecutionStrategySettings
{
    public CPlusPlusCompileExecuteAndCheckExecutionStrategy(
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
    {
        executionContext.SanitizeContent();

        return this.CompileExecuteAndCheck(
            executionContext,
            result,
                this.CreateRestrictedExecutor(),
            useSystemEncoding: false,
            dependOnExitCodeForRunTimeError: true);
    }
}

public record CPlusPlusCompileExecuteAndCheckExecutionStrategySettings(int BaseTimeUsed, int BaseMemoryUsed)
    : CompileExecuteAndCheckExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
