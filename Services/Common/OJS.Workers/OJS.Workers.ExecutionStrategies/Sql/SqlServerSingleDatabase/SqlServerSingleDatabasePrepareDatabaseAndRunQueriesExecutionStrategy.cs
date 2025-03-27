namespace OJS.Workers.ExecutionStrategies.Sql.SqlServerSingleDatabase;

using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.ExecutionStrategies.Models;

public class SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategy<TSettings> : BaseSqlServerSingleDatabaseExecutionStrategy<TSettings>
    where TSettings : SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategySettings
{
    public SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategy(
        IOjsSubmission submission,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, settingsProvider, logger)
    {
    }

    protected override Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
        => this.Execute(
            executionContext,
            result,
            (connection, test) =>
            {
                this.ExecuteNonQuery(connection, test.Input);
                var sqlTestResult = this.ExecuteReader(connection, executionContext.Code, executionContext.TimeLimit);
                ProcessSqlResult(sqlTestResult, executionContext, test, result);
            });
}

public record SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategySettings(
    string MasterDbConnectionString,
    string RestrictedUserId,
    string RestrictedUserPassword,
    string SubmissionProcessorIdentifier) : BaseSqlServerSingleDatabaseExecutionStrategySettings(
    MasterDbConnectionString, RestrictedUserId, RestrictedUserPassword, SubmissionProcessorIdentifier);
