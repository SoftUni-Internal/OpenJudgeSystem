#nullable disable
namespace OJS.Workers.ExecutionStrategies.Python;

using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;

using OJS.Workers.Common;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

using static OJS.Workers.Common.Constants;

public class PythonCodeExecuteAgainstUnitTestsExecutionStrategy<TSettings> : PythonExecuteAndCheckExecutionStrategy<TSettings>
    where TSettings : PythonCodeExecuteAgainstUnitTestsExecutionStrategySettings
{
    private const string ErrorInTestRegexPattern = @"^ERROR:[\s\S]+(^\w*Error:[\s\S]+)(?=^-{2,})";
    private const string FailedTestRegexPattern = @"^FAIL:[\s\S]+(^\w*Error:[\s\S]+)(?=^-{2,})";
    private const string SuccessTestsRegexPattern = @"^[.]+\s*[-]{2,}.+(?<=\r\n|\r|\n)OK(?=\s*?$)";
    private const string TestResultsRegexPattern = @"^([.FE]+)\s*.+(?<=\r\n|\r|\n)(OK|FAILED\s\(.+\))(?=\s*?$)";

    public PythonCodeExecuteAgainstUnitTestsExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, settingsProvider, logger)
    {
    }

    protected virtual Regex TestsRegex => new Regex(TestResultsRegexPattern, RegexOptions.Singleline);

    protected virtual Regex SuccessTestsRegex => new Regex(SuccessTestsRegexPattern, RegexOptions.Singleline);

    protected virtual Regex ErrorsInTestsRegex => new Regex(ErrorInTestRegexPattern, RegexOptions.Multiline);

    protected virtual Regex FailedTestsRegex => new Regex(FailedTestRegexPattern, RegexOptions.Multiline);

    protected static void WriteTestInCodeFile(string code, string codeSavePath, string testContent)
    {
        var codeAndTestText = code + Environment.NewLine + testContent;

        FileHelpers.WriteAllText(codeSavePath, codeAndTestText);
    }

    protected override async Task<TestResult> RunIndividualTest(
        string codeSavePath,
        IExecutor executor,
        IChecker checker,
        IExecutionContext<TestsInputModel> executionContext,
        TestContext test)
    {
        WriteTestInCodeFile(executionContext.Code, codeSavePath, test.Input);

        var processExecutionResult = await this.Execute(executionContext, executor, codeSavePath);

        var testResult = this.GetTestResult(processExecutionResult, test, checker);

        return testResult;
    }

    protected override async Task<ProcessExecutionResult> Execute<TInput>(
        IExecutionContext<TInput> executionContext,
        IExecutor executor,
        string codeSavePath,
        string input = null,
        string directory = null)
    {
        var processExecutionResult = await base.Execute(executionContext, executor, codeSavePath, input, directory);
        this.FixReceivedOutput(processExecutionResult);
        return processExecutionResult;
    }

    protected TestResult GetTestResult(
        ProcessExecutionResult processExecutionResult,
        TestContext test,
        IChecker checker)
    {
        var message = "Failing tests are not captured correctly. Please contact an Administrator.";

        var errorMatch = this.ErrorsInTestsRegex.Match(processExecutionResult.ReceivedOutput);
        var failedTestMatch = this.FailedTestsRegex.Match(processExecutionResult.ReceivedOutput);

        if (errorMatch.Success)
        {
            processExecutionResult.ErrorOutput = errorMatch.Groups[1].Value;
            processExecutionResult.Type = ProcessExecutionResultType.RunTimeError;
        }
        else if (failedTestMatch.Success)
        {
            message = failedTestMatch.Groups[1].Value;
        }
        else if (this.SuccessTestsRegex.IsMatch(processExecutionResult.ReceivedOutput))
        {
            message = TestPassedMessage;
        }

        var testResult = CheckAndGetTestResult(
            test,
            processExecutionResult,
            checker,
            message);

        return testResult;
    }

    protected void FixReceivedOutput(ProcessExecutionResult processExecutionResult)
    {
        var output = processExecutionResult.ErrorOutput;

        if (this.TestsRegex.IsMatch(output))
        {
            processExecutionResult.ReceivedOutput = output;
            processExecutionResult.ErrorOutput = string.Empty;
            processExecutionResult.Type = ProcessExecutionResultType.Success;
        }
    }
}

public record PythonCodeExecuteAgainstUnitTestsExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string PythonExecutablePath)
    : PythonExecuteAndCheckExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed, PythonExecutablePath);
