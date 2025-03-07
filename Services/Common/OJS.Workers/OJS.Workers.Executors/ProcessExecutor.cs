namespace OJS.Workers.Executors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using OJS.Workers.Common;
    using static OJS.Workers.Common.Constants;

    public abstract class ProcessExecutor : IExecutor
    {
#pragma warning disable SA1401
        protected readonly ITasksService TasksService;
#pragma warning restore SA1401

        private const int TimeIntervalBetweenTwoResourceConsumptionRequests = 10;

        private readonly int baseTimeUsed;
        private readonly int baseMemoryUsed;
        private int timeLimit;
        private int memoryLimit;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessExecutor"/> class. with base time and memory used.
        /// </summary>
        /// <param name="baseTimeUsed">The base time in milliseconds added to the time limit when executing.</param>
        /// <param name="baseMemoryUsed">The base memory in bytes added to the memory limit when executing.</param>
        /// <param name="tasksService">Service for running tasks in the background.</param>
        /// <param name="environmentVariables">Environment variables to be passed to the process.</param>
        protected ProcessExecutor(
            int baseTimeUsed,
            int baseMemoryUsed,
            ITasksService tasksService,
            IDictionary<string, string>? environmentVariables = null)
        {
            this.baseTimeUsed = baseTimeUsed;
            this.baseMemoryUsed = baseMemoryUsed;
            this.TasksService = tasksService;
            this.EnvironmentVariables = environmentVariables ?? new Dictionary<string, string>();
        }

        public async Task<ProcessExecutionResult> Execute(
            string fileName,
            int processTimeLimit,
            int processMemoryLimit,
            string? inputData = null,
            IEnumerable<string>? executionArguments = null,
            string? workingDirectory = null,
            bool useProcessTime = false,
            bool useSystemEncoding = false,
            bool dependOnExitCodeForRunTimeError = false,
            double timeoutMultiplier = 1.5)
        {
            this.timeLimit = processTimeLimit;
            this.memoryLimit = processMemoryLimit;

            workingDirectory ??= new FileInfo(fileName).DirectoryName;

            this.BeforeExecute();

            var processExecutionResult = await this.InternalExecute(
                fileName,
                this.timeLimit,
                inputData,
                executionArguments,
                workingDirectory,
                useSystemEncoding,
                timeoutMultiplier);

            this.AfterExecute(
                useProcessTime,
                dependOnExitCodeForRunTimeError,
                processExecutionResult);

            return processExecutionResult;
        }

        public IDictionary<string, string> EnvironmentVariables { get; }

        protected abstract Task<ProcessExecutionResult> InternalExecute(
            string fileName,
            int timeLimit,
            string? inputData,
            IEnumerable<string>? executionArguments,
            string? workingDirectory,
            bool useSystemEncoding,
            double timeoutMultiplier);

        protected TaskInfo StartResourceConsumptionSamplingThread(
            System.Diagnostics.Process process,
            ProcessExecutionResult result)
            => this.TasksService.RunWithInterval(
                TimeIntervalBetweenTwoResourceConsumptionRequests,
                () =>
                {
                    try
                    {
                        if (process.HasExited)
                        {
                            return;
                        }

                        result.PrivilegedProcessorTime = process.PrivilegedProcessorTime;
                        result.UserProcessorTime = process.UserProcessorTime;
                        result.MemoryUsed = Math.Max(result.MemoryUsed, process.PeakWorkingSet64);
                    }
                    catch (Exception e) when (e is InvalidOperationException or Win32Exception)
                    {
                        // Process has exited or is not running anymore
                        // Do nothing, as result will be calculated from the process exit time
                    }
                });

        private void BeforeExecute()
        {
            this.timeLimit += this.baseTimeUsed;

            if (this.timeLimit < MinimumTimeLimitInMilliseconds)
            {
                this.timeLimit = MinimumTimeLimitInMilliseconds;
            }

            if (this.timeLimit > MaxTimeLimitInMilliseconds)
            {
                this.timeLimit = MaxTimeLimitInMilliseconds;
            }

            this.memoryLimit += this.baseMemoryUsed;

            if (this.memoryLimit < MinimumMemoryLimitInBytes)
            {
                this.memoryLimit = MinimumMemoryLimitInBytes;
            }

            if (this.memoryLimit > MaxMemoryLimitInBytes)
            {
                this.memoryLimit = MaxMemoryLimitInBytes;
            }
        }

        private void AfterExecute(
            bool useProcessTime,
            bool dependOnExitCodeForRunTimeError,
            ProcessExecutionResult result)
        {
            var timeUsed = useProcessTime
                ? result.TimeWorked.TotalMilliseconds
                : result.TotalProcessorTime.TotalMilliseconds;

            if (timeUsed > this.timeLimit)
            {
                result.Type = ProcessExecutionResultType.TimeLimit;
            }

            if (result.MemoryUsed > this.memoryLimit)
            {
                result.Type = ProcessExecutionResultType.MemoryLimit;
            }

            // If there is any error output produced, we consider the process run as failed.
            // If there is any standard output, but no error output, we consider the process run as successful,
            // and we ignore the exit code, as the output might be valid, but either way it will be evaluated properly.
            var isRuntimeError =
                !string.IsNullOrEmpty(result.ErrorOutput) ||
                (dependOnExitCodeForRunTimeError &&
                    result.ExitCode != 0 &&
                    string.IsNullOrEmpty(result.ReceivedOutput));

            if (isRuntimeError)
            {
                result.Type = ProcessExecutionResultType.RunTimeError;

                if (string.IsNullOrEmpty(result.ErrorOutput))
                {
                    result.ErrorOutput = $"Runtime error has occured. Error code: {result.ExitCode}";
                }
            }

            result.ApplyTimeAndMemoryOffset(this.baseTimeUsed, this.baseMemoryUsed);
        }
    }
}
