namespace OJS.Services.Common.Telemetry;

using System.Collections.Generic;
using System.Diagnostics;
using MassTransit;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

/// <summary>
/// Helper for trace context propagation through MassTransit headers.
/// </summary>
public static class SimpleTracePropagation
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    /// <summary>
    /// Extracts trace context from MassTransit message headers and creates a child activity.
    /// </summary>
    public static Activity? CreateActivityFromHeaders<T>(
        ConsumeContext<T> context,
        ActivitySource activitySource,
        string activityName) where T : class
    {
        // Convert MassTransit headers into a dictionary carrier
        var carrier = new Dictionary<string, string>();
        foreach (var header in context.Headers.GetAll())
        {
            if (header.Value is string stringValue)
            {
                carrier[header.Key] = stringValue;
            }
        }

        // Extract trace context using the carrier
        var propagationContext = Propagator.Extract(default, carrier, (d, key) =>
            d.TryGetValue(key, out var val) ? new[] { val } : []);

        Baggage.Current = propagationContext.Baggage;

        return activitySource.StartActivity(activityName, ActivityKind.Consumer, propagationContext.ActivityContext);
    }
}
