namespace OJS.Services.Administration.Business.ProblemResources;

using OJS.Data.Models.Problems;
using OJS.Services.Administration.Models.ProblemResources;
using System.Threading.Tasks;
using OJS.Data.Models.Resources;

public interface IResourcesBusinessService : IAdministrationOperationService<Resource, int, ResourceAdministrationModel>
{
    Task<ResourceServiceModel> GetResourceFile(int id);
}