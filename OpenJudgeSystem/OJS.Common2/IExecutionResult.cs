namespace OJS.Workers.Common
{
    using System.Collections.Generic;

    public interface IExecutionResult<TResult>
        where TResult : ISingleCodeRunResult, new()
    {
        bool IsCompiledSuccessfully { get; set; }

        string CompilerComment { get; set; }

        ICollection<TResult> Results { get; }
    }
}