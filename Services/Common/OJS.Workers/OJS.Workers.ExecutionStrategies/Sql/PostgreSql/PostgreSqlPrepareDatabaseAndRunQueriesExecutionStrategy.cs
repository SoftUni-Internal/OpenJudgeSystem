﻿namespace OJS.Workers.ExecutionStrategies.Sql.PostgreSql
{
    using System.Data;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;

    public class PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategy<TSettings> : BasePostgreSqlExecutionStrategy<TSettings>
        where TSettings : PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategySettings
    {
        public PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategy(
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
                    var sqlTestResult = this.ExecuteReader(connection, executionContext.Code, executionContext.TimeLimit);
                    ProcessSqlResult(sqlTestResult, executionContext, test, result);
                });

        protected override void ExecuteBeforeTests(
            IDbConnection connection,
            IExecutionContext<TestsInputModel> executionContext)
            => this.ExecuteNonQuery(connection, executionContext.Input.TaskSkeletonAsString);

        protected override void ExecuteBeforeEachTest(
            IDbConnection connection,
            IExecutionContext<TestsInputModel> executionContext,
            TestContext test)
        {
            this.StartTransaction(connection);
            this.ExecuteNonQuery(connection, test.Input);
        }

        protected override void ExecuteAfterEachTest(
            IDbConnection connection,
            IExecutionContext<TestsInputModel> executionContext,
            TestContext test)
            => this.RollbackTransaction(connection);

        private void StartTransaction(IDbConnection connection)
        {
            var start = @"BEGIN;";
            this.ExecuteNonQuery(connection, start);
        }

        private void RollbackTransaction(IDbConnection connection)
        {
            var rollback = @"ROLLBACK;";
            this.ExecuteNonQuery(connection, rollback);
        }
    }

#pragma warning disable SA1402
    public class PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategySettings : BasePostgreSqlExecutionStrategySettings
#pragma warning restore SA1402
    {
    }
}