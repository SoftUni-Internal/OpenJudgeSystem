#nullable disable
namespace OJS.Workers.ExecutionStrategies.Sql.SqlServerSingleDatabase;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using System.Data;
using System.Transactions;

using OJS.Workers.ExecutionStrategies.Sql.SqlServerLocalDb;

#pragma warning disable CA1001
public abstract class BaseSqlServerSingleDatabaseExecutionStrategy<TSettings> : BaseSqlServerLocalDbExecutionStrategy<TSettings>
    where TSettings : BaseSqlServerSingleDatabaseExecutionStrategySettings
#pragma warning restore CA1001
{
    private readonly string databaseNameForSubmissionProcessor;

    private TransactionScope transactionScope;

    protected BaseSqlServerSingleDatabaseExecutionStrategy(
        IOjsSubmission submission,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, settingsProvider, logger)
        => this.databaseNameForSubmissionProcessor = $"worker_{this.Settings.SubmissionProcessorIdentifier}_DO_NOT_DELETE";

    protected override string RestrictedUserId => $"{this.GetDatabaseName()}_{this.Settings.RestrictedUserId}";

    private string WorkerDbConnectionString { get; set; }

    protected override async Task<IDbConnection> GetOpenConnection(string databaseName)
    {
        await this.EnsureDatabaseIsSetup();

        this.transactionScope = new TransactionScope();
        var createdDbConnection = new SqlConnection(this.WorkerDbConnectionString);
        createdDbConnection.Open();

        return createdDbConnection;
    }

    protected override async Task DropDatabase(string databaseName)
    {
        if (this.transactionScope != null)
        {
            this.transactionScope.Dispose();
        }
        else
        {
            await base.DropDatabase(databaseName);
        }
    }

    protected override string GetDatabaseName() => this.databaseNameForSubmissionProcessor;

    private async Task EnsureDatabaseIsSetup()
    {
        var databaseName = this.GetDatabaseName();

        await using (var connection = new SqlConnection(this.Settings.MasterDbConnectionString))
        {
            await connection.OpenAsync();

            var setupDatabaseQuery =
                $@"IF DB_ID('{databaseName}') IS NULL
                    BEGIN
                    CREATE DATABASE [{databaseName}];
                    IF NOT EXISTS
                        (SELECT name
                         FROM master.sys.server_principals
                         WHERE name = '{this.RestrictedUserId}')
                        BEGIN
                            CREATE LOGIN [{this.RestrictedUserId}] WITH PASSWORD=N'{this.Settings.RestrictedUserPassword}',
                            DEFAULT_DATABASE=[master],
                            DEFAULT_LANGUAGE=[us_english],
                            CHECK_EXPIRATION=OFF,
                            CHECK_POLICY=ON;
                        END
                    END";

            var setupUserAsOwnerQuery = $@"
                    USE [{databaseName}];
                    IF IS_ROLEMEMBER('db_owner', '{this.RestrictedUserId}') = 0 OR IS_ROLEMEMBER('db_owner', '{this.RestrictedUserId}') is NULL
                    BEGIN
                    CREATE USER [{this.RestrictedUserId}] FOR LOGIN [{this.RestrictedUserId}];
                    ALTER ROLE [db_owner] ADD MEMBER [{this.RestrictedUserId}];
                    END";

            this.ExecuteNonQuery(connection, setupDatabaseQuery);
            this.ExecuteNonQuery(connection, setupUserAsOwnerQuery);
        }

        this.WorkerDbConnectionString = this.BuildWorkerDbConnectionString(databaseName);
    }
}

public abstract record BaseSqlServerSingleDatabaseExecutionStrategySettings(
    string MasterDbConnectionString,
    string RestrictedUserId,
    string RestrictedUserPassword,
    string SubmissionProcessorIdentifier)
    : BaseSqlServerLocalDbExecutionStrategySettings(MasterDbConnectionString, RestrictedUserId,
        RestrictedUserPassword);