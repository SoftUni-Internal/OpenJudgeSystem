namespace OJS.Services.Administration.Models;

public class ApplicationConfig : Infrastructure.Configurations.ApplicationConfig
{
    public string MicrosoftTeamsWebhookUri { get; set; } = string.Empty;

    public int SecondsBetweenContestLimitsAdjustment { get; set; } = 3 * 60;

    public int SecondsBetweenWorkersBusyRatioPolling { get; set; } = 20;

    // Alpha for the exponential moving average of the busy ratio.
    // Formula for alpha is 2 / (N + 1), where N is the number of samples in the moving average.
    // The number of samples will be the number of seconds between the contest limits adjustment
    // divided by the number of seconds between the workers busy ratio polling.
    public double BusyRatioMonitorAlpha
        => 2.0 / (((double)this.SecondsBetweenContestLimitsAdjustment / this.SecondsBetweenWorkersBusyRatioPolling) + 1);
}