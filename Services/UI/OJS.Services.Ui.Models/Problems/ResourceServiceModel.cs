namespace OJS.Services.Ui.Models.Problems;

using OJS.Data.Models.Resources;
using OJS.Services.Infrastructure.Models.Mapping;

public class ResourceServiceModel : IMapFrom<Resource>
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public string Name { get; set; } = null!;

    public byte[]? File { get; set; }

    public string? FileExtension { get; set; }

    public string? Link { get; set; }

    public double OrderBy { get; set; }

    public ProblemResourceType Type { get; set; }
}