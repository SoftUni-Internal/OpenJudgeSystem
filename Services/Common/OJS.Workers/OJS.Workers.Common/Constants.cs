namespace OJS.Workers.Common
{
    using System;
    using System.IO;

    using OJS.Workers.Common.Extensions;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;

    public static class Constants
    {
        public const string LocalWorkerServiceName = "OJS Local Worker Service";
        public const string LocalWorkerMonitoringServiceName = "OJS Local Worker Monitoring Service";

        public const string LocalWorkerServiceLogName = "LocalWorkerService";
        public const string LocalWorkerMonitoringServiceLogName = "LocalWorkerMonitoringService";

        public const string RestrictedUserName = "restricted_user";

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
        public const int MinimumMemoryLimitInBytes = 1024 * 1024 * 5;
        public const int MaxMemoryLimitInBytes = 1024 * 1024 * 1000;
        public const int MinimumTimeLimitInMilliseconds = 1;
        public const int MaxTimeLimitInMilliseconds = 1000 * 60 * 5;

        public const string AppSettingsConfigSectionName = "appSettings";
        public const string ConnectionStringsConfigSectionName = "connectionStrings";

        public const string TestPassedMessage = "Test Passed!";

        // Environment variables
        public const string AspNetCoreEnvironmentVariable = "ASPNETCORE_ENVIRONMENT";
        public const string RunsInDockerEnvironmentVariable = "DOTNET_RUNNING_IN_CONTAINER";
        public const string HostIpEnvironmentVariable = "HOST_IP";

        // Runtime constants
        public const string NewLineUnix = "\n";
        public const string NewLineWin = "\r\n";
#pragma warning disable SA1307
#pragma warning disable SA1311
        public static readonly string javaSourceFileExtension = $".{CompilerType.Java.GetFileExtension()}";
        public static readonly string cSharpFileExtension = $".{CompilerType.CSharpDotNetCore.GetFileExtension()}";
        public static readonly string classDelimiterUnix = $"~~!!!==#==!!!~~{NewLineUnix}";
        public static readonly string classDelimiterWin = $"~~!!!==#==!!!~~{NewLineWin}";
#pragma warning restore SA1311
#pragma warning restore SA1307

        // Temp Directory folder paths
        public static string ExecutionStrategiesWorkingDirectoryPath
        {
            get
            {
                var rootPath = string.Empty;

                if (OsPlatformHelpers.IsUnix())
                {
                    rootPath = Path.GetTempPath();
                }

                if (OsPlatformHelpers.IsWindows())
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
