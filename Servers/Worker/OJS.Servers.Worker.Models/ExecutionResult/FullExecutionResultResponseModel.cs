﻿namespace OJS.Servers.Worker.Models.ExecutionResult;

using System;
using OJS.Workers.SubmissionProcessors.Models;

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

    public void SetException(Exception exception, bool includeStackTrace)
    {
        this.Exception = new ExceptionModel(exception, includeStackTrace);
        this.ExecutionResult = null;
    }

    public void SetStartedAndCompletedExecutionOn(DateTime startedExecutionOn, DateTime completedExecutionOn)
    {
        this.StartedExecutionOn = startedExecutionOn;
        this.CompletedExecutionOn = completedExecutionOn;
    }
}
