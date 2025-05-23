namespace OJS.Servers.Infrastructure.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        var applicationName = builder.Environment.GetShortApplicationName().ToLower();

        builder.Logging.AddOpenTelemetry(loggingBuilder =>
        {
            loggingBuilder.IncludeFormattedMessage = true;
            loggingBuilder.IncludeScopes = true;
        });

        var otel = builder.Services.AddOpenTelemetry();

        otel.ConfigureResource(resource => resource.AddService(applicationName));

        otel.WithMetrics(metrics =>
        {
            metrics
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation();
        });

        otel.WithTracing(tracing =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // In development, always sample traces, so we can see them in the console
                tracing.SetSampler<AlwaysOnSampler>();
            }

            tracing
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation();
        });

        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (otlpEndpoint != null)
        {
            otel.UseOtlpExporter();
        }

        return builder;
    }
}