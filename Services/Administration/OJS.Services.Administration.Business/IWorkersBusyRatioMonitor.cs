namespace OJS.Services.Administration.Business;

using OJS.Services.Administration.Models;
using OJS.Services.Infrastructure;
using System.Threading;

public interface IWorkersBusyRatioMonitor : ISingletonService
{
    void StartMonitoring(CancellationToken cancellationToken);

    void StopMonitoring();

    WorkersBusyRatioServiceModel GetWorkersBusyRatio();
}