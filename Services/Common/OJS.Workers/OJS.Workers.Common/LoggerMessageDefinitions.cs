namespace OJS.Workers.Common;

using Microsoft.Extensions.Logging;

public static partial class LoggerMessageDefinitions
{
    [LoggerMessage(900, LogLevel.Error, "Exception in writing to standard input with input data: {StandardProcessExecutorInputData}")]
    public static partial void LogErrorWritingToStandardInput(this ILogger logger, object standardProcessExecutorInputData, Exception ex);

    [LoggerMessage(902, LogLevel.Error, "Exception caught while closing the standard input")]
    public static partial void LogErrorClosingStandardInput(this ILogger logger, Exception ex);

    [LoggerMessage(903, LogLevel.Error, "Exception caught while reading the process error output")]
    public static partial void LogErrorReadingProcessErrorOutput(this ILogger logger, Exception ex);

    [LoggerMessage(950, LogLevel.Warning, "An aggregated exception occurred.")]
    public static partial void LogAggregatedException(this ILogger logger, Exception ex);

    // Execution strategies
    [LoggerMessage(1500, LogLevel.Error, "executionStrategy.SafeDeleteDirectory has thrown an exception: {ex}", SkipEnabledCheck = true)]
    public static partial void LogSafeDeleteDirectoryException(this ILogger logger, Exception ex);

    [LoggerMessage(1501, LogLevel.Error, "Unexpected execution result is produced with the following output: '{@UnexpectedProcessOutput}'", SkipEnabledCheck = true)]
    public static partial void LogUnexpectedProcessOutput(this ILogger logger, ProcessExecutionResult unexpectedProcessOutput);

    [LoggerMessage(1600, LogLevel.Information, "Execution strategy: '{ExecutionStrategyType}' created a working directory: '{WorkingDirectory}' for submission #{SubmissionId}")]
    public static partial void LogExecutionStrategyCreatedWorkingDirectory(this ILogger logger, string executionStrategyType, string workingDirectory, int submissionId);
}