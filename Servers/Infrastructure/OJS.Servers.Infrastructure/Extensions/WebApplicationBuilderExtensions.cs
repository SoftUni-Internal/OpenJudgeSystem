namespace OJS.Servers.Infrastructure.Extensions;

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

        if (otlpEndpoint != null)
        {
            // UseOtlpExporter() will use the endpoint from the OTEL_EXPORTER_OTLP_ENDPOINT environment variable
            otel.UseOtlpExporter();
        }

        return builder;
    }
}