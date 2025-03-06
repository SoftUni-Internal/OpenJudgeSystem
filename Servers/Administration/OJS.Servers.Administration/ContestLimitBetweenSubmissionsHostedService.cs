namespace OJS.Servers.Administration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OJS.Services.Administration.Business;
using OJS.Services.Administration.Business.Contests;
using OJS.Services.Administration.Models;
using OJS.Services.Infrastructure.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ContestLimitBetweenSubmissionsHostedService(
    IOptions<ApplicationConfig> applicationConfigAccessor,
    IServiceProvider serviceProvider,
    IWorkersBusyRatioMonitor workersBusyRatioMonitor,
    ILogger<ContestLimitBetweenSubmissionsHostedService> logger)
    : BackgroundService
{
    private readonly TimeSpan timeBetweenContestLimitsAdjustment =
        TimeSpan.FromSeconds(applicationConfigAccessor.Value.SecondsBetweenContestLimitsAdjustment);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        workersBusyRatioMonitor.StartMonitoring(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var busyRatio = workersBusyRatioMonitor.GetWorkersBusyRatio();

            try
            {
                using var scope = serviceProvider.CreateScope();
                var contestsBusiness = scope.ServiceProvider.GetRequiredService<IContestsBusinessService>();
                await contestsBusiness.AdjustLimitBetweenSubmissions(busyRatio);
            }
            catch (Exception ex)
            {
                logger.LogErrorAdjustingContestsLimitBetweenSubmissions(ex);
            }

            await Task.Delay(this.timeBetweenContestLimitsAdjustment, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        workersBusyRatioMonitor.StopMonitoring();
        return base.StopAsync(cancellationToken);
    }
}