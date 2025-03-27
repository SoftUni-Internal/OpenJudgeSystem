namespace OJS.Services.Common.Helpers;

using System;
using System.Collections.Generic;

public class RollingAverage(TimeSpan windowSize)
{
    private readonly List<(DateTime Timestamp, double Ratio)> entries = [];
    private readonly object lockObj = new();

    public void AddMeasurement(double ratio)
    {
        lock (this.lockObj)
        {
            var now = DateTime.UtcNow;
            this.entries.Add((now, ratio));
            this.entries.RemoveAll(e => (now - e.Timestamp) > windowSize);
        }
    }

    public double GetAverage()
    {
        lock (this.lockObj)
        {
            if (this.entries.Count == 0)
            {
                return 0.0;
            }

            double sum = 0;

            foreach (var (_, ratio) in this.entries)
            {
                sum += ratio;
            }

            return sum / this.entries.Count;
        }
    }
}