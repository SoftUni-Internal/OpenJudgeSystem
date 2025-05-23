namespace OJS.Servers.Infrastructure.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System.Collections.Generic;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var serviceName = builder.Configuration["OTEL_SERVICE_NAME"];

        builder.Host.UseSerilog((hostingContext, configuration) => configuration
            .ReadFrom.Configuration(hostingContext.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Async(wt => wt.Console())
            .WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = otlpEndpoint;
                options.Protocol = OtlpProtocol.Grpc;

                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = serviceName,
                    ["deployment.environment"] = builder.Environment.EnvironmentName.ToLower(),
                };
            }));

        var otel = builder.Services.AddOpenTelemetry();

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
            otel.UseOtlpExporter();
        }

        return builder;
    }
}