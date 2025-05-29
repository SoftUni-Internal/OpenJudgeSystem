namespace OJS.Workers.ExecutionStrategies.NodeJs;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

using OJS.Workers.Common;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

using static OJS.Workers.ExecutionStrategies.NodeJs.NodeJsConstants;

public class NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<TSettings>
    : NodeJsPreprocessExecuteAndCheckExecutionStrategy<TSettings>
    where TSettings : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings
{
    protected const string TestsPlaceholder = "#testsCode#";

    public NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy(
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

    protected override string JsCodeEvaluation => @"
	it('Test', function(done) {
		let content = '';
        process.stdin.resume();
        process.stdin.on('data', function(buf) { content += buf.toString(); });
        process.stdin.on('end', function() {
            let inputData = content.trim();

	        let testFunc = new Function('result', " + this.TestFuncVariables + @", inputData);
            testFunc.call({}, result,  " + this.TestFuncVariables.Replace("'", string.Empty) + @");

	        done();
        });
    });";

    protected override string JsCodePostevaulationCode => @"
});";

    protected virtual string TestFuncVariables => "'assert', 'expect', 'should', 'sinon'";

    protected virtual IEnumerable<string> AdditionalExecutionArguments
        => [TestsReporterArgument, JsonReportName];

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

        foreach (var test in executionContext.Input.Tests)
        {
            var processExecutionResult = await executor.Execute(
                this.Settings.NodeJsExecutablePath,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                test.Input,
                arguments);

            var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);

            var message = "yes";
            if (!string.IsNullOrEmpty(mochaResult.Error))
            {
                message = mochaResult.Error;
            }
            else if (mochaResult.TotalPassingTests != 1)
            {
                message = $"Unexpected error: {mochaResult.TestErrors[0]}";
            }

            var testResult = CheckAndGetTestResult(
                test,
                processExecutionResult,
                checker,
                message);
            testResults.Add(testResult);
        }

        return testResults;
    }
}

public record NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string NodeJsExecutablePath,
    string UnderscoreModulePath,
    string MochaModulePath,
    string ChaiModulePath,
    string SinonModulePath,
    string SinonChaiModulePath)
    : NodeJsPreprocessExecuteAndCheckExecutionStrategySettings(
        BaseTimeUsed,
        BaseMemoryUsed,
        NodeJsExecutablePath,
        UnderscoreModulePath);
