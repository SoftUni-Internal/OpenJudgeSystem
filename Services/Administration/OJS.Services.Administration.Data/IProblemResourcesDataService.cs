namespace OJS.Services.Administration.Data
{
    using OJS.Services.Common.Data;
    using System.Linq;
    using OJS.Data.Models.Resources;

    public interface IProblemResourcesDataService : IDataService<ProblemResource>
    {
        IQueryable<ProblemResource> GetByProblemQuery(int problemId);

        void DeleteByProblem(int problemId);
    }
}