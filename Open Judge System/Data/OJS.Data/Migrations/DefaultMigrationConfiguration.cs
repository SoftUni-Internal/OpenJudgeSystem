namespace OJS.Data.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Workers.Common.Models;

    public class DefaultMigrationConfiguration : DbMigrationsConfiguration<OjsDbContext>
    {
        public DefaultMigrationConfiguration()
        {
            this.AutomaticMigrationsEnabled = true;
            this.AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(OjsDbContext context)
        {
            this.SeedSettings(context, new HashSet<Setting>
            {
                new Setting
                {
                    Name = GlobalConstants.MaximumFileSizeDbName,
                    Value = GlobalConstants.OneMegaByteInBytes.ToString()
                },
                new Setting
                {
                    Name = GlobalConstants.RemoteWorkers,
                    Value = GlobalConstants.RemoteWorkersCount.ToString()
                },
                new Setting
                {
                    Name = GlobalConstants.MaxAllowedTimeForSubmissionCompletion,
                    Value = GlobalConstants.MaxAllowedTimeForSubmissionCompletionInSecs.ToString()
                }
            });

            if (context.Roles.Any())
            {
                return;
            }

            // this.SeedSubmissionsAndTestRuns(context);
            this.SeedSubmissionTypes(context);
            this.SeedRoles(context);
            this.SeedCheckers(context);

            // this.SeedContests(context);
            // this.SeedRandomContests(context);
            // this.SeedProblem(context);
            // this.SeedTest(context);
            // this.SeedCategoryContestProblem(context);
        }

        //// TODO: Add seed with .Any()
        protected void SeedRoles(OjsDbContext context)
        {
            context.Roles.AddOrUpdate(new IdentityRole(GlobalConstants.AdministratorRoleName));
            context.Roles.AddOrUpdate(new IdentityRole(GlobalConstants.LecturerRoleName));

            context.SaveChanges();
        }

        protected void SeedCheckers(OjsDbContext context)
        {
            var checkers = new[]
            {
                new Checker
                {
                    Name = "Exact",
                    Description =
                        "Compares exactly (char by char) the user output with the excepted output. Whitespace differences may cause incorrect output.",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "ExactChecker",
                },
                new Checker
                {
                    Name = "Trim",
                    Description = "Trims the user output and compares it with the expected output.",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "TrimChecker",
                },
                new Checker
                {
                    Name = "Sort lines",
                    Description =
                        "Sorts alphabetically all lines from the user output, then compares them with the expected output.",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "SortChecker",
                },
                new Checker
                {
                    Name = "Case-insensitive",
                    Description = "Compares case-insensitively the user output with expected output.",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "CaseInsensitiveChecker",
                },
                new Checker
                {
                    Name = "Precision checker - 14",
                    Description =
                        "Compares floating-point numbers with precision of 14 digits after the decimal separator.",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "PrecisionChecker",
                    Parameter = "14",
                },
                new Checker
                {
                    Name = "Precision checker - 7",
                    Description =
                        "Compares floating-point numbers with precision of 7 digits after the decimal separator.",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "PrecisionChecker",
                    Parameter = "7",
                },
                new Checker
                {
                    Name = "Precision checker - 3",
                    Description =
                        "Compares floating-point numbers with precision of 3 digits after the decimal separator.",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "PrecisionChecker",
                    Parameter = "3",
                },
                new Checker
                {
                    Name = "Last Number Checker",
                    Description =
                        "Checks if the last number in the user output matches the expected output. Intelligently skips the unneeded messages in the user output like \"Please enter a number n:\". Works for integer and floating-point numbers. Compares with precision of 2 digits after the decimal point. If the expected output is text like \"error\", checks for its existence as substring intelligently (lowercase, skipping whitespace and punctuation).",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "CSharpCodeChecker",
                    Parameter = "using System;\nusing System.Text.RegularExpressions;\nusing System.Collections.Generic;\nusing System.Globalization;\n\nusing OJS.Workers.Common;\n\npublic class LastNumberChecker : IChecker\n{\n    const double DecimalPrecision = 0.01;\n\n    public CheckerResult Check(string inputData, string receivedOutput,\n        string expectedOutput, bool isTrialTest)\n    {\n        string[] expectedNumbers = ExtractNumbersFromText(expectedOutput);\n        if (expectedNumbers.Length == 0)\n        {\n            return CompareTextOutput(receivedOutput, expectedOutput);\n        }\n\n        if (expectedNumbers.Length != 1)\n        {\n            return new CheckerResult()\n            {\n                IsCorrect = false,\n                ResultType = CheckerResultType.InvalidNumberOfLines,\n                CheckerDetails = new CheckerDetails()\n                {\n                    Comment = \"The test output should hold exactly one number.\",\n                    UserOutputFragment = receivedOutput,\n                    ExpectedOutputFragment = string.Join(\"\\r\\n\", expectedNumbers)\n                }\n            };\n        }\n\n        string[] receivedNumbers = ExtractNumbersFromText(receivedOutput);\n\n        if (receivedNumbers.Length < 1)\n        {\n            return new CheckerResult()\n            {\n                IsCorrect = false,\n                ResultType = CheckerResultType.InvalidNumberOfLines,\n                CheckerDetails = new CheckerDetails()\n                {\n                    Comment = \"The user output should hold at least one number.\",\n                    UserOutputFragment = receivedOutput,\n                    ExpectedOutputFragment = string.Join(\"\\r\\n\", expectedNumbers)\n                }\n            };\n        }\n\n        var lastReceivedNum = receivedNumbers[receivedNumbers.Length - 1];\n        var expectedNum = expectedNumbers[0];\n        bool equalNums;\n        try\n        {\n            var numReceived = double.Parse(lastReceivedNum, CultureInfo.InvariantCulture);\n            var numExpected = double.Parse(expectedNum, CultureInfo.InvariantCulture);\n            equalNums = Math.Abs(numReceived - numExpected) < DecimalPrecision;\n        }\n        catch (Exception)\n        {\n            equalNums = false;\n        }\n\n        if (!equalNums)\n        {\n            // Numbers do not match (or parse failed) --> incorrect result\n            return new CheckerResult()\n            {\n                IsCorrect = false,\n                ResultType = CheckerResultType.WrongAnswer,\n                CheckerDetails = new CheckerDetails()\n                {\n                    Comment = \"The last number in the user output \'\" + lastReceivedNum + \"\' does not match the expected output \'\" + expectedNum + \"\'.\",\n                    UserOutputFragment = lastReceivedNum,\n                    ExpectedOutputFragment = expectedNum\n                }\n            };\n        }\n        \n        // Correct result\n        return new CheckerResult()\n        {\n            IsCorrect = true,\n            ResultType = CheckerResultType.Ok,\n            CheckerDetails = new CheckerDetails()\n        };\n    }\n\n    private CheckerResult CompareTextOutput(string receivedOutput, string expectedOutput)\n    {\n        var receivedOutputCleaned =\n            Regex.Replace(receivedOutput, @\"\\W+\", \" \").Trim().ToLowerInvariant();\n        var expectedOutputCleaned =\n            Regex.Replace(expectedOutput, @\"\\W+\", \" \").Trim().ToLowerInvariant();\n\n        if (!Regex.IsMatch(receivedOutputCleaned, @\"\\b\" + expectedOutputCleaned + @\"\\b\"))\n        {\n            // The expected output was not found in the user output --> incorrect result\n            return new CheckerResult()\n            {\n                IsCorrect = false,\n                ResultType = CheckerResultType.WrongAnswer,\n                CheckerDetails = new CheckerDetails()\n                {\n                    Comment = \"The user output text does not contain the expected output text.\",\n                    UserOutputFragment = receivedOutputCleaned,\n                    ExpectedOutputFragment = expectedOutputCleaned\n                }\n            };\n        }\n\n        // Correct result\n        return new CheckerResult()\n        {\n            IsCorrect = true,\n            ResultType = CheckerResultType.Ok,\n            CheckerDetails = new CheckerDetails()\n        };\n    }\n\n    private string[] ExtractNumbersFromText(string text)\n    {\n        string numberPattern = @\"(-)?[0-9]+(\\.[0-9]+)?(e[+-]?[0-9]+)?\";\n        var numbers = new List<string>();\n        var matches = Regex.Matches(text, numberPattern);\n        foreach (var m in matches)\n        {\n            Match match = (Match)m;\n            if ((match.Index == 0) || (!char.IsLetter(text[match.Index - 1])))\n            {\n                if ((match.Index + match.Length == text.Length) ||\n                    (!char.IsLetter(text[match.Index + match.Length])))\n                {\n                    numbers.Add(match.Value);\n                }\n            }\n        }\n        return numbers.ToArray();\n    }\n\n    public void SetParameter(string parameter)\n    {\n        throw new NotImplementedException();\n    }\n}\n"
                },
                new Checker
                {
                    Name = "Text Snippet Checker",
                    Description =
                        "Checks whether the expected words and decimal numbers are present in the user output as a subsequence (after ignoring the punctuation, whitespace, and character casing). For example, \"sum = 500\" matches \"Enter 2 numbers: Num1 = Num2 = Result sum:500 USD\".",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "CSharpCodeChecker",
                    Parameter = "using System;\nusing System.Collections.Generic;\nusing System.Text.RegularExpressions;\n\nusing OJS.Workers.Common;\n\npublic class TextSnippetChecker : IChecker\n{\n    public CheckerResult Check(string inputData, string receivedOutput,\n        string expectedOutput, bool isTrialTest)\n    {\n        var receivedOutputCleaned = ExtractWordsAndNumbers(receivedOutput);\n        var expectedOutputCleaned = ExtractWordsAndNumbers(expectedOutput);\n\n        if (! Regex.IsMatch(receivedOutputCleaned, @\"\\b\" + expectedOutputCleaned + @\"\\b\"))\n        {\n            // The expected output was not found in the user output --> incorrect result\n            return new CheckerResult()\n            {\n                IsCorrect = false,\n                ResultType = CheckerResultType.WrongAnswer,\n                CheckerDetails = new CheckerDetails()\n                {\n                    Comment = \"The user output text does not contain the expected output text.\",\n                    UserOutputFragment = receivedOutputCleaned,\n                    ExpectedOutputFragment = expectedOutputCleaned\n                }\n            };\n        }\n        \n        // Correct result\n        return new CheckerResult()\n        {\n            IsCorrect = true,\n            ResultType = CheckerResultType.Ok,\n            CheckerDetails = new CheckerDetails()\n        };\n    }\n\n    private string ExtractWordsAndNumbers(string text)\n    {\n        // Skip all punctiation and whitespace and extract all words and numbers\n        var matches = Regex.Matches(text, @\"((-)?[0-9]+(\\.[0-9]+)?(e[+-]?[0-9]+)?)|\\w+\");\n        var wordsAndNumbers = new List<string>();\n        foreach (var m in matches)\n        {\n            Match match = (Match)m;\n            wordsAndNumbers.Add(match.Value);\n        }\n        var wordsAndNumbersStr = string.Join(\" \", wordsAndNumbers).ToLowerInvariant();\n        return wordsAndNumbersStr;\n    }\n    \n    public void SetParameter(string parameter)\n    {\n        throw new NotImplementedException();\n    }\n}\n"
                },
                new Checker
                {
                    Name = "Numbers Checker",
                    Description =
                        "Checks if the numbers in the user output match the expected output. Intelligently skips the unneeded messages in the user output like \"Please enter a number n:\". Works for integer and floating-point numbers. Compares with precision of 2 digits after the decimal point. If the expected output is text like \"error\", compares the texts intelligently (lowercase, skipping whitespace and punctuation).",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "CSharpCodeChecker",
                },
                new Checker
                {
                    Name = "Accept Everything",
                    Description = "Always accepts the solution as correct, disregarding its output.",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "CSharpCodeChecker",
                    Parameter = "using System;\n                    using OJS.Workers.Common;\n\n                    public class AcceptEverythingChecker : IChecker\n                    {\n                        public CheckerResult Check(string inputData, string receivedOutput,\n                            string expectedOutput, bool isTrialTest)\n                        {\n                            return new CheckerResult()\n                            {\n                                IsCorrect = true,\n                                ResultType = CheckerResultType.Ok,\n                                CheckerDetails = new CheckerDetails()\n                            };\n                        }\n\n                        public void SetParameter(string parameter)\n                        {\n                            throw new NotImplementedException();\n                        }\n                    }\n                    "
                },
                new Checker
                {
                    Name = "TrimEnd",
                    Description =
                        "Trims the end of each line (removes the trailing whitespace), then compares the user output with the expected output.",
                    DllFile = "OJS.Workers.Checkers",
                    ClassName = "TrimEndChecker",
                }
            };

            context.Checkers.AddOrUpdate(x => x.Name, checkers);
            context.SaveChanges();
        }

        protected void SeedSubmissionTypes(OjsDbContext context)
        {
            // foreach (var entity in context.SubmissionTypes)
            // {
            //    context.SubmissionTypes.Remove(entity);
            // }

            // context.SaveChanges();
            var submissionTypes = new[]
            {
                new SubmissionType
                {
                    Name = "C# code",
                    IsSelectedByDefault = true,
                    ExecutionStrategyType = ExecutionStrategyType.CompileExecuteAndCheck,
                    CompilerType = CompilerType.CSharp,
                    AdditionalCompilerArguments =
                        "/optimize+ /nologo /reference:System.Numerics.dll /reference:PowerCollections.dll",
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "C++ code",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.CPlusPlusCompileExecuteAndCheckExecutionStrategy,
                    CompilerType = CompilerType.CPlusPlusGcc,
                    AdditionalCompilerArguments =
                        "-pipe -mtune=generic -O3 -static-libgcc -static-libstdc++ -std=c++11",
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "JavaScript code (NodeJS)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "C# project/solution",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.CompileExecuteAndCheck,
                    CompilerType = CompilerType.MsBuild,
                    AdditionalCompilerArguments =
                        "/t:rebuild /p:Configuration=Release,Optimize=true /verbosity:quiet /nologo",
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "Java code",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.JavaPreprocessCompileExecuteAndCheck,
                    CompilerType = CompilerType.Java,
                    AdditionalCompilerArguments = "-encoding utf8",
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "PHP code (CGI)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PhpCgiExecuteAndCheck,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "PHP code (CLI)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PhpCliExecuteAndCheck,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "Plain text",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.CheckOnly,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "Java zip file",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.JavaZipFileCompileExecuteAndCheck,
                    CompilerType = CompilerType.JavaZip,
                    AdditionalCompilerArguments = "-encoding utf8",
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "Python code",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PythonExecuteAndCheck,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "JavaScript code (Mocha unit tests)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha,
                    CompilerType = CompilerType.None,
                    AdditionalCompilerArguments = "-R json",
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "JavaScript code (DOM unit tests)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests,
                    CompilerType = CompilerType.None,
                    AdditionalCompilerArguments = "-R json",
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "SQL Server prepare DB & run queries",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.SqlServerSingleDatabasePrepareDatabaseAndRunQueries,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "SQL Server run queries & check DB",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.SqlServerSingleDatabaseRunQueriesAndCheckDatabase,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "SQL Server run skeleton, run queries & check DB",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType =
                        ExecutionStrategyType.SqlServerSingleDatabaseRunSkeletonRunQueriesAndCheckDatabase,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "MySQL prepare DB & run queries",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.MySqlPrepareDatabaseAndRunQueries,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "MySQL run queries & check DB",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.MySqlRunQueriesAndCheckDatabase,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "MySQL run skeleton, run queries & check DB",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.MySqlRunSkeletonRunQueriesAndCheckDatabase,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "File upload",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.NotFound,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip, rar"
                },
                new SubmissionType
                {
                    Name = "JavaScript Zip File (DOM, Mocha and Module Transpiling)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType =
                        ExecutionStrategyType.NodeJsZipPreprocessExecuteAndRunUnitTestsWithDomAndMocha,
                    CompilerType = CompilerType.None,
                    AdditionalCompilerArguments = "--delay -R json",
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "JavaScript code (Unit Tests with Sinon and Mocha)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType
                        .NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy,
                    CompilerType = CompilerType.None,
                    AdditionalCompilerArguments = "-R json",
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "JavaScript code (Async DOM unit tests with React)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType =
                        ExecutionStrategyType.NodeJsExecuteAndRunAsyncJsDomTestsWithReactExecutionStrategy,
                    CompilerType = CompilerType.None,
                    AdditionalCompilerArguments = "-R json",
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "HTML and CSS Zip File (DOM and Mocha)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy,
                    CompilerType = CompilerType.None,
                    AdditionalCompilerArguments = "-R json",
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "C# Unit Tests",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.CSharpUnitTestsExecutionStrategy,
                    CompilerType = CompilerType.MsBuildLibrary,
                    AdditionalCompilerArguments =
                        "/t:rebuild /p:Configuration=Release,Optimize=true /verbosity:quiet /nologo",
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "C# Project Tests",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.CSharpProjectTestsExecutionStrategy,
                    CompilerType = CompilerType.MsBuildLibrary,
                    AdditionalCompilerArguments =
                        "/t:rebuild /p:Configuration=Release,Optimize=true /verbosity:quiet /nologo",
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "Java Project Tests",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.JavaProjectTestsExecutionStrategy,
                    CompilerType = CompilerType.Java,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "Java Unit Tests",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.JavaUnitTestsExecutionStrategy,
                    CompilerType = CompilerType.JavaInPlaceCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "C++ Zip File",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.CPlusPlusZipFileExecutionStrategy,
                    CompilerType = CompilerType.CPlusPlusZip,
                    AdditionalCompilerArguments = "-pipe -mtune=generic -static-libgcc -static-libstdc++ -std=c++11",
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "Java Project (Spring + Hibernate)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.JavaSpringAndHibernateProjectExecutionStrategy,
                    CompilerType = CompilerType.JavaZip,
                    AdditionalCompilerArguments = "-encoding utf8",
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "Ruby Code",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.RubyExecutionStrategy,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = ".NET Core Project",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCoreProjectExecutionStrategy,
                    CompilerType = CompilerType.DotNetCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = ".NET Core Project Tests",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCoreProjectTestsExecutionStrategy,
                    CompilerType = CompilerType.DotNetCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "C# code (.NET Core)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCoreCompileExecuteAndCheck,
                    CompilerType = CompilerType.CSharpDotNetCore,
                    AdditionalCompilerArguments = "-nologo",
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = ".NET Core Unit Tests",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCoreUnitTestsExecutionStrategy,
                    CompilerType = CompilerType.DotNetCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "Python code (unittest unit tests)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PythonCodeExecuteAgainstUnitTests,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "Python Unit Tests (unittest)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PythonUnitTests,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "Python Project (unittest unit tests)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PythonProjectTests,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "Python Project Unit Tests (unittest)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PythonProjectUnitTests,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "JS Projects Mocha Unit Tests",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.RunSpaAndExecuteMochaTestsExecutionStrategy,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "GО code",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.GolangCompileExecuteAndCheck,
                    CompilerType = CompilerType.GolangCompiler,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "C# code (.NET 5)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCore5CompileExecuteAndCheck,
                    CompilerType = CompilerType.CSharpDotNetCore,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "C# code (.NET 6)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck,
                    CompilerType = CompilerType.CSharpDotNetCore,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = ".NET Core Project (.NET 5)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCore5ProjectExecutionStrategy,
                    CompilerType = CompilerType.DotNetCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = ".NET Core Project (.NET 6)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCore6ProjectExecutionStrategy,
                    CompilerType = CompilerType.DotNetCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = ".NET Core Project Tests (.NET 5)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCore5ProjectTestsExecutionStrategy,
                    CompilerType = CompilerType.DotNetCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = ".NET Core Project Tests (.NET 6)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCore6ProjectTestsExecutionStrategy,
                    CompilerType = CompilerType.DotNetCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = ".NET Core Unit Tests (.NET 5)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCore5UnitTestsExecutionStrategy,
                    CompilerType = CompilerType.DotNetCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = ".NET Core Unit Tests (.NET 6)",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.DotNetCore6UnitTestsExecutionStrategy,
                    CompilerType = CompilerType.DotNetCompiler,
                    AllowBinaryFilesUpload = true,
                    AllowedFileExtensions = "zip"
                },
                new SubmissionType
                {
                    Name = "PG run queries & check DB",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PostgreSqlRunQueriesAndCheckDatabase,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "PG run skeleton, prepare DB & run queries",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PostgreSqlPrepareDatabaseAndRunQueries,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                },
                new SubmissionType
                {
                    Name = "PG run skeleton, run queries & check DB",
                    IsSelectedByDefault = false,
                    ExecutionStrategyType = ExecutionStrategyType.PostgreSqlRunSkeletonRunQueriesAndCheckDatabase,
                    CompilerType = CompilerType.None,
                    AllowBinaryFilesUpload = false,
                    AllowedFileExtensions = null
                }
            };

            context.SubmissionTypes.AddOrUpdate(x => x.Name, submissionTypes);
            context.SaveChanges();
        }

        private void SeedCategoryContestProblem(IOjsDbContext context)
        {
            foreach (var categoryToBeDeleted in context.ContestCategories)
            {
                context.ContestCategories.Remove(categoryToBeDeleted);
            }

            foreach (var contestToBeDeleted in context.Contests)
            {
                context.Contests.Remove(contestToBeDeleted);
            }

            foreach (var problemToBeDeleted in context.Problems)
            {
                context.Problems.Remove(problemToBeDeleted);
            }

            var category = new ContestCategory
            {
                Name = "Category",
                OrderBy = 1,
                IsVisible = true,
                IsDeleted = false,
            };

            var otherCategory = new ContestCategory
            {
                Name = "Other Category",
                OrderBy = 1,
                IsVisible = true,
                IsDeleted = false,
            };

            var contest = new Contest
            {
                Name = "Contest",
                OrderBy = 1,
                PracticeStartTime = DateTime.Now.AddDays(-2),
                StartTime = DateTime.Now.AddDays(-2),
                IsVisible = true,
                IsDeleted = false,
                Category = category
            };

            var otherContest = new Contest
            {
                Name = "Other Contest",
                OrderBy = 2,
                PracticeStartTime = DateTime.Now.AddDays(-2),
                StartTime = DateTime.Now.AddDays(-2),
                IsVisible = true,
                IsDeleted = false,
                Category = category
            };

            var problemGroup1 = new ProblemGroup
            {
                OrderBy = 0,
                Contest = contest
            };

            var problemGroup2 = new ProblemGroup
            {
                OrderBy = 1,
                Contest = contest
            };

            var problem = new Problem
            {
                Name = "Problem",
                MaximumPoints = 100,
                TimeLimit = 10,
                MemoryLimit = 10,
                OrderBy = 1,
                ShowResults = true,
                IsDeleted = false,
                ProblemGroup = problemGroup1
            };

            var otherProblem = new Problem
            {
                Name = "Other Problem",
                MaximumPoints = 100,
                TimeLimit = 10,
                MemoryLimit = 10,
                OrderBy = 1,
                ShowResults = true,
                IsDeleted = false,
                ProblemGroup = problemGroup2
            };

            var test = new Test
            {
                InputDataAsString = "Input",
                OutputDataAsString = "Output",
                OrderBy = 0,
                IsTrialTest = false,
                Problem = problem,
            };

            var user = new UserProfile
            {
                UserName = "Ifaka",
                Email = "Nekav@nekav.com"
            };

            var participant = new Participant
            {
                Contest = contest,
                IsOfficial = false,
                User = user
            };

            var submission = new Submission
            {
                Problem = problem,
                Participant = participant,
                CreatedOn = DateTime.Now
            };

            for (int i = 0; i < 10; i++)
            {
                test.TestRuns.Add(new TestRun
                {
                    MemoryUsed = 100,
                    TimeUsed = 100,
                    CheckerComment = "Checked!",
                    ExecutionComment = "Executed!",
                    ResultType = TestRunResultType.CorrectAnswer,
                    Submission = submission
                });
            }

            context.Problems.Add(problem);
            context.Problems.Add(otherProblem);
            context.Contests.Add(otherContest);
            context.ContestCategories.Add(otherCategory);
            context.Tests.Add(test);
        }

        private void SeedSubmissionsAndTestRuns(OjsDbContext context)
        {
            foreach (var submission in context.Submissions)
            {
                context.Submissions.Remove(submission);
            }

            foreach (var testRun in context.TestRuns)
            {
                context.TestRuns.Remove(testRun);
            }

            foreach (var participantToDelete in context.Participants)
            {
                context.Participants.Remove(participantToDelete);
            }

            Random random = new Random();

            List<TestRun> testRuns = new List<TestRun>();

            var test = new Test
            {
                IsTrialTest = false,
                OrderBy = 1
            };

            for (int i = 0; i < 1000; i++)
            {
                testRuns.Add(new TestRun
                {
                    TimeUsed = (random.Next() % 10) + 1,
                    MemoryUsed = (random.Next() % 1500) + 200,
                    ResultType = (TestRunResultType)(random.Next() % 5),
                    Test = test
                });
            }

            var contest = new Contest
            {
                Name = "Contest batka 2",
                StartTime = DateTime.Now.AddDays(1),
                EndTime = DateTime.Now.AddDays(2),
                IsDeleted = false,
                IsVisible = true,
                OrderBy = 1
            };

            var problemGroup = new ProblemGroup
            {
                OrderBy = 0,
                Contest = contest
            };

            var problem = new Problem
            {
                ProblemGroup = problemGroup,
                Name = "Problem",
                MaximumPoints = 100,
                MemoryLimit = 100,
                OrderBy = 1
            };

            var user = new UserProfile
            {
                UserName = "Ifaka",
                Email = "Nekav@nekav.com"
            };

            var participant = new Participant
            {
                Contest = contest,
                IsOfficial = false,
                User = user
            };

            for (int i = 0; i < 100; i++)
            {
                var submission = new Submission
                {
                    Problem = problem,
                    Participant = participant
                };

                for (int j = 0; j < (random.Next() % 20) + 5; j++)
                {
                    submission.TestRuns.Add(testRuns[random.Next() % 1000]);
                }

                context.Submissions.Add(submission);
            }
        }

        private void SeedContest(OjsDbContext context)
        {
            foreach (var entity in context.Contests)
            {
                context.Contests.Remove(entity);
            }

            context.Contests.Add(new Contest
            {
                Name = "Contest",
            });

            context.SaveChanges();
        }

        private void SeedProblem(OjsDbContext context)
        {
            foreach (var problem in context.Problems)
            {
                context.Problems.Remove(problem);
            }

            var contest = context.Contests.FirstOrDefault(x => x.Name == "Contest");

            contest.ProblemGroups.Add(new ProblemGroup
            {
                OrderBy = 0,
                Problems = new List<Problem>
                {
                    new Problem
                    {
                        Name = "Problem"
                    },
                    new Problem
                    {
                        Name = "Other problem"
                    }
                }
            });

            context.SaveChanges();
        }

        private void SeedTest(OjsDbContext context)
        {
            foreach (var entity in context.Tests)
            {
                context.Tests.Remove(entity);
            }

            var selectedProblem = context.Problems.FirstOrDefault(x => x.Name == "Problem" && !x.IsDeleted);

            selectedProblem.Tests.Add(new Test
            {
                InputDataAsString = "Trial input test 1",
                OutputDataAsString = "Trial output test 1",
                IsTrialTest = true
            });

            selectedProblem.Tests.Add(new Test
            {
                InputDataAsString = "Trial input test 2",
                OutputDataAsString = "Trial output test 2",
                IsTrialTest = true
            });

            for (int i = 0; i < 10; i++)
            {
                selectedProblem.Tests.Add(new Test
                {
                    InputDataAsString = i.ToString(),
                    OutputDataAsString = (i + 1).ToString(),
                    IsTrialTest = false
                });
            }

            var otherProblem = context.Problems.FirstOrDefault(x => x.Name == "Other problem" && !x.IsDeleted);

            otherProblem.Tests.Add(new Test
            {
                InputDataAsString = "Trial input test 1 other",
                OutputDataAsString = "Trial output test 1 other",
                IsTrialTest = true
            });

            otherProblem.Tests.Add(new Test
            {
                InputDataAsString = "Trial input test 2 other",
                OutputDataAsString = "Trial output test 2 other",
                IsTrialTest = true
            });

            for (int i = 0; i < 10; i++)
            {
                otherProblem.Tests.Add(new Test
                {
                    InputDataAsString = i.ToString() + "other",
                    OutputDataAsString = (i + 1).ToString() + "other",
                    IsTrialTest = false
                });
            }
        }

        private void SeedRandomContests(OjsDbContext context)
        {
            foreach (var contest in context.Contests)
            {
                context.Contests.Remove(contest);
            }

            var category = new ContestCategory
            {
                Name = "Category",
                OrderBy = 1,
                IsVisible = true,
                IsDeleted = false,
            };

            context.Contests.Add(
                new Contest
                {
                    Name = "DSA future",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(10),
                    EndTime = DateTime.Now.AddHours(19),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "DSA 2 out",
                    IsVisible = false,
                    StartTime = DateTime.Now.AddHours(10),
                    EndTime = DateTime.Now.AddHours(19),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "DSA 3 past",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(-10),
                    EndTime = DateTime.Now.AddHours(-8),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "DSA 2 active",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddHours(2),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "JS Apps another active",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddHours(2),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "JS Apps another active 2",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddHours(2),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "JS Apps another active 3",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(-2),
                    EndTime = DateTime.Now.AddHours(2),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "JS Apps 2 past",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(-10),
                    EndTime = DateTime.Now.AddHours(-8),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "JS Apps 3 past",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(-10),
                    EndTime = DateTime.Now.AddHours(-8),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "JS Apps 3 past",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(10),
                    EndTime = DateTime.Now.AddHours(18),
                    Category = category,
                });
            context.Contests.Add(
                new Contest
                {
                    Name = "JS Apps 3 past",
                    IsVisible = true,
                    StartTime = DateTime.Now.AddHours(10),
                    EndTime = DateTime.Now.AddHours(18),
                    Category = category,
                });
        }

        private void SeedSettings(OjsDbContext context, IEnumerable<Setting> settings)
        {
            foreach (var setting in settings)
            {
                if (context.Settings.Any(s => s.Name == setting.Name))
                {
                    continue;
                }

                context.Settings.Add(setting);
            }

            context.SaveChanges();
        }
    }
}
