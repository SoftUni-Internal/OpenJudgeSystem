namespace OJS.Workers.ExecutionStrategies.NodeJs.Typescript;

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using OJS.Workers.Common;
using OJS.Workers.Common.Extensions;
using OJS.Workers.Common.Helpers;
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
    protected const string SolutionSkeletonPlaceholder = "#solutionSkeleton#";

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
        // Prepare TypeScript file with combined user code and skeleton
        var typeScriptTemplate = this.TypeScriptTemplateContent;
        var userCode = executionContext.Code.Trim();
        typeScriptTemplate = typeScriptTemplate
            .Replace(SolutionSkeletonPlaceholder, executionContext.Input.TaskSkeletonAsString)
            .Replace(UserCodePlaceholder, userCode);

        // Process each test and wrap it in an it() block with @ts-ignore
        if (executionContext.Input.Tests.Any())
        {
            var formattedTests = this.FormatTests(executionContext.Input.Tests, true);
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

        var testResults = await base.ProcessTests(
            executionContext,
            executor,
            checker,
            compilerResult.OutputFile);
        result.Results.AddRange(testResults);

        return result;
    }
}