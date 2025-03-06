#nullable disable
namespace OJS.Workers.ExecutionStrategies.Java
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Exceptions;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    using static OJS.Workers.Common.Constants;
    using static OJS.Workers.ExecutionStrategies.Helpers.JavaStrategiesHelper;

    public class JavaProjectTestsExecutionStrategy<TSettings> : JavaUnitTestsExecutionStrategy<TSettings>
        where TSettings : JavaProjectTestsExecutionStrategySettings
    {
        private const string InvalidNumberOfTestCasesPrefix = "Invalid number of test cases";
        private const string TestRanPrefix = "Test Ran. Successful:";
        private readonly string testResultRegexPattern = $@"(?:{TestRanPrefix})\s*(true|false)";

        public JavaProjectTestsExecutionStrategy(
            IOjsSubmission submission,
            IProcessExecutorFactory processExecutorFactory,
            ICompilerFactory compilerFactory,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
            : base(submission, processExecutorFactory, compilerFactory, settingsProvider, logger)
            => this.UserClassNames = new List<string>();

        protected List<string> UserClassNames { get; }

        protected override string ClassPathArgument
            => $@" -classpath ""{this.WorkingDirectory}{ClassPathArgumentSeparator}{this.Settings.JavaLibrariesPath}*""";

        protected override string JUnitTestRunnerCode
            => $@"
import org.junit.runner.JUnitCore;
import org.junit.runner.Result;
import org.junit.runner.notification.Failure;

import org.junit.Test;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.PrintStream;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import java.lang.reflect.Method;

public class _$TestRunner {{
    public static void main(String[] args) {{
        for (String arg: args) {{
            try {{
                Class currentClass = Class.forName(arg);
                Classes.allClasses.put(currentClass.getSimpleName(),currentClass);
            }} catch (ClassNotFoundException e) {{}}
        }}

        Class[] testClasses = new Class[]{{{string.Join(", ", this.TestNames.Select(x => x + ".class"))}}};

        InputStream originalIn = System.in;
        PrintStream originalOut = System.out;

        InputStream in = new ByteArrayInputStream(new byte[0]);
        System.setIn(in);

        ByteArrayOutputStream out = new ByteArrayOutputStream();
        System.setOut(new PrintStream(out));

        List<Result> results = new ArrayList<>();
        for (Class testClass: testClasses) {{
            results.add(JUnitCore.runClasses(testClass));
        }}

        System.setIn(originalIn);
        System.setOut(originalOut);

        for (int i = 0; i < results.size(); i++){{
            var testMethodCount = 0;
            for (Method method : testClasses[i].getDeclaredMethods()) {{
                if (method.isAnnotationPresent(Test.class)) {{
                   testMethodCount++;
                }}
            }}

            if (testMethodCount > 1) {{
                System.out.printf(""{InvalidNumberOfTestCasesPrefix} "" + ""(%d) for %s. There should be a single test case per test. Please contact an administrator.%n"", testMethodCount, testClasses[i].getSimpleName());
                continue;
            }}

            Result result = results.get(i);

            System.out.println(testClasses[i].getSimpleName() + "" {TestRanPrefix} "" + result.wasSuccessful());

            for (Failure failure : result.getFailures()) {{
                String failureClass = failure.getDescription().getTestClass().getSimpleName();
                String failureException = failure.getException().toString().replaceAll(""\r"", ""\\\\r"").replaceAll(""\n"",""\\\\n"");
                System.out.printf(""%s %s%s"", failureClass, failureException, System.lineSeparator());
            }}
        }}
    }}
}}

class Classes{{
    public static Map<String, Class> allClasses = new HashMap<>();
}}";

        protected override string JUnit5TestRunnerCode
            => $@"
import org.junit.platform.launcher.Launcher;
import org.junit.platform.launcher.LauncherDiscoveryRequest;
import org.junit.platform.launcher.core.LauncherDiscoveryRequestBuilder;
import org.junit.platform.launcher.core.LauncherFactory;
import org.junit.platform.launcher.listeners.SummaryGeneratingListener;
import org.junit.platform.engine.discovery.DiscoverySelectors;

import org.junit.jupiter.api.Test;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.PrintStream;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import java.lang.reflect.Method;

class Classes {{
    public static Map<String, Class> allClasses = new HashMap<>();
}}

public class _$TestRunner {{
    public static void main(String[] args) {{
        Map<String, Class<?>> allClasses = new HashMap<>();
        for (String arg: args) {{
            try {{
                Class<?> currentClass = Class.forName(arg);
                Classes.allClasses.put(currentClass.getSimpleName(), currentClass);
            }} catch (ClassNotFoundException e) {{
                // Handle the exception, if needed
            }}
        }}

        Class[] testClasses = {{
                {string.Join(", ", this.TestNames.Select(x => x + ".class"))}
        }};

        InputStream originalIn = System.in;
        PrintStream originalOut = System.out;

        InputStream in = new ByteArrayInputStream(new byte[0]);
        System.setIn(in);

        ByteArrayOutputStream out = new ByteArrayOutputStream();
        System.setOut(new PrintStream(out));

        List<SummaryGeneratingListener> listeners = new ArrayList<>();
        Launcher launcher = LauncherFactory.create();

        for (Class<?> testClass : testClasses) {{
            LauncherDiscoveryRequest request = LauncherDiscoveryRequestBuilder.request()
                    .selectors(DiscoverySelectors.selectClass(testClass))
                    .build();

            SummaryGeneratingListener listener = new SummaryGeneratingListener();
            listeners.add(listener);

            launcher.execute(request, listener);
        }}

        System.setIn(originalIn);
        System.setOut(originalOut);

        for (int i = 0; i < listeners.size(); i++) {{
            var testMethodCount = 0;
            for (Method method : testClasses[i].getDeclaredMethods()) {{
                if (method.isAnnotationPresent(Test.class)) {{
                   testMethodCount++;
                }}
            }}

            if (testMethodCount > 1) {{
                System.out.printf(""{InvalidNumberOfTestCasesPrefix} "" + ""(%d) for %s. There should be a single test case per test. Please contact an administrator.%n"", testMethodCount, testClasses[i].getSimpleName());
                continue;
            }}

            SummaryGeneratingListener listener = listeners.get(i);
            var summary = listener.getSummary();

            var hasFailures = summary.getTotalFailureCount() > 0;
            System.out.println(testClasses[i].getSimpleName() + "" {TestRanPrefix} "" + !hasFailures);

            summary.getFailures().forEach(failure -> {{
                String failureClass = failure.getTestIdentifier().getDisplayName();
                String failureException = failure.getException().toString().replaceAll(""\r"", ""\\r"").replaceAll(""\n"",""\\n"");
                System.out.printf(""%s %s%s"", failureClass, failureException, System.lineSeparator());
            }});
        }}
    }}
}}";

        protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            // Create a temp file with the submission code
            string submissionFilePath;
            try
            {
                submissionFilePath = this.CreateSubmissionFile(executionContext);
            }
            catch (ArgumentException exception)
            {
                result.IsCompiledSuccessfully = false;
                result.CompilerComment = exception.Message;

                return result;
            }

            var combinedArguments = executionContext.AdditionalCompilerArguments + this.ClassPathArgument;

            var executor = this.CreateRestrictedExecutor();

            if (!string.IsNullOrWhiteSpace(executionContext.Input.TaskSkeletonAsString))
            {
                FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);

                var className = JavaCodePreprocessorHelper
                    .GetPublicClassName(executionContext.Input.TaskSkeletonAsString);

                var filePath = FileHelpers.BuildPath(this.WorkingDirectory, $"{className}{javaSourceFileExtension}");

                File.WriteAllText(filePath, executionContext.Input.TaskSkeletonAsString);
                FileHelpers.AddFilesToZipArchive(submissionFilePath, string.Empty, filePath);

                var preprocessCompileResult = this.Compile(
                    executionContext.CompilerType,
                    this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type),
                    combinedArguments,
                    submissionFilePath);

                result.IsCompiledSuccessfully = preprocessCompileResult.IsCompiledSuccessfully;
                result.CompilerComment = preprocessCompileResult.CompilerComment;
                if (!result.IsCompiledSuccessfully)
                {
                    return result;
                }

                var preprocessExecutor = this.CreateRestrictedExecutor();

                var preprocessArguments = new List<string>();
                preprocessArguments.Add(this.ClassPathArgument);
                preprocessArguments.Add(AdditionalExecutionArguments);
                preprocessArguments.Add(className);
                preprocessArguments.Add(this.WorkingDirectory);
                preprocessArguments.AddRange(this.UserClassNames);

                var preprocessExecutionResult = await preprocessExecutor.Execute(
                    this.Settings.JavaExecutablePath,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    executionArguments: preprocessArguments,
                    workingDirectory: this.WorkingDirectory);

                ValidateJvmInitialization(preprocessExecutionResult.ReceivedOutput);

                var filesToAdd = preprocessExecutionResult
                    .ReceivedOutput
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var file in filesToAdd)
                {
                    var path = Path.GetDirectoryName(file);
                    FileHelpers.AddFilesToZipArchive(
                        submissionFilePath,
                        path,
                        FileHelpers.BuildPath(this.WorkingDirectory, file));
                }

                File.Delete(filePath);
            }

            var compilerResult = this.Compile(
                executionContext.CompilerType,
                this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type),
                combinedArguments,
                submissionFilePath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;
            result.CompilerComment = compilerResult.CompilerComment;
            if (!result.IsCompiledSuccessfully)
            {
                return result;
            }

            var arguments = new List<string>
            {
                this.ClassPathArgument,
                AdditionalExecutionArguments,
                JUnitRunnerClassName,
            };

            arguments.AddRange(this.UserClassNames);

            var processExecutionResult = await executor.Execute(
                this.Settings.JavaExecutablePath,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                executionArguments: arguments,
                workingDirectory: this.WorkingDirectory,
                useProcessTime: true);

            if (!string.IsNullOrWhiteSpace(processExecutionResult.ErrorOutput))
            {
                throw new InvalidProcessExecutionOutputException(processExecutionResult.ErrorOutput);
            }

            ValidateJvmInitialization(processExecutionResult.ReceivedOutput);

            Dictionary<string, string> errorsByFiles = null;

            if (processExecutionResult.Type == ProcessExecutionResultType.Success)
            {
                errorsByFiles = this.GetTestErrors(processExecutionResult.ReceivedOutput);
            }

            var testIndex = 0;

            var checker = executionContext.Input.GetChecker();

            foreach (var test in executionContext.Input.Tests)
            {
                var message = TestPassedMessage;
                var testFile = this.TestNames[testIndex++];
                if (errorsByFiles?.ContainsKey(testFile) ?? false)
                {
                    message = errorsByFiles[testFile];
                }

                var testResult = CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    message);

                result.Results.Add(testResult);
            }

            return result;
        }

        protected override string PrepareSubmissionFile(IExecutionContext<TestsInputModel> context)
        {
            var submissionFilePath = FileHelpers.BuildPath(this.WorkingDirectory, SubmissionFileName);
            File.WriteAllBytes(submissionFilePath, context.FileContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
            this.ExtractUserClassNames(submissionFilePath);
            this.AddTestsToUserSubmission(context, submissionFilePath);
            this.AddTestRunnerTemplate(submissionFilePath);

            return submissionFilePath;
        }

        protected virtual void AddTestsToUserSubmission(
            IExecutionContext<TestsInputModel> context,
            string submissionZipFilePath)
        {
            var testNumber = 0;
            var filePaths = new string[context.Input.Tests.Count()];

            foreach (var test in context.Input.Tests)
            {
                var className = JavaCodePreprocessorHelper.GetPublicClassName(test.Input);
                var testFileName =
                    FileHelpers.BuildPath(this.WorkingDirectory, $"{className}{javaSourceFileExtension}");

                File.WriteAllText(testFileName, test.Input);
                filePaths[testNumber] = testFileName;
                this.TestNames.Add(className);
                testNumber++;
            }

            var duplicateTest = this.TestNames
                .GroupBy(t => t)
                .Select(g => new { TestName = g.Key, Count = g.Count() })
                .FirstOrDefault(g => g.Count > 1);

            if (duplicateTest is not null)
            {
                throw new InvalidOperationException($"There are multiple tests ({duplicateTest.Count}) with the same name: {duplicateTest.TestName}. Please contact an administrator.");
            }

            FileHelpers.AddFilesToZipArchive(submissionZipFilePath, string.Empty, filePaths);
            FileHelpers.DeleteFiles(filePaths);
        }

        protected virtual void AddTestRunnerTemplate(string submissionFilePath)
        {
            // It is important to call the JUintTestRunnerCodeTemplate after the TestClasses have been filled
            // otherwise no tests will be queued in the JUnitTestRunner, which would result in no tests failing.
            File.WriteAllText(
                    this.JUnitTestRunnerSourceFilePath,
                    this.Type.ToString().Contains("21") ? this.JUnit5TestRunnerCode : this.JUnitTestRunnerCode);
            FileHelpers.AddFilesToZipArchive(submissionFilePath, string.Empty, this.JUnitTestRunnerSourceFilePath);
            FileHelpers.DeleteFiles(this.JUnitTestRunnerSourceFilePath);
        }

        protected virtual void ExtractUserClassNames(string submissionFilePath)
            => this.UserClassNames.AddRange(
                FileHelpers.GetFilePathsFromZip(submissionFilePath)
                    .Where(x => !x.EndsWith("/") && x.EndsWith(javaSourceFileExtension))
                    .Select(x => x.Contains(".") ? x.Substring(0, x.LastIndexOf(".", StringComparison.Ordinal)) : x)
                    .Select(x => x.Replace("/", ".")));

        private static string ReadAndValidateLine(StringReader output)
        {
            var line = output.ReadLine();

            if (line == null)
            {
                throw new InvalidOperationException("Unexpected end of output. Please verify that all test cases are executed correctly and produce the expected results.");
            }

            if (line.StartsWith(InvalidNumberOfTestCasesPrefix))
            {
                throw new InvalidOperationException(line);
            }

            return line;
        }

        private Dictionary<string, string> GetTestErrors(string receivedOutput)
        {
            if (string.IsNullOrWhiteSpace(receivedOutput))
            {
                throw new InvalidProcessExecutionOutputException();
            }

            var errorsByFiles = new Dictionary<string, string>();
            var output = new StringReader(receivedOutput);
            var testResultRegex = new Regex(this.testResultRegexPattern);

            foreach (var testName in this.TestNames)
            {
                var line = ReadAndValidateLine(output);

                var firstSpaceIndex = line.IndexOf(" ", StringComparison.Ordinal);
                var fileName = line.Substring(0, firstSpaceIndex);

                // Validating that test name is the same as the one from the output
                // ensuring the output is from the JUnit test runner
                if (testName != fileName)
                {
                    throw new InvalidProcessExecutionOutputException();
                }

                var isTestSuccessful = bool.Parse(testResultRegex.Match(line).Groups[1].Value);

                if (!isTestSuccessful)
                {
                    var errorLine = ReadAndValidateLine(output);

                    var errorMessage = errorLine.Substring(firstSpaceIndex);

                    errorsByFiles.Add(fileName, errorMessage);
                }
            }

            return errorsByFiles;
        }
    }

    public record JavaProjectTestsExecutionStrategySettings(
        int BaseTimeUsed,
        int BaseMemoryUsed,
        string JavaExecutablePath,
        string JavaLibrariesPath,
        int BaseUpdateTimeOffset) : JavaUnitTestsExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed,
        JavaExecutablePath, JavaLibrariesPath, BaseUpdateTimeOffset);
}
