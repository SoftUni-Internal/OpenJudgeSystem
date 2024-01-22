﻿namespace OJS.Workers.ExecutionStrategies.Sql.MySql
{
    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;

    public class MySqlRunQueriesAndCheckDatabaseExecutionStrategy<TSettings> : BaseMySqlExecutionStrategy<TSettings>
        where TSettings : MySqlRunQueriesAndCheckDatabaseExecutionStrategySettings
    {
        public MySqlRunQueriesAndCheckDatabaseExecutionStrategy(
            ExecutionStrategyType type,
            IExecutionStrategySettingsProvider settingsProvider)
            : base(type, settingsProvider)
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

    public record MySqlRunQueriesAndCheckDatabaseExecutionStrategySettings(
        string MasterDbConnectionString,
        string RestrictedUserId,
        string RestrictedUserPassword)
        : BaseMySqlExecutionStrategySettings(MasterDbConnectionString, RestrictedUserId, RestrictedUserPassword);
}
