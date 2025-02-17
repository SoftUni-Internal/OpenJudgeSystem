namespace OJS.Workers.ExecutionStrategies.NodeJs.Typescript;

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Extensions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;
using static OJS.Workers.ExecutionStrategies.NodeJs.NodeJsConstants;

public class TypeScriptPreprocessExecuteAndCheckExecutionStrategy<TSettings> : NodeJsPreprocessExecuteAndCheckExecutionStrategy<TSettings>
    where TSettings : TypeScriptPreprocessExecuteAndCheckExecutionStrategySettings
{
    private const string JsExtension = ".js";
    private const string TsExtension = ".ts";
    private const char TrimCharacter = ';';
    private const string TranspiledSubmissionFile = "/transpiled-submission.ts";
    private const string StrictJavaScriptPrefix = "\"use strict\";";

    public TypeScriptPreprocessExecuteAndCheckExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, settingsProvider, logger)
    {
        if (!File.Exists(this.Settings.TypeScriptExecutablePath))
        {
            throw new ArgumentException(
                $"TypeScript not found in: {this.Settings.TypeScriptExecutablePath}",
                nameof(this.Settings.TypeScriptExecutablePath));
        }
    }

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        var executor = this.CreateExecutor();

        string jsCodeSavePath;
        try
        {
            jsCodeSavePath = await this.SaveAndTranspileTypeScriptCodeAndGetJavaScriptCodePath(executionContext, executor);
        }
        catch (InvalidProcessExecutionOutputException exception)
        {
            result.IsCompiledSuccessfully = false;
            result.CompilerComment = exception.Message;
            return result;
        }

        if (string.IsNullOrWhiteSpace(jsCodeSavePath))
        {
            // The javascript code was not saved after transpilation.
            throw new FileNotFoundException("The transpiled javascript file was not found.");
        }

        var checker = executionContext.Input.GetChecker();

        var testResults = await this.ProcessTests(executionContext, executor, checker, jsCodeSavePath);

        result.Results.AddRange(testResults);

        return result;
    }

    protected override async Task<IExecutionResult<OutputResult>> ExecuteAgainstSimpleInput(
        IExecutionContext<SimpleInputModel> executionContext,
        IExecutionResult<OutputResult> result)
    {
        var executor = this.CreateExecutor();

        string jsCodeSavePath;
        try
        {
            jsCodeSavePath = await this.SaveAndTranspileTypeScriptCodeAndGetJavaScriptCodePath(executionContext, executor);
        }
        catch (InvalidProcessExecutionOutputException exception)
        {
            result.IsCompiledSuccessfully = false;
            result.CompilerComment = exception.Message;
            return result;
        }

        if (string.IsNullOrWhiteSpace(jsCodeSavePath))
        {
            // The javascript code was not saved after transpilation.
            throw new FileNotFoundException("The transpiled javascript file was not found.");
        }

        var processExecutionResult = await this.ExecuteCode(
            executionContext,
            executor,
            jsCodeSavePath,
            executionContext.Input.Input);

        result.Results.Add(GetOutputResult(processExecutionResult));

        return result;
    }

    private async Task<string> SaveAndTranspileTypeScriptCodeAndGetJavaScriptCodePath<TInput>(
        IExecutionContext<TInput> executionContext,
        IExecutor executor)
    {
        var tsCodeSavePath = FileHelpers.SaveStringToFile(executionContext.Code.Trim(TrimCharacter), FileHelpers.BuildPath(this.WorkingDirectory, TranspiledSubmissionFile));

        var transpilationResult = await this.TranspileCode(
            executionContext,
            executor,
            tsCodeSavePath);

        if (!string.IsNullOrWhiteSpace(transpilationResult.ReceivedOutput))
        {
            // Any output retrieved from the transpilation process is a type error, it should be shown to the user.
            throw new InvalidProcessExecutionOutputException(transpilationResult.ReceivedOutput);
        }

        if (!string.IsNullOrWhiteSpace(transpilationResult.ErrorOutput))
        {
            // An error occurred during the transpilation, this is a process error and should not be shown to the user.
            throw new ArgumentException(transpilationResult.ErrorOutput);
        }

        return this.ReadTranspiledCodeAndSaveAsCodeToExecute(executionContext, tsCodeSavePath);
    }

    private string ReadTranspiledCodeAndSaveAsCodeToExecute<TInput>(
        IExecutionContext<TInput> executionContext,
        string tsCodeSavePath)
    {
        var jsFile = FileHelpers.ReadFile(tsCodeSavePath.Replace(TsExtension, JsExtension));

        executionContext.Code = jsFile[StrictJavaScriptPrefix.Length..];

        return this.SaveCodeToTempFile(executionContext);
    }

    private Task<ProcessExecutionResult> TranspileCode<TInput>(
        IExecutionContext<TInput> executionContext,
        IExecutor executor,
        string tsFilePath)
        => executor.Execute(
            this.Settings.TypeScriptExecutablePath,
            executionContext.TimeLimit,
            executionContext.MemoryLimit,
            null,
            [tsFilePath, StrictFlag, TargetFlag, TargetValue, ModuleFlag, ModuleValue, LibFlag, LibValue]
        );

}

public record TypeScriptPreprocessExecuteAndCheckExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string TypeScriptExecutablePath,
    string NodeJsExecutablePath,
    string UnderscoreModulePath)
    : NodeJsPreprocessExecuteAndCheckExecutionStrategySettings(
        BaseTimeUsed,
        BaseMemoryUsed,
        NodeJsExecutablePath,
        UnderscoreModulePath);