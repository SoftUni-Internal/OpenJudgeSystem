namespace OJS.Workers.Common
{
    public interface IExecutionStrategy
    {
        Task<IExecutionResult<TResult>> SafeExecute<TInput, TResult>(
            IExecutionContext<TInput> executionContext,
            IOjsSubmission submission,
            CancellationToken cancellationToken = default)
            where TResult : ISingleCodeRunResult, new();
    }
}