namespace OJS.Services.Ui.Data;

using System.Linq;
using OJS.Data.Models.Resources;
using OJS.Services.Common.Data;

public interface IContestResourcesDataService : IDataService<ContestResource>
{
    public IQueryable<ContestResource> GetByContestQuery(int contestId);
}