#nullable disable
namespace OJS.Services.Worker.Business.Implementations
{
    using Microsoft.Extensions.Logging;
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

    public class ExecutionStrategyFactory : IExecutionStrategyFactory
    {
        private readonly ICompilerFactory compilerFactory;
        private readonly IExecutionStrategySettingsProvider executionStrategySettingsProvider;
        private readonly ILoggerFactory loggerFactory;

        public ExecutionStrategyFactory(
            ICompilerFactory compilerFactory,
            IExecutionStrategySettingsProvider executionStrategySettingsProvider,
            ILoggerFactory loggerFactory)
        {
            this.compilerFactory = compilerFactory;
            this.executionStrategySettingsProvider = executionStrategySettingsProvider;
            this.loggerFactory = loggerFactory;
        }

        public IExecutionStrategy CreateExecutionStrategy(IOjsSubmission submission)
        {
            IExecutionStrategy executionStrategy;
            var tasksService = new TasksService();
            var processExecutorFactory = new ProcessExecutorFactory(tasksService, this.loggerFactory.CreateLogger<StandardProcessExecutor>());

            switch (submission.ExecutionStrategyType)
            {
                case ExecutionStrategyType.CompileExecuteAndCheck:
                    executionStrategy = new CompileExecuteAndCheckExecutionStrategy<CompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<CompileExecuteAndCheckExecutionStrategy<CompileExecuteAndCheckExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy:
                    executionStrategy = new CPlusPlusCompileExecuteAndCheckExecutionStrategy<CPlusPlusCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<CPlusPlusCompileExecuteAndCheckExecutionStrategy<CPlusPlusCompileExecuteAndCheckExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.CPlusPlusZipFileExecutionStrategy:
                    executionStrategy = new CPlusPlusZipFileExecutionStrategy<CPlusPlusZipFileExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<CPlusPlusZipFileExecutionStrategy<CPlusPlusZipFileExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.DotNetCoreCompileExecuteAndCheck:
                case ExecutionStrategyType.DotNetCore5CompileExecuteAndCheck:
                case ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck:
                    executionStrategy = new DotNetCoreCompileExecuteAndCheckExecutionStrategy<DotNetCoreCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<DotNetCoreCompileExecuteAndCheckExecutionStrategy<DotNetCoreCompileExecuteAndCheckExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.GolangCompileExecuteAndCheck:
                    executionStrategy = new GolangCompileExecuteAndCheckExecutionStrategy<GolangCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<GolangCompileExecuteAndCheckExecutionStrategy<GolangCompileExecuteAndCheckExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.DotNetCoreUnitTestsExecutionStrategy:
                case ExecutionStrategyType.DotNetCore5UnitTestsExecutionStrategy:
                case ExecutionStrategyType.DotNetCore6UnitTestsExecutionStrategy:
                    executionStrategy = new DotNetCoreUnitTestsExecutionStrategy<DotNetCoreUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<DotNetCoreUnitTestsExecutionStrategy<DotNetCoreUnitTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.DotNetCoreProjectExecutionStrategy:
                case ExecutionStrategyType.DotNetCore5ProjectExecutionStrategy:
                case ExecutionStrategyType.DotNetCore6ProjectExecutionStrategy:
                    executionStrategy = new DotNetCoreProjectExecutionStrategy<DotNetCoreProjectExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<DotNetCoreProjectExecutionStrategy<DotNetCoreProjectExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.DotNetCoreProjectTestsExecutionStrategy:
                case ExecutionStrategyType.DotNetCore5ProjectTestsExecutionStrategy:
                case ExecutionStrategyType.DotNetCore6ProjectTestsExecutionStrategy:
                    executionStrategy = new DotNetCoreProjectTestsExecutionStrategy<DotNetCoreProjectTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<DotNetCoreProjectTestsExecutionStrategy<DotNetCoreProjectTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck:
                case ExecutionStrategyType.Java21PreprocessCompileExecuteAndCheck:
                    executionStrategy = new JavaPreprocessCompileExecuteAndCheckExecutionStrategy<JavaPreprocessCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<JavaPreprocessCompileExecuteAndCheckExecutionStrategy<JavaPreprocessCompileExecuteAndCheckExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.JavaZipFileCompileExecuteAndCheck:
                case ExecutionStrategyType.Java21ZipFileCompileExecuteAndCheck:
                    executionStrategy = new JavaZipFileCompileExecuteAndCheckExecutionStrategy<JavaZipFileCompileExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<JavaZipFileCompileExecuteAndCheckExecutionStrategy<JavaZipFileCompileExecuteAndCheckExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.JavaProjectTestsExecutionStrategy:
                case ExecutionStrategyType.Java21ProjectTestsExecutionStrategy:
                    executionStrategy = new JavaProjectTestsExecutionStrategy<JavaProjectTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<JavaProjectTestsExecutionStrategy<JavaProjectTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.JavaUnitTestsExecutionStrategy:
                case ExecutionStrategyType.Java21UnitTestsExecutionStrategy:
                    executionStrategy = new JavaUnitTestsExecutionStrategy<JavaUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<JavaUnitTestsExecutionStrategy<JavaUnitTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.JavaSpringAndHibernateProjectExecutionStrategy:
                case ExecutionStrategyType.Java21SpringAndHibernateProjectExecution:
                    executionStrategy = new JavaSpringAndHibernateProjectExecutionStrategy<JavaSpringAndHibernateProjectExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.compilerFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<JavaSpringAndHibernateProjectExecutionStrategy<JavaSpringAndHibernateProjectExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck:
                case ExecutionStrategyType.NodeJsV20PreprocessExecuteAndCheck:
                    executionStrategy = new NodeJsPreprocessExecuteAndCheckExecutionStrategy<NodeJsPreprocessExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<NodeJsPreprocessExecuteAndCheckExecutionStrategy<NodeJsPreprocessExecuteAndCheckExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.TypeScriptV20PreprocessExecuteAndCheck:
                    executionStrategy = new TypeScriptPreprocessExecuteAndCheckExecutionStrategy<TypeScriptPreprocessExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<TypeScriptPreprocessExecuteAndCheckExecutionStrategy<TypeScriptPreprocessExecuteAndCheckExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha:
                case ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunUnitTestsWithMocha:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy<NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests:
                case ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunJsDomUnitTests:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy<NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy<NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy:
                case ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy:
                    executionStrategy = new NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy<NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy<NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy:
                    executionStrategy = new NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy<NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy<NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy:
                case ExecutionStrategyType.NodeJsV20ZipExecuteHtmlAndCssStrategy:
                    executionStrategy = new NodeJsZipExecuteHtmlAndCssStrategy<NodeJsZipExecuteHtmlAndCssStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<NodeJsZipExecuteHtmlAndCssStrategy<NodeJsZipExecuteHtmlAndCssStrategySettings>>());
                    break;
                case ExecutionStrategyType.RunSpaAndExecuteMochaTestsExecutionStrategy:
                    executionStrategy = new RunSpaAndExecuteMochaTestsExecutionStrategy<RunSpaAndExecuteMochaTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<RunSpaAndExecuteMochaTestsExecutionStrategy<RunSpaAndExecuteMochaTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.NodeJsV20RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests:
                case ExecutionStrategyType.RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests:
                    executionStrategy = new RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests<RunSpaAndExecuteMochaTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests<RunSpaAndExecuteMochaTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.PythonExecuteAndCheck:
                    executionStrategy = new PythonExecuteAndCheckExecutionStrategy<PythonExecuteAndCheckExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<PythonExecuteAndCheckExecutionStrategy<PythonExecuteAndCheckExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.PythonUnitTests:
                    executionStrategy = new PythonUnitTestsExecutionStrategy<PythonUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<PythonUnitTestsExecutionStrategy<PythonUnitTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.PythonCodeExecuteAgainstUnitTests:
                    executionStrategy = new PythonCodeExecuteAgainstUnitTestsExecutionStrategy<PythonCodeExecuteAgainstUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<PythonCodeExecuteAgainstUnitTestsExecutionStrategy<PythonCodeExecuteAgainstUnitTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.PythonProjectTests:
                    executionStrategy = new PythonProjectTestsExecutionStrategy<PythonProjectTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<PythonProjectTestsExecutionStrategy<PythonProjectTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.PythonProjectUnitTests:
                    executionStrategy = new PythonProjectUnitTestsExecutionStrategy<PythonProjectUnitTestsExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<PythonProjectUnitTestsExecutionStrategy<PythonProjectUnitTestsExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.SqlServerSingleDatabasePrepareDatabaseAndRunQueries:
                    executionStrategy = new SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategy<SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategySettings>(
                        submission,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategy<SqlServerSingleDatabasePrepareDatabaseAndRunQueriesExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.SqlServerSingleDatabaseRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategy<SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategy<SqlServerSingleDatabaseRunQueriesAndCheckDatabaseExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.MySqlPrepareDatabaseAndRunQueries:
                    executionStrategy = new MySqlPrepareDatabaseAndRunQueriesExecutionStrategy<MySqlPrepareDatabaseAndRunQueriesExecutionStrategySettings>(
                        submission,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<MySqlPrepareDatabaseAndRunQueriesExecutionStrategy<MySqlPrepareDatabaseAndRunQueriesExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.MySqlRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunQueriesAndCheckDatabaseExecutionStrategy<MySqlRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<MySqlRunQueriesAndCheckDatabaseExecutionStrategy<MySqlRunQueriesAndCheckDatabaseExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.MySqlRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy = new MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<MySqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.DoNothing:
                    executionStrategy = new DoNothingExecutionStrategy<DoNothingExecutionStrategySettings>(
                        submission,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<DoNothingExecutionStrategy<DoNothingExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.CheckOnly:
                    executionStrategy = new CheckOnlyExecutionStrategy<CheckOnlyExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<CheckOnlyExecutionStrategy<CheckOnlyExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.PostgreSqlPrepareDatabaseAndRunQueries:
                    executionStrategy = new PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategy<PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategySettings>(
                        submission,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategy<PostgreSqlPrepareDatabaseAndRunQueriesExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.PostgreSqlRunQueriesAndCheckDatabase:
                    executionStrategy = new PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategy<PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                        submission,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategy<PostgreSqlRunQueriesAndCheckDatabaseExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.PostgreSqlRunSkeletonRunQueriesAndCheckDatabase:
                    executionStrategy =
                        new PostgreSqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<PostgreSqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>(
                            submission,
                            this.executionStrategySettingsProvider,
                            this.loggerFactory.CreateLogger<PostgreSqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategy<PostgreSqlRunSkeletonRunQueriesAndCheckDatabaseExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.PythonDjangoOrmExecutionStrategy:
                    executionStrategy = new PythonDjangoOrmExecutionStrategy<PythonDjangoOrmExecutionStrategySettings>(
                        submission,
                        processExecutorFactory,
                        this.executionStrategySettingsProvider,
                        this.loggerFactory.CreateLogger<PythonDjangoOrmExecutionStrategy<PythonDjangoOrmExecutionStrategySettings>>());
                    break;
                case ExecutionStrategyType.NotFound:
                default:
                    throw new ArgumentOutOfRangeException(nameof(executionStrategy), "Invalid execution strategy type.");
            }

            return executionStrategy;
        }
    }
}