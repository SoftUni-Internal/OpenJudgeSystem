namespace OJS.Services.Administration.Business.Implementations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OJS.Common.Utils;
using OJS.Services.Administration.Models;
using OJS.Services.Common.Helpers;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.HttpClients;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static OJS.Common.Constants.ServiceConstants;

public class WorkersBusyRatioMonitor(
    IOptions<ApplicationConfig> applicationConfigAccessor,
    IRabbitMqHttpClient rabbitMqClient,
    ILogger<WorkersBusyRatioMonitor> logger)
    : IWorkersBusyRatioMonitor
{
    private readonly RollingAverage busyRatioRollingAverage = new(TimeSpan.FromSeconds(applicationConfigAccessor.Value.SecondsBetweenContestLimitsAdjustment));
    private readonly RollingAverage totalWorkersCountRollingAverage = new(TimeSpan.FromSeconds(applicationConfigAccessor.Value.SecondsBetweenContestLimitsAdjustment));
    private readonly RollingAverage submissionsAwaitingExecutionCountRollingAverage = new(TimeSpan.FromSeconds(applicationConfigAccessor.Value.SecondsBetweenContestLimitsAdjustment));
    private readonly ExponentialMovingAverage ema = new(applicationConfigAccessor.Value.BusyRatioMonitorAlpha);
    private readonly TimeSpan pollInterval = TimeSpan.FromSeconds(applicationConfigAccessor.Value.SecondsBetweenWorkersBusyRatioPolling);

    private bool IsMonitoringRunning { get; set; }

    public void StartMonitoring(CancellationToken cancellationToken)
    {
        this.IsMonitoringRunning = true;
        Task.Run(async () =>
        {
            while (this.IsMonitoringRunning)
            {
                try
                {
                    await this.TakeMeasurement();
                }
                catch (Exception ex)
                {
                    logger.LogErrorMeasuringBusyWorkersRatio(ex);
                }

                await Task.Delay(this.pollInterval, cancellationToken);
            }
        }, cancellationToken);
    }

    private async Task TakeMeasurement()
    {
        var (consumers, channels, queue) = await TasksUtils.WhenAll(
            rabbitMqClient.GetConsumers(),
            rabbitMqClient.GetChannels(),
            rabbitMqClient.GetQueue(WorkersQueueName));

        ArgumentNullException.ThrowIfNull(queue, nameof(queue));

        var workerConsumers = consumers
            .Where(c => c is { Active: true, PrefetchCount: 1, Queue.Name: WorkersQueueName })
            .ToList();

        var workerChannels = channels
            .Where(ch => workerConsumers.Select(w => w.ChannelDetails.Name).Contains(ch.Name))
            .ToList();

        var workersTotalCount = workerConsumers.Count;
        var busyWorkers = workerChannels.Count(ch => ch.MessagesUnacknowledged > 0);
        var busyRatio = (double)busyWorkers / workersTotalCount;

        this.ema.AddDataPoint(busyRatio);
        this.busyRatioRollingAverage.AddMeasurement(busyRatio);
        this.totalWorkersCountRollingAverage.AddMeasurement(workersTotalCount);
        this.submissionsAwaitingExecutionCountRollingAverage.AddMeasurement(queue.MessagesReady);
    }

    public void StopMonitoring() => this.IsMonitoringRunning = false;

    public WorkersBusyRatioServiceModel GetWorkersBusyRatio()
        => new(
            ExponentialMovingAverageRatio: this.ema.CurrentValue,
            RollingAverageRatio: this.busyRatioRollingAverage.GetAverage(),
            WorkersTotalCount: (int)this.totalWorkersCountRollingAverage.GetAverage(),
            SubmissionsAwaitingExecution: (int)this.submissionsAwaitingExecutionCountRollingAverage.GetAverage());
}