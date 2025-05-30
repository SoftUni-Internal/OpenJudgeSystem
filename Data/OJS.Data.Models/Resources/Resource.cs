namespace OJS.Data.Models.Resources;

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OJS.Common.Enumerations;
using OJS.Data.Models.Common;

using static Validation.ConstraintConstants;
using static Validation.ConstraintConstants.Problem;

public abstract class Resource : DeletableAuditInfoEntity<int>, IOrderableEntity
{
    [Required]
    public string ResourceType { get; set; } = null!;

    [NotMapped]
    public int ParentId => this.ResourceType switch
    {
        nameof(ContestResource) => ((ContestResource)this).ContestId ?? throw new ArgumentException($"The property {nameof(ContestResource.ContestId)} is null."),
        nameof(ProblemResource) => ((ProblemResource)this).ProblemId ?? throw new ArgumentException($"The property {nameof(ProblemResource.ProblemId)} is null."),
        _ => throw new InvalidOperationException($"Unknown resource type: {this.ResourceType}")
    };

    [Required]
    [MinLength(ResourceNameMinLength)]
    [MaxLength(ResourceNameMaxLength)]
    public string Name { get; set; } = string.Empty;

    public ProblemResourceType Type { get; set; }

    public byte[]? File { get; set; }

    [MaxLength(FileExtensionMaxLength)]
    public string? FileExtension { get; set; }

    public string? Link { get; set; }

    public double OrderBy { get; set; }
}