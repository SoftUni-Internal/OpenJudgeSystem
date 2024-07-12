﻿namespace OJS.Workers.ExecutionStrategies.Golang
{
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
            IExecutionStrategySettingsProvider settingsProvider)
            : base(submission, processExecutorFactory, compilerFactory, settingsProvider)
        {
        }

        protected override Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
            => this.CompileExecuteAndCheck(
                executionContext,
                result,
                this.CreateExecutor(),
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
}