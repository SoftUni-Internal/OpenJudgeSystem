namespace OJS.Servers.Infrastructure.Extensions
{
    using AutoMapper;
    using Hangfire;
    using HealthChecks.UI.Client;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using OJS.Common;
    using OJS.Servers.Infrastructure.Filters;
    using OJS.Services.Infrastructure;
    using Serilog;
    using Serilog.Events;
    using System;
    using static OJS.Common.GlobalConstants.Urls;
    using static OJS.Servers.Infrastructure.ServerConstants.Authorization;

    public static class WebApplicationExtensions
    {
        public static WebApplication UseDefaults(this WebApplication app)
        {
            SetupRequestLoggingBehavior(app);

            // Exception is handled in the exception handler, configured in services.
            // Passing empty lambda as a workaround suggested here: https://github.com/dotnet/aspnetcore/issues/51888
            app.UseExceptionHandler(_ => { });

            app.UseAutoMapper();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapHealthMonitoring();
            app.MapControllers();

            return app;
        }

        public static WebApplication UseAndMapHangfireDashboard(this WebApplication app)
        {
            var dashboardOptions = new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
            };

            app.UseHangfireDashboard(HangfirePath, dashboardOptions);

            return app;
        }

        public static WebApplication UseCorsPolicy(this WebApplication app)
        {
            app.UseCors(GlobalConstants.CorsDefaultPolicyName);
            return app;
        }

        public static WebApplication UseSwaggerDocs(this WebApplication app, string name)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{name}/swagger.json", name);
            });

            return app;
        }

        public static void MapHealthMonitoring(this WebApplication app)
            => app
                .MapHealthChecks("/api/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                })
                .RequireAuthorization(ApiKeyPolicyName);

        public static WebApplication MigrateDatabase<TDbContext>(this WebApplication app, IConfiguration configuration)
            where TDbContext : DbContext
        {
            var timeout = configuration.GetValue("MigrationsDbTimeoutInSeconds", 60);

            using var scope = app.Services.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            dbContext.Database.SetCommandTimeout(TimeSpan.FromSeconds(timeout));
            dbContext.Database.Migrate();

            return app;
        }

        public static WebApplication SeedRoles(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.CreateOrUpdateRoles().GetAwaiter().GetResult();

            return app;
        }

        public static IApplicationBuilder UseAutoMapper(this IApplicationBuilder app)
        {
            var services = app.ApplicationServices;

            var mapper = services.GetRequiredService<IMapper>();

            AutoMapperSingleton.Init(mapper);

            return app;
        }

        private static void SetupRequestLoggingBehavior(WebApplication app)
        {
            app.UseSerilogRequestLogging(options =>
            {
                options.GetLevel = (httpContext, elapsed, ex) =>
                {
                    var isSuccessfulHealthCheck = httpContext.Request.Path.StartsWithSegments("/api/health") &&
                                                  httpContext.Response.StatusCode == StatusCodes.Status200OK;

                    if (isSuccessfulHealthCheck)
                    {
                        // Suppress successful health checks
                        return LogEventLevel.Debug;
                    }

                    if (TimeSpan.FromMilliseconds(elapsed) > TimeSpan.FromSeconds(1))
                    {
                        return LogEventLevel.Warning;
                    }

                    return ex != null || httpContext.Response.StatusCode >= 500
                        ? LogEventLevel.Error
                        : LogEventLevel.Information;
                };
            });
        }
    }
}