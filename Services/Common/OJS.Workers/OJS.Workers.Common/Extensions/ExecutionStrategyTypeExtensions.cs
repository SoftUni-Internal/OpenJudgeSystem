namespace OJS.Workers.Common.Extensions
{
    using OJS.Workers.Common.Models;

    public static class ExecutionStrategyTypeExtensions
    {
        public static string? GetFileExtension(this ExecutionStrategyType executionStrategyType)
            => executionStrategyType switch
            {
                ExecutionStrategyType.CompileExecuteAndCheck => null, // The file extension depends on the compiler.
                ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck => "js",
                ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck => "java",
                ExecutionStrategyType.PythonExecuteAndCheck => "py",
                _ => null,
            };
    }
}