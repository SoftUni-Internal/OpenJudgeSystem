using OJS.Services.Cache.Implementations;

namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using System.Linq;
    using MissingFeatures;
    using OJS.Services;
    using OJS.Services.Business.ExamGroups;
    using OJS.Services.Cache;
    using OJS.Services.Common;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Common.Emails;
    using OJS.Services.Common.HttpRequester;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Services.Data.Users;
    using SimpleInjector;
    using SimpleInjector.Packaging;
    using StackExchange.Redis;

    public class ServicesPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            this.RegisterCustomTypes(container);

            this.RegisterNonGenericTypes(container);

            this.RegisterRedisCache(container);
        }

        private void RegisterRedisCache(Container container)
        {
            var redisConfig = ConfigurationOptions.Parse(Settings.RedisConnectionString);
            redisConfig.AbortOnConnectFail = false;

            var redisConnection = ConnectionMultiplexer.Connect(redisConfig);
            var redisCache = redisConnection.GetDatabase();

            container.Register<IDatabase>(() => redisCache, Lifestyle.Singleton);
        }

        private void RegisterCustomTypes(Container container)
        {
            container.Register<IExamGroupsBusinessService>(
                () => new ExamGroupsBusinessService(
                    container.GetInstance<IExamGroupsDataService>(),
                    container.GetInstance<IUsersDataService>(),
                    container.GetInstance<IHttpRequesterService>(),
                    container.GetInstance<IHangfireBackgroundJobService>(),
                    Settings.SulsPlatformBaseUrl,
                    Settings.SulsApiKey),
                Lifestyle.Scoped);

            container.Register<IEmailSenderService>(
                () => new EmailSenderService(
                    Settings.EmailServerHost,
                    Settings.EmailServerPort,
                    Settings.EmailServerUsername,
                    Settings.EmailServerPassword,
                    Settings.EmailSenderEmail,
                    Settings.EmailSenderDisplayName),
                Lifestyle.Scoped);

#if DEBUG
            container.Register<ICacheService>(
                () => new MemoryCacheService(),
                Lifestyle.Scoped);
#else
             container.Register<IMemoryCacheService>(
                () => new MemoryCacheService(),
                Lifestyle.Scoped);
            
            container.Register<ICacheService>(
                () => new RedisCacheService(
                    container.GetInstance<IDatabase>(),
                    container.GetInstance<IEmailSenderService>(),
                    Settings.DevEmail,
                    container.GetInstance<IMemoryCacheService>()),
                Lifestyle.Scoped);
#endif
        }

        private void RegisterNonGenericTypes(Container container)
        {
            var serviceAssemblies = new[]
            {
                typeof(ISubmissionsForProcessingDataService).Assembly,
                typeof(IHangfireBackgroundJobService).Assembly
            };

            var registrations = serviceAssemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(type =>
                    typeof(IService).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsGenericTypeDefinition)
                .Select(type => new
                {
                    ServiceTypes = type
                        .GetInterfaces()
                        .Where(i =>
                            i.IsPublic &&
                            !i.GenericTypeArguments.Any() &&
                            i != typeof(IService)),
                    Implementation = type
                });

            foreach (var registration in registrations)
            {
                registration.ServiceTypes.ForEach(
                    service => container.Register(service, registration.Implementation, Lifestyle.Scoped));
            }
        }
    }
}