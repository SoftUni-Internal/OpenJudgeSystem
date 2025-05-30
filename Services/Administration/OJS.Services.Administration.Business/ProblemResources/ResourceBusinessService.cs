namespace OJS.Services.Administration.Business.ProblemResources;

using Microsoft.EntityFrameworkCore;
using OJS.Common;
using OJS.Data.Models.Resources;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.ProblemResources;
using OJS.Services.Common.Data;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using System.Linq;
using System.Threading.Tasks;

public class ResourceBusinessService : IResourcesBusinessService
{
    private readonly IDataService<Resource> resourceDataService;
    private readonly IProblemResourcesDataService problemResourcesDataService;
    private readonly IContestResourceDataService contestResourceDataService;
    private readonly IProblemsCacheService problemsCache;

    public ResourceBusinessService(
        IDataService<Resource> resourceDataService,
        IProblemResourcesDataService problemResourcesDataService,
        IContestResourceDataService contestResourceDataService,
        IProblemsCacheService problemsCache)
    {
        this.resourceDataService = resourceDataService;
        this.problemResourcesDataService = problemResourcesDataService;
        this.contestResourceDataService = contestResourceDataService;
        this.problemsCache = problemsCache;
    }

    public async Task Delete(int id)
    {
        var resource = await this.resourceDataService
            .GetByIdQuery(id)
            .FirstOrDefaultAsync();

        if (resource is null)
        {
            throw new BusinessServiceException($"Resource with id {id} was not found.");
        }

        switch (resource)
        {
            case ProblemResource pr:
                this.problemResourcesDataService.Delete(pr);
                break;

            case ContestResource cr:
                this.contestResourceDataService.Delete(cr);
                break;

            default:
                throw new BusinessServiceException("Unsupported resource type.");
        }

        await this.problemsCache.ClearProblemCacheById(resource.ParentId);

        await this.resourceDataService.SaveChanges();
    }

    public async Task<ResourceAdministrationModel> Create(ResourceAdministrationModel model)
    {
        Resource resource = model.ResourceType switch
        {
            nameof(ProblemResource) => model.Map<ProblemResource>(),
            nameof(ContestResource) => model.Map<ContestResource>(),
            _ => throw new BusinessServiceException("Invalid resource type.")
        };

        if (model.File != null)
        {
            AssignFileExtension(resource, model.File.FileName);
        }

        switch (resource)
        {
            case ProblemResource pr:
                await this.problemResourcesDataService.Add(pr);
                break;

            case ContestResource cr:
                await this.contestResourceDataService.Add(cr);
                break;
        }

        await this.problemsCache.ClearProblemCacheById(resource.ParentId);

        await this.resourceDataService.SaveChanges();

        return model;
    }

    public async Task<ResourceAdministrationModel> Get(int id)
    {
        var resource = await this.resourceDataService
            .GetByIdQuery(id)
            .FirstOrDefaultAsync();

        if (resource is null)
        {
            throw new BusinessServiceException($"Resource with id {id} was not found.");
        }

        return resource.Map<ResourceAdministrationModel>();
    }

    public async Task<ResourceAdministrationModel> Edit(ResourceAdministrationModel model)
    {
        var resource = await this.resourceDataService
            .GetByIdQuery(model.Id)
            .FirstOrDefaultAsync();

        if (resource is null)
        {
            throw new BusinessServiceException($"A resource with id {model.Id} was not found.");
        }

        var areFileAndLinkNull = model is { File: null, Link: null };

        if (resource.File == null && areFileAndLinkNull)
        {
            throw new BusinessServiceException("The resource should contain either a file or a link.");
        }

        var shouldKeepFile = resource.File != null && areFileAndLinkNull;

        resource.MapFrom(model);

        var entry = this.resourceDataService.GetEntry(resource);

        if (shouldKeepFile)
        {
            entry.Property(pr => pr.File).IsModified = false;
            entry.Property(pr => pr.FileExtension).IsModified = false;
        }

        if (model.File != null)
        {
            AssignFileExtension(resource, model.File.FileName);
        }

        entry.Property(pr => pr.ResourceType).IsModified = false;

        this.resourceDataService.Update(resource);
        await this.resourceDataService.SaveChanges();

        await this.problemsCache.ClearProblemCacheById(resource.ParentId);

        return model;
    }

    public async Task<ResourceServiceModel> GetResourceFile(int id)
    {
        var hasResource = await this.resourceDataService.ExistsById(id);

        if (!hasResource)
        {
            throw new BusinessServiceException("Resource not found.");
        }

        var resource = await this.resourceDataService
            .GetByIdQuery(id)
            .FirstAsync();

        return new ResourceServiceModel
        {
            Content = resource.File!,
            MimeType = GlobalConstants.MimeTypes.ApplicationOctetStream,
            FileName = $"Resource-{resource.Id}-{resource.Name}.{resource.FileExtension}",
        };
    }

    private static void AssignFileExtension(Resource resource, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !fileName.Contains('.'))
        {
            throw new BusinessServiceException("Invalid file name or no extension found.", nameof(fileName));
        }

        var extension = fileName.Substring(fileName.LastIndexOf('.') + 1);

        if (string.IsNullOrWhiteSpace(extension) || !extension.All(char.IsLetterOrDigit))
        {
            throw new BusinessServiceException("Invalid file extension.", nameof(fileName));
        }

        resource.FileExtension = extension;
    }
}
