namespace OJS.Servers.Ui
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using OJS.Data;
    using OJS.Data.Models.Users;
    using OJS.Servers.Infrastructure.Extensions;
    using OJS.Services.Common.Data;
    using OJS.Services.Common.Data.Implementations;
    using OJS.Services.Infrastructure.Configurations;
    using static OJS.Common.GlobalConstants;
    using static OJS.Common.GlobalConstants.BackgroundJobs;

    internal class Program
    {
        private const string ApiVersion = "v1";
        private const string AppName = "Ui";
        private const string ApiDocsTitle = $"{ApplicationFullName} {AppName} Api";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            var configuration = builder.Configuration;
            var environment = builder.Environment;
            var host = builder.Host;

            services
                .AddWebServer<Program>(configuration, environment)
                .AddHttpClients(configuration)
                .AddSwaggerDocs(ApiVersion.ToApiName(), ApiDocsTitle, ApiVersion)
                .AddHangfireServer(configuration, AppName, [UiQueueName])
                .ConfigureCorsPolicy(configuration)
                .AddMessageQueue<Program>(configuration)
                .AddTransient(typeof(IDataService<>), typeof(DataService<>))
                .AddTransient<ITransactionsProvider, TransactionsProvider<OjsDbContext>>()
                .AddIdentityDatabase<OjsDbContext, UserProfile, Role, UserInRole>(configuration)
                .AddResiliencePipelines()
                .AddOpenAiClient(configuration)
                .AddMemoryCache()
                .AddDistributedCaching(configuration)
                .AddOptionsWithValidation<ApplicationConfig>()
                .AddOptionsWithValidation<ApplicationUrlsConfig>()
                .AddOptionsWithValidation<EmailServiceConfig>()
                .AddOptionsWithValidation<MentorConfig>()
                .AddOptionsWithValidation<SvnConfig>()
                .AddControllers();

            host.UseLogger(environment);

            var app = builder.Build();

            app.UseCorsPolicy();
            app.UseDefaults();

            app.MigrateDatabase<OjsDbContext>(configuration);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwaggerDocs(ApiVersion.ToApiName());
            }

            app.Run();
        }
    }
}