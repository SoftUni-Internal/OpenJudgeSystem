namespace OJS.Services.Ui.Models.Cache;

using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Resources;
using OJS.Services.Infrastructure.Models.Mapping;

public class ResourceCacheModel : IMapExplicitly
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Link { get; set; } = null!;

    public ProblemResourceType Type { get; set; }

    public double OrderBy { get; set; }
    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<ProblemResource, ResourceCacheModel>()
            .ReverseMap();
}