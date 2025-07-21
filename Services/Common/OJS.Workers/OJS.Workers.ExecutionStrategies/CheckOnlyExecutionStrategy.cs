namespace OJS.Workers.ExecutionStrategies
{
    using Microsoft.Extensions.Logging;
    using OJS.Workers.Common;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class CheckOnlyExecutionStrategy<TSettings> : BaseCodeExecutionStrategy<TSettings>
        where TSettings : CheckOnlyExecutionStrategySettings
    {
        public CheckOnlyExecutionStrategy(
            IOjsSubmission submission,
            IProcessExecutorFactory processExecutorFactory,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
            : base(submission, processExecutorFactory, settingsProvider, logger)
        {
        }

        protected override Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result,
            CancellationToken cancellationToken = default)
        {
            result.IsCompiledSuccessfully = true;

            var processExecutionResult = new ProcessExecutionResult
            {
                Type = ProcessExecutionResultType.Success,
                ReceivedOutput = executionContext.Code,
            };

            var checker = executionContext.Input.GetChecker();

            foreach (var test in executionContext.Input.Tests)
            {
                var testResult = CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.Results.Add(testResult);
            }

            return Task.FromResult(result);
        }
    }

    public record CheckOnlyExecutionStrategySettings(int BaseTimeUsed, int BaseMemoryUsed)
        : BaseCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
}
