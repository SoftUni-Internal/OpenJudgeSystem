namespace OJS.Workers.ExecutionStrategies.CodeSanitizers
{
    using System;
    using System.IO;
    using System.Linq;

    using Ionic.Zip;

    using OJS.Workers.Common;

    /// <summary>
    /// Used to sanitize content.
    /// </summary>
    public abstract class BaseCodeSanitizer
    {
        /// <summary>
        /// Processes the text of a submission and removes potentially harmful code from the execution context.
        /// </summary>
        /// <param name="executionContext">Execution context of the submission.</param>
        /// <typeparam name="TInput">Type of the input.</typeparam>
        public void Sanitize<TInput>(IExecutionContext<TInput> executionContext)
        {
            if (ExecutionContextContainsZipFile(executionContext))
            {
                executionContext.FileContent = this.SanitizeZipFileContent(
                    executionContext.FileContent,
                    this.DoSanitize);
            }
            else if (!string.IsNullOrWhiteSpace(executionContext.Code))
            {
                executionContext.Code = this.DoSanitize(executionContext.Code);
            }
        }

        /// <summary>
        /// Does the actual sanitizing operation on the text content.
        /// </summary>
        /// <param name="content">Directly submitted Code from submission or the content of each file from zip.</param>
        /// <returns>The sanitized content.</returns>
        protected abstract string DoSanitize(string content);

        protected virtual byte[] SanitizeZipFileContent(byte[] zipFileContent, Func<string, string> sanitizingFunc)
        {
            var sanitizedZipFile = new ZipFile();

            using (var fileContentMemoryStream = new MemoryStream(zipFileContent))
            {
                var zipFile = ZipFile.Read(fileContentMemoryStream);

                foreach (var zipEntry in zipFile.Entries.Where(e => !e.IsDirectory))
                {
                    var normalizedPath = NormalizePath(zipEntry.FileName);
                    if (this.ShouldRemovePathInZipEntry(normalizedPath))
                    {
                        continue; // drop __MACOSX, AppleDouble, and .DS_Store
                    }

                    using var memoryInputStream = new MemoryStream();
                    zipEntry.Extract(memoryInputStream);

                    memoryInputStream.Seek(0, SeekOrigin.Begin);

                    using var streamReader = new StreamReader(memoryInputStream);
                    var sanitizedText = sanitizingFunc(streamReader.ReadToEnd());

                    sanitizedZipFile.AddEntry(zipEntry.FileName, sanitizedText);
                }
            }

            using var outputStream = new MemoryStream();
            sanitizedZipFile.Save(outputStream);

            return outputStream.ToArray();
        }

        protected virtual bool ShouldRemovePathInZipEntry(string normalizedPath)
        {
            if (normalizedPath.StartsWith("__MACOSX/", StringComparison.Ordinal))
            {
                return true;
            }

            var trimmed = normalizedPath.TrimEnd('/');
            var leafStart = trimmed.LastIndexOf('/') + 1;
            var leaf = trimmed[leafStart..];

            return leaf.StartsWith("._", StringComparison.Ordinal) || // AppleDouble files
                   leaf.Equals(".DS_Store", StringComparison.Ordinal); // macOS Finder metadata
        }

        private static bool ExecutionContextContainsZipFile<TInput>(IExecutionContext<TInput> executionContext) =>
            !string.IsNullOrWhiteSpace(executionContext.AllowedFileExtensions) &&
            executionContext.AllowedFileExtensions.Contains(Constants.ZipFileExtension[1..]);

        private static string NormalizePath(string path)
        {
            var p = path.Replace('\\', '/');
            if (p.StartsWith("./", StringComparison.Ordinal))
            {
                p = p[2..];
            }

            return p;
        }
    }
}