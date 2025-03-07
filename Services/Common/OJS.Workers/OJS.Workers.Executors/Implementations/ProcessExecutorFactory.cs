namespace OJS.Workers.Executors.Implementations
{
    using Microsoft.Extensions.Logging;
    using OJS.Workers.Common;

    public class ProcessExecutorFactory : IProcessExecutorFactory
    {
        private readonly ITasksService tasksService;
        private readonly ILogger<StandardProcessExecutor> logger;

        public ProcessExecutorFactory(
            ITasksService tasksService,
            ILogger<StandardProcessExecutor> logger)
        {
            this.tasksService = tasksService;
            this.logger = logger;
        }

        public IExecutor CreateProcessExecutor<T>(
            int baseTimeUsed,
            int baseMemoryUsed,
            ProcessExecutorType type,
            ILogger<T> strategyLogger)
            where T : IExecutionStrategy =>
            type switch
            {
                ProcessExecutorType.Standard => new StandardProcessExecutor(baseTimeUsed, baseMemoryUsed, this.tasksService, this.logger, strategyLogger),
                ProcessExecutorType.Restricted => new StandardProcessExecutor(baseTimeUsed, baseMemoryUsed, this.tasksService, this.logger, strategyLogger, runAsRestrictedUser: true),
                _ => throw new ArgumentOutOfRangeException($"Unsupported process executor: {type}"),
            };
    }
}
