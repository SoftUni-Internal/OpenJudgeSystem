namespace OJS.Services.Ui.Models.Contests;

using OJS.Common.Enumerations;
using OJS.Data.Models.Resources;
using OJS.Services.Infrastructure.Models.Mapping;

public class ContestResourceDetailsServiceModel : IMapFrom<ContestResource>
{
    public int Id { get; set; }

    public int? ContestId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ResourceType { get; set; } = null!;

    public ProblemResourceType Type { get; set; }

    public byte[]? File { get; set; }

    public string? FileExtension { get; set; }

    public string? Link { get; set; }

    public double OrderBy { get; set; }
}