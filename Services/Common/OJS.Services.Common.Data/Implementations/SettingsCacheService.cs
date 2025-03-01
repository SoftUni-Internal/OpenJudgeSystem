namespace OJS.Services.Common.Data.Implementations;

using FluentExtensions.Extensions;
using OJS.Common.Enumerations;
using OJS.Data.Models;
using OJS.Services.Infrastructure.Cache;
using System;
using System.Globalization;
using System.Threading.Tasks;
using static OJS.Services.Infrastructure.Constants.CacheConstants;

public class SettingsCacheService(
    ICacheService cache,
    IDataService<Setting> settingsData)
    : ISettingsCacheService
{
    public async Task<T> GetRequiredValue<T>(string name)
    {
        var setting = await this.GetValueFromCache(name);

        return setting == null
            ? throw new ArgumentNullException(name)
            : ChangeType<T>(setting.Value, setting.Type);
    }

    public async Task<T> GetValue<T>(string name, T defaultValue)
    {
        var setting = await this.GetValueFromCache(name);

        return setting == null
            ? defaultValue
            : ChangeType<T>(setting.Value, setting.Type);
    }

    private async Task<Setting?> GetValueFromCache(string name)
        => await cache.Get(
            string.Format(CultureInfo.InvariantCulture, SettingsFormat, name),
            async () => await settingsData.One(s => s.Name == name));

    private static T ChangeType<T>(string value, SettingType settingType)
        => settingType == SettingType.Json
            ? value.FromJson<T>()
            : (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
}