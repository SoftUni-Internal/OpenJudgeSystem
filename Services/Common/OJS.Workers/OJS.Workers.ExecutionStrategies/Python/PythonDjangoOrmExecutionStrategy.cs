#nullable disable
namespace OJS.Workers.ExecutionStrategies.Python;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OJS.Workers.Common;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.Executors;

public class PythonDjangoOrmExecutionStrategy<TSettings> : PythonProjectTestsExecutionStrategy<TSettings>
    where TSettings : PythonDjangoOrmExecutionStrategySettings
{
    private const string ProjectSettingsFolder = "orm_skeleton";
    private const string SettingsFileName = "settings.py";
    private const string PyenvAppFileName = "pyenv";
    private const string RequirementsFileName = "requirements.txt";
    private const int MaximumTimeForEnvDeletion = 10000;

    private const string InvalidProjectStructureErrorMessage =
        "Folder project structure is invalid! Please check your zip file! It should contain requirements.txt in root of the zip and {0}/settings.py";

    private const string DatabaseConfigRegexPattern = @"(?:^|^\n\s*)DATABASES\s*=\s*\{[\s\S]*?\}\s*(?=\n{1,2}#|\n{2,}|\Z)(?!\s*\Z)";
    private const string TestResultsRegexPattern = @"(FAIL|OK)";
    private const string SuccessTestsRegexPattern = @"^\s*OK\s*$";

    private const string SqlLiteConfig =
        "DATABASES = {\n    'default': {\n        'ENGINE': 'django.db.backends.sqlite3',\n        'NAME': 'db.sqlite3',\n    }\n}\n";

    public PythonDjangoOrmExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, settingsProvider, logger)
    {
    }

    protected override Regex TestsRegex => new Regex(TestResultsRegexPattern, RegexOptions.Multiline);

    protected override Regex SuccessTestsRegex => new Regex(SuccessTestsRegexPattern, RegexOptions.Multiline);

    protected override IEnumerable<string> ExecutionArguments
        => [];

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        var virtualEnvironmentName = Guid.NewGuid().ToString();
        SaveZipSubmission(executionContext.FileContent, this.WorkingDirectory);
        var requirementsFilePath = this.WorkingDirectory + Path.DirectorySeparatorChar + RequirementsFileName;
        var pathToSettingsFile = this.WorkingDirectory + Path.DirectorySeparatorChar + ProjectSettingsFolder +
                                 Path.DirectorySeparatorChar + SettingsFileName;

        if (!File.Exists(requirementsFilePath) || !File.Exists(pathToSettingsFile))
        {
            throw new SolutionException(string.Format(InvalidProjectStructureErrorMessage, ProjectSettingsFolder));
        }

        var executor = this.CreateExecutor();
        var checker = executionContext.Input.GetChecker();

        try
        {
            await this.CreateVirtualEnvironment(executor, executionContext, virtualEnvironmentName);
            await this.ActivateVirtualEnvironment(executor, executionContext, virtualEnvironmentName);
            ChangeDbConnection(pathToSettingsFile);
            await this.ExportDjangoSettingsModule(executor, executionContext, virtualEnvironmentName);
            await this.ApplyMigrations(executor, executionContext);

            await this.RunTests(string.Empty, executor, checker, executionContext, result);
        }
        finally
        {
            this.DeleteVirtualEnvironment(executor, executionContext, virtualEnvironmentName);
        }

        return result;
    }

    protected override async Task<IExecutionResult<TestResult>> RunTests(
        string codeSavePath,
        IExecutor executor,
        IChecker checker,
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        var tests = executionContext.Input.Tests.ToList();

        this.SaveTests(tests);

        for (var i = 0; i < tests.Count; i++)
        {
            var test = tests[i];
            var testPath = this.TestPaths[i];

            var processExecutionResult = await this.ExecuteTest(executor, executionContext, testPath);

            var testResult = this.GetTestResult(processExecutionResult, test, checker);

            result.Results.Add(testResult);
        }

        return result;
    }

    private static void ChangeDbConnection(string pathToSettingsFile, string pattern = DatabaseConfigRegexPattern, string replacement = SqlLiteConfig)
    {
        var settingsContent = File.ReadAllText(pathToSettingsFile);

        var newSettingsContent = Regex.Replace(
            settingsContent,
            pattern,
            replacement,
            RegexOptions.Multiline);

        FileHelpers.WriteAllText(pathToSettingsFile, newSettingsContent);
    }

    private static string GetErrorOutput(ProcessExecutionResult result)
        => $"Error output: {result.ReceivedOutput + Environment.NewLine + result.ErrorOutput} and result type: {result.Type}";

    private async Task<ProcessExecutionResult> ExecuteTest(
        IExecutor executor,
        IExecutionContext<TestsInputModel> executionContext,
        string testPath)
    {
        var processExecutionResult = await this.Execute(
            this.Settings.PythonExecutablePath,
            this.ExecutionArguments.Concat([
                $"manage.py test --pattern=\"{testPath.Split(Path.DirectorySeparatorChar).Last()}\""
            ]),
            executor,
            executionContext);

        this.FixReceivedOutput(processExecutionResult);
        return processExecutionResult;
    }

    private async Task CreateVirtualEnvironment(IExecutor executor, IExecutionContext<TestsInputModel> executionContext, string envName)
    {
        var result = await this.Execute(
            PyenvAppFileName,
            this.ExecutionArguments.Concat([$"virtualenv 3.11 {envName}"]),
            executor,
            executionContext);

        if (result.ExitCode == 0)
        {
            return;
        }

        throw new ArgumentException($"Failed to create virtual environment! {GetErrorOutput(result)}");
    }

    private async Task ActivateVirtualEnvironment(IExecutor executor, IExecutionContext<TestsInputModel> executionContext, string envName)
    {
        var result = await this.Execute(
            PyenvAppFileName,
            this.ExecutionArguments.Concat([$"local {envName}"]),
            executor,
            executionContext);

        if (result.ExitCode == 0)
        {
            return;
        }

        throw new ArgumentException("Failed to activate virtual environment! " + GetErrorOutput(result));
    }

    private void DeleteVirtualEnvironment(IExecutor executor, IExecutionContext<TestsInputModel> executionContext, string envName)
        => this.Execute(
            PyenvAppFileName,
            this.ExecutionArguments.Concat([$"virtualenv-delete {envName}"]),
            executor,
            executionContext,
            MaximumTimeForEnvDeletion,
            "y");

    private async Task ExportDjangoSettingsModule(IExecutor executor, IExecutionContext<TestsInputModel> executionContext, string envName)
    {
        var result = await this.Execute(
            "/bin/bash",
            this.ExecutionArguments.Concat([$"-c export DJANGO_SETTINGS_MODULE={envName}.settings"]),
            executor,
            executionContext);

        if (result.ExitCode == 0)
        {
            return;
        }

        throw new ArgumentException("Failed to export DJANGO_SETTINGS_MODULE! " + GetErrorOutput(result));
    }

    private async Task ApplyMigrations(IExecutor executor, IExecutionContext<TestsInputModel> executionContext)
    {
        var result = await this.Execute(
            this.Settings.PythonExecutablePath,
            this.ExecutionArguments.Concat(["manage.py migrate"]),
            executor,
            executionContext);

        if (result.ExitCode == 0)
        {
            return;
        }

        throw new ArgumentException("Failed to apply migrations! " + GetErrorOutput(result));
    }

    private Task<ProcessExecutionResult> Execute(
        string fileName,
        IEnumerable<string> arguments,
        IExecutor executor,
        IExecutionContext<TestsInputModel> executionContext,
        int timeLimit = 0,
        string inputData = null)
        => executor.Execute(
            fileName,
            timeLimit == 0 ? executionContext.TimeLimit : timeLimit,
            executionContext.MemoryLimit,
            inputData,
            arguments,
            this.WorkingDirectory,
            useProcessTime: false,
            useSystemEncoding: true);
}

public record PythonDjangoOrmExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string PythonExecutablePath,
    string PipExecutablePath,
    int InstallPackagesTimeUsed)
    : PythonProjectTestsExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed, PythonExecutablePath);