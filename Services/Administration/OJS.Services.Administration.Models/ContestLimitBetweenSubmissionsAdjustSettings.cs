namespace OJS.Services.Administration.Models;

public class ContestLimitBetweenSubmissionsAdjustSettings
{
    public int MaxLimitBetweenSubmissionsInSeconds { get; set; } = 300;

    /// <summary>
    /// The max factor by which the limit between submissions can be increased due to the busy ratio of the workers.
    /// </summary>
    public double BusyRatioMaxFactor { get; set; } = 1.5;

    /// <summary>
    /// The threshold for the busy ratio below which the limit between submissions is decreased by the ratio multiplier.
    /// e.g. for 0.3, the moderate threshold is reached when less than 30% of the workers are busy.
    /// </summary>
    public double BusyRatioModerateThreshold { get; set; } = 0.3;

    /// <summary>
    /// The threshold for the busy ratio above which the limit between submissions is increased by the ratio multiplier.
    /// e.g. for 0.7, the critical threshold is reached when more than 70% of the workers are busy.
    /// </summary>
    public double BusyRatioCriticalThreshold { get; set; } = 0.7;

    /// <summary>
    /// The maximum factor by which the limit between submissions can be increased due to the queue size.
    /// </summary>
    public double QueueLenghtMaxFactor { get; set; } = 4;

    /// <summary>
    /// The multiplier by which the queue length is considered to be moderately full.
    /// e.g. for 0.2, the queue is considered to be moderately full if it has 20% more submissions than the number of workers.
    /// </summary>
    public double QueueLenghtModerateThresholdMultiplier { get; set; } = 0.2;

    /// <summary>
    /// The multiplier by which the queue length is considered to be critically full.
    /// e.g. for 3.0, if the queue is 3 times larger than the number of workers, it is considered to be critically full.
    /// </summary>
    public double QueueLenghtCriticalThresholdMultiplier { get; set; } = 3;
}