namespace OJS.Workers.ExecutionStrategies.Java;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using OJS.Workers.Common;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using OJS.Workers.Compilers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

using static OJS.Workers.ExecutionStrategies.Helpers.JavaStrategiesHelper;

public class JavaPreprocessCompileExecuteAndCheckExecutionStrategy<TSettings> : BaseCompiledCodeExecutionStrategy<TSettings>
    where TSettings : JavaPreprocessCompileExecuteAndCheckExecutionStrategySettings
{
    protected const string TimeMeasurementFileName = "_$time.txt";
    protected const string SandboxExecutorClassName = "_$SandboxExecutor";
    protected const string JavaCompiledFileExtension = ".class";

    private const double NanosecondsInOneMillisecond = 1000000;

    public JavaPreprocessCompileExecuteAndCheckExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        ICompilerFactory compilerFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, compilerFactory, settingsProvider, logger)
    {
        if (!File.Exists(this.Settings.JavaExecutablePath))
        {
            throw new ArgumentException($"Java not found in: {this.Settings.JavaExecutablePath}!",
                nameof(this.Settings.JavaExecutablePath));
        }

        if (!Directory.Exists(this.Settings.JavaLibrariesPath))
        {
            throw new ArgumentException(
                $"Java libraries not found in: {this.Settings.JavaLibrariesPath}",
                nameof(this.Settings.JavaLibrariesPath));
        }
    }

    protected static string SandboxExecutorCode
        => @"
import java.io.File;
import java.io.FilePermission;
import java.io.FileWriter;
import java.lang.reflect.Method;
import java.lang.reflect.ReflectPermission;
import java.net.NetPermission;
import java.security.Permission;
import java.util.PropertyPermission;

public class " + SandboxExecutorClassName + @" {
    private static final String MAIN_METHOD_NAME = ""main"";

