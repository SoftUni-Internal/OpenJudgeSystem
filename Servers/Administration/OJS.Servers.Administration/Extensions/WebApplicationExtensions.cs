namespace OJS.Servers.Administration.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OJS.Data;
using OJS.Servers.Administration.Middleware;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Services.Administration.Business.Contests;
using Serilog;
using Serilog.Events;
using System;
using static OJS.Common.GlobalConstants;
using static Common.GlobalConstants.Roles;

internal static class WebApplicationExtensions
{
    public static WebApplication ConfigureWebApplication(this WebApplication app, IConfiguration configuration)
    {
        SetupRequestLoggingBehavior(app);

        app.UseCorsPolicy();
        app.UseDefaults();

        // Enable serving static files from wwwroot folder
        app.UseStaticFiles();

        app.UseMiddleware<AdministrationExceptionMiddleware>();
        app.MigrateDatabase<OjsDbContext>(configuration);

        app.SeedRoles();
        app.SeedSettings();
        app.SeedMentorPromptTemplates();

        app
            .MapHealthChecksUI()
            .RequireAuthorization(auth => auth.RequireRole(Administrator));

        // API endpoint for importing contests
        app.MapGet("/api/temp/ImportContestsFromCategory", async (
                IContestsImportBusinessService contestsImportBusinessService,
                HttpContext httpContext,
                int sourceContestCategoryId,
                int destinationContestCategoryId,
                bool dryRun = true) =>
            {
                await contestsImportBusinessService.StreamImportContestsFromCategory(
                    sourceContestCategoryId,
                    destinationContestCategoryId,
                    httpContext.Response,
                    dryRun);

                return Results.Empty;
            })
            .RequireAuthorization(auth => auth.RequireRole(Administrator))
            .WithRequestTimeout(TimeSpan.FromMinutes(10));

        // UI page for the contest import tool
        app.MapGet("/contest-import", () => Results.File("contest-import.html", MimeTypes.TextHtml))
            .RequireAuthorization(auth => auth.RequireRole(Administrator));

        return app
            .UseAndMapHangfireDashboard();
    }

    private static void SetupRequestLoggingBehavior(WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                var isSuccessfulHealthCheck = httpContext.Request.Path.StartsWithSegments("/api/health") &&
                                              httpContext.Response.StatusCode == StatusCodes.Status200OK;

                // Suppress successful health checks
                if (httpContext.Request.Path.StartsWithSegments("/healthchecks-ui")
                    || isSuccessfulHealthCheck)
                {
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

    private static WebApplication SeedSettings(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        scope.ServiceProvider
            .SeedSettings()
            .GetAwaiter()
            .GetResult();

        return app;
    }

    private static WebApplication SeedMentorPromptTemplates(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        scope.ServiceProvider
            .SeedMentorPromptTemplates()
            .GetAwaiter()
            .GetResult();

        return app;
    }
}
