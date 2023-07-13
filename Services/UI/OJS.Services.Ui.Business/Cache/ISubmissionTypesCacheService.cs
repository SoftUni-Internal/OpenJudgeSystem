namespace OJS.Services.Ui.Business.Cache;

using Models.SubmissionTypes;
using SoftUni.Services.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using OJS.Services.Infrastructure.Constants;

public interface ISubmissionTypesCacheService : IService
{
    Task<IEnumerable<SubmissionTypeFilterServiceModel>> GetAllOrderedByLatestUsage(
        int? cacheSeconds = CacheConstants.OneHourInSeconds);
}