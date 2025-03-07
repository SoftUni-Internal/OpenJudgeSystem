namespace OJS.Workers.ExecutionStrategies.Sql.PostgreSql;

using Microsoft.Extensions.Logging;
using System.Data;
using OJS.Workers.Common;
using OJS.Workers.ExecutionStrategies.Models;

public class PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategy<TSettings> : BasePostgreSqlExecutionStrategy<TSettings>
    where TSettings : PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategySettings
{
    public PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategy(
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
                var sqlTestResult = this.ExecuteReader(connection, test.Input);
                ProcessSqlResult(sqlTestResult, executionContext, test, result);
            });

    protected override void ExecuteBeforeTests(IDbConnection connection, IExecutionContext<TestsInputModel>
        executionContext)
        => this.ExecuteNonQuery(connection, executionContext.Code, executionContext.TimeLimit);
}

public record PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategySettings(
    string MasterDbConnectionString,
    string RestrictedUserId,
    string RestrictedUserPassword,
    string SubmissionProcessorIdentifier) : BasePostgreSqlExecutionStrategySettings(MasterDbConnectionString,
    RestrictedUserId, RestrictedUserPassword, SubmissionProcessorIdentifier);
