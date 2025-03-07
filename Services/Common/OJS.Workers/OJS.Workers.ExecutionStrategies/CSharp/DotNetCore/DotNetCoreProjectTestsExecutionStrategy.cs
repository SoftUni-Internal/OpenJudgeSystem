namespace OJS.Workers.ExecutionStrategies.CSharp.DotNetCore
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.Compilers;
    using OJS.Workers.ExecutionStrategies.Extensions;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class DotNetCoreProjectTestsExecutionStrategy<TSettings> : CSharpProjectTestsExecutionStrategy<TSettings>
        where TSettings : DotNetCoreProjectTestsExecutionStrategySettings
    {
        protected new const string AdditionalExecutionArguments = "--noresult";
        protected const string CsProjFileExtension = ".csproj";

        private const string ProjectPathPlaceholder = "##projectPath##";
        private const string ProjectReferencesPlaceholder = "##ProjectReferences##";
        private const string NUnitLiteConsoleAppFolderName = "NUnitLiteConsoleApp";
        private const string UserSubmissionFolderName = "UserProject";
        private const string NUnitLiteConsoleAppProgramName = "Program";

        private const string NUnitLiteConsoleAppProgramTemplate = @"
            using System;
            using System.Reflection;
            using NUnit.Common;
            using NUnitLite;

            public class Program
            {
                public static void Main(string[] args)
                {
                    var writter = new ExtendedTextWrapper(Console.Out);
                    new AutoRun(typeof(Program).GetTypeInfo().Assembly).Execute(args, writter, Console.In);
                }
            }";

        private readonly string projectReferenceTemplate =
            $@"<ProjectReference Include=""{ProjectPathPlaceholder}"" />";

        public DotNetCoreProjectTestsExecutionStrategy(
            IOjsSubmission submission,
            IProcessExecutorFactory processExecutorFactory,
            ICompilerFactory compilerFactory,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
            : base(submission, processExecutorFactory, compilerFactory, settingsProvider, logger)
        {
        }

        protected string NUnitLiteConsoleAppDirectory =>
            Path.Combine(this.WorkingDirectory, NUnitLiteConsoleAppFolderName);

        protected string UserProjectDirectory =>
            Path.Combine(this.WorkingDirectory, UserSubmissionFolderName);

        private string NUnitLiteConsoleAppCsProjTemplate => $@"
            <Project Sdk=""Microsoft.NET.Sdk"">
                <PropertyGroup>
                    <OutputType>Exe</OutputType>
                    <TargetFramework>{this.Settings.TargetFrameworkName}</TargetFramework>
                </PropertyGroup>
                <ItemGroup>
                    <PackageReference Include=""NUnitLite"" Version=""3.13.2"" />
                    <PackageReference Include=""Microsoft.EntityFrameworkCore.InMemory"" Version=""{this.Settings.MicrosoftEntityFrameworkCoreInMemoryVersion}"" />
                    <PackageReference Include=""Microsoft.EntityFrameworkCore.Proxies"" Version=""{this.Settings.MicrosoftEntityFrameworkCoreProxiesVersion}"" />
                </ItemGroup>
                <ItemGroup>
                    {ProjectReferencesPlaceholder}
                </ItemGroup>
            </Project>";

        protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            executionContext.SanitizeContent();

            Directory.CreateDirectory(this.NUnitLiteConsoleAppDirectory);
            Directory.CreateDirectory(this.UserProjectDirectory);

            SaveZipSubmission(executionContext.FileContent, this.UserProjectDirectory);
            this.ExtractTestNames(executionContext.Input.Tests);

            this.SaveTestFiles(executionContext.Input.Tests, this.NUnitLiteConsoleAppDirectory);
            this.SaveSetupFixture(this.NUnitLiteConsoleAppDirectory);

            var userCsProjPaths = FileHelpers.FindAllFilesMatchingPattern(
                this.UserProjectDirectory, CsProjFileSearchPattern);

            var nUnitLiteConsoleApp = this.CreateNUnitLiteConsoleApp(userCsProjPaths);

            var compilerPath = this.CompilerFactory.GetCompilerPath(executionContext.CompilerType, this.Type);

            var compilerResult = this.Compile(
                executionContext.CompilerType,
                compilerPath,
                executionContext.AdditionalCompilerArguments,
                nUnitLiteConsoleApp.csProjPath);

            result.IsCompiledSuccessfully = compilerResult.IsCompiledSuccessfully;

            if (!result.IsCompiledSuccessfully)
            {
                result.CompilerComment = compilerResult.CompilerComment;
                return result;
            }

            // Delete tests before execution so the user can't access them
            FileHelpers.DeleteFiles([.. this.TestPaths]);

            var executor = this.CreateRestrictedExecutor();

            return await this.RunUnitTests(
                compilerPath,
                executionContext,
                executor,
                executionContext.Input.GetChecker(),
                result,
                compilerResult.OutputFile,
                AdditionalExecutionArguments);
        }

        protected (string csProjTemplate, string csProjPath) CreateNUnitLiteConsoleApp(
            IEnumerable<string> projectsToTestCsProjPaths)
        {
            var consoleAppEntryPointPath = FileHelpers.BuildPath(
                this.NUnitLiteConsoleAppDirectory,
                $"{NUnitLiteConsoleAppProgramName}{Constants.cSharpFileExtension}");

            File.WriteAllText(consoleAppEntryPointPath, NUnitLiteConsoleAppProgramTemplate);

            var references = projectsToTestCsProjPaths
                .Select(path => this.projectReferenceTemplate.Replace(ProjectPathPlaceholder, path));

            var csProjTemplate = this.NUnitLiteConsoleAppCsProjTemplate
                .Replace(ProjectReferencesPlaceholder, string.Join(Environment.NewLine, references));

            var csProjPath = this.CreateNUnitLiteConsoleAppCsProjFile(csProjTemplate);

            return (csProjTemplate, csProjPath);
        }

        protected string CreateNUnitLiteConsoleAppCsProjFile(string csProjTemplate)
        {
            var consoleAppCsProjPath = FileHelpers.BuildPath(
                this.NUnitLiteConsoleAppDirectory,
                $"{NUnitLiteConsoleAppFolderName}{CsProjFileExtension}");

            File.WriteAllText(consoleAppCsProjPath, csProjTemplate);

            return consoleAppCsProjPath;
        }
    }

    public record DotNetCoreProjectTestsExecutionStrategySettings(
        int BaseTimeUsed,
        int BaseMemoryUsed,
        string TargetFrameworkName,
        string MicrosoftEntityFrameworkCoreInMemoryVersion,
        string MicrosoftEntityFrameworkCoreProxiesVersion)
        : CSharpProjectTestsExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
}