using Microsoft.Extensions.Configuration;
using OJS.Services.Common.Models.Configurations;

namespace OJS.Servers.Ui.Infrastructure.Extensions;

using Microsoft.AspNetCore.Builder;
using OJS.Servers.Infrastructure.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder ConfigureBuilder<TProgram>(
        this WebApplicationBuilder builder,
        string apiVersion)
    {
        builder.Configuration.AddEnvironmentVariables($"{nameof(MessageQueueConfig)}_");
        builder.Services.ConfigureServices<TProgram>(builder.Configuration, apiVersion);
        builder.Host.UseFileLogger<TProgram>();

        return builder;
    }
}