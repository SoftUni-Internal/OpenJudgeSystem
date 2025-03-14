namespace OJS.Workers.ExecutionStrategies.Sql.MySql;

using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.ExecutionStrategies.Models;

public class MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<TSettings> : BaseMySqlExecutionStrategy<TSettings>
    where TSettings : MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings
{
    public MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy(
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
                this.ExecuteNonQuery(connection, executionContext.Input.TaskSkeletonAsString);
                this.ExecuteNonQuery(connection, executionContext.Code, executionContext.TimeLimit);
                var sqlTestResult = this.ExecuteReader(connection, test.Input);
                ProcessSqlResult(sqlTestResult, executionContext, test, result);
            });
}

public record MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings(
    string MasterDbConnectionString,
    string RestrictedUserId,
    string RestrictedUserPassword)
    : BaseMySqlExecutionStrategySettings(MasterDbConnectionString, RestrictedUserId, RestrictedUserPassword);
