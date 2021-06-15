namespace OJS.Workers.Common.Exceptions
{
    using System;

    public class InvalidProcessExecutionOutputException : Exception
    {
        private const string CustomMessage =
            "The process did not produce any valid output! " +
            "Please try again later or contact an administrator if the problem persists.";

        public InvalidProcessExecutionOutputException()
            : base(CustomMessage)
        {
        }
    }
}
