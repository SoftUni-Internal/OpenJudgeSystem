using System.Collections;
using System.Collections.Generic;

namespace OJS.Services.Data.Settings
{
    using Common;

    public interface ISettingsService : IService
    {
        T Get<T>(string name);
        
        T Get<T>(string name, T defaultValue);

        string AddOrUpdate(string name, string value);

        void Delete(string name);
    }
}