namespace OJS.Services.Ui.Data.Implementations
{
    using OJS.Data;
    using OJS.Data.Models.Problems;
    using OJS.Data.Models.Resources;
    using OJS.Services.Common.Data.Implementations;

    public class ResourcesDataService : DataService<ProblemResource>, IProblemResourcesDataService
    {
        public ResourcesDataService(OjsDbContext problemResources)
            : base(problemResources)
        {
        }
    }
}