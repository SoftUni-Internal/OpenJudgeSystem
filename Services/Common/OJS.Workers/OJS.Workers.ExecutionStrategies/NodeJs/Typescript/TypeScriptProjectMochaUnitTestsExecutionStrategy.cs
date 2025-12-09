namespace OJS.Workers.ExecutionStrategies.NodeJs.Typescript;

using Common;
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
    where TSettings : TypeScriptProjectMochaUnitTestsExecutionStrategySettings
{
    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(IExecutionContext<TestsInputModel> executionContext, IExecutionResult<TestResult> result,
        CancellationToken cancellationToken = default)
    {
        SaveZipSubmission(executionContext.FileContent, this.WorkingDirectory);

        var executor = this.CreateStandardExecutor();
        var bundleResult = await executor.Execute(
            this.Settings.EsBuildModulePath,
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

public record TypeScriptProjectMochaUnitTestsExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string NodeJsExecutablePath,
    string UnderscoreModulePath,
    string MochaModulePath,
    string ChaiModulePath,
    string SinonModulePath,
    string SinonChaiModulePath,
    string EsBuildModulePath)
    : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings(
        BaseTimeUsed,
        BaseMemoryUsed,
        NodeJsExecutablePath,
        UnderscoreModulePath,
        MochaModulePath,
        ChaiModulePath,
        SinonModulePath,
        SinonChaiModulePath);