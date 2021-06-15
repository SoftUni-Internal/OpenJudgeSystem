namespace OJS.Workers.Common
{
    using System;
    using System.IO;
    using OJS.Common2.Extensions;
    using OJS.Workers.Common.Extensions;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;

    public static class Constants
    {
        public const string LocalWorkerServiceName = "OJS Local Worker Service";
        public const string LocalWorkerMonitoringServiceName = "OJS Local Worker Monitoring Service";

        public const string LocalWorkerServiceLogName = "LocalWorkerService";
        public const string LocalWorkerMonitoringServiceLogName = "LocalWorkerMonitoringService";

        public const string DefaultCheckerAssemblyName = "OJS.Workers.Checkers";

        public const int DefaultJobLoopWaitTimeInMilliseconds = 1000;
        public const int DefaultTimeBeforeAbortingThreadsInMilliseconds = 10000;

        public const int DefaultTimeLimitInMilliseconds = 100;
        public const int DefaultMemoryLimitInBytes = 16 * 1024 * 1024;

        // File extensions
        public const string ClassLibraryFileExtension = ".dll";
        public const string ExecutableFileExtension = ".exe";
        public const string JavaScriptFileExtension = ".js";
        public const string PythonFileExtension = ".py";
        public const string ZipFileExtension = ".zip";
        public const string JsonFileExtension = ".json";
        public const string SolidityFileExtension = ".sol";
        public const string ByteCodeFileExtension = ".bin";
        public const string AbiFileExtension = ".abi";

        // Folder names
        public const string ExecutionStrategiesFolderName = "ExecutionStrategies";

        // Other
        public const int DefaultProcessExitTimeOutMilliseconds = 5000;
        public const int ProcessDefaultBufferSizeInBytes = 4096;

        public const string AppSettingsConfigSectionName = "appSettings";
        public const string ConnectionStringsConfigSectionName = "connectionStrings";

        public const string TestPassedMessage = "Test Passed!";

        // Environment variables
        public const string AspNetCoreEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";

        // Runtime constants
        public const string NewLineUnix = "\n";
        public const string NewLineWin = "\r\n";
        public static readonly string JavaSourceFileExtension = $".{CompilerType.Java.GetFileExtension()}";
        public static readonly string CSharpFileExtension = $".{CompilerType.CSharp.GetFileExtension()}";
        public static readonly string ClassDelimiterUnix = $"~~!!!==#==!!!~~{NewLineUnix}";
        public static readonly string ClassDelimiterWin = $"~~!!!==#==!!!~~{NewLineWin}";

        // Temp Directory folder paths
        public static string ExecutionStrategiesWorkingDirectoryPath
        {
            get
            {
                var rootPath = string.Empty;

                if (OSPlatformHelpers.IsUnix())
                {
                    rootPath = Path.GetTempPath();
                }

                if (OSPlatformHelpers.IsWindows())
                {
                    rootPath = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
                }

                if (string.IsNullOrEmpty(rootPath))
                {
                    throw new InvalidOperationException(
                        "Root path for the Execution strategies working directory cannot be empty or null");
                }

                return Path.Combine(
                    rootPath,
                    ExecutionStrategiesFolderName);
            }
        }
    }
}
