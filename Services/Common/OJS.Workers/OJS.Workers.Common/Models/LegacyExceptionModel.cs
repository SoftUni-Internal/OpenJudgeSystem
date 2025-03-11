namespace OJS.Workers.Common.Models;

using OJS.Workers.Common.Extensions;

public class LegacyExceptionModel
{
    // Used to deserialize from json
    public LegacyExceptionModel()
    {
    }

    public LegacyExceptionModel(
        Exception exception,
        bool includeStackTrace = false,
        ExceptionType? exceptionType = null)
    {
        this.Message = exception.GetAllMessages();
        this.ExceptionType = exceptionType.ToString();

        if (includeStackTrace)
        {
            this.StackTrace = exception.GetBaseException().StackTrace;
        }
    }

    public string? Message { get; set; }

    public string? StackTrace { get; set; }

    public string? ExceptionType { get; set; }
}