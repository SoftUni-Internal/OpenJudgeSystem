namespace OJS.Services.Administration.Models.ProblemResources;

using OJS.Common.Enumerations;
using OJS.Services.Infrastructure.Models.Mapping;
using System;
using OJS.Data.Models.Resources;

public class ResourceInListModel : IMapFrom<Resource>
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ProblemResourceType Type { get; set; }

    public string? FileExtension { get; set; }

    public string? Link { get; set; }

    public double OrderBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }
}