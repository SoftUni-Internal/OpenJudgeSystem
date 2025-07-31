namespace OJS.Workers.Executors
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    using Microsoft.Extensions.Logging;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using System.Globalization;

#pragma warning disable CA1848
    // TODO: Implement memory constraints
    public class StandardProcessExecutor(
        int baseTimeUsed,
        int baseMemoryUsed,
        ITasksService tasksService,
        ILogger<StandardProcessExecutor> logger,
        ILogger strategyLogger,
        bool runAsRestrictedUser = false,
        IDictionary<string, string>? environmentVariables = null)
        : ProcessExecutor(baseTimeUsed, baseMemoryUsed, tasksService, environmentVariables)
    {
        private const int TimeBeforeClosingOutputStreams = 100;

        protected override async Task<ProcessExecutionResult> InternalExecute(
            string fileName,
            int timeLimit,
            string? inputData,
            IEnumerable<string>? executionArguments,
            string? workingDirectory,
            bool useSystemEncoding,
            double timeoutMultiplier,
            CancellationToken cancellationToken)
        {
            var result = new ProcessExecutionResult { Type = ProcessExecutionResultType.Success };

            var processStartInfo = new ProcessStartInfo(fileName)
            {
                Arguments = executionArguments == null ? string.Empty : string.Join(" ", executionArguments),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                ErrorDialog = false,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WorkingDirectory = workingDirectory,
                StandardOutputEncoding = useSystemEncoding ? Encoding.Default : new UTF8Encoding(false),
            };

            if (runAsRestrictedUser)
            {
                processStartInfo.UserName = Constants.RestrictedUserName;
            }

            // If we don't clear the environment variables,
            // the process will inherit the environment variables of the current process, which is a security risk
            // If any env variables are needed, they should be set explicitly
            processStartInfo.EnvironmentVariables.Clear();

            // Add custom environment variables
            foreach (var environmentVariable in this.EnvironmentVariables)
            {
                processStartInfo.EnvironmentVariables.Add(environmentVariable.Key, environmentVariable.Value);
            }

            strategyLogger.LogInformation("Starting process: {FileName} as user: {UserName} in directory: {WorkingDirectory}", fileName, processStartInfo.UserName, workingDirectory);
            strategyLogger.LogInformation("With time limit: {TimeLimit} ms", timeLimit);
            strategyLogger.LogInformation("With arguments: {Arguments}", processStartInfo.Arguments);

            var envVariablesString = new StringBuilder();
            foreach (var key in processStartInfo.EnvironmentVariables.Keys)
            {
                envVariablesString.AppendLine(CultureInfo.InvariantCulture, $"{key}={processStartInfo.EnvironmentVariables[key.ToString() ?? string.Empty]}");
            }

            if (envVariablesString.Length > 0)
            {
                strategyLogger.LogInformation("With environment variables: {EnvironmentVariables}", envVariablesString.ToString());
            }

            using var process = Process.Start(processStartInfo)
                ?? throw new Exception($"Could not start process: {fileName}!");

            var processStartTime = process.StartTime;

            if (!OsPlatformHelpers.IsUnix())
            {
                process.PriorityClass = ProcessPriorityClass.High;
            }

            var resourceConsumptionSamplingThread = this.StartResourceConsumptionSamplingThread(process, result);

            var processTimeoutMs = (int)(timeLimit * timeoutMultiplier);
            using var timeoutCts = new CancellationTokenSource(processTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            var safetyCts = CreateSafetyTimeout(process, processTimeoutMs, linkedCts.Token);

            // Start reading standard output and error before writing to standard input to avoid deadlocks
            // and ensure fast reading of the output in case of a fast execution
            var processOutputTask = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
            var errorOutputTask = process.StandardError.ReadToEndAsync(CancellationToken.None);

            if (inputData is not null)
            {
                strategyLogger.LogInformation("Writing the following input data to process: {NewLine}{InputData}", Environment.NewLine, inputData);

                await this.WriteInputToProcess(process, inputData, linkedCts.Token);
            }

            // Wait the process to complete. Kill it after (timeLimit * 1.5) milliseconds if not completed.
            // We are waiting the process for more than defined time and after this we compare the process time with the real time limit.
            bool exited;

            try
            {
                await process.WaitForExitAsync(linkedCts.Token);
                exited = true;
            }
            catch (OperationCanceledException)
            {
                exited = false;
            }

            if (!exited)
            {
                strategyLogger.LogWarning("Process exceeded time limit; terminating …");
                await this.KillProcessTreeAsync(process);
                result.ProcessWasKilled = true;
                result.Type = ProcessExecutionResultType.TimeLimit;
            }

            strategyLogger.LogInformation("Process exited with code: {ExitCode}", process.ExitCode);

            try
            {
                await safetyCts.CancelAsync();
                safetyCts.Dispose();
            }
            catch
            {
                /* ignored */
            }

            try
            {
                this.TasksService.Stop(resourceConsumptionSamplingThread);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is not TaskCanceledException)
                {
                    logger.LogAggregatedException(ex);
                }
            }

            // Read the standard output and error and set the result
            result.ErrorOutput = await this.GetReceivedOutput(errorOutputTask, "error output");
            result.ReceivedOutput = await this.GetReceivedOutput(processOutputTask, "standard output");

            Debug.Assert(process.HasExited, "Standard process didn't exit!");

            // Report exit code and total process working time
            result.ExitCode = process.ExitCode;
            result.TimeWorked = process.ExitTime - processStartTime;

            strategyLogger.LogInformation("Total time worked: {TimeWorked} ms", result.TimeWorked.TotalMilliseconds);
            strategyLogger.LogInformation("Total processor time: {TotalProcessorTime} ms", result.TotalProcessorTime.TotalMilliseconds);

            if (!string.IsNullOrEmpty(result.ErrorOutput))
            {
                strategyLogger.LogError("Error output: {NewLine}{ErrorOutput}", Environment.NewLine, result.ErrorOutput);
            }

            if (!string.IsNullOrEmpty(result.ReceivedOutput))
            {
                strategyLogger.LogInformation("Received output: {NewLine}{ReceivedOutput}", Environment.NewLine, result.ReceivedOutput);
            }

            strategyLogger.LogInformation("================ Finished process execution ================");

            return result;
        }

        private async Task WriteInputToProcess(Process process, string inputData, CancellationToken cancellationToken)
        {
            try
            {
                await process.StandardInput.WriteLineAsync(inputData);
                await process.StandardInput.FlushAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogErrorWritingToStandardInput(inputData, ex);
            }
            finally
            {
                // Close the standard input stream to signal the process that we have finished writing to it
                try
                {
                    // Check if the process has exited before closing the standard input, preventing broken pipe exceptions
                    if (!process.HasExited)
                    {
                        process.StandardInput.Close();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogErrorClosingStandardInput(ex);
                }
            }
        }

        private async Task<string> GetReceivedOutput(Task<string> outputTask, string outputName)
        {
            try
            {
                // Read the output with a timeout, ensuring that will not wait indefinitely
                var timeoutTask = Task.Delay(TimeBeforeClosingOutputStreams);
                var completedTask = await Task.WhenAny(outputTask, timeoutTask);
                if (completedTask == outputTask)
                {
                    // Only awaits if the task completed before the timeout
                    return await outputTask;
                }

                return $"{outputName} was too large and was not read.";
            }
            catch (Exception ex)
            {
                logger.LogErrorReadingProcessErrorOutput(ex);
                return $"Error while reading the {outputName} of the underlying process: {ex.Message}";
            }
        }

        /// <summary>
        /// Secondary watchdog that hard‑kills the entire process tree if the main path fails to do so.
        /// The timeout is 25% longer than the primary one but never exceeds the global safety cap.
        /// </summary>
        private static CancellationTokenSource CreateSafetyTimeout(Process process, int primaryTimeoutMs, CancellationToken externalToken)
        {
            // 25% extra time, but not more than 5 seconds
            var extraTimeout = Math.Min(primaryTimeoutMs / 4, 5000);

            // totalTimeout is never more than 2x the max time limit
            var totalTimeout = Math.Min(primaryTimeoutMs + extraTimeout, Constants.MaxTimeLimitInMilliseconds + extraTimeout);

            var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
            cts.CancelAfter(totalTimeout);

            _ = Task.Run(async () =>
            {
                try
                {
                    // Wait until either the timer fires *or* the linked token is
                    // cancelled (whichever comes first).
                    await Task.Delay(Timeout.Infinite, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Either the timer elapsed or an external cancellation fired –
                    // in both cases we fall through to the kill check below.
                }

                if (!process.HasExited)
                {
                    // The timer elapsed (not an external cancellation) – force kill.
                    try
                    {
                        process.Kill(entireProcessTree: true);
                    }
                    catch (Exception ex)
                    {
                        // Best‑effort: log but don’t throw from the background task.
                        Debug.WriteLine($"Watchdog kill failed: {ex.Message}");
                    }
                }

            }, CancellationToken.None);

            return cts;
        }

        private async Task KillProcessTreeAsync(Process process)
        {
            const int maxAttempts = 3;
            const int delayBetweenAttemptsMs = 1000;
            const int waitForExitMs = 2000;

            for (var attempt = 1; attempt <= maxAttempts && !process.HasExited; attempt++)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                    if (process.WaitForExit(waitForExitMs))
                    {
                        return;
                    }
                }
                catch when (attempt < maxAttempts)
                {
                    await Task.Delay(delayBetweenAttemptsMs);
                }
            }

            if (!process.HasExited)
            {
                strategyLogger.LogError("Unable to kill process tree with PID: {Pid}", process.Id);
            }
        }
    }
#pragma warning restore CA1848
}
