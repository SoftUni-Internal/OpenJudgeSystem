namespace OJS.Workers.ExecutionStrategies.Sql.MySql;

using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.ExecutionStrategies.Models;

public class MySqlPrepareDatabaseAndRunQueriesExecutionStrategy<TSettings> : BaseMySqlExecutionStrategy<TSettings>
    where TSettings : MySqlPrepareDatabaseAndRunQueriesExecutionStrategySettings
{
    public MySqlPrepareDatabaseAndRunQueriesExecutionStrategy(
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

public record MySqlPrepareDatabaseAndRunQueriesExecutionStrategySettings(
    string MasterDbConnectionString,
    string RestrictedUserId,
    string RestrictedUserPassword)
    : BaseMySqlExecutionStrategySettings(MasterDbConnectionString, RestrictedUserId, RestrictedUserPassword);
