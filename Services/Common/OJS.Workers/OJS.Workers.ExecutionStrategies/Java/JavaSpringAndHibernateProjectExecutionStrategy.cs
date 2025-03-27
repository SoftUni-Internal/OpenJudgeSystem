#nullable disable
namespace OJS.Workers.ExecutionStrategies.Java;

using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using OJS.Workers.Common;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using OJS.Workers.Compilers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;
using static OJS.Workers.Common.Constants;
using static OJS.Workers.ExecutionStrategies.Helpers.JavaStrategiesHelper;

public class JavaSpringAndHibernateProjectExecutionStrategy<TSettings> : JavaProjectTestsExecutionStrategy<TSettings>
    where TSettings : JavaSpringAndHibernateProjectExecutionStrategySettings
{
    private const string PomXmlFileNameAndExtension = "pom.xml";
    private const string ApplicationPropertiesFileName = "application.properties";
    private const string IntelliJProjectTemplatePattern = "src/main/java";
    private const string IntelliJTestProjectTemplatePattern = "src/test/java";
    private const string PropertySourcePattern = @"(@PropertySources?\((?:.*?)\))";
    private const string PomXmlNamespace = @"http://maven.apache.org/POM/4.0.0";
    private const string StartClassNodeXPath = @"//pomns:properties/pomns:start-class";
    private const string MavenTestCommand = "-o test -f {0} -Dtest=\"{1}\"";
    private const string MavenBuild = "-o compile";
    private const string TestsFolderPattern = @"src/test/java/*";
    private const string MainCodeFolderPattern = @"src/main/java/";
    private const string MavenErrorFailurePattern = @"\[ERROR\]";

    public JavaSpringAndHibernateProjectExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        ICompilerFactory compilerFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, compilerFactory, settingsProvider, logger)
    {
    }

    protected string PackageName { get; set; }

    protected string MainClassFileName { get; set; }

    protected string ProjectRootDirectoryInSubmissionZip { get; set; }

    protected string ProjectTestDirectoryInSubmissionZip { get; set; }

    protected override string ClassPathArgument
        => $"-cp {this.Settings.JavaLibrariesPath}*{ClassPathArgumentSeparator}{this.WorkingDirectory}{Path.DirectorySeparatorChar}target{Path.DirectorySeparatorChar}* ";

    protected void PreparePomXml(string submissionFilePath)
    {
        var extractionDirectory = DirectoryHelpers.CreateTempDirectoryForExecutionStrategy();

        var pomXmlFilePath = FileHelpers.ExtractFileFromZip(
            submissionFilePath,
            PomXmlFileNameAndExtension,
            extractionDirectory);

        if (string.IsNullOrEmpty(pomXmlFilePath))
        {
            throw new SolutionException($"A {PomXmlFileNameAndExtension} file was not found in the submission!");
        }

        this.ReplacePom(pomXmlFilePath);
        var mainClassFolderPathInZip = Path.GetDirectoryName(FileHelpers
            .GetFilePathsFromZip(submissionFilePath)
            .FirstOrDefault(f => f.EndsWith(PomXmlFileNameAndExtension)));

        FileHelpers.AddFilesToZipArchive(submissionFilePath, mainClassFolderPathInZip, pomXmlFilePath);
        DirectoryHelpers.SafeDeleteDirectory(extractionDirectory, true);
    }

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        string submissionFilePath;
        try
        {
            submissionFilePath = this.CreateSubmissionFile(executionContext);
        }
        catch (SolutionException exception)
        {
            result.IsCompiledSuccessfully = false;
            result.CompilerComment = exception.Message;

            return result;
        }

        FileHelpers.UnzipFile(submissionFilePath, this.WorkingDirectory);

        var pomXmlPath = FileHelpers.FindFileMatchingPattern(this.WorkingDirectory, PomXmlFileNameAndExtension);

        var mavenArgs = new[] { MavenBuild };

        var mavenExecutor = this.CreateRestrictedExecutor();

        var packageExecutionResult = await mavenExecutor.Execute(
          this.Settings.MavenPath,
          executionContext.TimeLimit,
          executionContext.MemoryLimit,
          executionArguments: mavenArgs,
          workingDirectory: this.WorkingDirectory);

        var mavenBuildFailureRegex = new Regex(MavenErrorFailurePattern);

        result.IsCompiledSuccessfully = !mavenBuildFailureRegex.IsMatch(packageExecutionResult.ReceivedOutput);

        if (!result.IsCompiledSuccessfully)
        {
            result.CompilerComment = GetMavenErrorsComment(packageExecutionResult.ReceivedOutput);
            return result;
        }

        var executor = this.CreateRestrictedExecutor();

        var checker = executionContext.Input.GetChecker();
        var testIndex = 0;

        foreach (var test in executionContext.Input.Tests)
        {
            var testFile = this.TestNames[testIndex++];
            mavenArgs = [string.Format(MavenTestCommand, pomXmlPath, testFile)];

            var processExecutionResult = await executor.Execute(
            this.Settings.MavenPath,
            executionContext.TimeLimit,
            executionContext.MemoryLimit,
            executionArguments: mavenArgs,
            workingDirectory: this.WorkingDirectory);

            ValidateJvmInitialization(processExecutionResult.ReceivedOutput);

            if (processExecutionResult.ReceivedOutput.Contains($"Could not find class: {testFile}"))
            {
                throw new SolutionException("The tests could not be loaded, the project structure is incorrect.");
            }

            var message = EvaluateMavenTestOutput(processExecutionResult.ReceivedOutput, mavenBuildFailureRegex);

            var testResult = CheckAndGetTestResult(
                test,
                processExecutionResult,
                checker,
                message);

            result.Results.Add(testResult);
        }

        return result;
    }

    protected override string PrepareSubmissionFile(IExecutionContext<TestsInputModel> context)
    {
        var submissionFilePath = $"{this.WorkingDirectory}{Path.DirectorySeparatorChar}{SubmissionFileName}";
        File.WriteAllBytes(submissionFilePath, context.FileContent);
        FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);

        this.ExtractPackageAndDirectoryNames(submissionFilePath);
        this.ValidateSubmissionFile(submissionFilePath);
        this.OverwriteApplicationProperties(submissionFilePath);
        this.RemovePropertySourceAnnotationsFromMainClass(submissionFilePath);
        this.AddTestsToUserSubmission(context, submissionFilePath);
        this.PreparePomXml(submissionFilePath);

        return submissionFilePath;
    }

    protected void ExtractPackageAndDirectoryNames(string submissionFilePath)
    {
        this.MainClassFileName = this.ExtractEntryPointFromPomXml(submissionFilePath);

        this.PackageName = this.MainClassFileName[..this.MainClassFileName.LastIndexOf('.')];

        var normalizedPath = this.PackageName.Replace(".", "/");

        this.ProjectRootDirectoryInSubmissionZip = $"{IntelliJProjectTemplatePattern}/{normalizedPath}/";
        this.ProjectTestDirectoryInSubmissionZip = $"{IntelliJTestProjectTemplatePattern}/{normalizedPath}/";

        var fileNameWithoutExtension = this.MainClassFileName[(this.MainClassFileName.LastIndexOf('.') + 1)..];

        this.MainClassFileName = fileNameWithoutExtension + javaSourceFileExtension;
    }

    protected void OverwriteApplicationProperties(string submissionZipFilePath)
    {
        var fakeApplicationPropertiesText = @"
                spring.jpa.properties.hibernate.show_sql= false
                spring.jpa.properties.hibernate.use_sql_comments=false
                spring.jpa.properties.hibernate.format_sql=false
                logging.level.root=off
                spring.datasource.url=jdbc:h2:mem:testdb
                spring.datasource.driverClassName=org.h2.Driver
                spring.datasource.username=sa
                spring.datasource.password=
                spring.jpa.database-platform=org.hibernate.dialect.H2Dialect
                spring.jpa.hibernate.ddl-auto=create-drop";

        var fakeApplicationPropertiesPath = $"{this.WorkingDirectory}{Path.DirectorySeparatorChar}{ApplicationPropertiesFileName}";
        File.WriteAllText(fakeApplicationPropertiesPath, fakeApplicationPropertiesText);

        var pathsInZip = FileHelpers.GetFilePathsFromZip(submissionZipFilePath);

        var resourceDirectory = Path.GetDirectoryName(pathsInZip.FirstOrDefault(f => f.EndsWith(ApplicationPropertiesFileName)));

        if (string.IsNullOrEmpty(resourceDirectory))
        {
            throw new SolutionException("The resource directory was not found in the project!");
        }

        FileHelpers.AddFilesToZipArchive(submissionZipFilePath, resourceDirectory, fakeApplicationPropertiesPath);
        File.Delete(fakeApplicationPropertiesPath);
    }

    protected void RemovePropertySourceAnnotationsFromMainClass(string submissionFilePath)
    {
        var extractionDirectory = DirectoryHelpers.CreateTempDirectoryForExecutionStrategy();

        var mainClassFilePath = FileHelpers.ExtractFileFromZip(
            submissionFilePath,
            this.MainClassFileName,
            extractionDirectory);

        var mainClassContent = File.ReadAllText(mainClassFilePath);

        var propertySourceMatcher = new Regex(PropertySourcePattern);
        while (propertySourceMatcher.IsMatch(mainClassContent))
        {
            mainClassContent = Regex.Replace(mainClassContent, PropertySourcePattern, string.Empty);
        }

        File.WriteAllText(mainClassFilePath, mainClassContent);
        var pomXmlFolderPathInZip = Path.GetDirectoryName(FileHelpers
            .GetFilePathsFromZip(submissionFilePath)
            .FirstOrDefault(f => f.EndsWith(this.MainClassFileName)));

        if (pomXmlFolderPathInZip is null)
        {
            throw new SolutionException($"A {PomXmlFileNameAndExtension} file was not found in the submission!");
        }

        FileHelpers.AddFilesToZipArchive(submissionFilePath, pomXmlFolderPathInZip, mainClassFilePath);
        DirectoryHelpers.SafeDeleteDirectory(extractionDirectory, true);
    }

    protected override void AddTestsToUserSubmission(
        IExecutionContext<TestsInputModel> context,
        string submissionZipFilePath)
    {
        var testNumber = 0;
        var filePaths = new string[context.Input.Tests.Count()];

        FileHelpers.RemoveFilesFromZip(
            submissionZipFilePath,
            TestsFolderPattern);

        foreach (var test in context.Input.Tests)
        {
            var className = JavaCodePreprocessorHelper.GetPublicClassName(test.Input);
            var testFileName = $"{this.WorkingDirectory}{Path.DirectorySeparatorChar}{className}{javaSourceFileExtension}";

            File.WriteAllText(testFileName, $"package {this.PackageName};{Environment.NewLine}{test.Input}");

            filePaths[testNumber] = testFileName;
            this.TestNames.Add($"{this.PackageName}.{className}");
            testNumber++;
        }

        FileHelpers.AddFilesToZipArchive(
            submissionZipFilePath,
            this.ProjectTestDirectoryInSubmissionZip,
            filePaths);
        FileHelpers.DeleteFiles(filePaths);
    }

    protected override void ExtractUserClassNames(string submissionFilePath) =>
        this.UserClassNames.AddRange(FileHelpers
            .GetFilePathsFromZip(submissionFilePath)
            .Where(x => !x.EndsWith("/") && x.EndsWith(javaSourceFileExtension))
            .Select(x => x.Contains(IntelliJProjectTemplatePattern)
                ? x.Substring(x.LastIndexOf(
                                  IntelliJProjectTemplatePattern,
                                  StringComparison.Ordinal)
                              + IntelliJProjectTemplatePattern.Length
                              + 1)
                : x)
            .Select(x => x.Contains(".") ? x.Substring(0, x.LastIndexOf(".", StringComparison.Ordinal)) : x)
            .Select(x => x.Replace("/", ".")));

    private static string GetMavenErrorsComment(string testOutput)
    {
        var sb = new StringBuilder();

        foreach (var line in testOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                     .Where(x => x
                         .StartsWith("[ERROR]") || x.StartsWith("[FAILURE]")))
        {
            sb.Append("\t" + line);
        }

        return sb.ToString();
    }

    private static string EvaluateMavenTestOutput(string testOutput, Regex testErrorMatcher)
    {
        var message = TestPassedMessage;
        var errorMatch = testErrorMatcher.Match(testOutput);

        if (!errorMatch.Success)
        {
            return message;
        }

        return GetMavenErrorsComment(testOutput);
    }

    private void ValidateSubmissionFile(string submissionFilePath)
    {
        // Main class is validated against the value in pom.xml.
        // If package names don't correspond,
        // then the tests are saved in a different folder structure (see how this.PackageName is exported)
        // and when ran, tests are unable to find their correct configuration.
        if (!this.ValidateMainClassFileName(submissionFilePath))
        {
            throw new SolutionException($"The submission does not contain a start class at '{this.GetMainClassFilePath()}'. Check your folder structure and {PomXmlFileNameAndExtension} start-class definition.");
        }

        if (!ValidateFolderStructure(submissionFilePath))
        {
            throw new SolutionException($"The folder structure is invalid! The zip folder structure must contain files with paths: '{MainCodeFolderPattern}' and '{PomXmlFileNameAndExtension}' start-class definition.");
        }
    }

    private static bool ValidateFolderStructure(string submissionFilePath)
    {
        var paths = FileHelpers.GetFilePathsFromZip(submissionFilePath).ToList();

        return paths.Any(x => x.StartsWith(MainCodeFolderPattern)) && paths.Any(x => x.StartsWith(PomXmlFileNameAndExtension));
    }

    private bool ValidateMainClassFileName(string submissionFilePath) => FileHelpers.FileExistsInZip(submissionFilePath, this.GetMainClassFilePath());

    private string GetMainClassFilePath() => FileHelpers.BuildPath(this.ProjectRootDirectoryInSubmissionZip, this.MainClassFileName);

    private void ReplacePom(string pomXmlFilePath)
    {
        FileHelpers.DeleteFiles(pomXmlFilePath);

        var newPomFileContent = File.ReadAllText(this.Settings.JavaSpringAndHibernateStrategyPomFilePath);
        FileHelpers.WriteAllText(pomXmlFilePath, newPomFileContent);
    }

    private string ExtractEntryPointFromPomXml(string submissionFilePath)
    {
        var pomXmlPath = FileHelpers.ExtractFileFromZip(submissionFilePath, PomXmlFileNameAndExtension, this.WorkingDirectory);

        if (string.IsNullOrEmpty(pomXmlPath))
        {
            throw new SolutionException($"A {PomXmlFileNameAndExtension} file was not found in the submission!");
        }

        var pomXml = new XmlDocument();
        pomXml.Load(pomXmlPath);

        var namespaceManager = new XmlNamespaceManager(pomXml.NameTable);
        namespaceManager.AddNamespace("pomns", PomXmlNamespace);

        XmlNode rootNode = pomXml.DocumentElement;

        var packageName = rootNode?.SelectSingleNode(StartClassNodeXPath, namespaceManager);

        if (packageName == null)
        {
            throw new SolutionException($"Missing entry point: No start-class defined in {PomXmlFileNameAndExtension}. Ensure your submission specifies a valid main class.");
        }

        FileHelpers.DeleteFiles(pomXmlPath);
        return packageName.InnerText.Trim();
    }
}

public record JavaSpringAndHibernateProjectExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string JavaExecutablePath,
    string JavaLibrariesPath,
    int BaseUpdateTimeOffset,
    string MavenPath,
    string JavaSpringAndHibernateStrategyPomFilePath)
    : JavaProjectTestsExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed, JavaExecutablePath, JavaLibrariesPath,
        BaseUpdateTimeOffset);
