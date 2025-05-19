namespace OJS.Servers.Worker;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Services.Infrastructure.Configurations;
using OJS.Services.Worker.Business.Implementations;
using OJS.Services.Worker.Models.Configuration;
using OJS.Workers.Compilers;
using OJS.Workers.ExecutionStrategies;
using ApplicationConfig = OJS.Services.Worker.Models.Configuration.ApplicationConfig;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        var configuration = builder.Configuration;
        var environment = builder.Environment;
        var host = builder.Host;

        services
            .AddWebServer<Program>(configuration, environment)
            .AddSingleton<ICompilerFactory, CompilerFactory>()
            .AddSingleton<IExecutionStrategySettingsProvider, ExecutionStrategySettingsProvider>()
            .AddMemoryCache();

        if (configuration.GetSectionValueWithValidation<ApplicationConfig, bool>(nameof(ApplicationConfig.UseMessageQueue)))
        {
            services.AddMessageQueue<Program>(configuration);
        }

        services
            .AddOptionsWithValidation<ApplicationConfig>()
            .AddOptionsWithValidation<SubmissionExecutionConfig>()
            .AddOptionsWithValidation<OjsWorkersConfig>()
            .AddControllers();

        host.UseLogger(builder.Environment);

        var app = builder.Build();

        app.UseDefaults();

        var healthCheckConfig = app.Services.GetRequiredService<IOptions<HealthCheckConfig>>().Value;

        // Health check endpoint with password protection for external services (like Legacy Judge)
        app.MapWhen(
            httpContext =>
                httpContext.Request.Query.TryGetValue(healthCheckConfig.Key, out var healthPassword)
                && healthPassword == healthCheckConfig.Password,
            appBuilder => appBuilder.UseHealthChecks("/health", new HealthCheckOptions
            {
                // Exclude all checks from the response, just validate the service is up
                Predicate = _ => false,
            }));

        app.Run();
    }
}
