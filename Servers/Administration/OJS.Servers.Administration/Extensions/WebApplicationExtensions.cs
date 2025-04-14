namespace OJS.Servers.Administration.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OJS.Common;
using OJS.Data;
using OJS.Servers.Administration.Middleware;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Services.Administration.Business.Contests;
using System;
using static OJS.Common.GlobalConstants.Roles;

internal static class WebApplicationExtensions
{
    public static WebApplication ConfigureWebApplication(this WebApplication app, IConfiguration configuration)
    {
        app.UseCorsPolicy();
        app.UseDefaults();

        app.UseMiddleware<AdministrationExceptionMiddleware>();
        app.MigrateDatabase<OjsDbContext>(configuration);

        app.SeedRoles();
        app.SeedSettings();
        app.SeedMentorPromptTemplates();

        app
            .MapHealthChecksUI()
            .RequireAuthorization(auth => auth.RequireRole(Administrator));

        app.MapGet("/api/temp/ImportContestsFromCategory", async (
                IContestsImportBusinessService contestsImportBusinessService,
                int sourceContestCategoryId,
                int destinationContestCategoryId,
                bool dryRun = true) =>
            {
                var result = await contestsImportBusinessService.ImportContestsFromCategory(
                    sourceContestCategoryId,
                    destinationContestCategoryId,
                    dryRun);

                return result.IsError
                    ? Results.BadRequest(result.Error)
                    : Results.Content(result.Data, GlobalConstants.MimeTypes.TextHtml);
            })
            .RequireAuthorization(auth => auth.RequireRole(Administrator))
            .WithRequestTimeout(TimeSpan.FromMinutes(10));

        return app
            .UseAndMapHangfireDashboard();
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
