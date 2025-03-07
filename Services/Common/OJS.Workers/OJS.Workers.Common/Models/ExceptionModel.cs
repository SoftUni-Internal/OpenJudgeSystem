namespace OJS.Workers.Common.Models
{
    using OJS.Workers.Common.Extensions;
    using System;

    public class ExceptionModel
    {
        // Used to deserialize from json
        public ExceptionModel()
        {
        }

        public ExceptionModel(
            Exception exception,
            bool includeStackTrace = false,
             ExceptionType? exceptionType = null)
        {
            this.Message = exception.GetAllMessages();
            this.ExceptionType = exceptionType;

            if (includeStackTrace)
            {
                this.StackTrace = exception.GetBaseException().StackTrace;
            }
        }

        public string? Message { get; set; }

        public string? StackTrace { get; set; }

        public ExceptionType? ExceptionType { get; set; }
    }
}