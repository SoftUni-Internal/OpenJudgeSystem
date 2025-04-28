namespace OJS.Workers.ExecutionStrategies;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Ionic.Zip;
using Microsoft.Extensions.Logging;
using OJS.Workers.Common;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Extensions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Workers.ExecutionStrategies.Python;
using OJS.Workers.Executors;
using static OJS.Workers.Common.Constants;

public class RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests<TSettings> : PythonExecuteAndCheckExecutionStrategy<TSettings>
    where TSettings : RunSpaAndExecuteMochaTestsExecutionStrategySettings
{
    private const string UserApplicationHttpPortPlaceholder = "#userApplicationHttpPort#";
    private const string MochaTestsPlaceholder = "#mochaTests#";
    private const string MochaTestsFullTitlePrefix = "E2E tests";
    private const string ContainerNamePlaceholder = "#containerNamePlaceholder#";
    private const string KillContainerPlaceholder = "#killContainerPlaceholder#";
    private const string TestFilePathPlaceholder = "#testFilePathPlaceholder#";
    private const string NodeModulesRequirePattern = "(require\\((?'quote'[\'\"]))([\\w\\-]*)(\\k<quote>)";
    private const string TestsDirectoryName = "test";
    private const string UserApplicationDirectoryName = "app";
    private const string NginxConfFileName = "nginx.conf";
    private readonly Regex testTimeoutRegex = new Regex(@"Timeout (?:of )?\d+ms exceeded\.");

    public RunSpaAndExecuteMochaTestsExecutionStrategySeparateTests(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, settingsProvider, logger)
    {
    }
    private static string NginxFileContent => $@"
worker_processes  1;

events {{
    worker_connections  1024;
}}

http {{
    include mime.types;
    types
    {{
        application/javascript mjs;
    }}

    default_type  application/octet-stream;
    sendfile        on;
    keepalive_timeout  65;

    server {{
      root /usr/share/nginx/html;

      listen 80;
      server_name localhost;

      location / {{
        try_files $uri $uri/ /index.html;
        autoindex on;
      }}
    }}
}}";

    private string TestsPath => FileHelpers.BuildPath(this.WorkingDirectory, TestsDirectoryName);

    private string UserApplicationPath => FileHelpers.BuildPath(this.WorkingDirectory, UserApplicationDirectoryName);

    private string NginxConfFileDirectory => FileHelpers.BuildPath(this.WorkingDirectory, "nginx");

    private string NginxConfFileFullPath => FileHelpers.BuildPath(this.NginxConfFileDirectory, NginxConfFileName);

    private string PythonPreExecuteCodeTemplate => $@"
import docker
import shutil
import tarfile
import uuid

from os import chdir, remove
from os.path import basename, join, dirname
from datetime import datetime, timezone

image_name = 'nginx'
path_to_project = '{this.UserApplicationPath}'
path_to_nginx_conf = '{this.NginxConfFileDirectory}/nginx.conf'
path_to_node_modules = '{this.Settings.JsProjNodeModulesPath}'
network_name = 'ojs_workers'

class DockerExecutor:
    def __init__(self):
        self.client = docker.from_env()
        self.container_name = f""nginx-container-{{uuid.uuid4().hex[:8]}}""
        self.__ensure_image_is_present()
        self.container = self.client.containers.create(
            image=image_name,
            name=self.container_name,
            labels=['js-apps'],
            volumes={{
                path_to_nginx_conf: {{
                    'bind': '/etc/nginx/nginx.conf',
                    'mode': 'ro',
                }},
                path_to_project: {{
                    'bind': '/usr/share/nginx/html',
                    'mode': 'rw',
                }},
            }},
            network=network_name
        )

    def start(self):
        self.container.start()
        self.copy_to_container(path_to_node_modules, '/usr/share/nginx/html/node_modules');

    def stop(self):
        self.container.stop()
        self.container.wait()
        self.container.remove()

    def get_container(self):
        return self.container

    def copy_to_container(self, source, destination):
        chdir(dirname(source))
        local_dest_name = join(dirname(source), basename(destination))
        if local_dest_name != source:
            shutil.copy2(source, local_dest_name)
        dst_name = basename(destination)
        tar_path = local_dest_name + '.tar'

        tar = tarfile.open(tar_path, mode='w')
        try:
            tar.add(dst_name)
        finally:
            tar.close()

        data = open(tar_path, 'rb').read()
        self.container.put_archive(dirname(destination), data)

        remove(tar_path)

    def __ensure_image_is_present(self):
        def is_latest_image_present(name):
            image_tag = name + ':latest'
            all_tags = [tag for img in self.client.images.list() for tag in img.tags]
            return any(tag for tag in all_tags if tag == image_tag)

        if not is_latest_image_present(image_name):
            self.client.images.pull(image_name)


executor = DockerExecutor()

try:
    # Cleanup old running js-apps containers
    datetime_now = datetime.now(timezone.utc)
    client = docker.from_env()
    js_apps_containers = client.containers.list(all=True, filters={{""label"": ""js-apps"", ""status"": ""running""}})

    for apps_container in js_apps_containers:
        container_info = client.api.inspect_container(apps_container.name)
        started_at_string = container_info['State']['StartedAt']

        processed_time_str = started_at_string[0:-4]
        start_at_date = datetime.strptime(processed_time_str, ""%Y-%m-%dT%H:%M:%S.%f"").replace(tzinfo=timezone.utc)
        time_diff = datetime_now - start_at_date

        # Remove container if older than 15 minutes
        if time_diff.total_seconds() > 900:
            apps_container.stop()
            apps_container.wait()
            apps_container.remove()

    executor.start()

    # Print container name for direct network communication
    container = executor.get_container()
    print(f'{{container.name}},{{client.api.inspect_container(container.name)[""NetworkSettings""][""Networks""][network_name][""IPAddress""]}}')

except Exception as e:
    print(e)
    executor.stop()
";

    private string PythonCodeTemplate => $@"
import docker
import subprocess

mocha_path = '{this.Settings.MochaModulePath}'
tests_path = '{TestFilePathPlaceholder}'
container_name = '{ContainerNamePlaceholder}'
kill_container = {KillContainerPlaceholder}

try:
    docker_client = docker.from_env()
    container = docker_client.containers.get(container_name)

    commands = [mocha_path, tests_path, '-R', 'json']

    process = subprocess.run(
        commands,
    )

    print(process.stdout)

except Exception as e:
    print(e)
finally:
    if kill_container:
        container.stop()
        container.wait()
        container.remove()
";

    private string NginxContainerName { get; set; } = string.Empty;

    private string NginxIpAddress { get; set; } = string.Empty;

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        if (string.IsNullOrEmpty(executionContext.Input.TaskSkeletonAsString))
        {
            throw new ConfigurationException("The submission type skeleton must not be empty.");
        }

        try
        {
            this.ExtractSubmissionFiles(executionContext);
        }
        catch (SolutionException exception)
        {
            result.IsCompiledSuccessfully = false;
            result.CompilerComment = exception.Message;

            return result;
        }

        this.SaveNginxFile();

        var preExecuteCodeSavePath = this.SavePythonCodeTemplateToTempFile(this.PythonPreExecuteCodeTemplate);
        var executor = this.CreateStandardExecutor();
        try
        {
            var checker = executionContext.Input.GetChecker();
            var preExecutionResult = await this.Execute(executionContext, executor, preExecuteCodeSavePath);
            preExecutionResult.ReceivedOutput = "testing";
            var output = preExecutionResult.ReceivedOutput.Trim().Split(',');

            if (output.Length == 2)
            {
                this.NginxContainerName = output[0];
                this.NginxIpAddress = output[1];
            }
            else
            {
                this.Logger.LogUnexpectedProcessOutput(preExecutionResult);
                throw new ArgumentException("Failed to run the strategy pre execute step. Please contact a developer.");
            }

            return await this.RunTests(string.Empty, executor, checker, executionContext, result);
        }
        catch (Exception)
        {
            var cleanupResult = await this.DeleteNginxContainer(executor, executionContext);

            if (cleanupResult is not null && !string.IsNullOrWhiteSpace(cleanupResult.ErrorOutput))
            {
                this.Logger.LogUnexpectedProcessOutput(cleanupResult);
            }

            throw;
        }
    }

    protected override async Task<IExecutionResult<TestResult>> RunTests(
        string codeSavePath,
        IExecutor executor,
        IChecker checker,
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        var filePath = this.BuildTestPath("tests");

        var testsConcatenated = string.Join(
            $"{Environment.NewLine}{Environment.NewLine}",
            executionContext
                .Input
                .Tests
                .Select(t => t.Input));

        var skeletonWithTests = executionContext
            .Input
            .TaskSkeletonAsString
            .Replace(MochaTestsPlaceholder, testsConcatenated);

        FileHelpers.SaveStringToFile(
            this.PreprocessTestInput(skeletonWithTests),
            filePath);

        // pass in container name in order to close container after execution
        // pass test file path to mocha so it executes only this test file, and not all test files each run
        var processedPythonCodeTemplate = this.PythonCodeTemplate
            .Replace(ContainerNamePlaceholder, this.NginxContainerName)
            .Replace(TestFilePathPlaceholder, filePath)
            .Replace(KillContainerPlaceholder, "True");

        var mainCodeSavePath = this.SavePythonCodeTemplateToTempFile(processedPythonCodeTemplate);

        var processExecutionResult = await this.Execute(executionContext, executor, mainCodeSavePath);

        var testResults = this.ExtractTestResultsFromReceivedOutput(
            processExecutionResult.ReceivedOutput,
            executionContext.Input.Tests);

        result.Results.AddRange(testResults);

        return result;
    }

    private static Dictionary<string, int> MapTitlesToTestId(IEnumerable<TestContext> tests, IEnumerable<string> titles)
    {
        try
        {
            return titles.ToDictionary(
                title => title,
                title => tests.FirstOrDefault(t => t
                        .Input
                        .Contains(title
                            .Replace(MochaTestsFullTitlePrefix, string.Empty)
                            .Trim()))?
                    .Id ?? 0);
        }
        catch (ArgumentException exception)
        {
            var keyStr = "Key:";
            var testName = exception.Message
                .Substring(exception.Message.IndexOf(keyStr, StringComparison.Ordinal) + keyStr.Length)
                .Trim();

            throw new ConfigurationException($"Duplicate naming of tests: {testName}");
        }
    }

    private static void ValidateAllowedFileExtension<TInput>(IExecutionContext<TInput> executionContext)
    {
        var trimmedAllowedFileExtensions = executionContext.AllowedFileExtensions?.Trim();
        var allowedFileExtensions = (!trimmedAllowedFileExtensions?.StartsWith(".") ?? false)
            ? $".{trimmedAllowedFileExtensions}"
            : trimmedAllowedFileExtensions;

        if (allowedFileExtensions != ZipFileExtension)
        {
            throw new SolutionException("The submission file is not a zip file!");
        }
    }

    private static string PreproccessReceivedExecutionOutput(string receivedOutput)
        => receivedOutput
            .Trim()
            .Replace("b'", string.Empty)
            .Replace("}'", "}")
            .Replace("}None", "}");

    private static IEnumerable<(string, string)> GetNodeModules(string testInputContent)
    {
        var requirePattern = new Regex(NodeModulesRequirePattern);
        var results = requirePattern.Matches(testInputContent);
        var nodeModules = new List<(string, string)>();

        foreach (Match match in results)
        {
            var fullRequireStatement = match.Groups[0].ToString();
            var nodeModuleName = match.Groups[2].ToString();
            nodeModules.Add((nodeModuleName, fullRequireStatement));
        }

        return nodeModules;
    }

    private void ExtractSubmissionFiles<TInput>(IExecutionContext<TInput> executionContext)
    {
        ValidateAllowedFileExtension(executionContext);

        var submissionFilePath = FileHelpers.BuildPath(this.WorkingDirectory, "temp");
        File.WriteAllBytes(submissionFilePath, executionContext.FileContent);
        using (var zip = ZipFile.Read(submissionFilePath))
        {
            zip.RemoveSelectedEntries("node_modules/*");
            zip.Save();
        }

        FileHelpers.RemoveFilesFromZip(submissionFilePath, RemoveMacFolderPattern);
        FileHelpers.UnzipFile(submissionFilePath, this.UserApplicationPath);

        if (executionContext.AdditionalFiles.Length != 0)
        {
            var additionalFilesPath = FileHelpers.BuildPath(this.WorkingDirectory, "additionalFiles");
            File.WriteAllBytes(additionalFilesPath, executionContext.AdditionalFiles);
            FileHelpers.UnzipFileAndOverwriteExistingFiles(additionalFilesPath, this.UserApplicationPath);
        }

        Directory.CreateDirectory(this.TestsPath);
        Directory.CreateDirectory(this.NginxConfFileDirectory);
    }

    private ICollection<TestResult> ExtractTestResultsFromReceivedOutput(string receivedOutput, IEnumerable<TestContext> tests)
    {
        var mochaResult = JsonExecutionResult.Parse(PreproccessReceivedExecutionOutput(receivedOutput));
        if (mochaResult.TotalTests == 0)
        {
            return tests
                .Select(t => new TestResult
                {
                    Id = t.Id,
                    IsTrialTest = t.IsTrialTest,
                    ResultType = TestRunResultType.WrongAnswer,
                    CheckerDetails = new CheckerDetails
                    {
                        UserOutputFragment = string.IsNullOrWhiteSpace(mochaResult.Error) ? receivedOutput : "An error occurred while processing the submission, please contact and administrator.",
                    },
                })
                .ToList();
        }

        var titlesToTestsMapping = MapTitlesToTestId(
            tests, mochaResult.TestTitles);

        return mochaResult.TestErrors
            .Select((test, index) => this.ParseTestResult(test, titlesToTestsMapping, index, mochaResult.TestTitles))
            .ToList();
    }

    private TestResult ParseTestResult(string? testResult, Dictionary<string, int> titlesToTestsMapping, int index, List<string> testTitles)
    {
        var isTimeout = false;
        if (testResult != null)
        {
            isTimeout = this.testTimeoutRegex.IsMatch(testResult);
        }

        return new TestResult
        {
            Id = titlesToTestsMapping[testTitles[index]],
            IsTrialTest = false,
            ResultType = testResult == null
                            ? TestRunResultType.CorrectAnswer
                            : isTimeout
                                ? TestRunResultType.TimeLimit
                                : TestRunResultType.WrongAnswer,
            CheckerDetails = new CheckerDetails
            {
                Comment = string.Empty,
                ExpectedOutputFragment = string.Empty,
                UserOutputFragment = isTimeout
                    ? $"{testTitles[index]}\n{testResult}"
                    : testResult,
            },
        };
    }

    private string SavePythonCodeTemplateToTempFile(string codeTemplate)
    {
        var pythonCodeTemplate = codeTemplate.Replace("\\", "\\\\");
        return FileHelpers.SaveStringToTempFile(this.WorkingDirectory, pythonCodeTemplate);
    }

    private void SaveNginxFile()
    {
        var nginxCorrectedContent = NginxFileContent.Replace("{{", "{").Replace("}}", "}");
        FileHelpers.SaveStringToFile(nginxCorrectedContent, this.NginxConfFileFullPath);
    }

    private string ReplaceNodeModulesRequireStatementsInTests(string testInputContent)
    {
        GetNodeModules(testInputContent)
            .ToList()
            .ForEach(nodeModule =>
            {
                var (name, requireStatement) = nodeModule;
                testInputContent = this.FixPathsForNodeModule(testInputContent, name, requireStatement);
            });

        return testInputContent;
    }

    private string FixPathsForNodeModule(string testInputContent, string name, string requireStatement)
    {
        var path = this.GetNodeModulePathByName(name);

        var fixedRequireStatement = requireStatement.Replace(name, path)
            .Replace("\\", "\\\\");

        return testInputContent.Replace(requireStatement, fixedRequireStatement);
    }

    private string? GetNodeModulePathByName(string name)
    {
        switch (name)
        {
            case "mocha":
                return this.Settings.MochaModulePath;
            case "chai":
                return this.Settings.ChaiModulePath;
            case "playwright-chromium":
                return this.Settings.PlaywrightChromiumModulePath;
            default:
                return null;
        }
    }

    private string PreprocessTestInput(string testInput)
    {
        testInput = this.ReplaceNodeModulesRequireStatementsInTests(testInput)
                .Replace(UserApplicationHttpPortPlaceholder, "");

        return testInput.Replace("localhost:", this.NginxIpAddress);
    }

    private string BuildTestPath(string fileName) => FileHelpers.BuildPath(this.TestsPath, $"{fileName}{JavaScriptFileExtension}");

    private async Task<ProcessExecutionResult?> DeleteNginxContainer(
        IExecutor executor,
        IExecutionContext<TestsInputModel> executionContext)
    {
        if (string.IsNullOrEmpty(this.NginxContainerName))
        {
            return null;
        }

        var cleanupScript = $@"
import docker

try:
    client = docker.from_env()
    container = client.containers.get('{this.NginxContainerName}')

    container.reload()  # Refresh container state info
    status = container.attrs['State']['Status']

    if status in ['created', 'running', 'paused', 'restarting', 'exited']:
        if status == 'running':
            container.stop()
            container.wait()
        elif status == 'created':
            print('The nginx container was created but not started.')

        container.remove()
        print(f'Cleanup succeeded!')
    else:
        print(f'The nginx container is in an unexpected state: {{status}}')

except docker.errors.NotFound:
    print(f'The cleanup was skipped, the nginx container {this.NginxContainerName} was not found.')
except Exception as e:
    print(f'The nginx container cleanup failed: {{e}}')
";

        var cleanupFilePath = this.SavePythonCodeTemplateToTempFile(cleanupScript);

        return await this.Execute(executionContext, executor, cleanupFilePath);
    }
}