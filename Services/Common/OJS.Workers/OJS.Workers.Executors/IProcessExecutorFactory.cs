namespace OJS.Workers.Executors
{
    using Microsoft.Extensions.Logging;
    using OJS.Workers.Common;

    public interface IProcessExecutorFactory
    {
        IExecutor CreateProcessExecutor<TStrategy>(
            int baseTimeUsed,
            int baseMemoryUsed,
            ProcessExecutorType type,
            ILogger<TStrategy> strategyLogger)
            where TStrategy : IExecutionStrategy;
    }
}
