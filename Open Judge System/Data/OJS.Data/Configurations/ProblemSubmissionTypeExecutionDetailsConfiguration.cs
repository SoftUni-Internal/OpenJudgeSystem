namespace OJS.Data.Configurations
{
    using System.Data.Entity.ModelConfiguration;
    using OJS.Data.Models;

    public class ProblemSubmissionTypeExecutionDetailsConfiguration : EntityTypeConfiguration<ProblemSubmissionTypeExecutionDetails>
    {
        public ProblemSubmissionTypeExecutionDetailsConfiguration()
        {
            this.HasKey(
                pst => 
                    new
                    { 
                        pst.ProblemId, pst.SubmissionTypeId
                    });

            this
                .HasRequired(pst => pst.Problem)
                .WithMany(p => p.ProblemSubmissionTypeExecutionDetails)
                .HasForeignKey(pst => pst.ProblemId);

            this
                .HasRequired(pst => pst.SubmissionType)
                .WithMany(st => st.ProblemSubmissionTypeExecutionDetails)
                .HasForeignKey(pst => pst.SubmissionTypeId);
        }
    }
}