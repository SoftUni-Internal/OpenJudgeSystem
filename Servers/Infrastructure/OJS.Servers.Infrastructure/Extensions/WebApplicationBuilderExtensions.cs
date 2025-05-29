namespace OJS.Servers.Infrastructure.Extensions;

using MassTransit.Logging;
using MassTransit.Monitoring;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System.Globalization;
using OJS.Servers.Infrastructure.Telemetry;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

        // Configure logging
        builder.Host.UseSerilog((hostingContext, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .WriteTo.Async(wt => wt.Console(formatProvider: CultureInfo.InvariantCulture));

            if (otlpEndpoint != null)
            {
                configuration.WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.Protocol = OtlpProtocol.Grpc;
                });
            }
        });

        // Configure metrics and tracing
        var otel = builder.Services.AddOpenTelemetry();

        otel.ConfigureResource(resource =>
            resource
                .AddTelemetrySdk()
                .AddEnvironmentVariableDetector());

        otel.WithMetrics(metrics =>
        {
            metrics
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(InstrumentationOptions.MeterName); // For MassTransit
        });

        otel.WithTracing(tracing =>
        {
            if (builder.Environment.IsDevelopment())
            {
                tracing.SetSampler<AlwaysOnSampler>();
            }

            tracing
                .AddSource(DiagnosticHeaders.DefaultListenerName); // For MassTransit

            // Add all OJS ActivitySources
            foreach (var sourceName in OjsActivitySources.AllSourceNames)
            {
                tracing.AddSource(sourceName);
            }

            tracing
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                })
                .AddEntityFrameworkCoreInstrumentation();
        });

        if (otlpEndpoint != null)
        {
            // UseOtlpExporter() will use the endpoint from the OTEL_EXPORTER_OTLP_ENDPOINT environment variable
            otel.UseOtlpExporter();
        }

        return builder;
    }
}