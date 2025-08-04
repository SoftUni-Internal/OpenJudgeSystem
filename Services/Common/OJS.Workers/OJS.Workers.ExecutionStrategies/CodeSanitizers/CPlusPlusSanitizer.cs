namespace OJS.Workers.ExecutionStrategies.CodeSanitizers
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <inheritdoc />
    public class CPlusPlusSanitizer : BaseCodeSanitizer
    {
        private const string ProcessAccessRightsPattern = @"(PROCESS_[A-Z_]+)|(0x0[0-9]+)";
        private const string VisualStudioPrecompiledHeaderPattern = @"#\s*include\s+\""pch\.h\""\s*";
        private static readonly string[] FunctionsToDisable =
        [
            "OpenProcess",
            "OpenThread",
            "GetProcessId",
            "GetThreadId",
            "GetCurrentProcess",
            "GetCurrentThread",
            "GetCurrentProcessId",
            "GetCurrentThreadId",
            "TerminateProcess",
            "TerminateThread",
            "SwitchToThread",
            "SuspendThread",
        ];

        /// <inheritdoc/>
        protected override string DoSanitize(string content)
        {
            content = RemoveProcessAccessRights(content);
            content = RemoveProcessAndThreadAccessFunctions(content);
            content = RemoveVisualStudioPrecompiledHeader(content);

            return content;
        }

        private static string RemoveProcessAndThreadAccessFunctions(string content)
        {
            var functionsToDisableRegexPattern = string.Join("|", FunctionsToDisable.Select(f => f + @"\s*\("));

            return Regex.Replace(content, functionsToDisableRegexPattern, string.Empty);
        }

        private static string RemoveProcessAccessRights(string content)
            => Regex.Replace(content, ProcessAccessRightsPattern, string.Empty);

        // using Environment.NewLine to preserve line numbers
        private static string RemoveVisualStudioPrecompiledHeader(string content)
            => Regex.Replace(content, VisualStudioPrecompiledHeaderPattern, Environment.NewLine);
    }
}