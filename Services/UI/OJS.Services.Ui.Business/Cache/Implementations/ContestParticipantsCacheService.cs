namespace OJS.Services.Ui.Business.Cache.Implementations;

using OJS.Common.Extensions;
using OJS.Common.Utils;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.Cache;
using OJS.Services.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Cache;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Ui.Business.Validations.Implementations.Contests;
using OJS.Services.Common.Models.Users;
using OJS.Services.Ui.Models.Contests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ContestParticipantsCacheService : IContestParticipantsCacheService
{
    private readonly ICacheService cache;
    private readonly IParticipantsDataService participantsDataService;
    private readonly IContestsDataService contestsData;
    private readonly IContestParticipationValidationService contestParticipationValidationService;

    public ContestParticipantsCacheService(
        ICacheService cache,
        IParticipantsDataService participantsDataService,
        IContestsDataService contestsData,
        IContestParticipationValidationService contestParticipationValidationService)
    {
        this.cache = cache;
        this.participantsDataService = participantsDataService;
        this.contestsData = contestsData;
        this.contestParticipationValidationService = contestParticipationValidationService;
    }

    public async Task<IDictionary<int, ContestParticipantsCountCacheModel>> GetParticipantsCount(
        IReadOnlyCollection<int> contestIds,
        int? page,
        int cacheSeconds = CacheConstants.FiveMinutesInSeconds)
    {
        var contestIdsKey = CacheKeyGenerator.GenerateKeyForPrimitiveCollection(contestIds);
        var cacheKey = string.Format(CacheConstants.ParticipantsCountByContestsPage, contestIdsKey, page);
        return await this.cache.Get(
            cacheKey,
            async () => await this.GetContestsParticipantsCount(contestIds),
            cacheSeconds);
    }

    public async Task<ContestParticipantsCountCacheModel> GetParticipantsCountForContest(
        int contestId,
        int cacheSeconds = CacheConstants.FiveMinutesInSeconds)
        => await this.cache.Get(
            string.Format(CacheConstants.ParticipantsCountByContest, contestId),
            async () => (await this.GetContestsParticipantsCount(new[] { contestId }))[contestId],
            cacheSeconds);

    public async Task<ContestServiceModel?> GetContestServiceModelForContest(
        int contestId,
        StartContestParticipationServiceModel model,
        int cacheSeconds = CacheConstants.FiveMinutesInSeconds)
        => await this.cache.Get(
            string.Format(CacheConstants.ContestDetailsForSubmit, contestId),
            async () => (await this.GetContestServiceModel(model)),
            cacheSeconds);

    /// <summary>
    /// Gets a dictionary with all provided contests (Id as Key) and their corresponding
    /// participants count, separated by Compete and Practice mode.
    /// <para>
    /// Even if a contest has no participants or the contest with the provided id does not exist,
    /// it will be included in the result with 0 participants.
    /// </para>
    /// </summary>
    /// <param name="contestIds">The contests for which to gather participants count.</param>
    /// <returns>Dictionary with all the provided <see cref="contestIds"/> as keys.</returns>
    private async Task<IDictionary<int, ContestParticipantsCountCacheModel>> GetContestsParticipantsCount(
        IReadOnlyCollection<int> contestIds)
    {
        var officialParticipants = await this.participantsDataService.GetParticipantsCountInContests(contestIds, true);
        var practiceParticipants = await this.participantsDataService.GetParticipantsCountInContests(contestIds, false);

        return contestIds.ToDictionary(
            id => id,
            id => new ContestParticipantsCountCacheModel
            {
                Official = officialParticipants.GetValueOrDefault(id),
                Practice = practiceParticipants.GetValueOrDefault(id),
            });
    }

    /// <summary>
    /// Gets the contest by its id with problem details and categories and maps it to a ContestServiceModel.
    /// </summary>
    /// <param name="model">The model containing the contest participation start details, including the contest id and whether it is official.</param>
    /// <returns>A ContestServiceModel containing detailed information about the contest.</returns>
    private async Task<ContestServiceModel?> GetContestServiceModel(
        StartContestParticipationServiceModel model)
    {
        var contest = await this.contestsData.GetById<ContestServiceModel>(model.ContestId);

        var contestServiceModel = contest?.Map<ContestServiceModel?>();

        return contestServiceModel;
    }
}