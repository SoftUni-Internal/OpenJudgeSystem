#nullable disable
namespace OJS.Workers.ExecutionStrategies.NodeJs;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using OJS.Workers.Common;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies.Helpers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

using static OJS.Workers.ExecutionStrategies.NodeJs.NodeJsConstants;

public class NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy<TSettings>
    : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<TSettings>
    where TSettings : NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategySettings
{
    public NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger) // TODO: make this modular by getting requires from test
        : base(submission, processExecutorFactory, settingsProvider, logger)
    {
        if (!Directory.Exists(this.Settings.JsDomModulePath))
        {
            throw new ArgumentException(
                $"jsDom not found in: {this.Settings.JsDomModulePath}",
                nameof(this.Settings.JsDomModulePath));
        }

        if (!Directory.Exists(this.Settings.JQueryModulePath))
        {
            throw new ArgumentException(
                $"jQuery not found in: {this.Settings.JQueryModulePath}",
                nameof(this.Settings.JQueryModulePath));
        }

        if (!Directory.Exists(this.Settings.HandlebarsModulePath))
        {
            throw new ArgumentException(
                $"Handlebars not found in: {this.Settings.HandlebarsModulePath}",
                nameof(this.Settings.HandlebarsModulePath));
        }
    }

    protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    jsdom = require('" + this.Settings.JsDomModulePath + @"'),
    jq = require('" + this.Settings.JQueryModulePath + @"'),
    sinon = require('" + this.Settings.SinonModulePath + @"'),
    sinonChai = require('" + this.Settings.SinonChaiModulePath + @"'),
    handlebars = require('" + this.Settings.HandlebarsModulePath + @"')";

    protected override string JsCodeEvaluation => TestsPlaceholder;

    protected override string TestFuncVariables => base.TestFuncVariables + ", '_'";

    protected virtual string BuildTests(IEnumerable<TestContext> tests)
    {
        var testsCode = string.Empty;
        var testsCount = 1;
        foreach (var test in tests)
        {
            var code = Regex.Replace(test.Input, "([\\\\`])", "\\$1");

            testsCode +=
                $@"
it('Test{testsCount++}', function(done) {{
    let content = `{code}`;
    let inputData = content.trim();
    let code = {{
        run: {UserInputPlaceholder}
    }};
    let testFunc = new Function('result', {this.TestFuncVariables}, inputData);
    testFunc.call({{}}, code.run, {this.TestFuncVariables.Replace("'", string.Empty)});
    done();
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

        var processExecutionResult = await executor.Execute(
            this.Settings.NodeJsExecutablePath,
            executionContext.TimeLimit,
            executionContext.MemoryLimit,
            executionArguments: arguments);

        var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
        var currentTest = 0;
        foreach (var test in executionContext.Input.Tests)
        {
            var message = "yes";
            if (!string.IsNullOrEmpty(mochaResult.Error))
            {
                message = mochaResult.Error;
            }
            else if (mochaResult.TestErrors[currentTest] != null)
            {
                message = $"Unexpected error: {mochaResult.TestErrors[currentTest]}";
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

    protected override string PreprocessJsSubmission<TInput>(string template, IExecutionContext<TInput> context)
    {
        var code = context.Code.Trim(';');
        var processedCode = template
            .Replace(RequiredModules, this.JsCodeRequiredModules)
            .Replace(PreevaluationPlaceholder, JsCodePreEvaluationCodeProvider.GetPreEvaluationCode(this.Type))
            .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
            .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
            .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
            .Replace(TestsPlaceholder, this.BuildTests((context.Input as TestsInputModel)?.Tests))
            .Replace(UserInputPlaceholder, code);
        return processedCode;
    }
}

public record NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategySettings(
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
    : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings(
        BaseTimeUsed,
        BaseMemoryUsed,
        NodeJsExecutablePath,
        UnderscoreModulePath,
        MochaModulePath,
        ChaiModulePath,
        SinonModulePath,
        SinonChaiModulePath);
