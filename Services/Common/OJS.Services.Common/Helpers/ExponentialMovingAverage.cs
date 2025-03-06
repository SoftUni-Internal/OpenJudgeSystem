namespace OJS.Services.Common.Helpers;

using System;

public class ExponentialMovingAverage
{
    private readonly double alpha;
    private bool isInitialized;

    /// <summary>
    /// Exponential Moving Average (EMA) is a type of infinite impulse response filter that applies weighting factors which decrease exponentially.
    /// </summary>
    /// <param name="alpha">Alpha is the smoothing factor, which depends on the period of the EMA. It must be in the range (0, 1].</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when alpha is less than or equal to 0 or greater than 1.</exception>
    public ExponentialMovingAverage(double alpha)
    {
        if (alpha is <= 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(alpha), "Alpha must be in (0, 1]");
        }

        this.alpha = alpha;
    }

    private double PreviousEma { get; set; }

    public double AddDataPoint(double value)
    {
        if (!this.isInitialized)
        {
            this.PreviousEma = value;
            this.isInitialized = true;
            return this.PreviousEma;
        }

        var currentEma = (this.alpha * value) + ((1 - this.alpha) * this.PreviousEma);
        this.PreviousEma = currentEma;
        return currentEma;
    }

    public double CurrentValue => this.PreviousEma;
}