namespace OJS.Workers.ExecutionStrategies.Models;

using System;
using System.Collections.Generic;
using OJS.Workers.Common;

public class ExecutionResult<TResult> : IExecutionResult<TResult>
    where TResult : ISingleCodeRunResult, new()
{
    public bool IsCompiledSuccessfully { get; set; }

    public string? CompilerComment { get; set; }

    public string? ProcessingComment { get; set; }

    public ICollection<TResult> Results { get; set; } = [];

    public DateTime? StartedExecutionOn { get; set; }

    public DateTime? CompletedExecutionOn { get; set; }

    public byte[]? VerboseLogFile { get; set; }
}
