namespace OJS.Services.Administration.Data.Implementations;

using System.Linq;
using OJS.Data;
using OJS.Data.Models.Resources;

public class ContestResourcesDataService : AdministrationDataService<ContestResource>, IContestResourceDataService
{
    public ContestResourcesDataService(OjsDbContext problemResources)
        : base(problemResources)
    {
    }

    public IQueryable<ContestResource> GetByContestQuery(int contestId)
        => this.GetQuery(pr => pr.ContestId == contestId && !pr.IsDeleted);

    public void DeleteByContest(int contestId)
        => this.Delete(pr => pr.ContestId == contestId);
}