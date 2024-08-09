namespace OJS.Servers.Administration.Extensions;

using Microsoft.AspNetCore.Builder;
using OJS.Data;
using OJS.Servers.Administration.Middleware;
using OJS.Servers.Infrastructure.Extensions;
using static OJS.Common.GlobalConstants.Roles;

internal static class WebApplicationExtensions
{
    public static WebApplication ConfigureWebApplication(this WebApplication app)
    {
        app.UseCorsPolicy();
        app.UseDefaults();

        app.UseMiddleware<AdministrationExceptionMiddleware>();
        app.MigrateDatabase<OjsDbContext>();
        app.SeedRoles();

        app
            .MapHealthChecksUI()
            .RequireAuthorization(auth => auth.RequireRole(Administrator));

        return app
            .UseAndMapHangfireDashboard();
    }
}
