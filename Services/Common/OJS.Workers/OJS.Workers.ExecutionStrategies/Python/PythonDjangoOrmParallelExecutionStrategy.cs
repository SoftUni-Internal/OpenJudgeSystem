#nullable disable
namespace OJS.Workers.ExecutionStrategies.Python;

using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OJS.Workers.Common;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

public class PythonDjangoOrmParallelExecutionStrategy<TSettings> : PythonDjangoOrmExecutionStrategy<TSettings>
    where TSettings : PythonDjangoOrmExecutionStrategySettings
{
    // Maximum parallel tests to run
    private const int MaxParallelTests = 10;

    public PythonDjangoOrmParallelExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, settingsProvider, logger)
    {
    }

    protected override async Task<IExecutionResult<TestResult>> RunTests(
        string codeSavePath,
        IExecutor executor,
        IChecker checker,
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        var tests = executionContext.Input.Tests.ToList();
        this.SaveTests(tests);

        // Use Django's built-in parallel test execution
        var testPatterns = tests.Select((test, index) =>
            this.TestPaths[index].Split(Path.DirectorySeparatorChar).Last()).ToList();

        // Join all test patterns with spaces to pass to Django's test command
        var testPatternsArg = string.Join(" ", testPatterns.Select(p => $"--pattern=\"{p}\""));

        // Execute all tests at once using Django's built-in parallel feature
        var processExecutionResult = await this.Execute(
            this.Settings.PythonExecutablePath,
            this.ExecutionArguments.Concat([
                $"manage.py test --parallel={Math.Min(MaxParallelTests, Environment.ProcessorCount)} {testPatternsArg}"
            ]),
            executor,
            executionContext);

        this.FixReceivedOutput(processExecutionResult);

        // Process results for all tests based on the output
        for (var i = 0; i < tests.Count; i++)
        {
            var testResult = this.GetTestResult(processExecutionResult, tests[i], checker);
            result.Results.Add(testResult);
        }

        return result;
    }
}