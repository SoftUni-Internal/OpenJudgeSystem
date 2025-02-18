namespace OJS.Workers.ExecutionStrategies.Sql.MySql;

using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using global::MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies.Models;

public abstract class BaseMySqlExecutionStrategy<TSettings> : BaseSqlExecutionStrategy<TSettings>
    where TSettings : BaseMySqlExecutionStrategySettings
{
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private const string TimeSpanFormat = "HH:mm:ss";

    protected BaseMySqlExecutionStrategy(
        IOjsSubmission submission,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, settingsProvider, logger)
    {
    }

    protected override async Task<IDbConnection> GetOpenConnection(string databaseName)
    {
        await using (var connection = new MySqlConnection(this.Settings.MasterDbConnectionString))
        {
            await connection.OpenAsync();

            var createDatabaseQuery = $"CREATE DATABASE `{databaseName}`;";

            var createUserQuery = $@"
                    CREATE USER IF NOT EXISTS '{this.RestrictedUserId}'@'%';
                    ALTER USER '{this.RestrictedUserId}' IDENTIFIED BY '{this.Settings.RestrictedUserPassword}'";
                /* SET PASSWORD FOR '{this.restrictedUserId}'@'%'=PASSWORD('{this.restrictedUserPassword}')"; */

            var grandPrivilegesToUserQuery = $@"
                    GRANT ALL PRIVILEGES ON `{databaseName}`.* TO '{this.RestrictedUserId}'@'%';
                    FLUSH PRIVILEGES;";

            var enableLogBinTrustFunctionCreatorsQuery = "SET GLOBAL log_bin_trust_function_creators = 1;";

            this.ExecuteNonQuery(connection, createDatabaseQuery);
            this.ExecuteNonQuery(connection, createUserQuery);
            this.ExecuteNonQuery(connection, grandPrivilegesToUserQuery);
            this.ExecuteNonQuery(connection, enableLogBinTrustFunctionCreatorsQuery);
        }

        var workerConnection = new MySqlConnection(this.BuildWorkerDbConnectionString(databaseName));
        workerConnection.Open();

        return workerConnection;
    }

    protected override async Task DropDatabase(string databaseName)
    {
        await using var connection = new MySqlConnection(this.Settings.MasterDbConnectionString);
        await connection.OpenAsync();

        this.ExecuteNonQuery(connection, $"DROP DATABASE IF EXISTS `{databaseName}`;");
    }

    protected override async Task<IExecutionResult<TestResult>> Execute(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result,
        Action<IDbConnection, TestContext> executionFlow)
    {
        result = await base.Execute(executionContext, result, executionFlow);

        // TODO: Fix concurrent execution of SQL queries.
        // This is a temporary fix for the following error,
        // but the strategy should be reworked to avoid this error in the first place.
        // It happens rarely, but it still happens. Chances are it will not happen again on the next execution.
        var concurrencyExceptionMessage =
            "The ReadAsync method cannot be called when another read operation is pending.";

        if (!result.IsCompiledSuccessfully &&
            (result.CompilerComment
                ?.Trim()
                .Equals(concurrencyExceptionMessage, StringComparison.InvariantCultureIgnoreCase) ?? false))
        {
            result.CompilerComment = "Please, re-submit your solution. If the problem persists, contact an administrator.";
        }

        return result;
    }

    protected override string GetDataRecordFieldValue(IDataRecord dataRecord, int index)
    {
        if (!dataRecord.IsDBNull(index))
        {
            var fieldType = dataRecord.GetFieldType(index);

            if (fieldType == DateTimeType)
            {
                return dataRecord.GetDateTime(index).ToString(DateTimeFormat, CultureInfo.InvariantCulture);
            }

            if (fieldType == TimeSpanType)
            {
                return ((MySqlDataReader)dataRecord)
                    .GetTimeSpan(index)
                    .ToString(TimeSpanFormat, CultureInfo.InvariantCulture);
            }
        }

        return base.GetDataRecordFieldValue(dataRecord, index);
    }

    protected override string BuildWorkerDbConnectionString(string databaseName)
    {
        var userIdRegex = new Regex("UID=.*?;");
        var passwordRegex = new Regex("Password=.*?;");

        var workerDbConnectionString = this.Settings.MasterDbConnectionString;

        workerDbConnectionString =
            userIdRegex.Replace(workerDbConnectionString, $"UID={this.RestrictedUserId};");

        workerDbConnectionString =
            passwordRegex.Replace(workerDbConnectionString, $"Password={this.Settings.RestrictedUserPassword};");

        workerDbConnectionString += workerDbConnectionString.EndsWith(';') ? string.Empty : ";";
        workerDbConnectionString += $"Database={databaseName};Pooling=False;";

        return workerDbConnectionString;
    }
}

public abstract record BaseMySqlExecutionStrategySettings(
    string MasterDbConnectionString,
    string RestrictedUserId,
    string RestrictedUserPassword)
    : BaseSqlExecutionStrategySettings(MasterDbConnectionString, RestrictedUserId, RestrictedUserPassword);