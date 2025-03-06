#nullable disable
namespace OJS.Workers.ExecutionStrategies.Python
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Helpers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    using static OJS.Workers.Common.Constants;
    using static OJS.Workers.ExecutionStrategies.Python.PythonConstants;

    public class PythonProjectUnitTestsExecutionStrategy<TSettings> : PythonUnitTestsExecutionStrategy<TSettings>
        where TSettings : PythonProjectUnitTestsExecutionStrategySettings
    {
        private const string ProjectFolderName = "project";
        private const string ProjectFilesCountPlaceholder = "# project_files_count ";
        private const string ClassNameRegexPattern = @"^class\s+(\w+)";
        private const string UpperCaseSplitRegexPattern = @"(?<!^)(?=[A-Z])";
        private const string ProjectFilesNotCapturedCorrectlyErrorMessageTemplate =
            "There should be {0} classes in test #{1}, but found {2}. Ensure the test is correct";

        private readonly string classesInSubfoldersRegexPattern =
            $@"^from\s+{ProjectFolderName}\.(\w+.+)(?:\.\w+\s+import)\s+(\w+)\s*$";

        private readonly string projectFilesCountRegexPattern = $@"^{ProjectFilesCountPlaceholder}\s*(\d+)\s*$";
        private readonly string projectFilesRegexPattern =
            $@"(?:^from\s+[\s\S]+?)?{ClassNameRegexPattern}[\s\S]+?(?=^from|^class)";

        private readonly string projectFilesCountNotSpecifiedInSolutionSkeletonErrorMessage =
            $"Expecting \"{ProjectFilesCountPlaceholder}\" in solution skeleton followed by the number of files that the project has";

        private string projectDirectoryPath;
        private int expectedProjectFilesCount;

        public PythonProjectUnitTestsExecutionStrategy(
            IOjsSubmission submission,
            IProcessExecutorFactory processExecutorFactory,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
            : base(submission, processExecutorFactory, settingsProvider, logger)
        {
        }

        protected override IEnumerable<string> ExecutionArguments
            => new[]
            {
                IgnorePythonEnvVarsArgument,
                DontAddUserSiteDirectoryArgument,
                ModuleNameArgument,
                UnitTestModuleName,
                DiscoverTestsCommandName,
            };

        protected override Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            SaveZipSubmission(executionContext.FileContent, this.WorkingDirectory);

            var executor = this.CreateRestrictedExecutor();
            var checker = executionContext.Input.GetChecker();

            this.projectDirectoryPath = FileHelpers.BuildPath(this.WorkingDirectory, ProjectFolderName);
            this.expectedProjectFilesCount = this.GetExpectedProjectFilesCount(
                executionContext.Input.TaskSkeletonAsString);

            return this.RunTests(string.Empty, executor, checker, executionContext, result);
        }

        protected override TestResult RunIndividualUnitTest(
            ref int originalTestsPassed,
            string codeSavePath,
            IExecutor executor,
            IChecker checker,
            IExecutionContext<TestsInputModel> executionContext,
            TestContext test,
            bool isFirstRun)
        {
            this.SaveTestProjectFiles(this.expectedProjectFilesCount, test);

            var processExecutionResult = this.Execute(
                executionContext,
                executor,
                codeSavePath,
                directory: this.WorkingDirectory)
                .GetAwaiter()
                .GetResult();

            return this.GetUnitTestsResultFromExecutionResult(
                ref originalTestsPassed,
                checker,
                processExecutionResult,
                test,
                isFirstRun);
        }

        /// <summary>
        /// Gets a file name with extension for the provided class. The convention is SampleClass -> sample_class.py.
        /// </summary>
        /// <param name="className">The python class name.</param>
        /// <returns>File name for the python class.</returns>
        private static string GetFileNameWithExtensionForClass(string className)
            => string.Join(
                "_",
                Regex
                    .Split(className, UpperCaseSplitRegexPattern)
                    .Select(x => x.ToLower()))
                + PythonFileExtension;

        /// <summary>
        /// Generates and saves all python files that are being tested by the user.
        /// Files are extracted and generated by the test input, which contains all file contents in a single string.
        /// </summary>
        /// <param name="expectedFilesCount">Predefined value that acts as a validity check.</param>
        /// <param name="test">The test on which the operation is performed.</param>
        /// <exception cref="ArgumentException">Thrown if the expected files count does not match the captured files from the test.</exception>
        private void SaveTestProjectFiles(int expectedFilesCount, TestContext test)
        {
            var projectFilesToBeCreated = this.GetProjectFilesToBeCreated(test);

            if (projectFilesToBeCreated.Count != expectedFilesCount)
            {
                throw new ArgumentException(string.Format(
                    ProjectFilesNotCapturedCorrectlyErrorMessageTemplate,
                    expectedFilesCount,
                    test.Id,
                    projectFilesToBeCreated.Count));
            }

            foreach (var projectFile in projectFilesToBeCreated)
            {
                PythonStrategiesHelper.CreateFileInPackage(projectFile.Key, projectFile.Value);
            }
        }

        /// <summary>
        /// Gets the predefined count of the files that need to be generated and put in the project directory.
        /// </summary>
        /// <param name="solutionSkeleton">The skeleton in which this count is written upon task creation.</param>
        /// <returns>Number of files that need to be extracted from every test input and saved in the working directory.</returns>
        /// <exception cref="ArgumentException">Exception thrown if the count is not given as expected.</exception>
        private int GetExpectedProjectFilesCount(string solutionSkeleton)
        {
            solutionSkeleton = solutionSkeleton ?? string.Empty;

            var projectFilesCountRegex = new Regex(this.projectFilesCountRegexPattern);
            var projectFilesCountAsString = projectFilesCountRegex.Match(solutionSkeleton).Groups[1].Value;

            if (int.TryParse(projectFilesCountAsString, out var projectFilesCount))
            {
                return projectFilesCount;
            }

            throw new ArgumentException(this.projectFilesCountNotSpecifiedInSolutionSkeletonErrorMessage);
        }

        /// <summary>
        /// Gets files to be created in a project directory, by extracting all classes from the test input.
        /// The test input contains multiple classes, that have to be extracted and put in separate files and folders.
        /// </summary>
        /// <param name="test">The test on with the operation is performed.</param>
        /// <returns>A dictionary containing full file path as a key and file content as a value.</returns>
        private Dictionary<string, string> GetProjectFilesToBeCreated(TestContext test)
        {
            var testInput = test.Input;

            var filesRegex = new Regex(this.projectFilesRegexPattern, RegexOptions.Multiline);
            var classNameRegex = new Regex(ClassNameRegexPattern, RegexOptions.Multiline);
            var classesInSubfoldersRegex = new Regex(this.classesInSubfoldersRegexPattern, RegexOptions.Multiline);

            // we get all distinct files that should be created in subfolders as Key -> className and Value -> filePath
            // example: 'from project.animal.dog.brown_dog import BrownDog'
            // the class name is 'BrownDog'
            // the file path is '{projectDirectory}/animal/dog/brown_dog.py'
            var filesInSubfolderPaths = classesInSubfoldersRegex.Matches(testInput)
                .Cast<Match>()
                .GroupBy(m => m.Groups[2].Value)
                .Select(gr => gr.First())
                .ToDictionary(
                    m => m.Groups[2].Value,
                    m => this.GetFilePathForClass(m.Groups[2].Value, m.Groups[1].Value.Split('.')));

            var projectFilesToBeCreated = filesRegex.Matches(testInput)
                .Cast<Match>()
                .ToDictionary(
                    m => GetFilePath(m.Groups[1].Value),
                    m => m.Value.Trim());

            // removing all matches and leaving the last/only one, which the regex does not capture
            var lastFileContent = filesRegex.Replace(testInput, string.Empty).Trim();
            var lastClassName = classNameRegex.Match(lastFileContent).Groups[1].Value;
            var lastFilePath = GetFilePath(lastClassName);

            projectFilesToBeCreated.Add(lastFilePath, lastFileContent);

            return projectFilesToBeCreated;

            string GetFilePath(string className)
                => filesInSubfolderPaths.TryGetValue(className, out var filePath)
                    ? filePath
                    : this.GetFilePathForClass(className);
        }

        /// <summary>
        /// Constructs full path for the class file, by combining all subfolders with the file name.
        /// </summary>
        /// <param name="className">The name of the class.</param>
        /// <param name="subfolderNames">The subfolders in which the class file should be created.</param>
        /// <returns>Full path of the file that should be created for a class.</returns>
        private string GetFilePathForClass(string className, IEnumerable<string> subfolderNames = null)
        {
            var pathArguments = new List<string> { this.projectDirectoryPath };

            pathArguments.AddRange(subfolderNames ?? Enumerable.Empty<string>());
            pathArguments.Add(GetFileNameWithExtensionForClass(className));

            return FileHelpers.BuildPath(pathArguments.ToArray());
        }
    }

    public record PythonProjectUnitTestsExecutionStrategySettings(
        int BaseTimeUsed,
        int BaseMemoryUsed,
        string PythonExecutablePath)
        : PythonUnitTestsExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed, PythonExecutablePath);
}