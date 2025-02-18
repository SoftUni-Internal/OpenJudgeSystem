#nullable disable
namespace OJS.Workers.ExecutionStrategies.CSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;
using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Extensions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using OJS.Workers.Compilers;
using OJS.Workers.ExecutionStrategies.Extensions;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;
using static OJS.Workers.Common.Constants;

public class CSharpProjectTestsExecutionStrategy<TSettings> : BaseCompiledCodeExecutionStrategy<TSettings>
    where TSettings : CSharpProjectTestsExecutionStrategySettings
{
    protected const string SetupFixtureTemplate = @"
        using System;
        using System.IO;
        using NUnit.Framework;

        [SetUpFixture]
        public class SetUpClass
        {
            [OneTimeSetUp]
            public void RedirectConsoleOutputBeforeEveryTest()
            {
                TextWriter writer = new StringWriter();
                Console.SetOut(writer);
            }
        }
";

    protected const string SetupFixtureFileName = "_$SetupFixture";
    protected const string CompeteTest = "Test";
    protected const string TrialTest = "Test.000";
    protected const string CsProjFileSearchPattern = "*.csproj";

    protected const string NUnitReference =
        "nunit.framework, Version=3.8.0.0, Culture=neutral, PublicKeyToken=2638cd05610744eb, processorArchitecture=MSIL";

    protected const string EntityFrameworkCoreInMemoryReference =
        "Microsoft.EntityFrameworkCore.InMemory, Version=1.1.3.0, Culture=neutral, PublicKeyToken=adb9793829ddae60, processorArchitecture=MSIL";

    protected const string SystemDataCommonReference =
        "System.Data.Common, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a, processorArchitecture=MSIL";

    protected const string AdditionalExecutionArguments = "--noresult --inprocess";
    protected const string VsttPackageName = "Microsoft.VisualStudio.QualityTools.UnitTestFramework";

    // Extracts the number of total and passed tests
    protected const string TestResultsRegex =
        @"Test Count: (\d+), Passed: (\d+), Failed: (\d+), Warnings: \d+, Inconclusive: \d+, Skipped: \d+";

    // Extracts error/failure messages and the class which threw it
    protected static readonly string ErrorMessageRegex =
        @"((?:\d+|\d+-\d+)\) (?:Failed|Error)\s:\s(.*)\.(.*))\r?\n((?:.*)\r?\n(?:.*))";

    public CSharpProjectTestsExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        ICompilerFactory compilerFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, compilerFactory, settingsProvider, logger)
    {
        this.TestNames = [];
        this.TestPaths = [];
    }

    protected string NUnitConsoleRunnerPath { get; }

    protected string SetupFixturePath { get; set; }

    protected List<string> TestNames { get; }

    protected List<string> TestPaths { get; }

    protected static (int totalTestsCount, int failedTestsCount) ExtractTotalFailedTestsCount(string testsOutput)
    {
        var testsSummaryMatcher = new Regex(TestResultsRegex);
        var testsSummaryMatches = testsSummaryMatcher.Matches(testsOutput);
        if (testsSummaryMatches.Count == 0)
        {
            throw new InvalidProcessExecutionOutputException();
        }

        var failedTestsCount = int.Parse(testsSummaryMatches[testsSummaryMatches.Count - 1].Groups[3].Value);
        var totalTestsCount = int.Parse(testsSummaryMatches[testsSummaryMatches.Count - 1].Groups[1].Value);
        return (totalTestsCount, failedTestsCount);
    }

    protected static (int totalTests, int passedTests) ExtractTotalAndPassedTestsCount(MatchCollection matches)
    {
        // Grabs the last match from a match collection,
        // since the NUnit output is always the last one,
        // thus ensuring that the tests output is the genuine one,
        // preventing the user from tampering with it
        var lastMatch = matches[^1];

        var totalTests = int.Parse(lastMatch.Groups[1].Value);
        var passedTests = int.Parse(lastMatch.Groups[2].Value);

        return (totalTests, passedTests);
    }

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        SaveZipSubmission(executionContext.FileContent, this.WorkingDirectory);

        var csProjFilePath = this.GetCsProjFilePath();

        this.ExtractTestNames(executionContext.Input.Tests);

        var project = new Project(csProjFilePath);
        var compileDirectory = project.DirectoryPath;

        this.SaveTestFiles(executionContext.Input.Tests, compileDirectory);
        this.SaveSetupFixture(compileDirectory);

        this.CorrectProjectReferences(project);

        var compilerPath = this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type);
        var compilerResult = this.Compile(
            executionContext.CompilerType,
            compilerPath,
            executionContext.AdditionalCompilerArguments,
            csProjFilePath);

        result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
        result.CompilerComment = compilerResult.CompilerComment;

        if (!compilerResult.IsCompiledSuccessfully)
        {
            return result;
        }

        // Delete tests before execution so the user can't access them
        FileHelpers.DeleteFiles([.. this.TestPaths]);

        var executor = this.CreateExecutor();

        return await this.RunUnitTests(
            this.NUnitConsoleRunnerPath,
            executionContext,
            executor,
            executionContext.Input.GetChecker(),
            result,
            compilerResult.OutputFile,
            AdditionalExecutionArguments);
    }

    protected void SaveSetupFixture(string directory)
    {
        this.SetupFixturePath = $"{directory}\\{SetupFixtureFileName}{cSharpFileExtension}";
        File.WriteAllText(this.SetupFixturePath, SetupFixtureTemplate);
        this.TestPaths.Add(this.SetupFixturePath);
    }

    protected void SaveTestFiles(IEnumerable<TestContext> tests, string compileDirectory)
    {
        var index = 0;
        foreach (var test in tests)
        {
            var testName = this.TestNames[index++];
            var testedCodePath = FileHelpers.BuildPath(compileDirectory, $"{testName}{cSharpFileExtension}");
            this.TestPaths.Add(testedCodePath);
            File.WriteAllText(testedCodePath, test.Input);
        }
    }

    protected virtual async Task<IExecutionResult<TestResult>> RunUnitTests(
        string consoleRunnerPath,
        IExecutionContext<TestsInputModel> executionContext,
        IExecutor executor,
        IChecker checker,
        IExecutionResult<TestResult> result,
        string compiledFile,
        string additionalExecutionArguments)
    {
        var arguments = new List<string> { $"\"{compiledFile}\"" };
        arguments.AddRange(additionalExecutionArguments.Split(' '));

        var processExecutionResult = await executor.Execute(
            consoleRunnerPath,
            executionContext.TimeLimit,
            executionContext.MemoryLimit,
            executionArguments: arguments,
            useProcessTime: false,
            useSystemEncoding: true);

        if (!string.IsNullOrWhiteSpace(processExecutionResult.ErrorOutput))
        {
            throw new InvalidProcessExecutionOutputException(processExecutionResult.ErrorOutput);
        }

        var (totalTestsCount, failedTestsCount) =
            ExtractTotalFailedTestsCount(processExecutionResult.ReceivedOutput);

        var errorsByFiles = this.GetTestErrors(processExecutionResult.ReceivedOutput);

        if (failedTestsCount != errorsByFiles.Count || totalTestsCount != executionContext.Input.Tests.Count())
        {
            throw new ArgumentException("Failed tests were not captured properly.");
        }

        var testIndex = 0;

        foreach (var test in executionContext.Input.Tests)
        {
            var message = TestPassedMessage;
            var testFile = this.TestNames[testIndex++];
            if (errorsByFiles.TryGetValue(testFile, out var file))
            {
                message = file;
            }

            var testResult = CheckAndGetTestResult(test, processExecutionResult, checker, message);
            result.Results.Add(testResult);
        }

        return result;
    }

    protected virtual Dictionary<string, string> GetTestErrors(string receivedOutput)
    {
        var errorsByFiles = new Dictionary<string, string>();
        var errorRegex = new Regex(ErrorMessageRegex);
        var errors = errorRegex.Matches(receivedOutput);

        foreach (Match error in errors)
        {
            var failedAssert = error.Groups[1].Value;
            var cause = error.Groups[4].Value;
            var fileName = error.Groups[2].Value;
            var output = $"{failedAssert} : {cause}".ToSingleLine();

            if (!errorsByFiles.TryAdd(fileName, output))
            {
                errorsByFiles[fileName] += ". " + output;
            }
        }

        return errorsByFiles;
    }

    protected virtual void CorrectProjectReferences(Project project)
    {
        var additionalCompileItems = new List<string>(this.TestNames) { SetupFixtureFileName };

        project.AddCompileItems(additionalCompileItems);

        project.EnsureAssemblyNameIsCorrect();

        project.SetProperty("OutputType", "Library");

        project.AddReferences(
            NUnitReference,
            EntityFrameworkCoreInMemoryReference,
            SystemDataCommonReference);

        // Check for VSTT just in case, we don't want Assert conflicts
        project.RemoveItemByName(VsttPackageName);

        project.Save(project.FullPath);
        project.ProjectCollection.UnloadAllProjects();

        project.RemoveNuGetPackageImportsTarget();
    }

    protected virtual void ExtractTestNames(IEnumerable<TestContext> tests)
    {
        foreach (var test in tests)
        {
            var testName = CSharpPreprocessorHelper.GetClassName(test.Input);
            this.TestNames.Add(testName);
        }
    }

    protected virtual string GetCsProjFilePath() => FileHelpers.FindFileMatchingPattern(
        this.WorkingDirectory,
        CsProjFileSearchPattern,
        f => new FileInfo(f).Length);
}

public record CSharpProjectTestsExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed)
    : BaseCompiledCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);