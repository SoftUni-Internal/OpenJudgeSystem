using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using OJS.Common.Constants;
using OJS.Data;
using OJS.Data.Models;
using OJS.Services.Cache;

namespace OJS.Services.Data.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly IOjsDbContext dbContext;
        private readonly ICacheService cacheService;

        public SettingsService(IOjsDbContext dbContext, ICacheService cacheService)
        {
            this.dbContext = dbContext;
            this.cacheService = cacheService;
        }

        public T Get<T>(string name)
        {
            var value = cacheService.GetOrSet(name, () =>
            {
                var dbSetting = this.dbContext.Settings.FirstOrDefault(s => s.Name == name);
                if (dbSetting == null)
                {
                    throw new ArgumentNullException();
                }

                return dbSetting.Value;
            });

            return ConvertValue<T>(value);
        }

        public T Get<T>(string name, T defaultValue)
        {
            var value = cacheService.GetOrSet<T>(name, () =>
            {
                var dbSetting = this.dbContext.Settings.FirstOrDefault(s => s.Name == name);
                if (dbSetting == null)
                {
                    return defaultValue;
                }

                return ConvertValue<T>(dbSetting.Value);
            });

            return value;
        }

        public string AddOrUpdate(string name, string value)
        {
            var setting = this.dbContext.Settings.FirstOrDefault(s => s.Name == name);
            if (setting == null)
            {
                setting = new Setting
                {
                    Name = name,
                    Value = value
                };

                this.dbContext.Settings.Add(setting);
                this.AddToCache(name, value);
            }
            else
            {
                setting.Value = value;
                this.AddToCache(name, value);
            }

            this.dbContext.SaveChanges();
            return setting.Name;
        }

        public void Delete(string name)
        {
            var setting = this.dbContext.Settings.FirstOrDefault(s => s.Name == name);
            if (setting == null)
            {
                throw new InvalidOperationException("Setting not found");
            }

            this.dbContext.Settings.Remove(setting);
            this.RemoveFromCache(setting.Name);
            this.dbContext.SaveChanges();
        }

        private static T ConvertValue<T>(string value)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception e)
            {
                throw new InvalidCastException($"Unable to convert type {typeof(string)} to {typeof(T)}");
            }
        }

        private void AddToCache(string name, string value)
        {
            this.RemoveFromCache(name);

            cacheService.Set(name, value);
        }

        private void RemoveFromCache(string name)
        {
            if (this.cacheService.ContainsKey(name))
            {
                cacheService.Remove(name);
            }
        }
    }
}