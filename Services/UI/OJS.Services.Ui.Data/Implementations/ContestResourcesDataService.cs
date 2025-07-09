namespace OJS.Services.Ui.Data.Implementations;

using System.Linq;
using OJS.Data;
using OJS.Data.Models.Resources;using OJS.Services.Common.Data.Implementations;

public class ContestResourcesDataService : DataService<ContestResource>, IContestResourcesDataService
{
    public ContestResourcesDataService(OjsDbContext db) : base(db)
    {
    }

    public IQueryable<ContestResource> GetByContestQuery(int contestId)
        => this.GetQuery(pr => pr.ContestId == contestId && !pr.IsDeleted);
}