namespace OJS.Data.Models.Checkers
{
    using SoftUni.Data.Infrastructure.Models;
    using System.ComponentModel.DataAnnotations;
    using static OJS.Data.Validation.ConstraintConstants.Checker;

    public class Checker : DeletableAuditInfoEntity<int>
    {
        [Required]
        [MinLength(NameMinLength)]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string DllFile { get; set; }

        public string ClassName { get; set; }

        public string Parameter { get; set; }

        public override string ToString() => this.Name;
    }
}