    public static void main(String[] args) throws Throwable {
        if (args.length == 0) {
            throw new IllegalArgumentException(""The name of the class to execute not provided!"");
        }

        String className = args[0];
        Class<?> userClass = Class.forName(className);

        Method mainMethod = userClass.getMethod(MAIN_METHOD_NAME, String[].class);

        FileWriter writer = null;
        long startTime = 0;
        try {
            if (args.length == 2) {
                String timeFilePath = args[1];
                writer = new FileWriter(timeFilePath, false);
            }

            // Set the sandbox security manager
            // _$SandboxSecurityManager securityManager = new _$SandboxSecurityManager();
            // System.setSecurityManager(securityManager);

            startTime = System.nanoTime();

            mainMethod.invoke(userClass, (Object) args);
        } catch (Throwable throwable) {
            Throwable cause = throwable.getCause();
            throw cause == null ? throwable : cause;
        } finally {
            if (writer != null) {
                long endTime = System.nanoTime();
                writer.write("""" + (endTime - startTime));
                writer.close();
            }
        }
    }
}";

    protected static string SandboxSecurityManagerCode
        => @"
class _$SandboxSecurityManager extends SecurityManager {
    private static final String JAVA_HOME_DIR = System.getProperty(""java.home"");
    private static final String USER_DIR = System.getProperty(""user.dir"");
    private static final String EXECUTING_FILE_PATH = _$SandboxSecurityManager.class.getProtectionDomain().getCodeSource().getLocation().getPath();

    @Override
    public void checkPermission(Permission permission) {
        if (permission instanceof PropertyPermission) {
            // Allow reading system properties
            return;
        }

        if (permission instanceof FilePermission) {
            FilePermission filePermission = (FilePermission) permission;
            String fileName = filePermission.getName();
            String filePath = new File(fileName).getPath();

            if (filePermission.getActions().equals(""read"") &&
                    (filePath.startsWith(JAVA_HOME_DIR) ||
                        filePath.startsWith(USER_DIR) ||
                        filePath.startsWith(new File(EXECUTING_FILE_PATH).getPath()))) {
                    // Allow reading Java system directories and user directories
                    return;
                }
            }

        if (permission instanceof NetPermission) {
            if (permission.getName().equals(""specifyStreamHandler"")) {
                // Allow specifyStreamHandler
                return;
            }
        }

        if (permission instanceof ReflectPermission) {
            if (permission.getName().equals(""suppressAccessChecks"")) {
                // Allow suppressAccessChecks
                return;
            }
        }

        if (permission instanceof RuntimePermission) {
            if (permission.getName().equals(""createClassLoader"") ||
                    permission.getName().equals(""getClassLoader"") ||
                    permission.getName().equals(""accessSystemModules"") ||
                    permission.getName().startsWith(""accessClassInPackage.sun."") ||
                    permission.getName().equals(""accessDeclaredMembers"") ||
                    permission.getName().equals(""accessClassInPackage.jdk.internal.reflect"") ||
                    permission.getName().equals(""getProtectionDomain"") ||
                    permission.getName().equals(""localeServiceProvider"")) {
                // Allow createClassLoader, getClassLoader, accessClassInPackage.sun,
                // getProtectionDomain, accessDeclaredMembers and localeServiceProvider
                return;
            }
        }

        throw new SecurityException(""Not allowed: "" + permission.getClass().getName());
    }

    @Override
    public void checkAccess(Thread thread) {
        throw new UnsupportedOperationException();
    }
}";

    protected string SandboxExecutorSourceFilePath
        => $"{Path.Combine(this.WorkingDirectory, SandboxExecutorClassName)}{Constants.javaSourceFileExtension}";

    protected virtual string ClassPathArgument
        => $@" -cp ""{this.Settings.JavaLibrariesPath}*{ClassPathArgumentSeparator}{this.WorkingDirectory}"" ";

    protected static void UpdateExecutionTime(
        string timeMeasurementFilePath,
        ProcessExecutionResult processExecutionResult,
        int timeLimit,
        int updateTimeOffset)
    {
        if (!File.Exists(timeMeasurementFilePath))
        {
            return;
        }

        var timeMeasurementFileContent = File.ReadAllText(timeMeasurementFilePath);
        if (long.TryParse(timeMeasurementFileContent, out var timeInNanoseconds))
        {
            var totalTimeUsed = TimeSpan.FromMilliseconds(timeInNanoseconds / NanosecondsInOneMillisecond);
            var timeOffset = TimeSpan.FromMilliseconds(Math.Abs(updateTimeOffset));
            var timeToSubtract = timeOffset < totalTimeUsed ? timeOffset : TimeSpan.Zero;

            processExecutionResult.TimeWorked = totalTimeUsed - timeToSubtract;

            if (processExecutionResult.Type == ProcessExecutionResultType.TimeLimit &&
                processExecutionResult.TimeWorked.TotalMilliseconds <= timeLimit)
            {
                // The time from the time measurement file is under the time limit
                processExecutionResult.Type = ProcessExecutionResultType.Success;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.Success &&
                     processExecutionResult.TimeWorked.TotalMilliseconds > timeLimit)
            {
                processExecutionResult.Type = ProcessExecutionResultType.TimeLimit;
            }
        }

        File.Delete(timeMeasurementFilePath);
    }

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        var compileResult = this.SetupAndCompile(executionContext, result);

        if (!compileResult.IsCompiledSuccessfully)
        {
            return result;
        }

        var executor = this.CreateRestrictedExecutor();

        var checker = executionContext.Input.GetChecker();

        foreach (var test in executionContext.Input.Tests)
        {
            var processExecutionResult = await this.Execute(
                executor,
                executionContext,
                compileResult.OutputFile,
                test.Input);

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
        var compileResult = this.SetupAndCompile(executionContext, result);

        if (!compileResult.IsCompiledSuccessfully)
        {
            return result;
        }

            var executor = this.CreateRestrictedExecutor();

        var processExecutionResult = await this.Execute(
            executor,
            executionContext,
            compileResult.OutputFile,
            executionContext.Input.Input);

        result.Results.Add(GetOutputResult(processExecutionResult));

        return result;
    }

    protected virtual string CreateSubmissionFile<TInput>(IExecutionContext<TInput> executionContext)
        => JavaCodePreprocessorHelper.CreateSubmissionFile(
            executionContext.Code,
            this.WorkingDirectory);

    protected virtual CompileResult DoCompile<TInput>(
        IExecutionContext<TInput> executionContext,
        string submissionFilePath)
    {
        // Compile all source files - sandbox executor and submission file
        var compilerResult = this.CompileSourceFiles(
            executionContext.CompilerType,
            this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type),
            executionContext.AdditionalCompilerArguments,
            [this.SandboxExecutorSourceFilePath, submissionFilePath]);

        return compilerResult;
    }

    private async Task<ProcessExecutionResult> Execute<TInput>(
        IExecutor executor,
        IExecutionContext<TInput> executionContext,
        string filePath,
        string? inputData = null)
    {
        var classToExecute = filePath
            .Substring(
                this.WorkingDirectory.Length + 1,
                filePath.Length - this.WorkingDirectory.Length - JavaCompiledFileExtension.Length - 1)
            .Replace('\\', '.')
            .Replace("/", ".");

        var timeMeasurementFilePath = Path.Combine(this.WorkingDirectory, TimeMeasurementFileName);

        var executionArguments = new[]
        {
            this.ClassPathArgument,
            SandboxExecutorClassName,
            classToExecute,
            $"\"{timeMeasurementFilePath}\"",
        };

        var processExecutionResult = await executor.Execute(
                this.Settings.JavaExecutablePath,
                executionContext.TimeLimit * 2, // Java virtual machine takes more time to start up
                executionContext.MemoryLimit,
                inputData,
                executionArguments,
                useProcessTime: false,
                useSystemEncoding: true);

        UpdateExecutionTime(
            timeMeasurementFilePath,
            processExecutionResult,
            executionContext.TimeLimit,
            this.Settings.BaseUpdateTimeOffset);

        return processExecutionResult;
    }

    private CompileResult SetupAndCompile<TInput, TResult>(
        IExecutionContext<TInput> executionContext,
        IExecutionResult<TResult> result)
        where TResult : ISingleCodeRunResult, new()
    {
        // Copy the sandbox executor source code to a file in the working directory
        File.WriteAllText(this.SandboxExecutorSourceFilePath, SandboxExecutorCode);

        // Create a temp file with the submission code
        string submissionFilePath;
        try
        {
            submissionFilePath = this.CreateSubmissionFile(executionContext);
        }
        catch (SolutionException exception)
        {
            result.IsCompiledSuccessfully = false;
            result.CompilerComment = exception.Message;

            return new CompileResult(false, exception.Message);
        }

        var compilerResult = this.DoCompile(executionContext, submissionFilePath);

        // Assign compiled result info to the execution result
        result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
        result.CompilerComment = compilerResult.CompilerComment;

        return compilerResult;
    }

    private CompileResult CompileSourceFiles(
        CompilerType compilerType,
        string compilerPath,
        string compilerArguments,
        IEnumerable<string> sourceFilesToCompile)
    {
        var compilerResult = new CompileResult(false, null);
        var compilerCommentBuilder = new StringBuilder();

        foreach (var sourceFile in sourceFilesToCompile)
        {
            compilerResult = this.Compile(compilerType, compilerPath, compilerArguments, sourceFile);

            compilerCommentBuilder.AppendLine(compilerResult.CompilerComment);

            if (!compilerResult.IsCompiledSuccessfully)
            {
                break; // The compilation of other files is not necessary
            }
        }

        var compilerComment = compilerCommentBuilder.ToString().Trim();
        compilerResult.CompilerComment = compilerComment.Length > 0 ? compilerComment : null;

        return compilerResult;
    }
}

public record JavaPreprocessCompileExecuteAndCheckExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string JavaExecutablePath,
    string JavaLibrariesPath,
    int BaseUpdateTimeOffset)
    : BaseCompiledCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
