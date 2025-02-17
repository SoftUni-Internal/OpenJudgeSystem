namespace OJS.Services.Worker.Business.Implementations;

using Microsoft.Extensions.Options;
using OJS.Services.Worker.Models.Configuration;
using OJS.Workers.Common;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies;
using OJS.Workers.ExecutionStrategies.CPlusPlus;
using OJS.Workers.ExecutionStrategies.CSharp.DotNetCore;
using OJS.Workers.ExecutionStrategies.Golang;
using OJS.Workers.ExecutionStrategies.Java;
using OJS.Workers.ExecutionStrategies.NodeJs;
using OJS.Workers.ExecutionStrategies.Python;
using OJS.Workers.ExecutionStrategies.Sql.MySql;
using OJS.Workers.ExecutionStrategies.Sql.PostgreSql;
using OJS.Workers.ExecutionStrategies.Sql.SqlServerSingleDatabase;
using System;
using OJS.Workers.ExecutionStrategies.NodeJs.Typescript;

public class ExecutionStrategySettingsProvider : IExecutionStrategySettingsProvider
{
    private readonly OjsWorkersConfig settings;
    private readonly string submissionProcessorIdentifier;

    public ExecutionStrategySettingsProvider(
        IOptions<OjsWorkersConfig> ojsWorkersConfigAccessor,
        IOptions<ApplicationConfig> appConfigAccessor)
    {
        this.settings = ojsWorkersConfigAccessor.Value;
        this.submissionProcessorIdentifier = appConfigAccessor.Value.SubmissionsProcessorIdentifierNumber.ToString();
    }

    public TSettings? GetSettings<TSettings>(IOjsSubmission submission)
        where TSettings : class, IExecutionStrategySettings
    {
        var executionStrategyType = submission.ExecutionStrategyType;
        return executionStrategyType switch
        {
            ExecutionStrategyType.CompileExecuteAndCheck => new
                CompileExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.MsBuildBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.MsBuildBaseMemoryUsedInBytes))

