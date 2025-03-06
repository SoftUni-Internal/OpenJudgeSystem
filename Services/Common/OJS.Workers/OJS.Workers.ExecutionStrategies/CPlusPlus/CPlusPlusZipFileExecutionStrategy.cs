namespace OJS.Workers.ExecutionStrategies.CPlusPlus
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies.Extensions;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class CPlusPlusZipFileExecutionStrategy<TSettings> : BaseCompiledCodeExecutionStrategy<TSettings>
        where TSettings : CPlusPlusZipFileExecutionStrategySettings
    {
        private const string FileNameAndExtensionPattern = @"//((\w+)\.(cpp|h))//";

        public CPlusPlusZipFileExecutionStrategy(
            IOjsSubmission submission,
            IProcessExecutorFactory processExecutorFactory,
            ICompilerFactory compilerFactory,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
            : base(submission, processExecutorFactory, compilerFactory, settingsProvider, logger)
        {
        }

        protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            executionContext.SanitizeContent();

            var submissionDestination = Path.Combine(this.WorkingDirectory, ZippedSubmissionName);
            File.WriteAllBytes(submissionDestination, executionContext.FileContent);
            FileHelpers.RemoveFilesFromZip(submissionDestination, RemoveMacFolderPattern);

            if (!string.IsNullOrEmpty(executionContext.Input.TaskSkeletonAsString))
            {
                var pathsOfHeadersAndCppFiles = this.ExtractTaskSkeleton(executionContext.Input.TaskSkeletonAsString);
                FileHelpers.AddFilesToZipArchive(submissionDestination, string.Empty, pathsOfHeadersAndCppFiles.ToArray());
            }

            var compilationResult = this.Compile(
                executionContext.CompilerType,
                this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type),
                executionContext.AdditionalCompilerArguments,
                submissionDestination);

            result.IsCompiledSuccessfully = compilationResult.IsCompiledSuccessfully;
            result.CompilerComment = compilationResult.CompilerComment;

            if (!compilationResult.IsCompiledSuccessfully)
            {
                return result;
            }

            var executor = this.CreateRestrictedExecutor();

            var checker = executionContext.Input.GetChecker();

            foreach (var test in executionContext.Input.Tests)
            {
                var processExecutionResult = await executor.Execute(
                    compilationResult.OutputFile,
                    executionContext.TimeLimit,
                    executionContext.MemoryLimit,
                    test.Input,
                    useProcessTime: false,
                    useSystemEncoding: false,
                    dependOnExitCodeForRunTimeError: true);

                var testResults = CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                result.Results.Add(testResults);
            }

            return result;
        }

        private IEnumerable<string> ExtractTaskSkeleton(string executionContextTaskSkeletonAsString)
        {
            var headersAndCppFiles = executionContextTaskSkeletonAsString.Split(
                new[] { Constants.classDelimiterUnix, Constants.classDelimiterWin },
                StringSplitOptions.RemoveEmptyEntries);

            var pathsToHeadersAndCppFiles = new List<string>();
            var fileNameAndExtensionMatcher = new Regex(FileNameAndExtensionPattern);

            foreach (var headersOrCppFile in headersAndCppFiles)
            {
                var match = fileNameAndExtensionMatcher.Match(headersOrCppFile);
                if (match.Success)
                {
                    File.WriteAllText(Path.Combine(this.WorkingDirectory, match.Groups[1].ToString()), headersOrCppFile);
                    pathsToHeadersAndCppFiles.Add(Path.Combine(this.WorkingDirectory, match.Groups[1].ToString()));
                }
            }

            return pathsToHeadersAndCppFiles;
        }
    }

    public record CPlusPlusZipFileExecutionStrategySettings(
        int BaseTimeUsed,
        int BaseMemoryUsed)
        : BaseCompiledCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
}
