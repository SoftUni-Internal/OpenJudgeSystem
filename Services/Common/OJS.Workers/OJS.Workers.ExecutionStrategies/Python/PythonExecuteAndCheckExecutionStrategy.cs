#nullable disable
namespace OJS.Workers.ExecutionStrategies.Python
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;
    using static OJS.Workers.ExecutionStrategies.Python.PythonConstants;

    public class PythonExecuteAndCheckExecutionStrategy<TSettings> : BaseInterpretedCodeExecutionStrategy<TSettings>
        where TSettings : PythonExecuteAndCheckExecutionStrategySettings
    {
        public PythonExecuteAndCheckExecutionStrategy(
            IOjsSubmission submission,
            IProcessExecutorFactory processExecutorFactory,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
            : base(submission, processExecutorFactory, settingsProvider, logger)
        {
            if (!FileHelpers.FileExists(this.Settings.PythonExecutablePath))
            {
                throw new ArgumentException($"Python not found in: {this.Settings.PythonExecutablePath}", nameof(this.Settings.PythonExecutablePath));
            }
        }

        protected virtual IEnumerable<string> ExecutionArguments
            => new[] { IsolatedModeArgument, OptimizeAndDiscardDocstringsArgument };

        protected override Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            var codeSavePath = this.SaveCodeToTempFile(executionContext);

            var executor = this.CreateRestrictedExecutor();

            var checker = executionContext.Input.GetChecker();

            return this.RunTests(codeSavePath, executor, checker, executionContext, result);
        }

        protected override async Task<IExecutionResult<OutputResult>> ExecuteAgainstSimpleInput(
            IExecutionContext<SimpleInputModel> executionContext,
            IExecutionResult<OutputResult> result)
        {
            var codeSavePath = this.SaveCodeToTempFile(executionContext);

            var executor = this.CreateRestrictedExecutor();

            var processExecutionResult = await this.Execute(
                executionContext,
                executor,
                codeSavePath,
                executionContext.Input.Input);

            result.Results.Add(GetOutputResult(processExecutionResult));

            return result;
        }

        protected virtual async Task<IExecutionResult<TestResult>> RunTests(
            string codeSavePath,
            IExecutor executor,
            IChecker checker,
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            foreach (var test in executionContext.Input.Tests)
            {
                var testResult = await this.RunIndividualTest(
                    codeSavePath,
                    executor,
                    checker,
                    executionContext,
                    test);

                result.Results.Add(testResult);
            }

            return result;
        }

        protected virtual async Task<TestResult> RunIndividualTest(
            string codeSavePath,
            IExecutor executor,
            IChecker checker,
            IExecutionContext<TestsInputModel> executionContext,
            TestContext test)
        {
            var testInput = PrepareTestInput(test.Input);
            var processExecutionResult = await this.Execute(executionContext, executor, codeSavePath, testInput);

            var testResult = CheckAndGetTestResult(
                test,
                processExecutionResult,
                checker,
                processExecutionResult.ReceivedOutput);

            return testResult;
        }

        protected virtual Task<ProcessExecutionResult> Execute<TInput>(
            IExecutionContext<TInput> executionContext,
            IExecutor executor,
            string codeSavePath,
            string inputData = null,
            string directory = null)
            => executor.Execute(
                this.Settings.PythonExecutablePath,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                inputData,
                this.ExecutionArguments.Concat(new[] { codeSavePath }),
                directory,
                useProcessTime: false,
                useSystemEncoding: true);
    }

    public record PythonExecuteAndCheckExecutionStrategySettings(
        int BaseTimeUsed,
        int BaseMemoryUsed,
        string PythonExecutablePath)
        : BaseInterpretedCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
}
