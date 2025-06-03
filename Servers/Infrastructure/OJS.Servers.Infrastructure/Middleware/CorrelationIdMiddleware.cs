namespace OJS.Servers.Infrastructure.Middleware;

using Microsoft.AspNetCore.Http;
using OJS.Servers.Infrastructure.Telemetry;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// Middleware to add business-specific correlation ID to OpenTelemetry traces.
/// OpenTelemetry handles automatic trace context propagation via W3C standards.
/// This middleware adds a user-friendly correlation ID for business purposes.
/// </summary>
public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrGenerateCorrelationId(context);

        // Set correlation ID in response headers for client visibility
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        // Add correlation ID to current activity as a business tag
        // OpenTelemetry automatically handles trace context propagation
        var currentActivity = Activity.Current;
        currentActivity?.SetTag(OjsActivitySources.CommonTags.CorrelationId, correlationId);

        // Add correlation ID to baggage for cross-service propagation
        currentActivity?.SetBaggage(OjsActivitySources.CommonTags.CorrelationId, correlationId);

        await next(context);
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        // Try to get correlation ID from request headers
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue))
        {
            var correlationId = headerValue.ToString();
            if (!string.IsNullOrEmpty(correlationId))
            {
                return correlationId;
            }
        }

        // Try to get from OpenTelemetry baggage
        var currentActivity = Activity.Current;
        var baggageCorrelationId = currentActivity?.GetBaggageItem(OjsActivitySources.CommonTags.CorrelationId);
        if (!string.IsNullOrEmpty(baggageCorrelationId))
        {
            return baggageCorrelationId;
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString("N")[..16]; // Short correlation ID
    }
}
