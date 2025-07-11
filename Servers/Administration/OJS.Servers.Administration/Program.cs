namespace OJS.Servers.Administration;

using Microsoft.AspNetCore.Builder;
using OJS.Data;
using OJS.Servers.Administration.Extensions;
using OJS.Servers.Infrastructure.Extensions;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

        builder.ConfigureOpenTelemetry();

        var app = builder.Build();

        app.MigrateDatabase<ArchivesDbContext>(builder.Configuration);

        app.ConfigureWebApplication(builder.Configuration);
        app.Run();
    }
}
