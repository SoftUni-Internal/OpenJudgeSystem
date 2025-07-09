namespace OJS.Data.Extensions;

using Microsoft.EntityFrameworkCore;
using OJS.Data.Models;
using OJS.Data.Models.Checkers;
using OJS.Data.Models.Contests;
using OJS.Data.Models.Mentor;
using OJS.Data.Models.Participants;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Resources;
using OJS.Data.Models.Submissions;
using OJS.Data.Models.Tests;
using OJS.Data.Models.Users;
using OJS.Data.Validation;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ConfigureEntities(this ModelBuilder builder)
    {
        builder.Entity<UserProfile>(b =>
        {
            b.Property(x => x.UserName)
                .IsRequired()
                .HasMaxLength(ConstraintConstants.User.UsernameMaxLength)
                .IsUnicode(false);

            b.OwnsOne(x => x.UserSettings);

            b.HasMany(x => x.UsersInRoles)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .IsRequired();
        });

        builder.Entity<SubmissionForProcessing>()
            .HasIndex(s => s.SubmissionId)
            .IsUnique();

        builder.Entity<Role>()
            .HasMany(x => x.UsersInRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .IsRequired();

        builder.Entity<Ip>()
            .HasIndex(x => x.Value)
            .IsUnique();

        builder.Entity<LecturerInContest>()
            .HasKey(x => new { x.LecturerId, x.ContestId });

        builder.Entity<LecturerInContestCategory>()
            .HasKey(x => new { x.LecturerId, x.ContestCategoryId });

        builder.Entity<UserInExamGroup>()
            .HasKey(x => new { x.UserId, x.ExamGroupId });

        builder.Entity<ProblemForParticipant>()
            .HasKey(x => new { x.ProblemId, x.ParticipantId });

        builder.Entity<SubmissionTypeInProblem>()
            .HasKey(x => new { x.SubmissionTypeId, x.ProblemId });

        builder.Entity<SubmissionTypeInSubmissionDocument>()
            .HasKey(x => new { x.SubmissionTypeId, x.SubmissionTypeDocumentId });

        builder.Entity<IpInContest>()
            .HasKey(x => new { x.ContestId, x.IpId });

        builder.Entity<Contest>(c =>
        {
            c.Property(cn => cn.AllowParallelSubmissionsInTasks).HasDefaultValue(true);
        });

        // Participant with submissions will not be able to be deleted. Use IsInvalidated instead.
        builder.Entity<Participant>()
            .HasMany(x => x.Submissions)
            .WithOne(x => x.Participant)
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Resource>()
            .HasDiscriminator<string>(nameof(Resource.ResourceType))
            .HasValue<ProblemResource>(nameof(ProblemResource))
            .HasValue<ContestResource>(nameof(ContestResource));

        builder.Entity<ProblemResource>()
            .HasOne(pr => pr.Problem)
            .WithMany(p => p.Resources)
            .HasForeignKey(pr => pr.ProblemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ContestResource>()
            .HasOne(cr => cr.Contest)
            .WithMany(c => c.Resources)
            .HasForeignKey(cr => cr.ContestId)
            .OnDelete(DeleteBehavior.Cascade);

        return builder.FixMultipleCascadePaths();
    }

    // TODO: Add automatic test to validate all IDeletableEntity entities have global query filter for IsDeleted
    public static ModelBuilder AddGlobalQueryFilters(this ModelBuilder builder)
    {
        builder.Entity<Checker>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Contest>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<ContestCategory>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<ExamGroup>()
            .HasQueryFilter(x => (x.Contest == null || !x.Contest.IsDeleted));

        builder.Entity<Participant>()
            .HasQueryFilter(x => !x.Contest.IsDeleted);

        builder.Entity<ParticipantScore>()
            .HasQueryFilter(x => !x.Problem.IsDeleted);

        builder.Entity<Problem>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<ProblemGroup>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Resource>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Submission>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<Test>()
            .HasQueryFilter(x => !x.Problem.IsDeleted);

        builder.Entity<TestRun>()
            .HasQueryFilter(x => !x.Submission.IsDeleted);

        builder.Entity<UserProfile>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<UserMentor>()
            .HasQueryFilter(x => !x.User.IsDeleted);

        builder.Entity<AccessLog>()
            .HasQueryFilter(x => !x.User.IsDeleted);

        builder.Entity<UserInExamGroup>()
            .HasQueryFilter(x => !x.User.IsDeleted);

        builder.Entity<UserInRole>()
            .HasQueryFilter(x => !x.User.IsDeleted);

        builder.Entity<FeedbackReport>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<IpInContest>()
            .HasQueryFilter(x => !x.Contest.IsDeleted);

        builder.Entity<LecturerInContest>()
            .HasQueryFilter(x => !x.Contest.IsDeleted);

        builder.Entity<LecturerInContestCategory>()
            .HasQueryFilter(x => !x.ContestCategory.IsDeleted);

        builder.Entity<ProblemForParticipant>()
            .HasQueryFilter(x => !x.Problem.IsDeleted);

        builder.Entity<SubmissionTypeInProblem>()
            .HasQueryFilter(x => !x.Problem.IsDeleted);

        builder.Entity<SubmissionTypeDocument>()
            .HasQueryFilter(x => !x.IsDeleted);

        builder.Entity<SubmissionTypeInSubmissionDocument>()
            .HasQueryFilter(x => !x.SubmissionTypeDocument.IsDeleted);

        return builder;
    }

    private static ModelBuilder FixMultipleCascadePaths(this ModelBuilder builder)
    {
        builder.Entity<ProblemGroup>()
            .HasMany(p => p.Problems)
            .WithOne(c => c.ProblemGroup)
            .HasForeignKey(p => p.ProblemGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Problem>()
            .HasMany(x => x.ParticipantScores)
            .WithOne(x => x.Problem)
            .HasForeignKey(x => x.ProblemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Test>()
            .HasMany(x => x.TestRuns)
            .WithOne(x => x.Test)
            .HasForeignKey(x => x.TestId)
            .OnDelete(DeleteBehavior.Restrict);

        return builder;
    }
}