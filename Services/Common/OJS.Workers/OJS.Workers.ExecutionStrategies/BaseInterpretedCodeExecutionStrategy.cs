namespace OJS.Workers.ExecutionStrategies
{
    using Microsoft.Extensions.Logging;
    using System;
    using OJS.Workers.Common;
    using OJS.Workers.Executors;

    using static OJS.Workers.Common.Constants;

    public class BaseInterpretedCodeExecutionStrategy<TSettings> : BaseCodeExecutionStrategy<TSettings>
        where TSettings : BaseInterpretedCodeExecutionStrategySettings
    {
        protected BaseInterpretedCodeExecutionStrategy(
            IOjsSubmission submission,
            IProcessExecutorFactory processExecutorFactory,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
            : base(submission, processExecutorFactory, settingsProvider, logger)
        {
        }

        protected static string PrepareTestInput(string testInput)
            => string.Join(
                Environment.NewLine,
                testInput.Split([NewLineUnix, NewLineWin], StringSplitOptions.None));

        protected override Task<IExecutionResult<TResult>> InternalExecute<TInput, TResult>(
            IExecutionContext<TInput> executionContext,
            IExecutionResult<TResult> result)
        {
            result.IsCompiledSuccessfully = true;

            return base.InternalExecute(executionContext, result);
        }
    }

    public abstract record BaseInterpretedCodeExecutionStrategySettings(
        int BaseTimeUsed, int BaseMemoryUsed)
        : BaseCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
}
