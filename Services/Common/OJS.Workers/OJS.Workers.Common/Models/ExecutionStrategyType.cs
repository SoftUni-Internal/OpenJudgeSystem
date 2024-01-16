﻿namespace OJS.Workers.Common.Models
{
    public enum ExecutionStrategyType
    {
        // Commented out execution strategy types are deprecated, but left here to preserve order,
        // as modifying values will require database migration.
        NotFound = 0,
        CompileExecuteAndCheck = 1,
        NodeJsPreprocessExecuteAndCheck = 2,
        JavaPreprocessCompileExecuteAndCheck = 4,
        // PhpCgiExecuteAndCheck = 5,
        // PhpCliExecuteAndCheck = 6,
        CheckOnly = 7,
        JavaZipFileCompileExecuteAndCheck = 8,
        PythonExecuteAndCheck = 9,
        NodeJsPreprocessExecuteAndRunUnitTestsWithMocha = 11,
        NodeJsPreprocessExecuteAndRunJsDomUnitTests = 12,
        MySqlPrepareDatabaseAndRunQueries = 16,
        MySqlRunQueriesAndCheckDatabase = 17,
        MySqlRunSkeletonRunQueriesAndCheckDatabase = 18,
        NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy = 19,
        // NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMocha = 20,
        // NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy = 21,
        NodeJsZipExecuteHtmlAndCssStrategy = 22,
        // CSharpUnitTestsExecutionStrategy = 23,
        // CSharpProjectTestsExecutionStrategy = 24,
        JavaProjectTestsExecutionStrategy = 25,
        CPlusPlusZipFileExecutionStrategy = 26,
        JavaUnitTestsExecutionStrategy = 27,
        CPlusPlusCompileExecuteAndCheckExecutionStrategy = 29,
        JavaSpringAndHibernateProjectExecutionStrategy = 30,
        // RubyExecutionStrategy = 32,
        DotNetCoreProjectExecutionStrategy = 33,
        DotNetCoreProjectTestsExecutionStrategy = 35,
        DotNetCoreCompileExecuteAndCheck = 37,
        DotNetCoreUnitTestsExecutionStrategy = 38,
        DoNothing = 40,
        PythonUnitTests = 41,
        PythonCodeExecuteAgainstUnitTests = 42,
        PythonProjectTests = 43,
        PythonProjectUnitTests = 44,
        SqlServerSingleDatabasePrepareDatabaseAndRunQueries = 45,
        SqlServerSingleDatabaseRunQueriesAndCheckDatabase = 46,
        SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase = 47,
        RunSpaAndExecuteMochaTestsExecutionStrategy = 48,
        GolangCompileExecuteAndCheck = 49,
        DotNetCore6ProjectTestsExecutionStrategy = 50,
        DotNetCore5ProjectTestsExecutionStrategy = 51,
        DotNetCore5CompileExecuteAndCheck = 52,
        DotNetCore6CompileExecuteAndCheck = 53,
        DotNetCore5UnitTestsExecutionStrategy = 54,
        DotNetCore6UnitTestsExecutionStrategy = 55,
        DotNetCore5ProjectExecutionStrategy = 56,
        DotNetCore6ProjectExecutionStrategy = 57,
        PostgreSqlPrepareDatabaseAndRunQueries = 58,
        PostgreSqlRunQueriesAndCheckDatabase = 59,
        PostgreSqlRunSkeletonRunQueriesAndCheckDatabase = 60,
        PythonDjangoOrmExecutionStrategy = 61,
    }
}