namespace OJS.Common.Constants
{
    public static class ServiceConstants
    {
        public const string WorkersQueueName = "OJS.Servers.Worker.Consumers.SubmissionsForProcessingConsumer";

        public static class CheckerTypes
        {
            public const string ExactMatch = "exact";
            public const string LegacyExactMatch = "exact-match";
            public const string CaseInsensitive = "caseinsensitive";
            public const string LegacyCaseInsensitive = "case-insensitive";
            public const string Precision = "precision";
            public const string Sort = "sort";
            public const string Trim = "trim";
            public const string TrimEnd = "trimend";
            public const string LegacyTrimEnd = "trim-end";
            public const string CSharpCode = "csharpcode";
            public const string LegacyCSharpCode = "csharp-code";
            public const string CSharpCoreCode = "csharpcorecode";

            public static string[] All =>
            [
                ExactMatch,
                LegacyExactMatch,
                CaseInsensitive,
                LegacyCaseInsensitive,
                Precision,
                Sort,
                Trim,
                TrimEnd,
                LegacyTrimEnd,
                CSharpCode,
                LegacyCSharpCode,
                CSharpCoreCode
            ];
        }

        public static class CodeExecutionContext
        {
            public const int DefaultFileSizeLimitOfCodeUploadInMegaBytes = 16;
            public const int TaskDefaultMaxPoints = 100;

            public static class ExecutionTypeNames
            {
                public const string SimpleExecution = "simple-execution";
                public const string TestsExecution = "tests-execution";
                public const string SimpleTemplateExecution = "simple-template-execution";
                public const string TestsTemplateExecution = "tests-template-execution";
            }

            public static class ExecutionStrategyNames
            {
                // .NET
                public const string CsharpCode = "csharp-code";

                // .NET Core
                public const string CsharpDotNetCoreCode = "csharp-dot-net-core-code";
                public const string CSharpDotNetCoreProjectTests = "dot-net-core-project-tests";

                // Java
                public const string JavaCode = "java-code";
                public const string JavaProjectTests = "java-project-tests";
                public const string JavaUnitTests = "java-unit-tests";
                public const string JavaZipFileCode = "java-zip-file-code";

                // JavaScript
                public const string JavaScriptCode = "javascript-code";
                public const string JavaScriptJsDomUnitTests = "javascript-js-dom-unit-tests";
                public const string JavaScriptUnitTestsWithMocha = "javascript-unit-tests-with-mocha";
                public const string JavaScriptAsyncJsDomTestsWithReact = "javascript-async-js-dom-tests-with-react";

                public const string JavaScriptCodeAgainstUnitTestsWithMocha =
                    "javascript-code-against-unit-tests-with-mocha";

                // Python
                public const string PythonCode = "python-code";
                public const string PythonProjectTests = "python-project-tests";
                public const string PythonProjectUnitTests = "python-project-unit-tests";

                // Php
                public const string PhpCode = "php-code";

                // HTML and CSS
                public const string HtmlAndCssZipFile = "html-and-css-zip-file";

                // C++
                public const string CppCode = "cpp-code";

                // Sql Server
                public const string SqlServerPrepareDatabaseAndRunQueries = "sql-server-prepare-db-and-run-queries";
                public const string SqlServerRunQueriesAndCheckDatabase = "sql-server-run-queries-and-check-database";
                public const string SqlServerRunSkeletonRunQueriesAndCheckDatabase = "sql-server-run-skeleton-run-queries-and-check-database";
                public const string SqlServerSingleDatabasePrepareDatabaseAndRunQueries = "sql-server-single-database-prepare-database-and-run-queries";
                public const string SqlServerSingleDatabaseRunQueriesAndCheckDatabase = "sql-server-single-database-run-queries-and-check-database";
                public const string SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase = "sql-server-single-database-run-skeleton-run-queries-and-check-database";

                // MySQL
                public const string MySqlPrepareDatabaseAndRunQueries = "mysql-prepare-db-and-run-queries";
                public const string MySqlRunQueriesAndCheckDatabase = "mysql-run-queries-and-check-database";
                public const string MySqlRunSkeletonRunQueriesAndCheckDatabase = "mysql-run-skeleton-run-queries-and-check-database";
                public const string MariaDbSingleDatabasePrepareDatabaseAndRunQueries = "mariadb-prepare-database-and-run-queries";
                public const string MariaDbSingleDatabaseRunQueriesAndCheckDatabase = "mariadb-run-queries-and-check-database";
                public const string MariaDbSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase = "mariadb-run-skeleton-run-queries-and-check-database";

                // Plain text
                public const string PlainText = "plaintext";
            }
        }
    }
}