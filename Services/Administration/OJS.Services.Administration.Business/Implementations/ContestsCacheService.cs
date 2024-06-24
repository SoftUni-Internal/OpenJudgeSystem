namespace OJS.Services.Administration.Business.Implementations;

using OJS.Services.Infrastructure.Cache;
using OJS.Services.Infrastructure.Constants;

public class ContestsCacheService : IContestsCacheService
{
    private readonly ICacheService cache;

    public ContestsCacheService(
        ICacheService cache)
        => this.cache = cache;

    public void ClearContestsCacheByContestId(int contestId)
    {
        this.cache.Remove(string.Format(CacheConstants.ContestDetails, contestId));
        this.cache.Remove(string.Format(CacheConstants.ContestDetailsForSubmit, contestId));
    }
}