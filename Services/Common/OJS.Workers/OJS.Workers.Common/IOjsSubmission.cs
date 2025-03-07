namespace OJS.Workers.Common
{
    using OJS.Workers.Common.Models;

    public interface IOjsSubmission
    {
        object Id { get; }

        string AdditionalCompilerArguments { get; }

        string? ProcessingComment { get; set; }

        int MemoryLimit { get; }

        int? ExecutionStrategyBaseMemoryLimit { get; }

        int TimeLimit { get; }

        int? ExecutionStrategyBaseTimeLimit { get; }

        string Code { get; }

        byte[] FileContent { get; }

        string AllowedFileExtensions { get; }

        CompilerType CompilerType { get; }

        ExecutionType ExecutionType { get; }

        ExecutionStrategyType ExecutionStrategyType { get; }

        ExceptionType ExceptionType { get; set; }

        DateTime? StartedExecutionOn { get; set; }

        DateTime? CompletedExecutionOn { get; set; }

        bool Verbosely { get; set; }
    }
}