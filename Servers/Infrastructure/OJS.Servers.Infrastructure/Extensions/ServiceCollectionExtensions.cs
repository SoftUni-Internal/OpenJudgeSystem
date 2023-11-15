namespace OJS.Servers.Infrastructure.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Reflection;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.SqlServer;
    using MassTransit;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Net.Http.Headers;
    using Microsoft.OpenApi.Models;
    using OJS.Common.Enumerations;
    using OJS.Services.Common;
    using OJS.Services.Common.Data;
    using OJS.Services.Common.Data.Implementations;
    using OJS.Services.Common.Implementations;
    using OJS.Services.Common.Models.Configurations;
    using OJS.Services.Infrastructure.Cache;
    using OJS.Services.Infrastructure.Cache.Implementations;
    using OJS.Services.Infrastructure.HttpClients;
    using OJS.Services.Infrastructure.HttpClients.Implementations;
    using OJS.Workers.SubmissionProcessors.Formatters;
    using SoftUni.AutoMapper.Infrastructure.Extensions;
    using SoftUni.Data.Infrastructure.Enumerations;
    using SoftUni.Data.Infrastructure.Extensions;
    using SoftUni.Services.Infrastructure.Extensions;
    using StackExchange.Redis;
    using static OJS.Common.GlobalConstants;
    using static OJS.Common.GlobalConstants.FileExtensions;

    public static class ServiceCollectionExtensions
    {
        private const string DefaultDbConnectionName = "DefaultConnection";

        public static IServiceCollection AddWebServer<TStartup>(this IServiceCollection services)
            => services
                .AddAutoMapperConfigurations<TStartup>()
                .AddWebServerServices<TStartup>()
                .AddOptionsWithValidation<ApplicationConfig>()
                .AddOptionsWithValidation<HealthCheckConfig>();

        /// <summary>
        /// Adds identity database and authentication services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="globalQueryFilterTypes">
        /// The global query filter types to add to the context.
        /// <br/> If null, adds all default global query filters,
        /// whereas if explicitly providing an empty collection, will not add any global query filters to the context.
        /// </param>
        /// <remarks>
        /// <see cref="globalQueryFilterTypes"/> can be null, in which case all default global query filters are added. (i.e DeletableEntityFilter)
        /// <br/><see cref="globalQueryFilterTypes"/> can be explicitly empty, in which case no global query filters are added.
        /// </remarks>
        public static IServiceCollection AddIdentityDatabase<TDbContext, TIdentityUser, TIdentityRole,
            TIdentityUserRole>(
            this IServiceCollection services,
            IConfiguration configuration,
            IEnumerable<GlobalQueryFilterType>? globalQueryFilterTypes = null)
            where TDbContext : DbContext, IDataProtectionKeyContext
            where TIdentityUser : IdentityUser
            where TIdentityRole : IdentityRole
            where TIdentityUserRole : IdentityUserRole<string>, new()
        {
            services
                .AddDbContext<TDbContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString(DefaultDbConnectionName));
                    // TODO: refactor app to not use lazy loading globally and make navigational properties non virtual
                    options.UseLazyLoadingProxies();
                })
                .AddGlobalQueryFilterTypes(globalQueryFilterTypes)
                .AddTransactionsProvider<TDbContext>();

            services
                .AddIdentity<TIdentityUser, TIdentityRole>()
                .AddRoles<TIdentityRole>()
                .AddEntityFrameworkStores<TDbContext>()
                .AddUserStore<UserStore<
                    TIdentityUser,
                    TIdentityRole,
                    TDbContext,
                    string,
                    IdentityUserClaim<string>,
                    TIdentityUserRole,
                    IdentityUserLogin<string>,
                    IdentityUserToken<string>,
                    IdentityRoleClaim<string>>>();

            var sharedAuthCookieDomain = configuration
                .GetSectionValueWithValidation<ApplicationConfig, string>(
                    nameof(ApplicationConfig.SharedAuthCookieDomain));

            services
                .ConfigureApplicationCookie(opt =>
                {
                    opt.Cookie.Domain = sharedAuthCookieDomain;
                    opt.Events.OnRedirectToAccessDenied = UnAuthorizedResponse;
                    opt.Events.OnRedirectToLogin = UnAuthorizedResponse;
                });

            // By default the data protection API that encrypts the authentication cookie generates a unique key for each application,
            // but in order to use/decrypt the same cookie across multiple servers, we need to use the same encryption key.
            // By setting custom data protection, we can use the same key in each server configured with the same application name.
            services
                .AddDataProtection()
                .PersistKeysToDbContext<TDbContext>()
                .SetApplicationName(ApplicationFullName);

            return services;
        }

        public static IServiceCollection AddHangfireServer(
            this IServiceCollection services,
            IConfiguration configuration,
            ApplicationName app)
        {
            var connectionString = configuration.GetConnectionString(DefaultDbConnectionName);

            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true,
                }));

            services.AddHangfireServer(options =>
            {
                options.ServerName = app.ToString();
                options.Queues = new[] { app.ToString().ToLowerInvariant() };
            });

            services.AddHostedService<BackgroundJobsHostedService>();

            return services;
        }

        public static IServiceCollection AddSwaggerDocs(
            this IServiceCollection services,
            string name,
            string title,
            string version)
            => services
                .AddSwaggerGen(options =>
                {
                    options.SwaggerDoc(name, new OpenApiInfo { Title = title, Version = version, });

                    options.MapType<FileContentResult>(() => new OpenApiSchema { Type = "file", });

                    var entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly != null)
                    {
                        var xmlFilename = $"{entryAssembly.GetName().Name}{Xml}";
                        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                    }
                });

        public static IServiceCollection AddDistributedCaching(
            this IServiceCollection services,
            IConfiguration configuration)
            => services.AddRedis(configuration, ApplicationFullName);

        public static IServiceCollection AddDistributedCaching(
            this IServiceCollection services,
            IConfiguration configuration,
            string instanceName)
            => services.AddRedis(configuration, instanceName);

        public static IServiceCollection AddMessageQueue<TStartup>(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddTransient<IPublisherService, PublisherService>();

            var consumers = typeof(TStartup).Assembly
                .GetExportedTypes()
                .Where(t => typeof(IConsumer).IsAssignableFrom(t))
                .ToList();

            var messageQueueConfig = configuration.GetSectionWithValidation<MessageQueueConfig>();

            services.AddMassTransit(config =>
            {
                consumers.ForEach(consumer => config
                    .AddConsumer(consumer)
                    .Endpoint(endpointConfig =>
                    {
                        endpointConfig.Name = consumer.Name;
                        if (endpointConfig is IRabbitMqReceiveEndpointConfigurator configurator)
                        {
                            configurator.Durable = true;
                        }
                    }));

                config.UsingRabbitMq((context, rmq) =>
                {
                    rmq.Host(messageQueueConfig.Host, messageQueueConfig.VirtualHost, h =>
                    {
                        h.Username(messageQueueConfig.User);
                        h.Password(messageQueueConfig.Password);
                    });

                    consumers.ForEach(consumer => rmq.ReceiveEndpoint(consumer.FullName!, endpoint =>
                    {
                        if (messageQueueConfig.PrefetchCount.HasValue)
                        {
                            endpoint.PrefetchCount = messageQueueConfig.PrefetchCount.Value;
                        }

                        endpoint.UseMessageRetry(retry =>
                            retry.Interval(messageQueueConfig.RetryCount, messageQueueConfig.RetryInterval));

                        endpoint.ConfigureConsumer(context, consumer);
                    }));
                });
            });

            return services;
        }

        public static IServiceCollection AddSoftUniJudgeCommonServices(this IServiceCollection services)
        {
            services.AddFrom(typeof(IFormatterServiceFactory).Assembly);

            return services;
        }

        public static IServiceCollection AddHttpContextServices(this IServiceCollection services)
            => services
                .AddHttpContextAccessor()
                .AddTransient(s =>
                    s.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ?? new ClaimsPrincipal());

        public static IServiceCollection AddOptionsWithValidation<T>(this IServiceCollection services)
            where T : BaseConfig
            => services
                .AddOptions<T>()
                .BindConfiguration(Activator.CreateInstance<T>().SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart()
                .Services;

        private static IServiceCollection AddWebServerServices<TStartUp>(this IServiceCollection services)
        {
            services
                .AddConventionServices<TStartUp>()
                .AddTransient(typeof(IDataService<>), typeof(DataService<>));

            services.AddHttpClient<IHttpClientService, HttpClientService>(ConfigureHttpClient);
            services.AddHttpClient<ISulsPlatformHttpClientService, SulsPlatformHttpClientService>(ConfigureHttpClient);

            return services;
        }

        private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration, string instanceName)
        {
            var redisConfig = configuration.GetSectionWithValidation<RedisConfig>();

            var redisConnection = ConnectionMultiplexer.Connect(redisConfig.ConnectionString);

            services.AddSingleton<IConnectionMultiplexer>(redisConnection);
            services.AddSingleton<ICacheService, CacheService>();

            return services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConfig.ConnectionString;
                options.InstanceName = $"{instanceName}:";
            });
        }

        private static void ConfigureHttpClient(HttpClient client)
            => client.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);

        private static Task UnAuthorizedResponse(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return Task.CompletedTask;
        }
    }
}