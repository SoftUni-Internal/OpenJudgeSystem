namespace OJS.Services.Ui.Business.Cache.Implementations;

using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Ui.Models.Submissions;
using OJS.Services.Ui.Data;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.Cache;
using OJS.Services.Ui.Models.Contests;
using System.Linq;
using System.Threading.Tasks;

public class ContestsCacheService : IContestsCacheService
{
    private readonly ICacheService cache;
    private readonly IContestsDataService contestsData;

    public ContestsCacheService(
        ICacheService cache,
        IContestsDataService contestsData)
    {
        this.cache = cache;
        this.contestsData = contestsData;
    }

    public async Task<ContestDetailsServiceModel?> GetContestDetailsServiceModel(
        int contestId,
        int cacheSeconds = CacheConstants.FiveMinutesInSeconds)
        => await this.cache.Get(
            string.Format(CacheConstants.ContestDetails, contestId),
            async () => (await this.GetContestServiceModel(contestId)),
            cacheSeconds);

    private async Task<ContestDetailsServiceModel?> GetContestServiceModel(int contestId)
    {
        var contestDetailsServiceModel = await this.contestsData.GetById<ContestDetailsServiceModel>(contestId);

        if (contestDetailsServiceModel != null)
        {
            contestDetailsServiceModel.AllowedSubmissionTypes = contestDetailsServiceModel.Problems
                .AsQueryable()
                .SelectMany(p => p.AllowedSubmissionTypes)
                .DistinctBy(st => st.Id)
                .Select(x => new ContestDetailsSubmissionTypeServiceModel { Id = x.Id, Name = x.Name })
                .ToList();
        }

        return contestDetailsServiceModel;
    }
}