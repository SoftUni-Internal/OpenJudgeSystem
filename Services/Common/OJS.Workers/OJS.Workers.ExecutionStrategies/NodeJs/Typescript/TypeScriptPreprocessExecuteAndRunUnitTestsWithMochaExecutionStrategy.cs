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

public class TypeScriptPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<TSettings>(
    IOjsSubmission submission,
    IProcessExecutorFactory processExecutorFactory,
    IExecutionStrategySettingsProvider settingsProvider,
    ILogger<BaseExecutionStrategy<TSettings>> logger,
    ICompilerFactory compilerFactory)
    : NodeJsPreprocessExecuteAndRunAllUnitTestsWithMochaExecutionStrategy<TSettings>(submission, processExecutorFactory,
        settingsProvider, logger)
    where TSettings : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings
{
    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result,
        CancellationToken cancellationToken = default)
    {
        // Prepare TypeScript file with combined user code and tests
        var typeScriptTemplate = this.GetTemplateContent(isTypeScript: true);
        var userCode = executionContext.Code.Trim();
        typeScriptTemplate = typeScriptTemplate
            .Replace(SolutionSkeletonPlaceholder, executionContext.Input.TaskSkeletonAsString)
            .Replace(UserCodePlaceholder, userCode);

        // Process each test and wrap it in an it() block with @ts-ignore
        if (executionContext.Input.Tests.Any())
        {
            var formattedTests = FormatTests(executionContext.Input.Tests, true);
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
}