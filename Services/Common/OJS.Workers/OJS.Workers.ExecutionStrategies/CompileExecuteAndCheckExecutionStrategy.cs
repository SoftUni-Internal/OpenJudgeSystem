namespace OJS.Workers.ExecutionStrategies
{
    using Microsoft.Extensions.Logging;
    using OJS.Workers.Common;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class CompileExecuteAndCheckExecutionStrategy<TSettings> : BaseCompiledCodeExecutionStrategy<TSettings>
        where TSettings : CompileExecuteAndCheckExecutionStrategySettings
    {
        public CompileExecuteAndCheckExecutionStrategy(
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
                this.CreateRestrictedExecutor());

        protected override async Task<IExecutionResult<OutputResult>> ExecuteAgainstSimpleInput(
            IExecutionContext<SimpleInputModel> executionContext,
            IExecutionResult<OutputResult> result)
        {
            var compileResult = this.ExecuteCompiling(
                executionContext,
                result);

            if (!compileResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var executor = this.CreateRestrictedExecutor();

            var processExecutionResult = await executor.Execute(
                compileResult.OutputFile,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                executionContext.Input?.Input,
                workingDirectory: this.WorkingDirectory);

            result.Results.Add(GetOutputResult(processExecutionResult));

            return result;
        }
    }

    public record CompileExecuteAndCheckExecutionStrategySettings(int BaseTimeUsed, int BaseMemoryUsed)
        : BaseCompiledCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
}
