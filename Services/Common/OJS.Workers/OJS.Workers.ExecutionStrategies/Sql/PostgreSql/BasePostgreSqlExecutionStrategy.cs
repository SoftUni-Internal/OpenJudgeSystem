#nullable disable

namespace OJS.Workers.ExecutionStrategies.Sql.PostgreSql;

using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Npgsql;
using OJS.Workers.Common;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies.Models;

public abstract class BasePostgreSqlExecutionStrategy<TSettings> : BaseSqlExecutionStrategy<TSettings>
    where TSettings : BasePostgreSqlExecutionStrategySettings
{
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
    private const string TimeSpanFormat = @"hh\:mm\:ss";

    private readonly string databaseNameForSubmissionProcessor;
    private string workerDbConnectionString;
    private NpgsqlConnection currentConnection;
    private bool isDisposed;

    protected BasePostgreSqlExecutionStrategy(
        IOjsSubmission submission,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, settingsProvider, logger)
        => this.databaseNameForSubmissionProcessor = $"worker_{this.Settings.SubmissionProcessorIdentifier}_do_not_delete";

    protected override string RestrictedUserId => $"{this.GetDatabaseName()}_{base.RestrictedUserId}";

    protected override async Task<IDbConnection> GetOpenConnection(string databaseName)
    {
        if (this.currentConnection != null)
        {
            return await this.CreateConnection();
        }

        this.EnsureDatabaseIsSetup();

        if (this.currentConnection != null && this.isDisposed)
        {
            this.currentConnection.Dispose();
        }

        return await this.CreateConnection();
    }

    protected override string GetDatabaseName() => this.databaseNameForSubmissionProcessor;

    protected override Task DropDatabase(string databaseName) => Task.CompletedTask;

    protected override string BuildWorkerDbConnectionString(string databaseName)
    {
        var userIdRegex = new Regex("UserId=.*?;");
        var passwordRegex = new Regex("Password=.*?;");

        var createdDbConnectionString = this.Settings.MasterDbConnectionString;

        createdDbConnectionString =
            userIdRegex.Replace(createdDbConnectionString, $"User Id={this.RestrictedUserId};");

        createdDbConnectionString =
            passwordRegex.Replace(createdDbConnectionString, $"Password={this.Settings.RestrictedUserPassword};");

        createdDbConnectionString += createdDbConnectionString.EndsWith(';') ? string.Empty : ";";
        createdDbConnectionString += $"Database={databaseName};";

        return createdDbConnectionString;
    }

    protected override async Task<IExecutionResult<TestResult>> Execute(IExecutionContext<TestsInputModel> executionContext, IExecutionResult<TestResult> result, Action<IDbConnection, TestContext> executionFlow)
    {
        result.IsCompiledSuccessfully = true;

        try
        {
            using var connection = await this.GetOpenConnection(this.GetDatabaseName());
            this.ExecuteBeforeTests(connection, executionContext);

            foreach (var test in executionContext.Input.Tests)
            {
                this.ExecuteBeforeEachTest(connection, executionContext, test);
                executionFlow(connection, test);
                this.ExecuteAfterEachTest(connection, executionContext, test);
            }

            this.ExecuteAfterTests(connection, executionContext);
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(this.GetDatabaseName()))
            {
                using var connection = await this.GetOpenConnection(this.GetDatabaseName());
                this.CleanUpDb(connection);
            }

            result.IsCompiledSuccessfully = false;
            result.CompilerComment = ex.Message;
        }

        return result;
    }

    protected virtual void ExecuteBeforeTests(IDbConnection connection, IExecutionContext<TestsInputModel>
        executionContext)
    {
    }

    protected virtual void ExecuteAfterTests(IDbConnection connection, IExecutionContext<TestsInputModel>
        executionContext) => this.CleanUpDb(connection);

    protected virtual void ExecuteBeforeEachTest(IDbConnection connection, IExecutionContext<TestsInputModel> executionContext, TestContext test)
    {
    }

    protected virtual void ExecuteAfterEachTest(IDbConnection connection, IExecutionContext<TestsInputModel> executionContext, TestContext test)
    {
    }

    protected override bool ExecuteNonQuery(IDbConnection connection, string commandText, int timeLimit =
        DefaultTimeLimit)
    {
        try
        {
            using var command = connection.CreateCommand();
            command.CommandTimeout = timeLimit / 1000;
            command.CommandText = this.FixCommandText(commandText);

            command.ExecuteNonQuery();
        }
        catch (TimeoutException)
        {
            return false;
        }

        return true;
    }

    protected override SqlResult ExecuteReader(
        IDbConnection connection,
        string commandText,
        int timeLimit = DefaultTimeLimit)
    {
        var sqlTestResult = new SqlResult { Completed = true };

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandTimeout = timeLimit / 1000;

            using var reader = command.ExecuteReader();
            do
            {
                while (reader.Read())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var fieldValue = this.GetDataRecordFieldValue(reader, i);

                        sqlTestResult.Results.Add(fieldValue);
                    }
                }
            }
            while (reader.NextResult());
        }
        catch (TimeoutException)
        {
            sqlTestResult.Completed = false;
        }

        return sqlTestResult;
    }

    protected override string GetDataRecordFieldValue(IDataRecord dataRecord, int index)
    {
        if (dataRecord.IsDBNull(index))
        {
            return base.GetDataRecordFieldValue(dataRecord, index);
        }

        var fieldType = dataRecord.GetFieldType(index);

        if (fieldType == DateTimeType)
        {
            return dataRecord.GetDateTime(index)
                .ToString(DateTimeFormat, CultureInfo.InvariantCulture);
        }

        if (fieldType == TimeSpanType)
        {
            return ((NpgsqlDataReader)dataRecord)
                .GetTimeSpan(index)
                .ToString(TimeSpanFormat, CultureInfo.InvariantCulture);
        }

        return base.GetDataRecordFieldValue(dataRecord, index);
    }

    private void CleanUpDb(IDbConnection connection)
    {
        var dropPublicScheme = @"
                DROP SCHEMA public CASCADE;
                CREATE SCHEMA public;
            ";

        var grantPermissions = @"
                GRANT ALL ON SCHEMA public TO postgres;
                GRANT ALL ON SCHEMA public TO public;
            ";

        this.ExecuteNonQuery(connection, dropPublicScheme);
        this.ExecuteNonQuery(connection, grantPermissions);
    }

    private void EnsureDatabaseIsSetup()
    {
        var databaseName = this.GetDatabaseName();
        var connectionString = this.Settings.MasterDbConnectionString;

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var createUserQuery = $@"
                    DO
                    $$
                    BEGIN
                      IF NOT EXISTS (SELECT * FROM pg_user WHERE usename = '{this.RestrictedUserId}') THEN
                        CREATE USER {this.RestrictedUserId} WITH PASSWORD '{this.Settings.RestrictedUserPassword}';
                      end if;
                    end
                    $$
                    ;
                ";

            this.ExecuteNonQuery(connection, createUserQuery);
            try
            {
                var setupDatabaseQuery = $@"
                        CREATE DATABASE {databaseName} OWNER {this.RestrictedUserId}
                    ";

                this.ExecuteNonQuery(connection, setupDatabaseQuery);
            }
            catch (Exception)
            {
                // PG doesn't support CREATE DATABASE IF Exists
                this.CleanUpDb(connection);
            }
        }

        this.workerDbConnectionString = this.BuildWorkerDbConnectionString(databaseName);
    }

    private async Task<IDbConnection> CreateConnection()
    {
        // Connection is not used in a "using" block because the connection variable is saved
        // in currentConnection and gets disposed otherwise
        var connection = new NpgsqlConnection(this.workerDbConnectionString);
        await connection.OpenAsync();

        connection.StateChange += (_, args) =>
        {
            if (args.CurrentState is ConnectionState.Closed or ConnectionState.Broken)
            {
                this.isDisposed = true;
            }
        };

        this.currentConnection = connection;
        this.isDisposed = false;

        return this.currentConnection;
    }
}

public abstract record BasePostgreSqlExecutionStrategySettings(
    string MasterDbConnectionString,
    string RestrictedUserId,
    string RestrictedUserPassword,
    string SubmissionProcessorIdentifier)
    : BaseSqlExecutionStrategySettings(MasterDbConnectionString, RestrictedUserId, RestrictedUserPassword);