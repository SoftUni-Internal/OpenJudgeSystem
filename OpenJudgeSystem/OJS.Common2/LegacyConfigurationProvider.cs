namespace OJS.Common2
{
    using System.Configuration;
    using Microsoft.Extensions.Configuration;

    public class LegacyConfigurationProvider : ConfigurationProvider, IConfigurationSource
    {
        public static bool HasSettings() => ConfigurationManager.AppSettings.HasKeys();

        public override void Load()
        {
            foreach (ConnectionStringSettings connectionString in ConfigurationManager.ConnectionStrings)
            {
                this.Data.Add($"ConnectionStrings:{connectionString.Name}", connectionString.ConnectionString);
            }

            foreach (var settingKey in ConfigurationManager.AppSettings.AllKeys)
            {
                this.Data.Add(settingKey, ConfigurationManager.AppSettings[settingKey]);
            }
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => this;
    }
}
