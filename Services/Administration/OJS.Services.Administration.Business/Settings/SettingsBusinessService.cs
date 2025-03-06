namespace OJS.Services.Administration.Business.Settings;

using Microsoft.EntityFrameworkCore;
using OJS.Data.Models;
using OJS.Services.Administration.Models.Settings;
using OJS.Services.Common.Data;
using OJS.Services.Infrastructure.Cache;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.Extensions;
using System.Globalization;
using System.Threading.Tasks;

public class SettingsBusinessService(
    IDataService<Setting> settingsService,
    ICacheService cache)
    : AdministrationOperationService<Setting, int, SettingAdministrationModel>, ISettingsBusinessService
{
    public override async Task<SettingAdministrationModel> Get(int id)
        => await settingsService.GetByIdQuery(id).FirstAsync().Map<SettingAdministrationModel>();

    public override async Task<SettingAdministrationModel> Create(SettingAdministrationModel model)
    {
        var setting = model.Map<Setting>();

        await settingsService.Add(setting);
        await settingsService.SaveChanges();

        return model;
    }

    public override async Task<SettingAdministrationModel> Edit(SettingAdministrationModel model)
    {
        var setting = await settingsService.GetByIdQuery(model.Id).FirstAsync();

        setting = setting.MapFrom(model);

        settingsService.Update(setting);
        await settingsService.SaveChanges();

        await cache.Remove(GetCacheKey(setting));

        return model;
    }

    public override async Task Delete(int id)
    {
        var setting = await settingsService.Find(id);
        if (setting == null)
        {
            return;
        }

        settingsService.Delete(setting);
        await settingsService.SaveChanges();
        await cache.Remove(GetCacheKey(setting));
    }

    public Task<SettingAdministrationModel> GetByKey(string settingKey)
        => settingsService
            .GetQuery(s => s.Name == settingKey)
            .FirstOrDefaultAsync()
            .Map<SettingAdministrationModel>();

    private static string GetCacheKey(Setting setting)
        => string.Format(CultureInfo.InvariantCulture, CacheConstants.SettingsFormat, setting.Name);
}