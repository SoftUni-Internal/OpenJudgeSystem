namespace OJS.Services.Administration.Models;

public class ContestLimitBetweenSubmissionsAdjustSettings
{
    public int MaxLimitBetweenSubmissionsInSeconds { get; set; } = 300;

    /// <summary>
    /// The factor by which the limit between submissions can be increased due to the busy ratio.
    /// </summary>
    public double RatioMultiplier { get; set; } = 1.5;

    /// <summary>
    /// The threshold for the busy ratio below which the limit between submissions is decreased by the ratio multiplier.
    /// </summary>
    public double RatioModerateThreshold { get; set; } = 0.3;

    /// <summary>
    /// The threshold for the busy ratio above which the limit between submissions is increased by the ratio multiplier.
    /// </summary>
    public double RatioCriticalThreshold { get; set; } = 0.7;

    /// <summary>
    /// The maximum factor by which the limit between submissions can be increased due to the queue size.
    /// </summary>
    public double QueueMaxFactor { get; set; } = 4;

    /// <summary>
    /// The multiplier by which the queue is considered to be critically full.
    /// e.g. if the queue is 3 times larger than the number of workers, it is considered to be critically full.
    /// </summary>
    public double QueueCriticalThresholdMultiplier { get; set; } = 3;
}