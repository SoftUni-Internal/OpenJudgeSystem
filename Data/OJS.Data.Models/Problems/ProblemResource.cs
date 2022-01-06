namespace OJS.Data.Models.Problems
{
    using OJS.Common.Enumerations;
    using SoftUni.Data.Infrastructure.Models;
    using System.ComponentModel.DataAnnotations;
    using static OJS.Data.Validation.ConstraintConstants;
    using static OJS.Data.Validation.ConstraintConstants.Problem;

    public class ProblemResource : DeletableAuditInfoEntity<int>, IOrderableEntity
    {
        public int ProblemId { get; set; }

        public virtual Problem Problem { get; set; } = null!;

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
}