namespace OJS.Services.Administration.Business;

using OJS.Services.Infrastructure;

public interface IContestsCacheService : IService
{
    void ClearContestsCacheByContestId(int contestId);
}