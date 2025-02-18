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

        public IExecutor CreateProcessExecutor(
            int baseTimeUsed,
            int baseMemoryUsed,
            ProcessExecutorType type) =>
            type switch
            {
                ProcessExecutorType.Default => new StandardProcessExecutor(baseTimeUsed, baseMemoryUsed, this.tasksService, this.logger),
                ProcessExecutorType.Standard => new StandardProcessExecutor(baseTimeUsed, baseMemoryUsed, this.tasksService, this.logger),
                _ => throw new ArgumentOutOfRangeException($"Unsupported process executor: {type}"),
            };
    }
}
