namespace OJS.Workers.ExecutionStrategies.NodeJs.Typescript;

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using OJS.Workers.Common;
using OJS.Workers.Common.Extensions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using OJS.Workers.Compilers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

public class TypeScriptV20PreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<TSettings>(
    IOjsSubmission submission,
    IProcessExecutorFactory processExecutorFactory,
    IExecutionStrategySettingsProvider settingsProvider,
    ILogger<BaseExecutionStrategy<TSettings>> logger,
    ICompilerFactory compilerFactory)
    : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<TSettings>(submission, processExecutorFactory,
        settingsProvider, logger)
    where TSettings : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings
{
    protected const string TestsPlaceholder = "/*#testsCode*/";
    protected const string UserCodePlaceholder = "/*#userCode#*/";
    protected const string SolutionSkeletonPlaceholder = "/*#solutionSkeleton#*/";

    private string TypeScriptTemplateContent => @$"
    // Imports
    // @ts-ignore
    const chai = require('{this.Settings.ChaiModulePath}');
    // @ts-ignore
    const sinon = require('{this.Settings.SinonModulePath}');
    // @ts-ignore
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
    // @ts-ignore
    describe('Tests', function () {{
        {TestsPlaceholder}
    }});
";

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        // Prepare TypeScript file with combined user code and tests
        var typeScriptTemplate = this.TypeScriptTemplateContent;
        var userCode = executionContext.Code.Trim();
        typeScriptTemplate = typeScriptTemplate
            .Replace(SolutionSkeletonPlaceholder, executionContext.Input.TaskSkeletonAsString)
            .Replace(UserCodePlaceholder, userCode);

        // Process each test and wrap it in an it() block with @ts-ignore
        if (executionContext.Input.Tests.Any())
        {
            var formattedTests = FormatTests(executionContext.Input.Tests);
            typeScriptTemplate = typeScriptTemplate.Replace(TestsPlaceholder, formattedTests);
        }

        // Save the combined TypeScript file
        var tsCodeSavePath = FileHelpers.SaveStringToTempFile(this.WorkingDirectory, typeScriptTemplate);

        // Run TypeScript compiler to generate JavaScript
        var compiler = compilerFactory.CreateCompiler(executionContext.CompilerType, this.Type);
        var compilerPath = compilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type);
        var compilerResult = compiler.Compile(compilerPath, tsCodeSavePath, executionContext.AdditionalCompilerArguments);

        if (!compilerResult.IsCompiledSuccessfully)
        {
            result.IsCompiledSuccessfully = false;
            result.CompilerComment = compilerResult.CompilerComment;
            return result;
        }

        // Execute tests using Node.js on the generated JavaScript file
        var executor = this.CreateRestrictedExecutor();
        var checker = executionContext.Input.GetChecker();

        var testResults = await this.ProcessTests(
            executionContext,
            executor,
            checker,
            compilerResult.OutputFile);
        result.Results.AddRange(testResults);

        return result;
    }

    private static string FormatTests(IEnumerable<TestContext> tests)
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
// @ts-ignore
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
            "--reporter", "json"
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
}