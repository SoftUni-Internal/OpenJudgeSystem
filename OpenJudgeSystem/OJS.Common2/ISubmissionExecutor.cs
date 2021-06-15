namespace OJS.Workers.Common
{
    public interface ISubmissionExecutor
    {
        IExecutionResult<TResult> Execute<TInput, TResult>(IOjsSubmission submission)
            where TResult : ISingleCodeRunResult, new();
    }
}