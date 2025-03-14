#nullable disable
namespace OJS.Workers.ExecutionStrategies.Python;

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

using OJS.Workers.Common;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies.Helpers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

using static OJS.Workers.Common.Constants;
using static OJS.Workers.ExecutionStrategies.Python.PythonConstants;

public class PythonProjectTestsExecutionStrategy<TSettings> : PythonCodeExecuteAgainstUnitTestsExecutionStrategy<TSettings>
    where TSettings : PythonProjectTestsExecutionStrategySettings
{
#pragma warning disable SA1306
#pragma warning disable SA1401
    protected string[] TestPaths;
#pragma warning restore SA1401
#pragma warning restore SA1306
    private const string TestsFolderName = "tests";

    public PythonProjectTestsExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, settingsProvider, logger)
    {
    }

    protected override IEnumerable<string> ExecutionArguments
        =>
        [
            IgnorePythonEnvVarsArgument,
            DontAddUserSiteDirectoryArgument,
            ModuleNameArgument,
            UnitTestModuleName
        ];

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        SaveZipSubmission(executionContext.FileContent, this.WorkingDirectory);

        var executor = this.CreateRestrictedExecutor();
        var checker = executionContext.Input.GetChecker();

        return await this.RunTests(string.Empty, executor, checker, executionContext, result);
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

        for (var i = 0; i < tests.Count; i++)
        {
            var test = tests[i];
            var testPath = this.TestPaths[i];

            var processExecutionResult = await this.Execute(
                executionContext,
                executor,
                testPath,
                directory: this.WorkingDirectory);

            var testResult = this.GetTestResult(processExecutionResult, test, checker);

            result.Results.Add(testResult);
        }

        return result;
    }

    /// <summary>
    /// Saves all tests from the execution context as separate files in tests directory.
    /// Full paths to the files are preserved in a private field.
    /// </summary>
    /// <param name="tests">All tests from the execution context.</param>
    protected void SaveTests(IList<TestContext> tests)
    {
        var testsDirectoryName = FileHelpers.BuildPath(this.WorkingDirectory, TestsFolderName);

        this.TestPaths = new string[tests.Count];

        for (var i = 0; i < tests.Count; i++)
        {
            var test = tests[i];
            var testFileName = $"test_{i}{PythonFileExtension}";
            var testSavePath = FileHelpers.BuildPath(testsDirectoryName, testFileName);

            PythonStrategiesHelper.CreateFileInPackage(testSavePath, test.Input);

            this.TestPaths[i] = testSavePath;
        }
    }
}

public record PythonProjectTestsExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string PythonExecutablePath)
    : PythonCodeExecuteAgainstUnitTestsExecutionStrategySettings(
        BaseTimeUsed,
        BaseMemoryUsed,
        PythonExecutablePath);