#nullable disable
namespace OJS.Workers.ExecutionStrategies
{
    using Microsoft.Extensions.Logging;
    using OJS.Workers.Common;

    public class DoNothingExecutionStrategy<TSettings> : BaseExecutionStrategy<TSettings>
        where TSettings : DoNothingExecutionStrategySettings
    {
        public DoNothingExecutionStrategy(
            IOjsSubmission submission,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
            : base(submission, settingsProvider, logger)
        {
        }

        protected override Task<IExecutionResult<TResult>> InternalExecute<TInput, TResult>(
            IExecutionContext<TInput> executionContext,
            IExecutionResult<TResult> result,
            CancellationToken cancellationToken = default)
        {
            result.CompilerComment = null;
            result.IsCompiledSuccessfully = true;

            return Task.FromResult(result);
        }
    }

    public record DoNothingExecutionStrategySettings : BaseExecutionStrategySettings
    {
    }
}
