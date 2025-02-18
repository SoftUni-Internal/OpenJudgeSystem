#nullable disable
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

public class RunSpaAndExecuteMochaTestsExecutionStrategy<TSettings> : PythonExecuteAndCheckExecutionStrategy<TSettings>
    where TSettings : RunSpaAndExecuteMochaTestsExecutionStrategySettings
{
    private const string UserApplicationHttpPortPlaceholder = "#userApplicationHttpPort#";
    private const string ContainerNamePlaceholder = "#containerNamePlaceholder#";
    private const string KillContainerPlaceholder = "#killContainerPlaceholder#";
    private const string TestFilePathPlaceholder = "#testFilePathPlaceholder#";
    private const string NodeModulesRequirePattern = "(require\\((?'quote'[\'\"]))([\\w\\-]*)(\\k<quote>)";
    private const string TestsDirectoryName = "test";
    private const string UserApplicationDirectoryName = "app";
    private const string NginxConfFileName = "nginx.conf";
    private readonly Regex testTimeoutRegex = new Regex(@"Timeout (?:of )?\d+ms exceeded\.");

    public RunSpaAndExecuteMochaTestsExecutionStrategy(
        IOjsSubmission submission,
        IProcessExecutorFactory processExecutorFactory,
        IExecutionStrategySettingsProvider settingsProvider,
        ILogger<BaseExecutionStrategy<TSettings>> logger)
        : base(submission, processExecutorFactory, settingsProvider, logger)
    {
    }

    private static string NginxFileContent => @"
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

    private int PortNumber { get; set; }

    private string TestsPath => FileHelpers.BuildPath(this.WorkingDirectory, TestsDirectoryName);

    private string UserApplicationPath => FileHelpers.BuildPath(this.WorkingDirectory, UserApplicationDirectoryName);

    private string NginxConfFileDirectory => FileHelpers.BuildPath(this.WorkingDirectory, "nginx");

    private string NginxConfFileFullPath => FileHelpers.BuildPath(this.NginxConfFileDirectory, NginxConfFileName);

    private string PythonPreExecuteCodeTemplate => $@"
import docker
import shutil
import tarfile

from os import chdir, remove
from os.path import basename, join, dirname
from datetime import datetime, timezone

image_name = 'nginx'
path_to_project = '{this.UserApplicationPath}'
path_to_nginx_conf = '{this.NginxConfFileDirectory}/nginx.conf'
path_to_node_modules = '{this.Settings.JsProjNodeModulesPath}'


class DockerExecutor:
    def __init__(self):
        self.client = docker.from_env()
        self.__ensure_image_is_present()
        self.container = self.client.containers.create(
            image=image_name,
            ports={{'80/tcp': '0'}},
            labels = ['js-apps'],
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

    def get_container_by_name(self, name):
        return self.client.containers.get(name)

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
        # remove(local_dest_name)

    def __ensure_image_is_present(self):
        def is_latest_image_present(name):
            image_tag = name + ':latest'
            all_tags = [tag for img in self.client.images.list() for tag in img.tags]
            return any(tag for tag in all_tags if tag == image_tag)

        if not is_latest_image_present(image_name):
            self.client.images.pull(image_name)


executor = DockerExecutor()

try:
    # code for cleaning up old js-apps containers
    datetime_now = datetime.now(timezone.utc)
    client = docker.from_env()
    js_apps_containers = client.containers.list(all=True, filters={{""label"":""js-apps"", ""status"": ""running""}})

    for apps_container in js_apps_containers:
        container_info = client.api.inspect_container(apps_container.name)
        started_at_string = container_info['State']['StartedAt']

        # Python 3.6 does not support a ton of datetime stuff, also docker provides 9 symbols for ticks
        # while python expects 6
        processed_time_str = started_at_string[0:-4]
        start_at_date = datetime.strptime(processed_time_str, ""%Y-%m-%dT%H:%M:%S.%f"").replace(tzinfo=timezone.utc)
        time_diff = datetime_now - start_at_date

        # check if container is older than 1 hour (1 hour was arbitrarily chosen)
        if time_diff.total_seconds() > 3600:
            apps_container.stop()
            apps_container.wait()
            apps_container.remove()


    executor.start()

    #get created container config so we can get the container name (note this config does not get ports automatically populated)
    container = executor.get_container()
    name = container.name

    # need to get container by name from docker again, so we can get info about the dynamically assigned port
    current_container = executor.get_container_by_name(name)
    if current_container.ports and list(current_container.ports):
        first_element = list(current_container.ports)[0]
        if current_container.ports[first_element]:
            # get container host port
            host_port = current_container.ports[first_element][0]['HostPort']
            print(f'Container port: {{host_port}};Container name: {{name}};')
        else:
            print(""No ports assigned to the container."")
            print(""Container logs:"", current_container.logs().decode('utf-8'))
    else:
        print(""No port information available."")
        print(""Container logs:"", current_container.logs().decode('utf-8'))
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
    if kill_container == True:
        container.stop()
        container.wait()
        container.remove()
";

    private string ContainerName { get; set; }

    protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
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
        var executor = this.CreateExecutor();
        var checker = executionContext.Input.GetChecker();
        var preExecutionResult = await this.Execute(executionContext, executor, preExecuteCodeSavePath);
        var match = Regex.Match(preExecutionResult.ReceivedOutput, @"Container port: (\d+);Container name: ([a-zA-Z-_]+);");
        if (match.Success)
        {
            this.PortNumber = int.Parse(match.Groups[1].Value);
            this.ContainerName = match.Groups[2].Value;
        }
        else
        {
            throw new ArgumentException("Failed to run the strategy pre execute step. Please contact a developer.");
        }

        return await this.RunTests(string.Empty, executor, checker, executionContext, result);
    }

    protected override async Task<IExecutionResult<TestResult>> RunTests(
        string codeSavePath,
        IExecutor executor,
        IChecker checker,
        IExecutionContext<TestsInputModel> executionContext,
        IExecutionResult<TestResult> result)
    {
        foreach (var test in executionContext.Input.Tests)
        {
            var testResult = await this.RunIndividualTest(
                codeSavePath,
                executor,
                executionContext,
                test,
                test.Id == executionContext.Input.Tests.Last().Id);

            result.Results.AddRange(testResult);
        }

        return result;
    }

    private static void ValidateAllowedFileExtension<TInput>(IExecutionContext<TInput> executionContext)
    {
        var trimmedAllowedFileExtensions = executionContext.AllowedFileExtensions?.Trim();
        var allowedFileExtensions = (!trimmedAllowedFileExtensions?.StartsWith('.') ?? false)
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

    private async Task<ICollection<TestResult>> RunIndividualTest(
        string codeSavePath,
        IExecutor executor,
        IExecutionContext<TestsInputModel> executionContext,
        TestContext test,
        bool shouldKillContainer)
    {
        var filePath = this.BuildTestPath(test.Id);

        // pass in container name in order to close container after execution
        // pass test file path to mocha so it executes only this test file, and not all test files each run
        var processedPythonCodeTemplate = this.PythonCodeTemplate
            .Replace(ContainerNamePlaceholder, this.ContainerName)
            .Replace(TestFilePathPlaceholder, filePath)
            .Replace(KillContainerPlaceholder, shouldKillContainer ? "True" : "False");

        var mainCodeSavePath = this.SavePythonCodeTemplateToTempFile(processedPythonCodeTemplate);

        this.SaveTestsToFiles(executionContext.Input.Tests);

        var processExecutionResult = await this.Execute(executionContext, executor, mainCodeSavePath, test.Input);
        return this.ExtractTestResultsFromReceivedOutput(processExecutionResult.ReceivedOutput, test.Id);
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

    private ICollection<TestResult> ExtractTestResultsFromReceivedOutput(string receivedOutput, int parentTestId)
    {
        JsonExecutionResult mochaResult = JsonExecutionResult.Parse(PreproccessReceivedExecutionOutput(receivedOutput));
        if (mochaResult.TotalTests == 0)
        {
            return new List<TestResult>
            {
                new TestResult
                {
                    Id = parentTestId,
                    IsTrialTest = false,
                    ResultType = TestRunResultType.WrongAnswer,
                    CheckerDetails = new CheckerDetails
                    {
                        UserOutputFragment = receivedOutput,
                    },
                },
            };
        }

        return mochaResult.TestErrors
            .Select((test, index) => this.ParseTestResult(test, parentTestId, index, mochaResult.TestTitles))
            .ToList();
    }

    private TestResult ParseTestResult(string testResult, int parentTestId, int index, List<string> testTitles)
    {
        var isTimeout = false;
        if (testResult != null)
        {
            isTimeout = this.testTimeoutRegex.IsMatch(testResult);
        }

        return new TestResult
        {
            Id = parentTestId,
            IsTrialTest = false,
            ResultType = testResult == null
                            ? TestRunResultType.CorrectAnswer
                            : isTimeout
                                ? TestRunResultType.TimeLimit
                                : TestRunResultType.WrongAnswer,
            CheckerDetails = testResult == null
                            ? new CheckerDetails()
                            : new CheckerDetails { UserOutputFragment = isTimeout ? $"{testTitles[index]}\n{testResult}" : testResult },
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

    private void SaveTestsToFiles(IEnumerable<TestContext> tests)
    {
        foreach (var test in tests)
        {
            var testInputContent = this.PreprocessTestInput(test.Input);
            var filePath = this.BuildTestPath(test.Id);

            FileHelpers.SaveStringToFile(
                testInputContent,
                filePath);
        }
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

    private string GetNodeModulePathByName(string name)
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
            .Replace(UserApplicationHttpPortPlaceholder, this.PortNumber.ToString());

        return OsPlatformHelpers.IsDocker()
            ? testInput.Replace("localhost", "host.docker.internal")
            : testInput;
    }

    private string BuildTestPath(int testId) => FileHelpers.BuildPath(this.TestsPath, $"{testId}{JavaScriptFileExtension}");
}

public record RunSpaAndExecuteMochaTestsExecutionStrategySettings(
    int BaseTimeUsed,
    int BaseMemoryUsed,
    string PythonExecutablePath,
    string JsProjNodeModulesPath,
    string MochaModulePath,
    string ChaiModulePath,
    string PlaywrightChromiumModulePath)
    : PythonExecuteAndCheckExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed, PythonExecutablePath);