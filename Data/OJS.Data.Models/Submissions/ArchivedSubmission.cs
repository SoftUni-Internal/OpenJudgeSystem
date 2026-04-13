namespace OJS.Data.Models.Submissions
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using AutoMapper;
    using OJS.Data.Validation;
    using OJS.Services.Infrastructure.Models.Mapping;
    using OJS.Workers.Common.Models;

    [Table("Submissions")]
    public class ArchivedSubmission : IMapExplicitly, IEquatable<ArchivedSubmission>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int ParticipantId { get; set; }

        public int ProblemId { get; set; }

        public int? SubmissionTypeId { get; set; }

        public byte[] Content { get; set; } = [];

        public string? FileExtension { get; set; }

        public byte[]? SolutionSkeleton { get; set; }

        public DateTime? StartedExecutionOn { get; set; }

        public DateTime? CompletedExecutionOn { get; set; }

        [StringLength(ConstraintConstants.IpAddressMaxLength)]
        [Column(TypeName = "varchar")]
        public string? IpAddress { get; set; }

        [StringLength(ConstraintConstants.Submission.WorkerNameMaxLength)]
        public string? WorkerName { get; set; }

        public ExceptionType? ExceptionType { get; set; }

        public bool Processed { get; set; }

        public int Points { get; set; }

        public string? ProcessingComment { get; set; }

        public string? TestRunsCache { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsHardDeletedFromMainDatabase { get; set; }

        [NotMapped]
        public bool IsBinaryFile => !string.IsNullOrWhiteSpace(this.FileExtension);

        [NotMapped]
        public string ContentAsString
            => this.IsBinaryFile ? string.Empty : this.Content.ToString();

        public override bool Equals(object? obj)
            => obj is ArchivedSubmission other && this.Equals(other);

        public bool Equals(ArchivedSubmission? other)
            => other != null && this.Id == other.Id;

        public override int GetHashCode()
            => this.Id.GetHashCode();

        public void RegisterMappings(IProfileExpression configuration)
            => configuration.CreateMap<Submission, ArchivedSubmission>()
                .ForMember(d => d.IsHardDeletedFromMainDatabase, opt => opt.MapFrom(s => false));
    }
}
