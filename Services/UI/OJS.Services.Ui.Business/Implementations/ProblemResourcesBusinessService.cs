namespace OJS.Services.Ui.Business.Implementations;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OJS.Services.Infrastructure.Configurations;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Problems;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Linq;

public class ProblemResourcesBusinessService(
    IProblemResourcesDataService problemResourcesDataService,
    IOptions<SvnConfig> svnConfigAccessor)
    : IProblemResourcesBusinessService
{
    private readonly SvnConfig svnConfig = svnConfigAccessor.Value;

    public async Task<ProblemResourceServiceModel> GetResource(int resourceId)
        => await problemResourcesDataService
            .GetByIdQuery(resourceId)
            .AsNoTracking()
            .MapCollection<ProblemResourceServiceModel>()
            .FirstOrDefaultAsync()
            ?? throw new BusinessServiceException($"Problem resource with ID {resourceId} not found.");

    public string SafeConvertToSvnLink(string link)
    {
        if (!Uri.TryCreate(link, UriKind.Absolute, out var incomingUri))
        {
            // Not a valid link
            return link;
        }

        if (!Uri.TryCreate(this.svnConfig.BaseUrl, UriKind.Absolute, out var svnUri))
        {
            // Not a valid SVN link provided in the config
            return link;
        }

        var alternativeUri = this.svnConfig.AlternativeBaseUrls?
            .Select(raw => Uri.TryCreate(raw, UriKind.Absolute, out var u) ? u : null)
            .FirstOrDefault(u => u != null && u.IsBaseOf(incomingUri));

        if (alternativeUri == null)
        {
            // Not an alternative SVN link, or not a valid alternative SVN link provided in the config
            return link;
        }

        // Get the part of the path that comes after the alternative base.
        var relativeUri = alternativeUri.MakeRelativeUri(incomingUri);

        var newUri = new Uri(svnUri, relativeUri);

        return newUri.ToString();
    }
}