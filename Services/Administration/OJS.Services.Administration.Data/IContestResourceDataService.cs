namespace OJS.Services.Administration.Data;

using System.Linq;
using OJS.Data.Models.Resources;
using OJS.Services.Common.Data;

public interface IContestResourceDataService : IDataService<ContestResource>
{
    IQueryable<ContestResource> GetByContestQuery(int problemId);

    void DeleteByContest(int problemId);
}