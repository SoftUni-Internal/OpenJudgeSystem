namespace OJS.Services.Worker.Business.Extensions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OJS.Workers.Common.Helpers;
using Serilog;
using Serilog.Extensions.Logging;
using System.Globalization;

public static class LoggerFactoryExtensions
{
    public static ILogger<T> CreateStrategyLogger<T>(
        this ILoggerFactory _,
        int submissionId,
        bool verbose,
        int maxLogFileSizeBytes)
    {
        if (!verbose)
        {
            // When verbose is false, return the NullLogger so that logging calls become no-ops,
            // which is useful for performance when logging is not needed.
            return NullLogger<T>.Instance;
        }

        // Create a dedicated file logger for this submission.
        var serilogLoggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: FileHelpers.BuildSubmissionLogFilePath(submissionId),
                fileSizeLimitBytes: maxLogFileSizeBytes,
                rollOnFileSizeLimit: false,
                formatProvider: CultureInfo.InvariantCulture);

        var serilogLogger = serilogLoggerConfiguration.CreateLogger();
        var loggerFactory = new SerilogLoggerFactory(serilogLogger, dispose: true);
        return loggerFactory.CreateLogger<T>();
    }
}