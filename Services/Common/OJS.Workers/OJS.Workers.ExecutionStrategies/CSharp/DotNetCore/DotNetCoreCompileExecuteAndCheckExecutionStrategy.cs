#nullable disable
namespace OJS.Workers.ExecutionStrategies.CSharp.DotNetCore;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OJS.Workers.Common;
using OJS.Workers.Common.Models;
using OJS.Workers.Compilers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;
using static OJS.Workers.Common.Constants;

public class DotNetCoreCompileExecuteAndCheckExecutionStrategy<TSettings> : BaseCompiledCodeExecutionStrategy<TSettings>
    where TSettings : DotNetCoreCompileExecuteAndCheckExecutionStrategySettings
{
    private const string DotNetCoreCodeStringTemplate = "{0}{1}{2}";

    public DotNetCoreCompileExecuteAndCheckExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        ICompilerFactory compilerFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, compilerFactory, settingsProvider, logger)
    {
    }

    private static IEnumerable<string> DotNetSixDefaultUsingNamespaces
        =>
        [
            "using System;",
            "using System.IO;",
            "using System.Collections.Generic;",
            "using System.Linq;",
            "using System.Net.Http;",
            "using System.Threading;",
            "using System.Threading.Tasks;",
        ];

    private string RuntimeConfigJsonTemplate => $@"
            {{
	            ""runtimeOptions"": {{
                    ""framework"": {{
                        ""name"": ""Microsoft.NETCore.App"",
                        ""version"": ""{this.Settings.DotNetCoreRuntimeVersion}""
                    }}
                }}
            }}";

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        var compileResult = this.ExecuteCompiling(
            executionContext,
            result);

        if (!compileResult.IsCompiledSuccessfully)
        {
            return result;
        }

        var executor = this.PrepareExecutor(
            compileResult,
            out var arguments);

        var checker = executionContext.Input.GetChecker();

        foreach (var test in executionContext.Input.Tests)
        {
            var processExecutionResult = await executor.Execute(
                this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type),
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                test.Input,
                arguments,
                this.WorkingDirectory);

            var testResult = CheckAndGetTestResult(
                test,
                processExecutionResult,
                checker,
                processExecutionResult.ReceivedOutput);

            result.Results.Add(testResult);
        }

        return result;
    }

    protected override async Task<IExecutionResult<OutputResult>> ExecuteAgainstSimpleInput(
        IExecutionContext<SimpleInputModel> executionContext,
        IExecutionResult<OutputResult> result)
    {
        var compileResult = this.ExecuteCompiling(
            executionContext,
            result);

        if (!compileResult.IsCompiledSuccessfully)
        {
            return result;
        }

        var executor = this.PrepareExecutor(
            compileResult,
            out var arguments);

        var processExecutionResult = await executor.Execute(
            this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type),
            executionContext.TimeLimit,
            executionContext.MemoryLimit,
            executionContext.Input?.Input,
            arguments,
            this.WorkingDirectory);

        var outputResult = GetOutputResult(processExecutionResult);

        result.Results.Add(outputResult);

        return result;
    }

    protected override string PreprocessCode<TInput>(IExecutionContext<TInput> executionContext)
    {
        if (this.Type != ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck ||
            string.IsNullOrWhiteSpace(executionContext.Code))
        {
            return base.PreprocessCode(executionContext);
        }

        return string.Format(
            DotNetCoreCodeStringTemplate,
            string.Join(
                Environment.NewLine,
                DotNetSixDefaultUsingNamespaces.Select(
                    ns => GetNamespaceIfNotExist(ns, executionContext.Code))),
            Environment.NewLine,
            executionContext.Code);
    }

    private static void CreateRuntimeConfigJsonFile(string directory, string text)
    {
        var compiledFileName = Directory
            .GetFiles(directory)
            .Select(Path.GetFileNameWithoutExtension)
            .First();

        var jsonFileName = $"{compiledFileName}.runtimeconfig{JsonFileExtension}";

        var jsonFilePath = Path.Combine(directory, jsonFileName);

        File.WriteAllText(jsonFilePath, text);
    }

    private static string GetNamespaceIfNotExist(string usingNamespace, string code)
        => code.Contains(usingNamespace)
            ? string.Empty
            : usingNamespace;

    private IExecutor PrepareExecutor(
        CompileResult compileResult,
        out string[] arguments)
    {
            var executor = this.CreateRestrictedExecutor();

        arguments =
        [
            compileResult.OutputFile
        ];

        CreateRuntimeConfigJsonFile(this.WorkingDirectory, this.RuntimeConfigJsonTemplate);

        return executor;
    }
}

public record DotNetCoreCompileExecuteAndCheckExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string DotNetCoreRuntimeVersion)
    : BaseCompiledCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
