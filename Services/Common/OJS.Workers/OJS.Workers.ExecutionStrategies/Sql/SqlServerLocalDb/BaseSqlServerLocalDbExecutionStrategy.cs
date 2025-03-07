namespace OJS.Workers.ExecutionStrategies.Sql.SqlServerLocalDb;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

public abstract class BaseSqlServerLocalDbExecutionStrategy<TSettings> : BaseSqlExecutionStrategy<TSettings>
    where TSettings : BaseSqlServerLocalDbExecutionStrategySettings
{
    private const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
    private const string DateTimeOffsetFormat = "yyyy-MM-dd HH:mm:ss.fffffff zzz";
    private const string TimeSpanFormat = "HH:mm:ss.fffffff";
    private static readonly Type DateTimeOffsetType = typeof(DateTimeOffset);

    protected BaseSqlServerLocalDbExecutionStrategy(
        IOjsSubmission submission,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, settingsProvider, logger)
    {
    }

    protected override async Task<IDbConnection> GetOpenConnection(string databaseName)
    {
        var databaseFilePath = $"{this.WorkingDirectory}\\{databaseName}.mdf";

        await using (var connection = new SqlConnection(this.Settings.MasterDbConnectionString))
        {
            await connection.OpenAsync();

            var createDatabaseQuery =
                $"CREATE DATABASE [{databaseName}] ON PRIMARY (NAME=N'{databaseName}', FILENAME=N'{databaseFilePath}');";

            var createLoginQuery = $@"
                    IF NOT EXISTS (SELECT name FROM master.sys.server_principals WHERE name=N'{this.RestrictedUserId}')
                    BEGIN
                    CREATE LOGIN [{this.RestrictedUserId}] WITH PASSWORD=N'{this.Settings.RestrictedUserPassword}',
                    DEFAULT_DATABASE=[master],
                    DEFAULT_LANGUAGE=[us_english],
                    CHECK_EXPIRATION=OFF,
                    CHECK_POLICY=ON;
                    END;";

            var createUserAsDbOwnerQuery = $@"
                    USE [{databaseName}];
                    CREATE USER [{this.RestrictedUserId}] FOR LOGIN [{this.RestrictedUserId}];
                    ALTER ROLE [db_owner] ADD MEMBER [{this.RestrictedUserId}];";

            this.ExecuteNonQuery(connection, createDatabaseQuery);

            this.ExecuteNonQuery(connection, createLoginQuery);

            this.ExecuteNonQuery(connection, createUserAsDbOwnerQuery);
        }

        var createdDbConnectionString = this.BuildWorkerDbConnectionString(databaseName);

        var createdDbConnection = new SqlConnection(createdDbConnectionString);
        createdDbConnection.Open();

        return createdDbConnection;
    }

    protected override async Task DropDatabase(string databaseName)
    {
        await using var connection = new SqlConnection(this.Settings.MasterDbConnectionString);
        await connection.OpenAsync();

        var dropDatabaseQuery = $@"
                    IF EXISTS (SELECT name FROM master.sys.databases WHERE name=N'{databaseName}')
                    BEGIN
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{databaseName}];
                    END;";

        this.ExecuteNonQuery(connection, dropDatabaseQuery);
    }

    protected override string BuildWorkerDbConnectionString(string databaseName)
    {
        var userIdRegex = new Regex("User Id=.*?;");
        var passwordRegex = new Regex("Password=.*?;");

        var createdDbConnectionString = this.Settings.MasterDbConnectionString;

        createdDbConnectionString =
            userIdRegex.Replace(createdDbConnectionString, $"User Id={this.RestrictedUserId};");

        createdDbConnectionString =
            passwordRegex.Replace(createdDbConnectionString, $"Password={this.Settings.RestrictedUserPassword};");

        createdDbConnectionString += createdDbConnectionString.EndsWith(';') ? string.Empty : ";";
        createdDbConnectionString += $"Database={databaseName};Pooling=False;";

        return createdDbConnectionString;
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

            if (fieldType == DateTimeOffsetType)
            {
                return ((SqlDataReader)dataRecord)
                    .GetDateTimeOffset(index)
                    .ToString(DateTimeOffsetFormat, CultureInfo.InvariantCulture);
            }

            if (fieldType == TimeSpanType)
            {
                return ((SqlDataReader)dataRecord)
                    .GetTimeSpan(index)
                    .ToString(TimeSpanFormat, CultureInfo.InvariantCulture);
            }
        }

        return base.GetDataRecordFieldValue(dataRecord, index);
    }
}

public abstract record BaseSqlServerLocalDbExecutionStrategySettings(
    string MasterDbConnectionString,
    string RestrictedUserId,
    string RestrictedUserPassword)
    : BaseSqlExecutionStrategySettings(MasterDbConnectionString, RestrictedUserId, RestrictedUserPassword);