                as TSettings,
            ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck or
            ExecutionStrategyType.NodeJsV20PreprocessExecuteAndCheck => new
                NodeJsPreprocessExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.NodeJsBaseTimeUsedInMilliseconds * 2),
                    GetBaseMemoryUsed(submission, this.settings.NodeJsBaseMemoryUsedInBytes),
                    this.GetNodeJsExecutablePath(executionStrategyType),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.UnderscoreModulePath))

                as TSettings,
            ExecutionStrategyType.TypeScriptV20PreprocessExecuteAndCheck => new
                TypeScriptPreprocessExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.NodeJsBaseTimeUsedInMilliseconds * 2),
                    GetBaseMemoryUsed(submission, this.settings.NodeJsBaseMemoryUsedInBytes),
                    this.GetTypeScriptExecutablePath(),
                    this.GetNodeJsExecutablePath(executionStrategyType),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.UnderscoreModulePath))

            as TSettings,
            ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck or
            ExecutionStrategyType.Java21PreprocessCompileExecuteAndCheck => new
                JavaPreprocessCompileExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.JavaBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.JavaBaseMemoryUsedInBytes),
                    this.GetJavaExecutablePath(executionStrategyType),
                    this.GetJavaLibsPath(executionStrategyType),
                    this.settings.JavaBaseUpdateTimeOffsetInMilliseconds)

                as TSettings,
            ExecutionStrategyType.CheckOnly => new
                CheckOnlyExecutionStrategySettings(0, 0)

                as TSettings,
            ExecutionStrategyType.JavaZipFileCompileExecuteAndCheck or
            ExecutionStrategyType.Java21ZipFileCompileExecuteAndCheck => new
                JavaZipFileCompileExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.JavaBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.JavaBaseMemoryUsedInBytes),
                    this.GetJavaExecutablePath(executionStrategyType),
                    this.GetJavaLibsPath(executionStrategyType),
                    this.settings.JavaBaseUpdateTimeOffsetInMilliseconds)

                as TSettings,
            ExecutionStrategyType.PythonExecuteAndCheck => new
                PythonExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.PythonBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.PythonBaseMemoryUsedInBytes),
                    this.settings.PythonExecutablePath)

                as TSettings,
            ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha or
            ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunUnitTestsWithMocha => new
                NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.NodeJsBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.NodeJsBaseMemoryUsedInBytes),
                    this.GetNodeJsExecutablePath(executionStrategyType),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.UnderscoreModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.MochaModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.ChaiModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.SinonModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.SinonChaiModulePath))

                as TSettings,
            ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests or
            ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunJsDomUnitTests => new
                NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.NodeJsBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.NodeJsBaseMemoryUsedInBytes),
                    this.GetNodeJsExecutablePath(executionStrategyType),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.UnderscoreModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.MochaModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.ChaiModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.SinonModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.SinonChaiModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.JsDomModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.JQueryModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.HandlebarsModulePath))
                as TSettings,

            ExecutionStrategyType.NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy => new
                    NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategySettings(
                        GetBaseTimeUsed(submission, this.settings.NodeJsBaseTimeUsedInMilliseconds),
                        GetBaseMemoryUsed(submission, this.settings.NodeJsBaseMemoryUsedInBytes),
                        this.GetNodeJsExecutablePath(executionStrategyType),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.UnderscoreModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.MochaModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.ChaiModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.SinonModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.SinonChaiModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.SinonJsDomModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.JsDomModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.JQueryModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.HandlebarsModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.BabelCoreModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.ReactJsxPluginPath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.ReactModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.ReactDomModulePath),
                        this.GetNodeResourcePath(executionStrategyType, this.settings.NodeFetchModulePath))

                as TSettings,
            ExecutionStrategyType.MySqlPrepareDatabaseAndRunQueries => new
                MySqlPrepareDatabaseAndRunQueriesExecutionStrategySettings(
                    this.settings.MySqlSysDbConnectionString,
                    this.settings.MySqlRestrictedUserId,
                    this.settings.MySqlRestrictedUserPassword)

                as TSettings,
            ExecutionStrategyType.MySqlRunQueriesAndCheckDatabase => new
                MySqlRunQueriesAndCheckDatabaseExecutionStrategySettings(
                    this.settings.MySqlSysDbConnectionString,
                    this.settings.MySqlRestrictedUserId,
                    this.settings.MySqlRestrictedUserPassword)

                as TSettings,
            ExecutionStrategyType.MySqlRunSkeletonRunQueriesAndCheckDatabase => new
                MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings(
                    this.settings.MySqlSysDbConnectionString,
                    this.settings.MySqlRestrictedUserId,
                    this.settings.MySqlRestrictedUserPassword)

                as TSettings,
            ExecutionStrategyType.NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy or
            ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy => new
                NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.NodeJsBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.NodeJsBaseMemoryUsedInBytes),
                    this.GetNodeJsExecutablePath(executionStrategyType),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.UnderscoreModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.MochaModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.ChaiModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.SinonModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.SinonChaiModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.JsDomModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.JQueryModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.HandlebarsModulePath))

                as TSettings,
            ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy or
            ExecutionStrategyType.NodeJsV20ZipExecuteHtmlAndCssStrategy => new
                NodeJsZipExecuteHtmlAndCssStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.NodeJsBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.NodeJsBaseMemoryUsedInBytes),
                    this.GetNodeJsExecutablePath(executionStrategyType),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.UnderscoreModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.MochaModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.ChaiModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.SinonModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.SinonChaiModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.JsDomModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.JQueryModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.BootstrapModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.BootstrapCssPath))

                as TSettings,
            ExecutionStrategyType.JavaProjectTestsExecutionStrategy or
            ExecutionStrategyType.Java21ProjectTestsExecutionStrategy => new
                JavaProjectTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.JavaBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.JavaBaseMemoryUsedInBytes),
                    this.GetJavaExecutablePath(executionStrategyType),
                    this.GetJavaLibsPath(executionStrategyType),
                    this.settings.JavaBaseUpdateTimeOffsetInMilliseconds)

                as TSettings,
            ExecutionStrategyType.CPlusPlusZipFileExecutionStrategy => new
                CPlusPlusZipFileExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.GPlusPlusBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.GPlusPlusBaseMemoryUsedInBytes))

                as TSettings,
            ExecutionStrategyType.JavaUnitTestsExecutionStrategy or
            ExecutionStrategyType.Java21UnitTestsExecutionStrategy => new
                JavaUnitTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.JavaBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.JavaBaseMemoryUsedInBytes),
                    this.GetJavaExecutablePath(executionStrategyType),
                    this.GetJavaLibsPath(executionStrategyType),
                    this.settings.JavaBaseUpdateTimeOffsetInMilliseconds)

                as TSettings,
            ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy => new
                CPlusPlusCompileExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.GPlusPlusBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.GPlusPlusBaseMemoryUsedInBytes))

                as TSettings,
            ExecutionStrategyType.JavaSpringAndHibernateProjectExecutionStrategy => new
                JavaSpringAndHibernateProjectExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.JavaBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.JavaBaseMemoryUsedInBytes),
                    this.GetJavaExecutablePath(executionStrategyType),
                    this.GetJavaLibsPath(executionStrategyType),
                    this.settings.JavaBaseUpdateTimeOffsetInMilliseconds,
                    this.settings.MavenPath,
                    this.settings.JavaSpringAndHibernateStrategyPomFilePath)

            as TSettings,
            ExecutionStrategyType.Java21SpringAndHibernateProjectExecution => new
                JavaSpringAndHibernateProjectExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.JavaBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.JavaBaseMemoryUsedInBytes),
                    this.GetJavaExecutablePath(executionStrategyType),
                    this.GetJavaLibsPath(executionStrategyType),
                    this.settings.JavaBaseUpdateTimeOffsetInMilliseconds,
                    this.settings.MavenPath,
                    this.settings.Java21SpringAndHibernateStrategyPomFilePath)

                as TSettings,
            ExecutionStrategyType.DotNetCoreProjectExecutionStrategy => new
                DotNetCoreProjectExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCliBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCliBaseMemoryUsedInBytes))

                as TSettings,
            ExecutionStrategyType.DotNetCoreProjectTestsExecutionStrategy => new
                DotNetCoreProjectTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCliBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCliBaseMemoryUsedInBytes),
                    "netcoreapp3.1",
                    "3.1.4",
                    "3.1.4")

                as TSettings,
            ExecutionStrategyType.DotNetCoreCompileExecuteAndCheck => new
                DotNetCoreCompileExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCscBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCscBaseMemoryUsedInBytes),
                    this.settings.DotNetCore3RuntimeVersion)

                as TSettings,
            ExecutionStrategyType.DotNetCoreUnitTestsExecutionStrategy => new
                DotNetCoreUnitTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCliBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCliBaseMemoryUsedInBytes),
                    "netcoreapp3.1",
                    "3.1.4",
                    "3.1.4")

                as TSettings,
            ExecutionStrategyType.PythonUnitTests => new
                PythonProjectUnitTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.PythonBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.PythonBaseMemoryUsedInBytes),
                    this.settings.PythonExecutablePath)

                as TSettings,
            ExecutionStrategyType.PythonCodeExecuteAgainstUnitTests => new
                PythonCodeExecuteAgainstUnitTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.PythonBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.PythonBaseMemoryUsedInBytes),
                    this.settings.PythonExecutablePath)

                as TSettings,
            ExecutionStrategyType.PythonProjectTests => new
                PythonProjectTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.PythonBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.PythonBaseMemoryUsedInBytes),
                    this.settings.PythonExecutablePath)

                as TSettings,
            ExecutionStrategyType.PythonProjectUnitTests => new
                PythonProjectUnitTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.PythonBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.PythonBaseMemoryUsedInBytes),
                    this.settings.PythonExecutablePath)

                as TSettings,
            ExecutionStrategyType.SqlServerSingleDatabasePrepareDatabaseAndRunQueries => new
                SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategySettings(
                    this.settings.SqlServerLocalDbMasterDbConnectionString,
                    this.settings.SqlServerLocalDbRestrictedUserId,
                    this.settings.SqlServerLocalDbRestrictedUserPassword,
                    this.submissionProcessorIdentifier)

                as TSettings,
            ExecutionStrategyType.SqlServerSingleDatabaseRunQueriesAndCheckDatabase => new
                SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategySettings(
                    this.settings.SqlServerLocalDbMasterDbConnectionString,
                    this.settings.SqlServerLocalDbRestrictedUserId,
                    this.settings.SqlServerLocalDbRestrictedUserPassword,
                    this.submissionProcessorIdentifier)

                as TSettings,
            ExecutionStrategyType.SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase => new
                SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings(
                    this.settings.SqlServerLocalDbMasterDbConnectionString,
                    this.settings.SqlServerLocalDbRestrictedUserId,
                    this.settings.SqlServerLocalDbRestrictedUserPassword,
                    this.submissionProcessorIdentifier)

                as TSettings,
            ExecutionStrategyType.RunSpaAndExecuteMochaTestsExecutionStrategy or
            ExecutionStrategyType.NodeJsV20RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests or
            ExecutionStrategyType.RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests => new
                RunSpaAndExecuteMochaTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.NodeJsBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.NodeJsBaseMemoryUsedInBytes),
                    this.settings.PythonExecutablePath,
                    this.GetNodeResourcePath(executionStrategyType, this.settings.JsProjNodeModules),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.MochaModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.ChaiModulePath),
                    this.GetNodeResourcePath(executionStrategyType, this.settings.PlaywrightChromiumModulePath))

                as TSettings,
            ExecutionStrategyType.GolangCompileExecuteAndCheck => new
                GolangCompileExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.GolangBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.GolangBaseMemoryUsedInBytes))

                as TSettings,
            ExecutionStrategyType.DotNetCore6ProjectTestsExecutionStrategy => new
                DotNetCoreProjectTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCliBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCliBaseMemoryUsedInBytes),
                    "net6.0",
                    "6.0.1",
                    "6.0.1")

                as TSettings,
            ExecutionStrategyType.DotNetCore5ProjectTestsExecutionStrategy => new
                DotNetCoreProjectTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCliBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCliBaseMemoryUsedInBytes),
                    "net5.0",
                    "5.0.13",
                    "5.0.13")

                as TSettings,
            ExecutionStrategyType.DotNetCore5CompileExecuteAndCheck => new
                DotNetCoreCompileExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCscBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCscBaseMemoryUsedInBytes),
                    this.settings.DotNetCore5RuntimeVersion)

                as TSettings,
            ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck => new
                DotNetCoreCompileExecuteAndCheckExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCscBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCscBaseMemoryUsedInBytes),
                    this.settings.DotNetCore6RuntimeVersion)

                as TSettings,
            ExecutionStrategyType.DotNetCore5UnitTestsExecutionStrategy => new
                DotNetCoreUnitTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCliBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCliBaseMemoryUsedInBytes),
                    "net5.0",
                    "5.0.13",
                    "5.0.13")

                as TSettings,
            ExecutionStrategyType.DotNetCore6UnitTestsExecutionStrategy => new
                DotNetCoreUnitTestsExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCliBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCliBaseMemoryUsedInBytes),
                    "net6.0",
                    "6.0.1",
                    "6.0.1")

                as TSettings,
            ExecutionStrategyType.DotNetCore5ProjectExecutionStrategy => new
                DotNetCoreProjectExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCliBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCliBaseMemoryUsedInBytes))

                as TSettings,
            ExecutionStrategyType.DotNetCore6ProjectExecutionStrategy => new
                DotNetCoreProjectExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.DotNetCliBaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.DotNetCliBaseMemoryUsedInBytes))

                as TSettings,
            ExecutionStrategyType.PostgreSqlPrepareDatabaseAndRunQueries => new
                PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategySettings(
                    this.settings.PostgreSqlMasterDbConnectionString,
                    this.settings.PostgreSqlRestrictedUserId,
                    this.settings.PostgreSqlRestrictedUserPassword,
                    this.submissionProcessorIdentifier)

                as TSettings,
            ExecutionStrategyType.PostgreSqlRunQueriesAndCheckDatabase => new
                PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategySettings(
                    this.settings.PostgreSqlMasterDbConnectionString,
                    this.settings.PostgreSqlRestrictedUserId,
                    this.settings.PostgreSqlRestrictedUserPassword,
                    this.submissionProcessorIdentifier)

                as TSettings,
            ExecutionStrategyType.PostgreSqlRunSkeletonRunQueriesAndCheckDatabase => new
                PostgreSqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings(
                    this.settings.PostgreSqlMasterDbConnectionString,
                    this.settings.PostgreSqlRestrictedUserId,
                    this.settings.PostgreSqlRestrictedUserPassword,
                    this.submissionProcessorIdentifier)

                as TSettings,
            ExecutionStrategyType.PythonDjangoOrmExecutionStrategy => new
                PythonDjangoOrmExecutionStrategySettings(
                    GetBaseTimeUsed(submission, this.settings.PythonV311BaseTimeUsedInMilliseconds),
                    GetBaseMemoryUsed(submission, this.settings.PythonV311BaseMemoryUsedInBytes),
                    this.settings.PythonExecutablePathV311,
                    this.settings.PipExecutablePathV311,
                    this.settings.PythonV311InstallPackagesTimeUsedInMilliseconds)

                as TSettings,
            ExecutionStrategyType.DoNothing => new DoNothingExecutionStrategySettings() as TSettings,
            ExecutionStrategyType.NotFound => throw new ArgumentException(
                $"Cannot get settings for {ExecutionStrategyType.NotFound} strategy.",
                nameof(executionStrategyType)),
            _ => throw new ArgumentOutOfRangeException(nameof(executionStrategyType), executionStrategyType, null),
        };
    }

    private static int GetBaseTimeUsed(IOjsSubmission submission, int defaultBaseTimeUsed)
        => submission.ExecutionStrategyBaseTimeLimit ?? defaultBaseTimeUsed;

    private static int GetBaseMemoryUsed(IOjsSubmission submission, int defaultBaseMemoryUsed)
        => submission.ExecutionStrategyBaseMemoryLimit ?? defaultBaseMemoryUsed;

    private static bool IsJava21(ExecutionStrategyType type)
        => type.ToString().Contains("21");

    private static bool IsNode20(ExecutionStrategyType type)
        => type.ToString().Contains("20");

    private string GetJavaExecutablePath(ExecutionStrategyType strategyType)
        => IsJava21(strategyType)
            ? this.settings.Java21ExecutablePath
            : this.settings.JavaExecutablePath;

    private string GetJavaLibsPath(ExecutionStrategyType strategyType)
        => IsJava21(strategyType)
            ? this.settings.Java21LibsPath
            : this.settings.JavaLibsPath;

    private string GetNodeJsExecutablePath(ExecutionStrategyType strategyType)
        => IsNode20(strategyType)
            ? this.settings.NodeJs20ExecutablePath
            : this.settings.NodeJsExecutablePath;

    private string GetTypeScriptExecutablePath()
        => this.settings.TypeScriptExecutablePath;

    private string GetNodeResourcePath(ExecutionStrategyType strategyType, string template)
    {
        var resourcesPath = IsNode20(strategyType)
            ? this.settings.Node20ResourcesPath
            : this.settings.NodeResourcesPath;

        return template.Replace(this.settings.NodeResourcesPathPlaceholder, resourcesPath);
    }
}