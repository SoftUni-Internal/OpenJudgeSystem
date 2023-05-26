namespace OJS.Servers.Ui.Infrastructure.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using OJS.Common.Enumerations;
    using OJS.Common.Utils;
    using OJS.Data;
    using OJS.Data.Models.Users;
    using OJS.Servers.Infrastructure.Extensions;
    using OJS.Services.Common.Models.Configurations;
    using SoftUni.Judge.Common.Extensions;
    using static OJS.Common.GlobalConstants;

    public static class ServiceCollectionExtensions
    {
        private const ApplicationName AppName = ApplicationName.Ui;

        private static readonly string ApiDocsTitle = $"{ApplicationFullName} {AppName} Api";

        private static readonly string[] RequiredConfigValues =
        {
            EnvironmentVariables.UiHomeYouTubeVideoId,
        };

        public static void ConfigureServices<TProgram>(
            this IServiceCollection services,
            IConfiguration configuration,
            string apiVersion)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                services
                    .AddSpaStaticFiles(cnfg => { cnfg.RootPath = "ClientApp/dist"; });
            }

            services
                .AddWebServer<TProgram>()
                .AddSwaggerDocs(apiVersion.ToApiName(), ApiDocsTitle, apiVersion)
                .AddHangfireServer(AppName)
                .AddIdentityDatabase<OjsDbContext, UserProfile, Role, UserInRole>()
                .AddMemoryCache()
                .AddSoftUniJudgeCommonServices()
                .AddLogging()
                .ConfigureSettings(configuration)
                .AddControllersWithViews();
        }

        private static IServiceCollection ConfigureSettings(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            EnvironmentUtils.ValidateEnvironmentVariableExists(RequiredConfigValues);

            services
                .Configure<DistributorConfig>(configuration.GetSection(nameof(DistributorConfig)));

            return services;
        }
    }
}