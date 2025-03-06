namespace OJS.Servers.Administration.Extensions;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OJS.Common;
using OJS.Common.Exceptions;
using OJS.Data;
using OJS.Data.Models.Users;
using OJS.Servers.Administration.Consumers;
using OJS.Servers.Administration.Middleware;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Services.Administration.Business.Contests.Validators;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Data.Implementations;
using OJS.Services.Common.Data;
using OJS.Services.Common.Data.Implementations;
using OJS.Services.Common.Validation;
using OJS.Services.Infrastructure.Configurations;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using static OJS.Common.GlobalConstants.BackgroundJobs;
using ApplicationConfig = OJS.Services.Administration.Models.ApplicationConfig;

internal static class ServiceCollectionExtensions
{
    private const string AppName = "Administration";

    public static void ConfigureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
        => services
            .AddGridServices()
            .AddValidators()
            .AddWebServer<Program>(configuration)
            .AddHttpClients(configuration)
            .AddTransient(typeof(IDataService<>), typeof(AdministrationDataService<>))
            .AddTransient<ITransactionsProvider, TransactionsProvider<OjsDbContext>>()
            .AddTransient<AdministrationExceptionMiddleware>()
            .AddHangfireServer(configuration, AppName, [AdministrationQueueName])
            .AddMessageQueue<Program>(configuration)
            .ConfigureGlobalDateFormat()
            .ConfigureCorsPolicy(configuration)
            .AddIdentityDatabase<OjsDbContext, UserProfile, Role, UserInRole>(configuration)
            .AddResiliencePipelines()
            .AddMemoryCache()
            .AddDistributedCaching(configuration)
            .AddOptionsWithValidation<ApplicationConfig>()
            .AddOptionsWithValidation<ApplicationUrlsConfig>()
            .AddOptionsWithValidation<EmailServiceConfig>()
            .AddHostedService<ContestLimitBetweenSubmissionsHostedService>()
            .AddHealthChecksDashboard(environment, configuration)
            .AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errorsList = new List<ExceptionResponseModel>();
                    var modelStateErrors = actionContext.ModelState
                        .Where(ms => ms.Value!.Errors.Count > 0).ToList();
                    foreach (var modelStateError in modelStateErrors)
                    {
                        var error = new ExceptionResponseModel
                        {
                            Name = modelStateError.Key,
                            Message =
                                $@"Error in {modelStateError.Key.Replace("$.", string.Empty) // Remove "$."
                                    .Replace("[", string.Empty)
                                    .Replace("]", string.Empty)
                                    .Replace(".", " > ")
                                    .Trim()}: {modelStateError.Value!.Errors[0].ErrorMessage}",
                        };
                        errorsList.Add(error);
                    }

                    return new BadRequestObjectResult(errorsList);
                };
            })
            .AddJsonOptions(jo => jo.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

    private static IServiceCollection AddValidators(this IServiceCollection services)
        => services
            .AddValidatorsFromAssemblyContaining(typeof(BaseValidator<>))
            .AddValidatorsFromAssemblyContaining<ContestAdministrationModelValidator>();

    private static IServiceCollection AddGridServices(this IServiceCollection services)
        => services
            .AddTransient(typeof(IGridDataService<>), typeof(GridDataService<>));

    private static IServiceCollection AddHealthChecksDashboard(
        this IServiceCollection services,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        services
            .AddHealthChecksUI(settings =>
            {
                if (!environment.IsDevelopment())
                {
                    var microsoftTeamsWebhookUri = configuration
                        .GetSectionValueWithValidation<ApplicationConfig, string>(nameof(ApplicationConfig.MicrosoftTeamsWebhookUri));
                    var payload = GetHealthCheckPayload(environment, configuration);
                    var restorePayload = GetRestorePayload(environment);

                    settings
                        .AddWebhookNotification("Microsoft Teams", microsoftTeamsWebhookUri, payload, restorePayload)
                        .SetNotifyUnHealthyOneTimeUntilChange(); // Notify once until status changes.
                }

                settings
                    .MaximumHistoryEntriesPerEndpoint(100) // Keep only 100 entries in history.
                    .ConfigureApiEndpointHttpclient((sp, client) =>
                    {
                        var apiKey = sp.GetRequiredService<IOptions<ApplicationConfig>>().Value.ApiKey;
                        client.DefaultRequestHeaders.Add(GlobalConstants.HeaderKeys.ApiKey, apiKey);
                    });
            })
            .AddInMemoryStorage();

        return services;
    }

    private static string GetRestorePayload(IHostEnvironment environment)
        => $$"""
        {
            "attachments": [
                 {
                   "contentType": "application/vnd.microsoft.card.adaptive",
                   "content": {
                     "type": "AdaptiveCard",
                     "body": [
                       {
                         "type": "TextBlock",
                         "text": "[[LIVENESS]] has been restored on {{environment.EnvironmentName}}!"
                       }
                     ],
                         "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                         "version": "1.0"
                       }
                }
            ]
        }
        """;

    private static string GetHealthCheckPayload(IHostEnvironment environment, IConfiguration configuration)
    {
        var administrationUrl = configuration.GetSectionValueWithValidation<ApplicationUrlsConfig, string>(nameof(ApplicationUrlsConfig.AdministrationUrl));

        return $$"""
                 {
                   "attachments": [
                     {
                       "contentType": "application/vnd.microsoft.card.adaptive",
                       "content": {
                         "type": "AdaptiveCard",
                         "body": [
                           {
                             "type": "TextBlock",
                             "text": "[[LIVENESS]] has failed on {{environment.EnvironmentName}}!"
                           },
                           {
                             "type": "TextBlock",
                             "text": "[[FAILURE]]"
                           },
                           {
                             "type": "TextBlock",
                             "text": "[[DESCRIPTIONS]]"
                           }
                         ],
                         "actions": [
                           {
                             "type": "Action.OpenUrl",
                             "title": "Check health status",
                             "url": "{{administrationUrl}}/healthchecks-ui"
                           }
                         ],
                         "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                         "version": "1.0"
                       }
                     }
                   ]
                 }
                 """;
    }
}