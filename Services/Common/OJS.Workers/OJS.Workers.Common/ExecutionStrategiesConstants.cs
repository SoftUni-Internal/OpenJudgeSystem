namespace OJS.Workers.Common
{
    using System.Collections.Generic;

    using OJS.Workers.Common.Models;

    public static class ExecutionStrategiesConstants
    {
        public static class ExecutionStrategyNames
        {
            // .NET
            public const string CsharpCode = "csharp-code";

            // .NET Core
            public const string CsharpDotNetCoreCode = "csharp-dot-net-core-code";
            public const string CsharpDotNetCore5Code = "csharp-dot-net-core-5-code";
            public const string CsharpDotNetCore6Code = "csharp-dot-net-core-6-code";

            public const string CSharpDotNetCoreProjectTests = "dot-net-core-project-tests";
            public const string CSharpDotNetCore5ProjectTests = "dot-net-core-5-project-tests";
            public const string CSharpDotNetCore6ProjectTests = "dot-net-core-6-project-tests";
            public const string CSharpDotNetCoreProject = "dot-net-core-project";
            public const string CSharpDotNetCore5Project = "dot-net-core-5-project";
            public const string CSharpDotNetCore6Project = "dot-net-core-6-project";
            public const string CSharpDotNetCoreUnitTests = "dot-net-core-unit-tests";
            public const string CSharpDotNetCore5UnitTests = "dot-net-core-5-unit-tests";
            public const string CSharpDotNetCore6UnitTests = "dot-net-core-6-unit-tests";

            // Java
            public const string JavaCode = "java-code";
            public const string JavaProjectTests = "java-project-tests";
            public const string JavaUnitTests = "java-unit-tests";
            public const string JavaZipFileCode = "java-zip-file-code";
            public const string JavaSpringAndHibernateProjectExecutionStrategy = "run-java-spring-data-junit-tests";

            // JavaScript
            public const string JavaScriptCode = "javascript-codeV20";
            public const string JavaScriptJsDomUnitTests = "javascript-js-dom-unit-testsV20";
            public const string JavaScriptUnitTestsWithMocha = "javascript-unit-tests-with-mochaV20";

            public const string JavaScriptCodeAgainstUnitTestsWithMocha =
                "javascript-code-against-unit-tests-with-mochaV20";

            // Python
            public const string PythonCode = "python-code";
            public const string PythonCodeUnitTests = "python-code-unit-tests";
            public const string PythonProjectTests = "python-project-tests";
            public const string PythonProjectUnitTests = "python-project-unit-tests";
            public const string PythontUnitTests = "python-unit-tests";
            public const string PythonDjangoOrmExecutionStrategy = "python-django-orm-project-tests";

            // Php
            public const string PhpCode = "php-code";
            public const string PhpCodeCgi = "php-code-cgi";

            // Go
            public const string GoCode = "go-code";
            // HTML and CSS
            public const string HtmlAndCssZipFile = "html-and-css-zip-fileV20";

            // C++
            public const string CppCode = "cpp-code";
            public const string CppZipFile = "cpp-zip-file";

            // Plain text
            public const string PlainText = "plaintext";

            // SqlServer
            public const string SqlServerPrepareDatabaseAndRunQueries = "sql-server-prepare-db-and-run-queries";
            public const string SqlServerRunQueriesAndCheckDatabase = "sql-server-run-queries-and-check-database";
            public const string SqlServerRunSkeletonRunQueriesAndCheckDatabase = "sql-server-run-skeleton-run-queries-and-check-database";

            // MySQL/MariaDb
            public const string MySqlPrepareDbAndRunQueries = "mysql-prepare-db-and-run-queries";
            public const string MySqlRunQueriesAndCheckDatabase = "mysql-run-queries-and-check-database";
            public const string MySqlRunSkeletonRunQueriesAndCheckDatabase = "mysql-run-skeleton-run-queries-and-check-database";

            // Run SPA and Execute mocha tests
            public const string RunSpaAndExecuteMochaTestsExecutionStrategy = "run-spa-and-execute-mocha-tests";

            // PostgreSql
            public const string PostgreSqlPrepareDbAndRunQueries = "postgres-prepare-db-and-run-queries";
            public const string PostgreSqlRunQueriesAndCheckDatabase = "postgres-run-queries-and-check-database";
            public const string PostgreSqlRunSkeletonRunQueriesAndCheckDatabase = "postgres-run-skeleton-run-queries-and-check-database";
        }

        public static class NameMappings
        {
            public static IDictionary<string, ExecutionStrategyType> NameToExecutionStrategyMappings =>
                new Dictionary<string, ExecutionStrategyType>
                {
                    // .Net Core
                    { ExecutionStrategyNames.CsharpDotNetCoreCode, ExecutionStrategyType.DotNetCoreCompileExecuteAndCheck },
                    { ExecutionStrategyNames.CsharpDotNetCore5Code, ExecutionStrategyType.DotNetCore5CompileExecuteAndCheck },
                    { ExecutionStrategyNames.CsharpDotNetCore6Code, ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck },
                    { ExecutionStrategyNames.CSharpDotNetCoreProject, ExecutionStrategyType.DotNetCoreProjectExecutionStrategy },
                    { ExecutionStrategyNames.CSharpDotNetCore5Project, ExecutionStrategyType.DotNetCore5ProjectExecutionStrategy },
                    { ExecutionStrategyNames.CSharpDotNetCore6Project, ExecutionStrategyType.DotNetCore6ProjectExecutionStrategy },
                    { ExecutionStrategyNames.CSharpDotNetCoreProjectTests, ExecutionStrategyType.DotNetCoreProjectTestsExecutionStrategy },
                    { ExecutionStrategyNames.CSharpDotNetCore5ProjectTests, ExecutionStrategyType.DotNetCore5ProjectTestsExecutionStrategy },
                    { ExecutionStrategyNames.CSharpDotNetCore6ProjectTests, ExecutionStrategyType.DotNetCore6ProjectTestsExecutionStrategy },
                    { ExecutionStrategyNames.CSharpDotNetCoreUnitTests, ExecutionStrategyType.DotNetCoreUnitTestsExecutionStrategy },
                    { ExecutionStrategyNames.CSharpDotNetCore5UnitTests, ExecutionStrategyType.DotNetCore5UnitTestsExecutionStrategy },
                    { ExecutionStrategyNames.CSharpDotNetCore6UnitTests, ExecutionStrategyType.DotNetCore6UnitTestsExecutionStrategy },

                    // Python
                    { ExecutionStrategyNames.PythonCode, ExecutionStrategyType.PythonExecuteAndCheck },
                    { ExecutionStrategyNames.PythonCodeUnitTests, ExecutionStrategyType.PythonCodeExecuteAgainstUnitTests },
                    { ExecutionStrategyNames.PythonProjectTests, ExecutionStrategyType.PythonProjectTests },
                    { ExecutionStrategyNames.PythonProjectUnitTests, ExecutionStrategyType.PythonProjectUnitTests },
                    { ExecutionStrategyNames.PythontUnitTests, ExecutionStrategyType.PythonUnitTests },
                    { ExecutionStrategyNames.PythonDjangoOrmExecutionStrategy, ExecutionStrategyType.PythonDjangoOrmExecutionStrategy },

                    // Go
                    { ExecutionStrategyNames.GoCode, ExecutionStrategyType.GolangCompileExecuteAndCheck },

                    // HTML
                    { ExecutionStrategyNames.HtmlAndCssZipFile, ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy },

                    // C++
                    { ExecutionStrategyNames.CppCode, ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy },
                    { ExecutionStrategyNames.CppZipFile, ExecutionStrategyType.CPlusPlusZipFileExecutionStrategy },

                    // JavaScript
                    { ExecutionStrategyNames.JavaScriptCode, ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck },
                    { ExecutionStrategyNames.JavaScriptUnitTestsWithMocha, ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha },
                    { ExecutionStrategyNames.JavaScriptJsDomUnitTests, ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests },
                    { ExecutionStrategyNames.JavaScriptCodeAgainstUnitTestsWithMocha, ExecutionStrategyType.NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy },

                    // Java
                    { ExecutionStrategyNames.JavaCode, ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck },
                    { ExecutionStrategyNames.JavaProjectTests, ExecutionStrategyType.JavaProjectTestsExecutionStrategy },
                    { ExecutionStrategyNames.JavaZipFileCode, ExecutionStrategyType.JavaZipFileCompileExecuteAndCheck },
                    { ExecutionStrategyNames.JavaUnitTests, ExecutionStrategyType.JavaUnitTestsExecutionStrategy },
                    { ExecutionStrategyNames.JavaSpringAndHibernateProjectExecutionStrategy, ExecutionStrategyType.JavaSpringAndHibernateProjectExecutionStrategy },

                    // Plain text
                    { ExecutionStrategyNames.PlainText, ExecutionStrategyType.CheckOnly },

                    // Sql Server
                    { ExecutionStrategyNames.SqlServerPrepareDatabaseAndRunQueries, ExecutionStrategyType.SqlServerSingleDatabasePrepareDatabaseAndRunQueries },
                    { ExecutionStrategyNames.SqlServerRunQueriesAndCheckDatabase, ExecutionStrategyType.SqlServerSingleDatabaseRunQueriesAndCheckDatabase },
                    { ExecutionStrategyNames.SqlServerRunSkeletonRunQueriesAndCheckDatabase, ExecutionStrategyType.SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase },

                    // MySQL/MariaDb
                    { ExecutionStrategyNames.MySqlPrepareDbAndRunQueries, ExecutionStrategyType.MySqlPrepareDatabaseAndRunQueries },
                    { ExecutionStrategyNames.MySqlRunQueriesAndCheckDatabase, ExecutionStrategyType.MySqlRunQueriesAndCheckDatabase },
                    { ExecutionStrategyNames.MySqlRunSkeletonRunQueriesAndCheckDatabase, ExecutionStrategyType.MySqlRunSkeletonRunQueriesAndCheckDatabase },

                    // Php
                    // { ExecutionStrategyNames.PhpCode, ExecutionStrategyType.PhpCliExecuteAndCheck },

                    // Run SPA and Execute mocha tests
                    { ExecutionStrategyNames.RunSpaAndExecuteMochaTestsExecutionStrategy, ExecutionStrategyType.RunSpaAndExecuteMochaTestsExecutionStrategy },

                    // PostgreSql
                    { ExecutionStrategyNames.PostgreSqlPrepareDbAndRunQueries, ExecutionStrategyType.PostgreSqlPrepareDatabaseAndRunQueries },
                    { ExecutionStrategyNames.PostgreSqlRunQueriesAndCheckDatabase, ExecutionStrategyType.PostgreSqlRunQueriesAndCheckDatabase },
                    { ExecutionStrategyNames.PostgreSqlRunSkeletonRunQueriesAndCheckDatabase, ExecutionStrategyType.PostgreSqlRunSkeletonRunQueriesAndCheckDatabase },
                };
        }
    }
}