namespace OJS.Services.Administration.Business.Contests;

using Microsoft.AspNetCore.Http;
using OJS.Services.Infrastructure;
using System.Threading.Tasks;

public interface IContestsImportBusinessService : IService
{
    Task StreamImportContestsFromCategory(int sourceContestCategoryId, int destinationContestCategoryId, HttpResponse response, bool dryRun = true);
}