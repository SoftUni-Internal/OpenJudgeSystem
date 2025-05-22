namespace OJS.Servers.Infrastructure.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OJS.Services.Infrastructure.Configurations;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System;
using System.Collections.Generic;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureOpenTelemetry(this WebApplicationBuilder builder)
    {
        var environment = builder.Environment;
        var environmentName = environment.EnvironmentName.ToLower();
        var applicationName = environment.GetShortApplicationName().ToLower();
        var otlpCollectorBaseUrl = builder.Configuration.GetSectionWithValidation<ApplicationConfig>().OtlpCollectorBaseUrl;
        var otlpCollectorEndpoint = otlpCollectorBaseUrl != null
            ? new Uri(otlpCollectorBaseUrl)
            : null;
        var hostName = Environment.GetEnvironmentVariable(Workers.Common.Constants.HostIpEnvironmentVariable)
                       ?? Environment.MachineName;

        builder.Host.UseSerilog((hostingContext, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Async(wt => wt.Console())
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = otlpCollectorBaseUrl;
                    options.Protocol = OtlpProtocol.Grpc;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = applicationName,
                        ["deployment.environment"] = environmentName,
                        ["host.name"] = hostName,
                    };
                });
        });

        var otel = builder.Services.AddOpenTelemetry();

        otel.ConfigureResource(resource => resource
            .AddService(serviceName: applicationName)
            .AddTelemetrySdk()
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment.name"] = environmentName,
                ["host.name"] = hostName,
            }));

        otel.WithMetrics(metrics =>
        {
            metrics
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter("System.Net.Http")
                .AddMeter("System.Net.NameResolution");

            metrics.AddConsoleExporter();

            if (otlpCollectorEndpoint != null)
            {
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = otlpCollectorEndpoint;
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
            }
            else
            {
                metrics.AddConsoleExporter();
            }
        });

        otel.WithTracing(tracing =>
        {
            if (environment.IsDevelopment())
            {
                // In development, always sample traces, so we can see them in the console
                tracing.SetSampler<AlwaysOnSampler>();
            }

            tracing
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddEntityFrameworkCoreInstrumentation();

            tracing.AddConsoleExporter();

            if (otlpCollectorEndpoint != null)
            {
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = otlpCollectorEndpoint;
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
            }
            else
            {
                tracing.AddConsoleExporter();
            }
        });

        return builder;
    }
}