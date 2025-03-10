namespace OJS.Workers.ExecutionStrategies.NodeJs.Typescript;

using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.Common.Extensions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Compilers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

public class TypeScriptPreprocessExecuteAndCheckExecutionStrategy<TSettings>(
    IOjsSubmission submission,
    IProcessExecutorFactory processExecutorFactory,
    IExecutionStrategySettingsProvider settingsProvider,
    ILogger<BaseExecutionStrategy<TSettings>> logger,
    ICompilerFactory compilerFactory)
    : NodeJsPreprocessExecuteAndCheckExecutionStrategy<TSettings>(submission, processExecutorFactory, settingsProvider, logger)
    where TSettings : TypeScriptPreprocessExecuteAndCheckExecutionStrategySettings
{
    private const char TrimCharacter = ';';
    private const string StrictJavaScriptPrefix = "\"use strict\";";

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        var compilerResult = this.Compile(executionContext);

        if (!compilerResult.IsCompiledSuccessfully)
        {
            result.IsCompiledSuccessfully = false;
            result.CompilerComment = compilerResult.CompilerComment;
            return result;
        }

        var jsCodeSavePath = this.ReadTranspiledCodeAndSaveAsCodeToExecute(executionContext, compilerResult.OutputFile);

        var executor = this.CreateRestrictedExecutor();
        var checker = executionContext.Input.GetChecker();

        var testResults = await this.ProcessTests(executionContext, executor, checker, jsCodeSavePath);

        result.Results.AddRange(testResults);

        return result;
    }

    protected override async Task<IExecutionResult<OutputResult>> ExecuteAgainstSimpleInput(
        IExecutionContext<SimpleInputModel> executionContext,
        IExecutionResult<OutputResult> result)
    {
        var compilerResult = this.Compile(executionContext);

        if (!compilerResult.IsCompiledSuccessfully)
        {
            result.IsCompiledSuccessfully = false;
            result.CompilerComment = compilerResult.CompilerComment;
            return result;
        }

        var jsCodeSavePath = this.ReadTranspiledCodeAndSaveAsCodeToExecute(executionContext, compilerResult.OutputFile);

        var executor = this.CreateRestrictedExecutor();

        var processExecutionResult = await this.ExecuteCode(
            executionContext,
            executor,
            jsCodeSavePath,
            executionContext.Input.Input);

        result.Results.Add(GetOutputResult(processExecutionResult));

        return result;
    }

    private CompileResult Compile<TInput>(IExecutionContext<TInput> executionContext)
    {
        var tsCodeSavePath = FileHelpers.SaveStringToTempFile(this.WorkingDirectory, executionContext.Code.Trim(TrimCharacter));

        var compiler = compilerFactory.CreateCompiler(executionContext.CompilerType, this.Type);
        var compilerPath = compilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type);
        var compilerResult = compiler.Compile(compilerPath, tsCodeSavePath, executionContext.AdditionalCompilerArguments);

        return compilerResult;
    }

    private string ReadTranspiledCodeAndSaveAsCodeToExecute<TInput>(
        IExecutionContext<TInput> executionContext,
        string jsCodeSavePath)
    {
        var jsFile = FileHelpers.ReadFile(jsCodeSavePath);

        executionContext.Code = jsFile[StrictJavaScriptPrefix.Length..];

        return this.SaveCodeToTempFile(executionContext);
    }
}

public record TypeScriptPreprocessExecuteAndCheckExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string NodeJsExecutablePath,
    string UnderscoreModulePath)
    : NodeJsPreprocessExecuteAndCheckExecutionStrategySettings(
        BaseTimeUsed,
        BaseMemoryUsed,
        NodeJsExecutablePath,
        UnderscoreModulePath);