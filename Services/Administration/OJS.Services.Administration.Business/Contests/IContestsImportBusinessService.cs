namespace OJS.Services.Administration.Business.Contests;

using OJS.Services.Common.Models;
using OJS.Services.Infrastructure;
using System.Threading.Tasks;

public interface IContestsImportBusinessService : IService
{
    Task<ServiceResult<string>> ImportContestsFromCategory(int sourceContestCategoryId, int destinationContestCategoryId, bool dryRun = true);
}