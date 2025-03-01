namespace OJS.Services.Administration.Models;

public class ContestLimitBetweenSubmissionsAdjustSettings
{
    public int MaxLimitBetweenSubmissionsInSeconds { get; set; }

    public double RatioMultiplier { get; set; } = 1;

    public double RatioModerateThreshold { get; set; } = 0.3;

    public double RatioCriticalThreshold { get; set; } = 0.8;

    public double QueueMaxFactor { get; set; } = 3;

    public double QueueModerateThresholdMultiplier { get; set; } = 1.5;

    public double QueueCriticalThresholdMultiplier { get; set; } = 4;
}