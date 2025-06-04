namespace OJS.Services.Administration.Models.Resources;

using System;
using OJS.Common.Enumerations;
using OJS.Data.Models.Resources;
using OJS.Services.Infrastructure.Models.Mapping;

public class ProblemResourceInListModel : IMapFrom<ProblemResource>
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

    public string ProblemName { get; set; } = string.Empty;
}