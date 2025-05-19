namespace OJS.Servers.Infrastructure.Extensions;

using Microsoft.Extensions.Hosting;
using OJS.Services.Infrastructure.Configurations;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System.Collections.Generic;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseLogger(this IHostBuilder builder, IHostEnvironment environment)
        => builder.UseSerilog((hostingContext, configuration) =>
        {
            var applicationName = environment.GetShortApplicationName();
            var appSettings = hostingContext.Configuration.GetSectionWithValidation<ApplicationConfig>();

            configuration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Async(wt => wt.Console())
                .WriteTo.OpenTelemetry(options =>
                {
                    options.Endpoint = appSettings.OtlpCollectorBaseUrl;
                    options.Protocol = OtlpProtocol.Grpc;
                    options.ResourceAttributes = new Dictionary<string, object>
                    {
                        ["service.name"] = applicationName.ToLower(),
                        ["deployment.environment"] = environment.EnvironmentName.ToLower(),
                    };
                });
        });
}