namespace OJS.Workers.Common.Models
{
    /// <summary>
    /// Possible types of exceptions during the execution of a worker.
    /// </summary>
    public enum ExceptionType
    {
        Solution = 0,
        Configuration = 1,
        Strategy = 2,
        Remote = 3,
        Other = 4,
    }
}
