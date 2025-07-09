namespace OJS.Data.Models.Problems
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using OJS.Data.Models.Checkers;
    using OJS.Data.Models.Participants;
    using OJS.Data.Models.Submissions;
    using OJS.Data.Models.Tests;
    using OJS.Data.Models.Common;
    using OJS.Data.Models.Resources;
    using static OJS.Data.Validation.ConstraintConstants.Problem;

    public class Problem : DeletableAuditInfoEntity<int>, IOrderableEntity
    {
        public int ProblemGroupId { get; set; }

        public virtual ProblemGroup ProblemGroup { get; set; } = null!;

        [Required]
        [MaxLength(NameMaxLength)]
        public string Name { get; set; } = string.Empty;

        public short MaximumPoints { get; set; }

        /// <summary>
        /// Gets or sets a Time limit for the problem. Measured in milliseconds.
        /// </summary>
        public int TimeLimit { get; set; }

        /// <summary>
        /// Gets or sets a Memory limit for the problem. Measured in bytes.
        /// </summary>
        public int MemoryLimit { get; set; }

        /// <summary>
        /// Gets or sets a File size limit (measured in bytes).
        /// </summary>
        public int? SourceCodeSizeLimit { get; set; }

        public int? CheckerId { get; set; }

        public virtual Checker? Checker { get; set; }

        public double OrderBy { get; set; }

        /// <summary>
        /// Gets or sets a predefined skeleton for the task.
        /// </summary>
        public byte[]? SolutionSkeleton { get; set; }

        /// <summary>
        /// Gets or sets Problem specific dependencies that will be compiled and executed with the user code
        /// such as Solution skeletons, mocks or data and text files.
        /// </summary>
        public byte[]? AdditionalFiles { get; set; }

        public bool ShowDetailedFeedback { get; set; }

        public int? DefaultSubmissionTypeId { get; set; }

        public virtual SubmissionType? DefaultSubmissionType { get; set; }

        public virtual ICollection<Test> Tests { get; set; } = [];

        public virtual ICollection<ProblemResource> Resources { get; set; } = [];

        public virtual ICollection<Submission> Submissions { get; set; } = [];

        public virtual ICollection<ParticipantScore> ParticipantScores { get; set; } = [];

        public virtual ICollection<SubmissionTypeInProblem> SubmissionTypesInProblems { get; set; } = [];

        public virtual ICollection<ProblemForParticipant> ProblemsForParticipants { get; set; } = [];

        public override string ToString() => $"#{this.Id} {this.Name}";
    }
}