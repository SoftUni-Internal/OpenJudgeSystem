namespace OJS.Workers.ExecutionStrategies.Java;

using System.IO;

using Ionic.Zip;
using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Compilers;
using OJS.Workers.Executors;

public class JavaZipFileCompileExecuteAndCheckExecutionStrategy<TSettings> : JavaPreprocessCompileExecuteAndCheckExecutionStrategy<TSettings>
    where TSettings : JavaZipFileCompileExecuteAndCheckExecutionStrategySettings
{
    protected const string SubmissionFileName = "_$submission";

    public JavaZipFileCompileExecuteAndCheckExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        ICompilerFactory compilerFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, compilerFactory, settingsProvider, logger)
    {
    }

    protected override string CreateSubmissionFile<TInput>(IExecutionContext<TInput> executionContext)
    {
        var trimmedAllowedFileExtensions = executionContext.AllowedFileExtensions?.Trim();

        var allowedFileExtensions = (!trimmedAllowedFileExtensions?.StartsWith('.') ?? false)
            ? $".{trimmedAllowedFileExtensions}"
            : trimmedAllowedFileExtensions;

        if (allowedFileExtensions != Constants.ZipFileExtension)
        {
            throw new SolutionException("The submission file is not a zip file!");
        }

        return this.PrepareSubmissionFile(executionContext.FileContent);
    }

    protected override CompileResult DoCompile<TInput>(
        IExecutionContext<TInput> executionContext,
        string submissionFilePath)
    {
        // Compile the zip file with user code and sandbox executor
        var compilerResult = this.Compile(
            executionContext.CompilerType,
            this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type),
            executionContext.AdditionalCompilerArguments + this.ClassPathArgument,
            submissionFilePath);

        return compilerResult;
    }

    private string PrepareSubmissionFile(byte[] submissionFileContent)
    {
        var submissionFilePath = FileHelpers.BuildPath(this.WorkingDirectory, SubmissionFileName);

        File.WriteAllBytes(submissionFilePath, submissionFileContent);
        FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
        this.AddSandboxExecutorSourceFileToSubmissionZip(submissionFilePath);

        return submissionFilePath;
    }

    private void AddSandboxExecutorSourceFileToSubmissionZip(string submissionZipFilePath)
    {
        using var zipFile = new ZipFile(submissionZipFilePath);

        zipFile.AddFile(this.SandboxExecutorSourceFilePath, string.Empty);

        zipFile.Save();
    }
}

public record JavaZipFileCompileExecuteAndCheckExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string JavaExecutablePath,
    string JavaLibrariesPath,
    int BaseUpdateTimeOffset) : JavaPreprocessCompileExecuteAndCheckExecutionStrategySettings(
    BaseTimeUsed, BaseMemoryUsed, JavaExecutablePath, JavaLibrariesPath, BaseUpdateTimeOffset);
