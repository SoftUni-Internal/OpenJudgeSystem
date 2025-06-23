namespace OJS.Services.Ui.Business;

using System.Threading.Tasks;
using OJS.Services.Ui.Models.Problems;
using OJS.Services.Infrastructure;

public interface IResourcesBusinessService : IService
{
    Task<ResourceServiceModel> GetResource(int resourceId);

    /// <summary>
    /// Converts a link to svn link, only if it is not already svn link and the link is a valid alternative, otherwise returns the same link.
    /// </summary>
    /// <param name="link">The link to convert.</param>
    /// <returns></returns>
    string SafeConvertToSvnLink(string link);
}