#nullable disable
namespace OJS.Workers.Compilers;

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using OJS.Workers.Common;

/// <summary>
/// Defines the base of the work with compilers algorithm and allow the subclasses to implement some of the algorithm parts.
/// </summary>
/// <remarks>Template method design pattern is used.</remarks>
public abstract class Compiler : ICompiler
{
    protected const string CompilationDirectoryName = "CompilationDir";

    protected Compiler(int processExitTimeOutMultiplier) =>
        this.MaxProcessExitTimeOutInMilliseconds =
            Constants.DefaultProcessExitTimeOutMilliseconds * processExitTimeOutMultiplier;

    public virtual bool ShouldDeleteSourceFile => true;

    public virtual int MaxProcessExitTimeOutInMilliseconds { get; }

    protected string CompilationDirectory { get; set; }

    public virtual CompileResult Compile(
        string compilerPath,
        string inputFile,
        string additionalArguments,
        bool useInputFileDirectoryAsWorking = false)
    {
        ArgumentNullException.ThrowIfNull(compilerPath);

        ArgumentNullException.ThrowIfNull(inputFile);

        if (!File.Exists(compilerPath))
        {
            return new CompileResult(false, $"Compiler not found! Searched in: {compilerPath}");
        }

        if (!File.Exists(inputFile))
        {
            return new CompileResult(false, $"Input file not found! Searched in: {inputFile}");
        }

        var inputFileDirectory = Path.GetDirectoryName(inputFile);

        if (inputFileDirectory == null)
        {
            return new CompileResult(false, $"Input file directory is null. Input file path value: {inputFile}");
        }

        this.CompilationDirectory = Path.Combine(inputFileDirectory, CompilationDirectoryName);
        Directory.CreateDirectory(this.CompilationDirectory);

        // Move source file if needed
        var newInputFilePath = this.RenameInputFile(inputFile);
        if (newInputFilePath != inputFile)
        {
            File.Move(inputFile, newInputFilePath);
            inputFile = newInputFilePath;
        }

        // Build compiler arguments
        var outputFile = this.GetOutputFileName(inputFile);
        var arguments = this.BuildCompilerArguments(inputFile, outputFile, additionalArguments);

        // Find compiler directory
        var directoryInfo = new FileInfo(compilerPath).Directory;
        if (directoryInfo == null)
        {
            return new CompileResult(false, $"Compiler directory is null. Compiler path value: {compilerPath}");
        }

        var workingDirectoryInfo = useInputFileDirectoryAsWorking
            ? new DirectoryInfo(inputFileDirectory)
            : directoryInfo;

        // Prepare process start information
        var processStartInfo = this.SetCompilerProcessStartInfo(compilerPath, workingDirectoryInfo, arguments);

        // Execute compiler
        var compilerOutput = ExecuteCompiler(processStartInfo, this.MaxProcessExitTimeOutInMilliseconds);

        if (this.ShouldDeleteSourceFile)
        {
            if (File.Exists(newInputFilePath))
            {
                File.Delete(newInputFilePath);
            }
        }

        // Check results and return CompilerResult instance
        if (!compilerOutput.IsSuccessful)
        {
            // Compiled file is missing
            return new CompileResult(false, $"Compiled file is missing. Compiler output: {compilerOutput.Output}");
        }

        outputFile = this.ChangeOutputFileAfterCompilation(outputFile);

        if (!string.IsNullOrWhiteSpace(compilerOutput.Output))
        {
            // Compile file is ready but the compiler has something on standard error (possibly compile warnings)
            return new CompileResult(true, compilerOutput.Output, outputFile);
        }

        // Compilation is ready without warnings
        return new CompileResult(outputFile);
    }

    public virtual string RenameInputFile(string inputFile)
        => inputFile;

    public virtual string GetOutputFileName(string inputFileName)
        => inputFileName + ".exe";

    public virtual string ChangeOutputFileAfterCompilation(string outputFile)
        => outputFile;

    public abstract string BuildCompilerArguments(string inputFile, string outputFile, string additionalArguments);

    public virtual ProcessStartInfo SetCompilerProcessStartInfo(
        string compilerPath,
        DirectoryInfo directoryInfo,
        string arguments)
        => new ProcessStartInfo(compilerPath)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = directoryInfo.ToString(),
            Arguments = arguments,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

    protected static CompilerOutput ExecuteCompiler(
        ProcessStartInfo compilerProcessStartInfo,
        int processExitTimeOutMillisecond)
    {
        var outputBuilder = new StringBuilder();
        var errorOutputBuilder = new StringBuilder();
        int exitCode;

        var outputWaitHandle = new AutoResetEvent(false);
        var errorWaitHandle = new AutoResetEvent(false);
        using (outputWaitHandle)
        {
            using (errorWaitHandle)
            {
                using var process = new Process();
                process.StartInfo = compilerProcessStartInfo;

                var outputHandle = new DataReceivedEventHandler((sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                });

                var errorHandle = new DataReceivedEventHandler((sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        errorOutputBuilder.AppendLine(e.Data);
                    }
                });

                process.OutputDataReceived += outputHandle;
                process.ErrorDataReceived += errorHandle;

                var started = process.Start();
                if (!started)
                {
                    return new CompilerOutput(1, "Could not start compiler.");
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var exited = process.WaitForExit(processExitTimeOutMillisecond);
                if (!exited)
                {
                    process.CancelOutputRead();
                    process.CancelErrorRead();

                    // Double check if the process has exited before killing it
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }

                    return new CompilerOutput(1, "Compiler process timed out.");
                }

                outputWaitHandle.WaitOne(300);
                errorWaitHandle.WaitOne(300);
                process.OutputDataReceived -= outputHandle;
                process.ErrorDataReceived -= errorHandle;
                exitCode = process.ExitCode;
            }
        }

        var output = outputBuilder.ToString().Trim();
        var errorOutput = errorOutputBuilder.ToString().Trim();

        var compilerOutput = $"{output}{Environment.NewLine}{errorOutput}".Trim();
        return new CompilerOutput(exitCode, compilerOutput);
    }
}
