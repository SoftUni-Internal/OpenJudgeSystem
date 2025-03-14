#nullable disable
namespace OJS.Workers.Common.Helpers
{
    using System.IO;

    public static class DirectoryHelpers
    {
        public static void CreateDirectory(string directoryPath)
            => Directory.CreateDirectory(directoryPath);

        public static string CreateDirectoryForFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            fileInfo.Directory?.Create();
            return fileInfo.DirectoryName;
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
    }
}
