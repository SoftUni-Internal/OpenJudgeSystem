namespace OJS.Workers.ExecutionStrategies.NodeJs;

using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.Common.Extensions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

using static OJS.Workers.ExecutionStrategies.NodeJs.NodeJsConstants;

// TODO: Should be renamed to NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy if no problems are found in the strategy
public class NodeJsPreprocessExecuteAndRunAllUnitTestsWithMochaExecutionStrategy<TSettings>
    : NodeJsPreprocessExecuteAndCheckExecutionStrategy<TSettings>
    where TSettings : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings
{
    protected const string TestsPlaceholder = "#testsCode#";
    protected const string UserCodePlaceholder = "#userCode#";
    protected const string SolutionSkeletonPlaceholder = "#solutionSkeleton#";

    public NodeJsPreprocessExecuteAndRunAllUnitTestsWithMochaExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, settingsProvider, logger)
    {
        if (!File.Exists(this.Settings.MochaModulePath))
        {
            throw new ArgumentException(
                $"Mocha not found in: {this.Settings.MochaModulePath}",
                nameof(this.Settings.MochaModulePath));
        }

        if (!Directory.Exists(this.Settings.ChaiModulePath))
        {
            throw new ArgumentException(
                $"Chai not found in: {this.Settings.ChaiModulePath}",
                nameof(this.Settings.ChaiModulePath));
        }

        if (!Directory.Exists(this.Settings.SinonModulePath))
        {
            throw new ArgumentException(
                $"Sinon not found in: {this.Settings.SinonModulePath}",
                nameof(this.Settings.SinonModulePath));
        }

        if (!Directory.Exists(this.Settings.SinonChaiModulePath))
        {
            throw new ArgumentException(
                $"Sinon-chai not found in: {this.Settings.SinonChaiModulePath}",
                nameof(this.Settings.SinonChaiModulePath));
        }
    }

    protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    chai = require('" + this.Settings.ChaiModulePath + @"'),
    sinon = require('" + this.Settings.SinonModulePath + @"'),
    sinonChai = require('" + this.Settings.SinonChaiModulePath + @"'),
	assert = chai.assert,
	expect = chai.expect,
	should = chai.should()";

    protected virtual string TestFuncVariables => "'assert', 'expect', 'should', 'sinon'";

    protected virtual IEnumerable<string> AdditionalExecutionArguments
        => [TestsReporterArgument, JsonReportName];

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result,
        CancellationToken cancellationToken = default)
    {
        // Prepare JavaScript file with combined user code and tests
        var jsTemplate = this.GetTemplateContent(isTypeScript: false);
        var userCode = executionContext.Code.Trim();
        jsTemplate = jsTemplate
            .Replace(SolutionSkeletonPlaceholder, executionContext.Input.TaskSkeletonAsString)
            .Replace(UserCodePlaceholder, userCode);

        // Process each test and wrap it in an it() block
        if (executionContext.Input.Tests.Any())
        {
            var formattedTests = FormatTests(executionContext.Input.Tests, false);
            jsTemplate = jsTemplate.Replace(TestsPlaceholder, formattedTests);
        }

        // Save the combined JavaScript file
        var jsCodeSavePath = FileHelpers.SaveStringToTempFile(this.WorkingDirectory, jsTemplate);

        // Execute tests using Node.js
        var executor = this.CreateRestrictedExecutor();
        var checker = executionContext.Input.GetChecker();

        var testResults = await this.ProcessTests(
            executionContext,
            executor,
            checker,
            jsCodeSavePath);
        result.Results.AddRange(testResults);

        return result;
    }

    protected static string FormatTests(IEnumerable<TestContext> tests, bool isTypeScript)
    {
        var formattedTests = new List<string>();
        var testCounter = 1;

        foreach (var test in tests)
        {
            // Use simple sequential test names
            var testName = $"Test{testCounter}";
            var testContent = test.Input.Trim();

            // Format the test with proper it() wrapper
            var formattedTest = $@"
            {(isTypeScript ? "// @ts-ignore" : "")}
            it('{testName}', function () {{
                    {testContent}
                }})";

            formattedTests.Add(formattedTest);
            testCounter++;
        }

        // Join all formatted tests
        return string.Join("\n", formattedTests);
    }

    protected override async Task<List<TestResult>> ProcessTests(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutor executor,
        IChecker checker,
        string codeSavePath)
    {
        var testResults = new List<TestResult>();

        // Configure arguments for Mocha with Node.js
        var arguments = new List<string>
        {
            this.Settings.MochaModulePath,
            codeSavePath,
        };
        arguments.AddRange(this.AdditionalExecutionArguments);

        // Execute Mocha with Node.js
        var processExecutionResult = await executor.Execute(
            this.Settings.NodeJsExecutablePath,
            executionContext.TimeLimit,
            executionContext.MemoryLimit,
            string.Empty,
            arguments);

        // Parse the results
        var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);

        // Map results to each test
        var currentTest = 0;
        foreach (var test in executionContext.Input.Tests)
        {
            var message = "yes";
            if (!string.IsNullOrEmpty(mochaResult.Error))
            {
                message = mochaResult.Error;
            }
            else if (currentTest < mochaResult.TestErrors.Count && mochaResult.TestErrors[currentTest] != null)
            {
                message = $"Test failed: {mochaResult.TestErrors[currentTest]}";
            }

            var testResult = CheckAndGetTestResult(
                test,
                processExecutionResult,
                checker,
                message);

            currentTest++;
            testResults.Add(testResult);
        }

        return testResults;
    }

    protected string GetTemplateContent(bool isTypeScript) => @$"
    // Imports
    {(isTypeScript ? "// @ts-ignore" : "")}
    const chai = require('{this.Settings.ChaiModulePath}');
    {(isTypeScript ? "// @ts-ignore" : "")}
    const sinon = require('{this.Settings.SinonModulePath}');
    {(isTypeScript ? "// @ts-ignore" : "")}
    const sinonChai = require('{this.Settings.SinonChaiModulePath}');

    chai.use(sinonChai);

    const expect = chai.expect;
    const assert = chai.assert;
    const should = chai.should();

    // Skeleton
    {SolutionSkeletonPlaceholder}

    // User code
    let result = {UserCodePlaceholder}

    // Tests
    {(isTypeScript ? "// @ts-ignore" : "")}
    describe('Tests', function () {{
        {TestsPlaceholder}
    }});
    ";
}