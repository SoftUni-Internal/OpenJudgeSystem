namespace OJS.Servers.Infrastructure.Telemetry;

using System.Diagnostics;

/// <summary>
/// Centralized ActivitySources for the OpenJudgeSystem application.
/// Organized by business domains for better maintainability and filtering.
/// </summary>
public static class OjsActivitySources
{
    /// <summary>
    /// The version used across all ActivitySources.
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// ActivitySource names as constants for configuration and sampling.
    /// </summary>
    public static class SourceNames
    {
        public const string Submissions = "OJS.Submissions";
        public const string Contests = "OJS.Contests";
        public const string Authentication = "OJS.Authentication";
        public const string Problems = "OJS.Problems";
        public const string DataAccess = "OJS.DataAccess";
        public const string Integrations = "OJS.Integrations";
        public const string BackgroundJobs = "OJS.BackgroundJobs";
        public const string Caching = "OJS.Caching";
    }

    /// <summary>
    /// ActivitySource for submission-related operations.
    /// </summary>
    public static readonly ActivitySource submissions = new(SourceNames.Submissions, Version);

    /// <summary>
    /// ActivitySource for contest-related operations.
    /// </summary>
    public static readonly ActivitySource contests = new(SourceNames.Contests, Version);

    /// <summary>
    /// ActivitySource for user authentication and authorization operations.
    /// </summary>
    public static readonly ActivitySource authentication = new(SourceNames.Authentication, Version);

    /// <summary>
    /// ActivitySource for problem management operations.
    /// </summary>
    public static readonly ActivitySource problems = new(SourceNames.Problems, Version);

    /// <summary>
    /// ActivitySource for data access operations.
    /// </summary>
    public static readonly ActivitySource dataAccess = new(SourceNames.DataAccess, Version);

    /// <summary>
    /// ActivitySource for external integrations (SVN, APIs, etc.).
    /// </summary>
    public static readonly ActivitySource integrations = new(SourceNames.Integrations, Version);

    /// <summary>
    /// ActivitySource for background jobs and scheduled tasks.
    /// </summary>
    public static readonly ActivitySource backgroundJobs = new(SourceNames.BackgroundJobs, Version);

    /// <summary>
    /// ActivitySource for caching operations.
    /// </summary>
    public static readonly ActivitySource caching = new(SourceNames.Caching, Version);

    /// <summary>
    /// Get all ActivitySource names for registration with OpenTelemetry.
    /// </summary>
    public static string[] AllSourceNames =>
    [
        submissions.Name,
        contests.Name,
        authentication.Name,
        problems.Name,
        dataAccess.Name,
        integrations.Name,
        backgroundJobs.Name,
        caching.Name,
    ];

    /// <summary>
    /// Activity names for submission operations.
    /// </summary>
    public static class SubmissionActivities
    {
        public const string Received = "submission.received";
        public const string Queued = "submission.queued";
        public const string ProcessingStarted = "submission.processing_started";
        public const string Execution = "submission.execution";
        public const string ProcessingExecutionResult = "submission.processing_result";
        public const string Retest = "submission.retest";
    }

    /// <summary>
    /// Activity names for contest operations.
    /// </summary>
    public static class ContestActivities
    {
        public const string Registration = "contest.registration";
        public const string ParticipantCreation = "contest.participant.creation";
        public const string ScoreCalculation = "contest.score.calculation";
        public const string RankingUpdate = "contest.ranking.update";
        public const string Export = "contest.export";
    }

    /// <summary>
    /// Activity names for authentication operations.
    /// </summary>
    public static class AuthenticationActivities
    {
        public const string Login = "auth.login";
        public const string Logout = "auth.logout";
        public const string Registration = "auth.registration";
        public const string PasswordReset = "auth.password.reset";
        public const string TokenValidation = "auth.token.validation";
    }

    /// <summary>
    /// Activity names for problem operations.
    /// </summary>
    public static class ProblemActivities
    {
        public const string Creation = "problem.creation";
        public const string Update = "problem.update";
        public const string TestsUpload = "problem.tests.upload";
        public const string ResourcesUpload = "problem.resources.upload";
        public const string Validation = "problem.validation";
    }

    /// <summary>
    /// Activity names for data access operations.
    /// </summary>
    public static class DataAccessActivities
    {
        public const string Query = "data.query";
        public const string Command = "data.command";
        public const string Transaction = "data.transaction";
        public const string Migration = "data.migration";
        public const string BulkOperation = "data.bulk.operation";
    }

    /// <summary>
    /// Activity names for integration operations.
    /// </summary>
    public static class IntegrationActivities
    {
        public const string SvnOperation = "integration.svn";
        public const string ExternalApiCall = "integration.api.call";
        public const string FileUpload = "integration.file.upload";
        public const string EmailSend = "integration.email.send";
    }

    /// <summary>
    /// Activity names for background job operations.
    /// </summary>
    public static class BackgroundJobActivities
    {
        public const string Execution = "job.execution";
        public const string Scheduling = "job.scheduling";
        public const string Retry = "job.retry";
        public const string Cleanup = "job.cleanup";
    }

    /// <summary>
    /// Activity names for caching operations.
    /// </summary>
    public static class CachingActivities
    {
        public const string Get = "cache.get";
        public const string Set = "cache.set";
        public const string Remove = "cache.remove";
        public const string Clear = "cache.clear";
        public const string Refresh = "cache.refresh";
    }

    /// <summary>
    /// Common tag names used across all activities.
    /// </summary>
    public static class CommonTags
    {
        // User context
        public const string UserId = "user.id";
        public const string UserName = "user.name";
        public const string UserRole = "user.role";

        // Request context
        public const string CorrelationId = "correlation.id";
        public const string RequestId = "request.id";
        public const string SessionId = "session.id";

        // Service context
        public const string ServiceName = "service.name";
        public const string ServiceVersion = "service.version";
        public const string Environment = "environment";

        // Business context
        public const string TenantId = "tenant.id";
        public const string ContestId = "contest.id";
        public const string ProblemId = "problem.id";
        public const string SubmissionId = "submission.id";
        public const string ParticipantId = "participant.id";

        // Technical context
        public const string Operation = "operation";
        public const string Component = "component";
        public const string ErrorType = "error.type";
        public const string Duration = "duration.ms";
        public const string ItemCount = "item.count";
        public const string DataSize = "data.size.bytes";
    }
}
