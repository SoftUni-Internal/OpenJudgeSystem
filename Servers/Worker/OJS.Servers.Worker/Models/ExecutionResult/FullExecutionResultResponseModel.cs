namespace OJS.Servers.Worker.Models.ExecutionResult;

using OJS.Workers.Common.Models;
using System;

public class FullExecutionResultResponseModel
{
    public ExceptionModel? Exception { get; set; }

    public ExecutionResultResponseModel? ExecutionResult { get; set; }

    public DateTime? StartedExecutionOn { get; set; }

    public DateTime? CompletedExecutionOn { get; set; }

    public void SetExecutionResult(ExecutionResultResponseModel executionResult)
    {
        this.ExecutionResult = executionResult;
        this.Exception = null;
    }

    public void SetException(Exception exception, bool includeStackTrace, ExceptionType? exceptionType = null)
    {
        this.Exception = new ExceptionModel(exception, includeStackTrace, exceptionType);
        this.ExecutionResult = null;
    }

    public void SetStartedAndCompletedExecutionOn(DateTime startedExecutionOn, DateTime completedExecutionOn)
    {
        this.StartedExecutionOn = startedExecutionOn;
        this.CompletedExecutionOn = completedExecutionOn;
    }
}
