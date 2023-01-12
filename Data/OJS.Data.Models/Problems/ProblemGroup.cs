namespace OJS.Data.Models.Problems
{
    using OJS.Common.Enumerations;
    using OJS.Data.Models.Contests;
    using System.Collections.Generic;
    using SoftUni.Data.Infrastructure.Models;

    public class ProblemGroup : DeletableAuditInfoEntity<int>, IOrderableEntity
    {
        public int ContestId { get; set; }

        public virtual Contest Contest { get; set; } = null!;

        public double OrderBy { get; set; }

        public ProblemGroupType? Type { get; set; }

        public virtual ICollection<Problem> Problems { get; set; } = new HashSet<Problem>();

        public override string ToString() => $"{this.OrderBy}";
    }
}