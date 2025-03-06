namespace OJS.Services.Administration.Models;

public record WorkersBusyRatioServiceModel(
    double ExponentialMovingAverageRatio,
    double RollingAverageRatio,
    int WorkersTotalCount,
    int SubmissionsAwaitingExecution);
