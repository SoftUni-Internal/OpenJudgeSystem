namespace OJS.Data
{
    using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using OJS.Data.Extensions;
    using OJS.Data.Models;
    using OJS.Data.Models.Checkers;
    using OJS.Data.Models.Contests;
    using OJS.Data.Models.Mentor;
    using OJS.Data.Models.Participants;
    using OJS.Data.Models.Problems;
    using OJS.Data.Models.Submissions;
    using OJS.Data.Models.Tests;
    using OJS.Data.Models.Users;

    public class OjsDbContext : BaseAuthDbContext<OjsDbContext, UserProfile, Role, UserInRole>,
        IDataProtectionKeyContext
    {
        public OjsDbContext(DbContextOptions<OjsDbContext> options)
            : base(options)
        {
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

        public DbSet<Setting> Settings { get; set; } = null!;

        public DbSet<Contest> Contests { get; set; } = null!;

        public DbSet<IpInContest> ContestIps { get; set; } = null!;

        public DbSet<ExamGroup> ExamGroups { get; set; } = null!;

        public DbSet<UserInExamGroup> UsersInExamGroups { get; set; } = null!;

        public DbSet<Problem> Problems { get; set; } = null!;

        public DbSet<ProblemGroup> ProblemGroups { get; set; } = null!;

        public DbSet<ProblemResource> ProblemResources { get; set; } = null!;

        public DbSet<ProblemForParticipant> ProblemsForParticipants { get; set; } = null!;

        public DbSet<Participant> Participants { get; set; } = null!;

        public DbSet<ParticipantScore> ParticipantScores { get; set; } = null!;

        public DbSet<ContestCategory> ContestCategories { get; set; } = null!;

        public DbSet<Checker> Checkers { get; set; } = null!;

        public DbSet<Test> Tests { get; set; } = null!;

        public DbSet<Submission> Submissions { get; set; } = null!;

        public DbSet<SubmissionForProcessing> SubmissionsForProcessing { get; set; } = null!;

        public DbSet<SubmissionType> SubmissionTypes { get; set; } = null!;

        public DbSet<SubmissionTypeInProblem> SubmissionTypeProblems { get; set; } = null!;

        public DbSet<SubmissionTypeDocument> SubmissionTypeDocuments { get; set; } = null!;

        public DbSet<SubmissionTypeInSubmissionDocument> SubmissionTypesInSubmissionDocuments { get; set; } = null!;

        public DbSet<TestRun> TestRuns { get; set; } = null!;

        public DbSet<FeedbackReport> FeedbackReports { get; set; } = null!;

        public DbSet<LecturerInContest> LecturersInContests { get; set; } = null!;

        public DbSet<LecturerInContestCategory> LecturersInContestCategories { get; set; } = null!;

        public DbSet<Ip> Ips { get; set; } = null!;

        public DbSet<AccessLog> AccessLogs { get; set; } = null!;

        public DbSet<MentorPromptTemplate> MentorPromptTemplates { get; set; } = null!;

        public DbSet<UserMentor> UsersMentors { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder
                .ConfigureEntities()
                .AddGlobalQueryFilters();
        }
    }
}