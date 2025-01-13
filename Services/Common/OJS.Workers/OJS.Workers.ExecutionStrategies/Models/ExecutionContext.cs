#nullable disable
namespace OJS.Workers.ExecutionStrategies.Models
{
    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;

    public class ExecutionContext<TInput> : IExecutionContext<TInput>
    {
        public CompilerType CompilerType { get; set; }

        public string AdditionalCompilerArguments { get; set; }

        public string Code { get; set; }

        public byte[] FileContent { get; set; }

        public byte[] AdditionalFiles { get; set; }

        public string AllowedFileExtensions { get; set; }

        public int TimeLimit { get; set; }

        public int MemoryLimit { get; set; }

        public TInput Input { get; set; }
    }
}
