namespace OJS.Services.Common.Models.Submissions
{
    using OJS.Workers.Common.Models;
    using System;

    public class SubmissionExecutionResult
    {
        public int SubmissionId { get; set; }

        public ExceptionModel? Exception { get; set; }

        public ExecutionResultServiceModel? ExecutionResult { get; set; }

        public DateTime? StartedExecutionOn { get; set; }

        public DateTime? CompletedExecutionOn { get; set; }

        public string? WorkerName { get; set; }

        public byte[]? VerboseLogFile { get; set; }
    }
}