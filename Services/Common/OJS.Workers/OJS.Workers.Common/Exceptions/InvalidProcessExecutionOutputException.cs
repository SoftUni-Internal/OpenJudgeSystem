namespace OJS.Workers.Common.Exceptions;

using System;

public class InvalidProcessExecutionOutputException : SolutionException
{
    private const string MessageTitle = "The process did not produce any valid output!";

    private const string DefaultMessage = $"{MessageTitle} Please try again later or contact an administrator if the problem persists.";

    public InvalidProcessExecutionOutputException()
        : base(DefaultMessage)
    {
    }

    public InvalidProcessExecutionOutputException(string message)
        : base(MessageTitle + Environment.NewLine + message)
    {
    }
}
