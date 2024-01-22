﻿namespace OJS.Workers.ExecutionStrategies.CPlusPlus
{
    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies.Extensions;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class CPlusPlusCompileExecuteAndCheckExecutionStrategy<TSettings> : CompileExecuteAndCheckExecutionStrategy<TSettings>
        where TSettings : CPlusPlusCompileExecuteAndCheckExecutionStrategySettings
    {
        public CPlusPlusCompileExecuteAndCheckExecutionStrategy(
            ExecutionStrategyType type,
            IProcessExecutorFactory processExecutorFactory,
            ICompilerFactory compilerFactory,
            IExecutionStrategySettingsProvider settingsProvider)
            : base(type, processExecutorFactory, compilerFactory, settingsProvider)
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
                this.CreateExecutor(),
                useSystemEncoding: false,
                dependOnExitCodeForRunTimeError: true);
        }
    }

    public record CPlusPlusCompileExecuteAndCheckExecutionStrategySettings(int BaseTimeUsed, int BaseMemoryUsed)
        : CompileExecuteAndCheckExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
}
