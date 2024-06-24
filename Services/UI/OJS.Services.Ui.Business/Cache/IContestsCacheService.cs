namespace OJS.Services.Ui.Business.Cache;

using OJS.Services.Infrastructure.Constants;
using OJS.Services.Ui.Models.Contests;
using OJS.Services.Infrastructure;
using System.Threading.Tasks;

public interface IContestsCacheService : IService
{
    Task<ContestDetailsServiceModel?> GetContestDetailsServiceModel(
        int contestId,
        int cacheSeconds = CacheConstants.FiveMinutesInSeconds);
}