namespace OJS.Common2.Helpers
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using OJS.Workers.Common;
    using static OJS.Workers.Common.Constants;

    public static class SettingsHelper
    {
        private static readonly IConfiguration Configuration;

        static SettingsHelper() => Configuration = BuildConfiguration();

        public static string GetSetting(string settingName)
            => GetSection(settingName)?.Value
                ?? throw new Exception($"{settingName} setting not found in Config file!");

        public static T GetSettingOrDefault<T>(string settingName, T defaultValue)
        {
            var section = GetSection(settingName);

            return section?.Value == null
                ? defaultValue
                : (T)Convert.ChangeType(section.Value, typeof(T));
        }

        private static IConfigurationSection GetSection(string settingName)
        {
            var section = Configuration.GetSection($"OjsWorkersConfig:{settingName}");

            if (LegacyConfigurationProvider.HasSettings())
            {
                section = Configuration.GetSection($"{settingName}");
            }

            return section;
        }

        private static IConfiguration BuildConfiguration()
        {
            var env = Environment.GetEnvironmentVariable(AspNetCoreEnvironmentVariable);

            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings{JsonFileExtension}", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}{JsonFileExtension}", optional: true, reloadOnChange: true)
                .Add(new LegacyConfigurationProvider())
                .Build();
        }
    }
}
