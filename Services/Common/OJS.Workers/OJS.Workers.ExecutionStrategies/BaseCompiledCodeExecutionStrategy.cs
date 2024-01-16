﻿namespace OJS.Workers.ExecutionStrategies
{
    using System;
    using System.IO;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public abstract class BaseCompiledCodeExecutionStrategy<TSettings> : BaseCodeExecutionStrategy<TSettings>
        where TSettings : BaseCompiledCodeExecutionStrategySettings
    {
        protected BaseCompiledCodeExecutionStrategy(
            ExecutionStrategyType type,
            IProcessExecutorFactory processExecutorFactory,
            ICompilerFactory compilerFactory,
            IExecutionStrategySettingsProvider settingsProvider)
            : base(type, processExecutorFactory, settingsProvider)
            => this.CompilerFactory = compilerFactory;

        protected ICompilerFactory CompilerFactory { get; }

        protected async Task<IExecutionResult<TestResult>> CompileExecuteAndCheck(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result,
            IExecutor executor,
            bool useSystemEncoding = true,
            bool dependOnExitCodeForRunTimeError = false,
            bool useWorkingDirectoryForProcess = false)
        {
            // Compile the file
            var compileResult = this.ExecuteCompiling(
                executionContext,
                result,
                useWorkingDirectoryForProcess);

            if (!compileResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var outputFile = compileResult.OutputFile;

            // Execute and check each test
            var checker = executionContext.Input.GetChecker();

            foreach (var test in executionContext.Input.Tests)
            {
                var processExecutionResult = await executor.Execute(
                    outputFile,
                    test.Input,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    null,
                    null,
                    false,
                    useSystemEncoding,
                    dependOnExitCodeForRunTimeError);

                var testResult = CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.Results.Add(testResult);
            }

            return result;
        }

        protected CompileResult ExecuteCompiling<TInput, TResult>(
            IExecutionContext<TInput> executionContext,
            IExecutionResult<TResult> result,
            bool useWorkingDirectoryForProcess = false)
            where TResult : ISingleCodeRunResult, new()
        {
            var submissionFilePath = this.SaveCodeToTempFile(executionContext);

            var compileResult = this.Compile(
                executionContext.CompilerType,
                this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type),
                executionContext.AdditionalCompilerArguments,
                submissionFilePath,
                useWorkingDirectoryForProcess);

            result.IsCompiledSuccessfully = compileResult.IsCompiledSuccessfully;
            result.CompilerComment = compileResult.CompilerComment;

            return compileResult;
        }

        protected virtual CompileResult Compile(
            CompilerType compilerType,
            string compilerPath,
            string compilerArguments,
            string submissionFilePath,
            bool useWorkingDirectoryForProcess = false)
        {
            if (compilerType == CompilerType.None)
            {
                return new CompileResult(true, null) { OutputFile = submissionFilePath };
            }

            if (!File.Exists(compilerPath))
            {
                throw new ArgumentException($"Compiler not found in: {compilerPath}", nameof(compilerPath));
            }

            var compiler = this.CompilerFactory.CreateCompiler(compilerType, this.Type);
            var compilerResult = compiler.Compile(compilerPath, submissionFilePath, compilerArguments, useWorkingDirectoryForProcess);

            return compilerResult;
        }
    }

#pragma warning disable SA1402
    public class BaseCompiledCodeExecutionStrategySettings : BaseCodeExecutionStrategySettings
#pragma warning restore SA1402
    {
    }
}