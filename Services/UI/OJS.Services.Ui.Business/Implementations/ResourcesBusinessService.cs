namespace OJS.Services.Ui.Business.Implementations;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OJS.Services.Infrastructure.Configurations;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Problems;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Linq;
using OJS.Data.Models.Resources;
using OJS.Services.Common.Data;

public class ResourcesBusinessService(
    IDataService<Resource> resourcesDataService,
    IOptions<SvnConfig> svnConfigAccessor,
    ILogger<ResourcesBusinessService> logger)
    : IResourcesBusinessService
{
    private readonly SvnConfig svnConfig = svnConfigAccessor.Value;

    public async Task<ResourceServiceModel> GetResource(int resourceId)
        => await resourcesDataService
            .GetByIdQuery(resourceId)
            .AsNoTracking()
            .MapCollection<ResourceServiceModel>()
            .FirstOrDefaultAsync()
            ?? throw new BusinessServiceException($"Resource with ID {resourceId} not found.");

    public string SafeConvertToSvnLink(string link)
    {
        if (!Uri.TryCreate(link, UriKind.Absolute, out var incomingUri))
        {
            // Not a valid link
            return link;
        }

        if (!Uri.TryCreate(this.svnConfig.BaseUrl, UriKind.Absolute, out var svnUri))
        {
            logger.LogSvnBaseUrlNotValid(this.svnConfig.BaseUrl);
            return link;
        }

        var alternativeUri = this.svnConfig.AlternativeBaseUrls?
            .Select(raw => Uri.TryCreate(raw, UriKind.Absolute, out var u) ? u : null)
            .FirstOrDefault(u => u != null && u.IsBaseOf(incomingUri));

        if (alternativeUri == null)
        {
            logger.LogAlternativeBaseUrlNotFoundForLink(link);
            return link;
        }

        // Get the part of the path that comes after the alternative base.
        var relativeUri = alternativeUri.MakeRelativeUri(incomingUri);

        var newUri = new Uri(svnUri, relativeUri);

        var newLink = newUri.ToString();

        logger.LogLinkSuccessfullyConvertedToSvnLink(link, newLink);

        return newLink;
    }
}