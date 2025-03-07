#nullable disable
namespace OJS.Workers.ExecutionStrategies.NodeJs;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using OJS.Workers.Common;
using OJS.Workers.Common.Extensions;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies.Helpers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

using static OJS.Workers.Common.Constants;
using static OJS.Workers.ExecutionStrategies.NodeJs.NodeJsConstants;

public class NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy<TSettings> :
    NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy<TSettings>
    where TSettings : NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategySettings
{
    public NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, settingsProvider, logger)
        => this.Random = new Random();

    protected override string JsCodeEvaluation => @"
        " + TestsPlaceholder;

    protected override string JsCodePostevaulationCode => string.Empty;

    private Random Random { get; }

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
            var executor = this.CreateRestrictedExecutor();

        var codeSavePath = this.SaveCodeToTempFile(executionContext);

        result.Results.AddRange(await this.ProcessTests(
            executionContext,
            executor,
            executionContext.Input.GetChecker(),
            codeSavePath));

        File.Delete(codeSavePath);

        return result;
    }

    protected override string BuildTests(IEnumerable<TestContext> tests)
    {
        // Swap the testInput for every copy of the user's tests
        var testGroupRoof = 1;

        // Create a random name for the variable keeping track of the testGroup, so that the user can't manipulate it
        var testGroupVariableName = "testGroup" + this.Random.Next(10000);
        var problemTests = tests.ToList();
        var testsCode = problemTests[0].Input;

        // We set the state of the tested entity in a beforeEach hook to ensure the user doesnt manipulate the entity
        testsCode += @"
let " + testGroupVariableName + $@" = 0;
beforeEach(function(){{
    if(" + testGroupVariableName + $@" < {testGroupRoof}) {{
        {problemTests[1].Input}
    }}";

        testGroupRoof++;
        var beforeHookTests = problemTests.Skip(1).ToList();

        foreach (var test in beforeHookTests)
        {
            testsCode += @"
    else if(" + testGroupVariableName + $@" < {testGroupRoof}) {{
        {test.Input}
    }}";
            testGroupRoof++;
        }

        testsCode += @"
});";

        // Insert a copy of the user tests for each test file
        for (int i = 0; i < problemTests.Count; i++)
        {
            testsCode += $@"
describe('Test {i} ', function(){{
    after(function () {{
        " + testGroupVariableName + $@"++;
    }});
{UserInputPlaceholder}
}});";
        }

        return testsCode;
    }

    protected override async Task<List<TestResult>> ProcessTests(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutor executor,
        IChecker checker,
        string codeSavePath)
    {
        var testResults = new List<TestResult>();

        var arguments = new List<string>
        {
            this.Settings.MochaModulePath,
            codeSavePath
        };
        arguments.AddRange(this.AdditionalExecutionArguments);

        var testCount = 0;
        var processExecutionResult = await executor.Execute(
            this.Settings.NodeJsExecutablePath,
            executionContext.TimeLimit,
            executionContext.MemoryLimit,
            executionArguments: arguments);

        var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
        var numberOfUserTests = mochaResult.UsersTestCount;
        var correctSolutionTestPasses = mochaResult.InitialPassingTests;

        // an offset for tracking the current subset of tests (by convention we always have 2 Zero tests)
        var testOffset = numberOfUserTests * 2;
        foreach (var test in executionContext.Input.Tests)
        {
            var message = TestPassedMessage;
            TestResult testResult = null;
            if (testCount == 0)
            {
                var minTestCount = int.Parse(
                    Regex.Match(
                        test.Input,
                        "<minTestCount>(\\d+)</minTestCount>").Groups[1].Value);

                if (numberOfUserTests < minTestCount)
                {
                    message = $"Insufficient amount of tests, you have to have at least {minTestCount} tests!";
                }

                testResult = CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    message);
            }
            else if (testCount == 1)
            {
                if (numberOfUserTests == 0)
                {
                    message = "The submitted code was either incorrect or contained no tests!";
                }
                else if (correctSolutionTestPasses != numberOfUserTests)
                {
                    message = "Error: Some tests failed while running the correct solution!";
                }

                testResult = CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    message);
            }
            else
            {
                var numberOfPasses = mochaResult.TestErrors.Skip(testOffset).Take(numberOfUserTests).Count(x => x == null);
                if (numberOfPasses >= correctSolutionTestPasses)
                {
                    message = "No unit test covering this functionality!";
                }

                testResult = CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    message);
                testOffset += numberOfUserTests;
            }

            testCount++;
            testResults.Add(testResult);
        }

        return testResults;
    }

    protected override string PreprocessJsSubmission<TInput>(string template, IExecutionContext<TInput> context)
    {
        var code = context.Code.Trim(';');

        var processedCode =
            template.Replace(RequiredModules, this.JsCodeRequiredModules)
                .Replace(PreevaluationPlaceholder, JsCodePreEvaluationCodeProvider.GetPreEvaluationCode(this.Type))
                .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                .Replace(TestsPlaceholder, this.BuildTests((context.Input as TestsInputModel)?.Tests))
                .Replace(UserInputPlaceholder, code);

        return processedCode;
    }
}

public record NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string NodeJsExecutablePath,
    string UnderscoreModulePath,
    string MochaModulePath,
    string ChaiModulePath,
    string SinonModulePath,
    string SinonChaiModulePath,
    string JsDomModulePath,
    string JQueryModulePath,
    string HandlebarsModulePath)
    : NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed,
        NodeJsExecutablePath, UnderscoreModulePath, MochaModulePath, ChaiModulePath, SinonModulePath,
        SinonChaiModulePath, JsDomModulePath, JQueryModulePath, HandlebarsModulePath);
