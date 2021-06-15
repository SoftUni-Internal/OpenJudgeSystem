namespace OJS.Workers.Common.Helpers
{
    using System;
    using System.IO;
    using System.Threading;

    public static class DirectoryHelpers
    {
        private const int ThreadSleepMilliseconds = 1000;

        public static void CreateDirectory(string directoryPath)
            => Directory.CreateDirectory(directoryPath);

        public static string CreateDirectoryForFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            fileInfo.Directory?.Create();
            return fileInfo.DirectoryName;
        }

        public static string CreateTempDirectory()
        {
            while (true)
            {
                var randomDirectoryName = Path.GetRandomFileName();
                var path = Path.Combine(Path.GetTempPath(), randomDirectoryName);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    return path;
                }
            }
        }

        public static string CreateTempDirectoryForExecutionStrategy()
        {
            var isDirectoryCreated = false;
            var path = string.Empty;
            while (!isDirectoryCreated)
            {
                var randomDirectoryName = Path.GetRandomFileName();
                path = Path.Combine(Constants.ExecutionStrategiesWorkingDirectoryPath, randomDirectoryName);
                if (Directory.Exists(path))
                {
                    continue;
                }

                Directory.CreateDirectory(path);
                isDirectoryCreated = true;
            }

            return path;
        }

        public static void SafeDeleteDirectory(string path, bool recursive = false)
        {
            if (Directory.Exists(path))
            {
                var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(path, "*", searchOption))
                {
                    File.SetAttributes(fileSystemEntry, FileAttributes.Normal);
                }

                Directory.Delete(path, recursive);
            }
        }

        public static void DeleteExecutionStrategyWorkingDirectories()
        {
            var executionStrategiesDirectoryPath = Constants.ExecutionStrategiesWorkingDirectoryPath;

            if (!Directory.Exists(executionStrategiesDirectoryPath))
            {
                return;
            }

            var workingDirectoryPaths = Directory.GetDirectories(executionStrategiesDirectoryPath);
            foreach (var directoryPath in workingDirectoryPaths)
            {
                var directory = new DirectoryInfo(directoryPath);
                if (directory.Exists && directory.CreationTime < DateTime.Now.AddHours(-1))
                {
                    var isDeleted = false;
                    var retryCount = 0;
                    while (!isDeleted && retryCount <= 3)
                    {
                        try
                        {
                            SafeDeleteDirectory(directoryPath, true);
                            isDeleted = true;
                        }
                        catch
                        {
                            Thread.Sleep(ThreadSleepMilliseconds);
                            retryCount++;
                        }
                    }
                }
            }
        }
    }
}
