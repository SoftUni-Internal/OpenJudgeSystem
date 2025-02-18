namespace OJS.Workers.ExecutionStrategies.Sql.SqlServerSingleDatabase;

using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.ExecutionStrategies.Models;

public class SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategy<TSettings> : BaseSqlServerSingleDatabaseExecutionStrategy<TSettings>
    where TSettings : SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategySettings
{
    public SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategy(
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
                this.ExecuteNonQuery(connection, executionContext.Code, executionContext.TimeLimit);
                var sqlTestResult = this.ExecuteReader(connection, test.Input);
                ProcessSqlResult(sqlTestResult, executionContext, test, result);
            });
}

public record SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategySettings(
    string MasterDbConnectionString,
    string RestrictedUserId,
    string RestrictedUserPassword,
    string SubmissionProcessorIdentifier) : BaseSqlServerSingleDatabaseExecutionStrategySettings(
    MasterDbConnectionString, RestrictedUserId, RestrictedUserPassword, SubmissionProcessorIdentifier);
