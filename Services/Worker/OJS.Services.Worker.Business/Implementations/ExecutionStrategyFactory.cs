#nullable disable
namespace OJS.Services.Worker.Business.Implementations
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using OJS.Services.Worker.Business.Extensions;
    using OJS.Services.Worker.Models.Configuration;
    using System;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies;
    using OJS.Workers.ExecutionStrategies.CPlusPlus;
    using OJS.Workers.ExecutionStrategies.CSharp.DotNetCore;
    using OJS.Workers.ExecutionStrategies.Golang;
    using OJS.Workers.ExecutionStrategies.Java;
    using OJS.Workers.ExecutionStrategies.NodeJs;
    using OJS.Workers.ExecutionStrategies.NodeJs.Typescript;
    using OJS.Workers.ExecutionStrategies.Python;
    using OJS.Workers.ExecutionStrategies.Sql.MySql;
    using OJS.Workers.ExecutionStrategies.Sql.PostgreSql;
    using OJS.Workers.ExecutionStrategies.Sql.SqlServerSingleDatabase;
    using OJS.Workers.Executors.Implementations;
    using OJS.Workers.Executors;

    public class ExecutionStrategyFactory(
        ICompilerFactory compilerFactory,
        IExecutionStrategySettingsProvider executionStrategySettingsProvider,
        ILoggerFactory loggerFactory,
        IOptions<SubmissionExecutionConfig> submissionExecutionConfigAccessor)
        : IExecutionStrategyFactory
    {
        private readonly SubmissionExecutionConfig submissionExecutionConfig = submissionExecutionConfigAccessor.Value;

        public IExecutionStrategy CreateExecutionStrategy(IOjsSubmission submission)
        {
            IExecutionStrategy executionStrategy;
            var submissionId = (int)submission.Id;
            var verbosely = submission.Verbosely;
            var logFileMaxBytes = this.submissionExecutionConfig.SubmissionVerboseLogFileMaxBytes;
            var tasksService = new TasksService();
            var processExecutorFactory = new ProcessExecutorFactory(
                tasksService,
                loggerFactory.CreateLogger<StandardProcessExecutor>());

            switch (submission.ExecutionStrategyType)
            {
                case ExecutionStrategyType.CompileExecuteAndCheck:
                    executionStrategy = new CompileExecuteAndCheckExecutionStrategy<CompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<CompileExecuteAndCheckExecutionStrategy<CompileExecuteAndCheckExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy:
                    executionStrategy = new CPlusPlusCompileExecuteAndCheckExecutionStrategy<CPlusPlusCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<CPlusPlusCompileExecuteAndCheckExecutionStrategy<CPlusPlusCompileExecuteAndCheckExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.CPlusPlusZipFileExecutionStrategy:
                    executionStrategy = new CPlusPlusZipFileExecutionStrategy<CPlusPlusZipFileExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<CPlusPlusZipFileExecutionStrategy<CPlusPlusZipFileExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.DotNetCoreCompileExecuteAndCheck:
                case ExecutionStrategyType.DotNetCore5CompileExecuteAndCheck:
                case ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck:
                    executionStrategy = new DotNetCoreCompileExecuteAndCheckExecutionStrategy<DotNetCoreCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<DotNetCoreCompileExecuteAndCheckExecutionStrategy<DotNetCoreCompileExecuteAndCheckExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.GolangCompileExecuteAndCheck:
                    executionStrategy = new GolangCompileExecuteAndCheckExecutionStrategy<GolangCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<GolangCompileExecuteAndCheckExecutionStrategy<GolangCompileExecuteAndCheckExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.DotNetCoreUnitTestsExecutionStrategy:
                case ExecutionStrategyType.DotNetCore5UnitTestsExecutionStrategy:
                case ExecutionStrategyType.DotNetCore6UnitTestsExecutionStrategy:
                    executionStrategy = new DotNetCoreUnitTestsExecutionStrategy<DotNetCoreUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<DotNetCoreUnitTestsExecutionStrategy<DotNetCoreUnitTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.DotNetCoreProjectExecutionStrategy:
                case ExecutionStrategyType.DotNetCore5ProjectExecutionStrategy:
                case ExecutionStrategyType.DotNetCore6ProjectExecutionStrategy:
                    executionStrategy = new DotNetCoreProjectExecutionStrategy<DotNetCoreProjectExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<DotNetCoreProjectExecutionStrategy<DotNetCoreProjectExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.DotNetCoreProjectTestsExecutionStrategy:
                case ExecutionStrategyType.DotNetCore5ProjectTestsExecutionStrategy:
                case ExecutionStrategyType.DotNetCore6ProjectTestsExecutionStrategy:
                    executionStrategy = new DotNetCoreProjectTestsExecutionStrategy<DotNetCoreProjectTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<DotNetCoreProjectTestsExecutionStrategy<DotNetCoreProjectTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck:
                case ExecutionStrategyType.Java21PreprocessCompileExecuteAndCheck:
                    executionStrategy = new JavaPreprocessCompileExecuteAndCheckExecutionStrategy<JavaPreprocessCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<JavaPreprocessCompileExecuteAndCheckExecutionStrategy<JavaPreprocessCompileExecuteAndCheckExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.JavaZipFileCompileExecuteAndCheck:
                case ExecutionStrategyType.Java21ZipFileCompileExecuteAndCheck:
                    executionStrategy = new JavaZipFileCompileExecuteAndCheckExecutionStrategy<JavaZipFileCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<JavaZipFileCompileExecuteAndCheckExecutionStrategy<JavaZipFileCompileExecuteAndCheckExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.JavaProjectTestsExecutionStrategy:
                case ExecutionStrategyType.Java21ProjectTestsExecutionStrategy:
                    executionStrategy = new JavaProjectTestsExecutionStrategy<JavaProjectTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<JavaProjectTestsExecutionStrategy<JavaProjectTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.JavaUnitTestsExecutionStrategy:
                case ExecutionStrategyType.Java21UnitTestsExecutionStrategy:
                    executionStrategy = new JavaUnitTestsExecutionStrategy<JavaUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<JavaUnitTestsExecutionStrategy<JavaUnitTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.JavaSpringAndHibernateProjectExecutionStrategy:
                case ExecutionStrategyType.Java21SpringAndHibernateProjectExecution:
                    executionStrategy = new JavaSpringAndHibernateProjectExecutionStrategy<JavaSpringAndHibernateProjectExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        compilerFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<JavaSpringAndHibernateProjectExecutionStrategy<JavaSpringAndHibernateProjectExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck:
                case ExecutionStrategyType.NodeJsV20PreprocessExecuteAndCheck:
                    executionStrategy = new NodeJsPreprocessExecuteAndCheckExecutionStrategy<NodeJsPreprocessExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<NodeJsPreprocessExecuteAndCheckExecutionStrategy<NodeJsPreprocessExecuteAndCheckExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.TypeScriptV20PreprocessExecuteAndCheck:
                    executionStrategy = new TypeScriptPreprocessExecuteAndCheckExecutionStrategy<TypeScriptPreprocessExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<TypeScriptPreprocessExecuteAndCheckExecutionStrategy<TypeScriptPreprocessExecuteAndCheckExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes),
                        compilerFactory);
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha:
                case ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunUnitTestsWithMocha:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests:
                case ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunJsDomUnitTests:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy<NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy<NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy:
                case ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy<NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy<NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy:
                    executionStrategy = new NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy<NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy<NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy:
                case ExecutionStrategyType.NodeJsV20ZipExecuteHtmlAndCssStrategy:
                    executionStrategy = new NodeJsZipExecuteHtmlAndCssStrategy<NodeJsZipExecuteHtmlAndCssStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<NodeJsZipExecuteHtmlAndCssStrategy<NodeJsZipExecuteHtmlAndCssStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.RunSpaAndExecuteMochaTestsExecutionStrategy:
                    executionStrategy = new RunSpaAndExecuteMochaTestsExecutionStrategy<RunSpaAndExecuteMochaTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<RunSpaAndExecuteMochaTestsExecutionStrategy<RunSpaAndExecuteMochaTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.NodeJsV20RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests:
                case ExecutionStrategyType.RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests:
                    executionStrategy = new RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests<RunSpaAndExecuteMochaTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests<RunSpaAndExecuteMochaTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.PythonExecuteAndCheck:
                    executionStrategy = new PythonExecuteAndCheckExecutionStrategy<PythonExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<PythonExecuteAndCheckExecutionStrategy<PythonExecuteAndCheckExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.PythonUnitTests:
                    executionStrategy = new PythonUnitTestsExecutionStrategy<PythonUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<PythonUnitTestsExecutionStrategy<PythonUnitTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.PythonCodeExecuteAgainstUnitTests:
                    executionStrategy = new PythonCodeExecuteAgainstUnitTestsExecutionStrategy<PythonCodeExecuteAgainstUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<PythonCodeExecuteAgainstUnitTestsExecutionStrategy<PythonCodeExecuteAgainstUnitTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.PythonProjectTests:
                    executionStrategy = new PythonProjectTestsExecutionStrategy<PythonProjectTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<PythonProjectTestsExecutionStrategy<PythonProjectTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.PythonProjectUnitTests:
                    executionStrategy = new PythonProjectUnitTestsExecutionStrategy<PythonProjectUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<PythonProjectUnitTestsExecutionStrategy<PythonProjectUnitTestsExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.SqlServerSingleDatabasePrepareDatabaseAndRunQueries:
                    executionStrategy = new SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategy<SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategySettings>(
                        submission,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategy<SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.SqlServerSingleDatabaseRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategy<SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategy<SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.MySqlPrepareDatabaseAndRunQueries:
                    executionStrategy = new MySqlPrepareDatabaseAndRunQueriesExecutionStrategy<MySqlPrepareDatabaseAndRunQueriesExecutionStrategySettings>(
                        submission,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<MySqlPrepareDatabaseAndRunQueriesExecutionStrategy<MySqlPrepareDatabaseAndRunQueriesExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.MySqlRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunQueriesAndCheckDatabaseExecutionStrategy<MySqlRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<MySqlRunQueriesAndCheckDatabaseExecutionStrategy<MySqlRunQueriesAndCheckDatabaseExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.MySqlRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.DoNothing:
                    executionStrategy = new DoNothingExecutionStrategy<DoNothingExecutionStrategySettings>(
                        submission,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<DoNothingExecutionStrategy<DoNothingExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.CheckOnly:
                    executionStrategy = new CheckOnlyExecutionStrategy<CheckOnlyExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<CheckOnlyExecutionStrategy<CheckOnlyExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.PostgreSqlPrepareDatabaseAndRunQueries:
                    executionStrategy = new PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategy<PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategySettings>(
                        submission,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategy<PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.PostgreSqlRunQueriesAndCheckDatabase:
                    executionStrategy = new PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategy<PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategy<PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.PostgreSqlRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy =
                        new PostgreSqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<PostgreSqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                            submission,
                            executionStrategySettingsProvider,
                            loggerFactory.CreateStrategyLogger<PostgreSqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<PostgreSqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.PythonDjangoOrmExecutionStrategy:
                    executionStrategy = new PythonDjangoOrmExecutionStrategy<PythonDjangoOrmExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        executionStrategySettingsProvider,
                        loggerFactory.CreateStrategyLogger<PythonDjangoOrmExecutionStrategy<PythonDjangoOrmExecutionStrategySettings>>(submissionId, verbosely, logFileMaxBytes));
                    break;
                case ExecutionStrategyType.NotFound:
                default:
                    throw new ArgumentOutOfRangeException(nameof(executionStrategy), "Invalid execution strategy type.");
            }

            return executionStrategy;
        }
    }
}