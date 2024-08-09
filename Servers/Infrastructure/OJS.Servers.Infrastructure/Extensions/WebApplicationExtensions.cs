namespace OJS.Servers.Infrastructure.Extensions
{
    using AutoMapper;
    using Hangfire;
    using HealthChecks.UI.Client;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using OJS.Common;
    using OJS.Servers.Infrastructure.Filters;
    using OJS.Servers.Infrastructure.Middleware;
    using OJS.Services.Infrastructure;
    using static OJS.Common.GlobalConstants.Urls;
    using static OJS.Servers.Infrastructure.ServerConstants.Authorization;

    public static class WebApplicationExtensions
    {
        public static WebApplication UseDefaults(this WebApplication app)
        {
            app.UseCustomExceptionHandling();

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

        public static WebApplication UseCustomExceptionHandling(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(errorApp => errorApp.Run(new DevelopmentExceptionMiddleware().Get));
            }
            else
            {
                app.UseExceptionHandler(errorApp => errorApp.Run(new Rfc7807ExceptionMiddleware().Get));
            }

            return app;
        }

        public static void MapHealthMonitoring(this WebApplication app)
            => app
                .MapHealthChecks("/api/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                })
                .RequireAuthorization(ApiKeyPolicyName);

        public static WebApplication MigrateDatabase<TDbContext>(this WebApplication app)
            where TDbContext : DbContext
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

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
    }
}