﻿namespace OJS.Workers.ExecutionStrategies
{
    using System;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Extensions;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class BaseCodeExecutionStrategy<TSettings> : BaseExecutionStrategy<TSettings>
        where TSettings : BaseCodeExecutionStrategySettings
    {
        protected const string RemoveMacFolderPattern = "__MACOSX/*";

        protected const string ZippedSubmissionName = "Submission.zip";

#pragma warning disable SA1401
        protected readonly IProcessExecutorFactory ProcessExecutorFactory;
#pragma warning restore SA1401

        protected BaseCodeExecutionStrategy(
            ExecutionStrategyType type,
            IProcessExecutorFactory processExecutorFactory,
            IExecutionStrategySettingsProvider settingsProvider)
            : base(type, settingsProvider)
            => this.ProcessExecutorFactory = processExecutorFactory;

        protected static void SaveZipSubmission(byte[] submissionContent, string directory)
        {
            var submissionFilePath = FileHelpers.BuildPath(directory, ZippedSubmissionName);
            FileHelpers.WriteAllBytes(submissionFilePath, submissionContent);
            FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
            FileHelpers.UnzipFile(submissionFilePath, directory);
            FileHelpers.DeleteFile(submissionFilePath);
        }

        protected static TestResult CheckAndGetTestResult(
            TestContext test,
            ProcessExecutionResult processExecutionResult,
            IChecker checker,
            string receivedOutput)
        {
            var testResult = new TestResult
            {
                Id = test.Id,
                TimeUsed = (int)processExecutionResult.TimeWorked.TotalMilliseconds,
                MemoryUsed = (int)processExecutionResult.MemoryUsed,
                IsTrialTest = test.IsTrialTest,
            };

            if (processExecutionResult.Type == ProcessExecutionResultType.RunTimeError)
            {
                testResult.ResultType = TestRunResultType.RunTimeError;
                testResult.ExecutionComment = processExecutionResult.ErrorOutput.MaxLength(2048); // Trimming long error texts
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.TimeLimit)
            {
                testResult.ResultType = TestRunResultType.TimeLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.MemoryLimit)
            {
                testResult.ResultType = TestRunResultType.MemoryLimit;
            }
            else if (processExecutionResult.Type == ProcessExecutionResultType.Success)
            {
                var checkerResult = checker.Check(test.Input, receivedOutput, test.Output, test.IsTrialTest);

                testResult.ResultType = checkerResult.IsCorrect
                    ? TestRunResultType.CorrectAnswer
                    : TestRunResultType.WrongAnswer;

                // TODO: Do something with checkerResult.ResultType
                testResult.CheckerDetails = checkerResult.CheckerDetails;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(processExecutionResult),
                    "Invalid ProcessExecutionResultType value.");
            }

            testResult.Input = test.Input;

            return testResult;
        }

        protected static OutputResult GetOutputResult(ProcessExecutionResult processExecutionResult)
            => new OutputResult
            {
                TimeUsed = (int)processExecutionResult.TimeWorked.TotalMilliseconds,
                MemoryUsed = (int)processExecutionResult.MemoryUsed,
                ResultType = processExecutionResult.Type,
                Output = string.IsNullOrWhiteSpace(processExecutionResult.ErrorOutput)
                    ? processExecutionResult.ReceivedOutput
                    : processExecutionResult.ErrorOutput,
            };

        protected IExecutor CreateExecutor(ProcessExecutorType processExecutorType = ProcessExecutorType.Default)
            => this.ProcessExecutorFactory
                .CreateProcessExecutor(this.Settings.BaseTimeUsed, this.Settings.BaseMemoryUsed, processExecutorType);

        protected virtual string SaveCodeToTempFile<TINput>(IExecutionContext<TINput> executionContext)
            => string.IsNullOrEmpty(executionContext.AllowedFileExtensions)
                ? FileHelpers.SaveStringToTempFile(this.WorkingDirectory, executionContext.Code)
                : FileHelpers.SaveByteArrayToTempFile(this.WorkingDirectory, executionContext.FileContent);
    }

#pragma warning disable SA1402
    public class BaseCodeExecutionStrategySettings : BaseExecutionStrategySettings
#pragma warning restore SA1402
    {
        public int BaseTimeUsed { get; set; }
        public int BaseMemoryUsed { get; set; }
    }
}
