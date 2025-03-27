namespace OJS.Services.Common.Data;

using OJS.Services.Infrastructure;
using System.Threading.Tasks;

public interface ISettingsCacheService : IService
{
    Task<T> GetRequiredValue<T>(string name);

    Task<T> GetValue<T>(string name, T defaultValue);
}