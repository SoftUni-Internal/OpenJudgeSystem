namespace OJS.Workers.ExecutionStrategies.NodeJs.Typescript;

using Common;
using Common.Helpers;
using Compilers;
using Executors;
using Microsoft.Extensions.Logging;
using Models;

public class TypeScriptProjectMochaUnitTestsExecutionStrategy<TSettings>(
    IOjsSubmission submission,
    IProcessExecutorFactory processExecutorFactory,
    IExecutionStrategySettingsProvider settingsProvider,
    ILogger<BaseExecutionStrategy<TSettings>> logger,
    ICompilerFactory compilerFactory)
    : TypeScriptPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<TSettings>(
        submission,
        processExecutorFactory,
        settingsProvider,
        logger,
        compilerFactory)
    where TSettings : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings
{
    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(IExecutionContext<TestsInputModel> executionContext, IExecutionResult<TestResult> result,
        CancellationToken cancellationToken = default)
    {
        SaveZipSubmission(executionContext.FileContent, this.WorkingDirectory);

        var executor = this.CreateStandardExecutor();
        var bundleResult = await executor.Execute(
            "/judge-resources/js/v20/node_modules/esbuild/bin/esbuild",
            executionContext.TimeLimit,
            executionContext.MemoryLimit,
            executionArguments: ["src/index.ts", "--bundle", "--platform=node", "--format=cjs", "--target=node21", "--packages=external", "--outfile=dist/app.bundle.js"],
            workingDirectory: this.WorkingDirectory,
            cancellationToken: cancellationToken);

        if (bundleResult.ExitCode != 0)
        {
            return new ExecutionResult<TestResult>
            {
                IsCompiledSuccessfully = false,
                CompilerComment = bundleResult.ErrorOutput,
            };
        }

        return await base.ExecuteAgainstTestsInput(executionContext, result, cancellationToken);
    }
}