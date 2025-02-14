namespace OJS.Workers.ExecutionStrategies.NodeJs
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Extensions;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.ExecutionStrategies.Helpers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    using static OJS.Workers.ExecutionStrategies.NodeJs.NodeJsConstants;

    public class NodeJsPreprocessExecuteAndCheckExecutionStrategy<TSettings> : BaseInterpretedCodeExecutionStrategy<TSettings>
        where TSettings : NodeJsPreprocessExecuteAndCheckExecutionStrategySettings
    {
        protected const string RequiredModules = "#requiredModule#";
        protected const string PreevaluationPlaceholder = "#preevaluationCode#";
        protected const string PostevaluationPlaceholder = "#postevaluationCode#";
        protected const string EvaluationPlaceholder = "#evaluationCode#";

        private const string DefaultAdapterFunctionCode = "(input, code) => code(input);";

        public NodeJsPreprocessExecuteAndCheckExecutionStrategy(
            IOjsSubmission submission,
            IProcessExecutorFactory processExecutorFactory,
            IExecutionStrategySettingsProvider settingsProvider,
            ILogger<BaseExecutionStrategy<TSettings>> logger)
            : base(submission, processExecutorFactory, settingsProvider, logger)
        {
            if (!File.Exists(this.Settings.NodeJsExecutablePath))
            {
                throw new ArgumentException(
                    $"NodeJS not found in: {this.Settings.NodeJsExecutablePath}",
                    nameof(this.Settings.NodeJsExecutablePath));
            }

            if (!Directory.Exists(this.Settings.UnderscoreModulePath))
            {
                throw new ArgumentException(
                    $"Underscore not found in: {this.Settings.UnderscoreModulePath}",
                    nameof(this.Settings.UnderscoreModulePath));
            }

            if (this.Settings.BaseTimeUsed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Settings.BaseTimeUsed));
            }

            if (this.Settings.BaseMemoryUsed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(this.Settings.BaseMemoryUsed));
            }
        }

        protected virtual string JsCodeRequiredModules => $@"
var EOL = require('os').EOL,
_ = require('{this.Settings.UnderscoreModulePath}')";

        protected virtual string JsNodeDisableCode => @"
// DataView = undefined;
DTRACE_NET_SERVER_CONNECTION = undefined;
// DTRACE_NET_STREAM_END = undefined;
DTRACE_NET_SOCKET_READ = undefined;
DTRACE_NET_SOCKET_WRITE = undefined;
DTRACE_HTTP_SERVER_REQUEST = undefined;
DTRACE_HTTP_SERVER_RESPONSE = undefined;
DTRACE_HTTP_CLIENT_REQUEST = undefined;
DTRACE_HTTP_CLIENT_RESPONSE = undefined;
COUNTER_NET_SERVER_CONNECTION = undefined;
COUNTER_NET_SERVER_CONNECTION_CLOSE = undefined;
COUNTER_HTTP_SERVER_REQUEST = undefined;
COUNTER_HTTP_SERVER_RESPONSE = undefined;
COUNTER_HTTP_CLIENT_REQUEST = undefined;
COUNTER_HTTP_CLIENT_RESPONSE = undefined;
process.argv = undefined;
process.versions = undefined;
process.env = { NODE_DEBUG: false };
process.addListener = undefined;
process.EventEmitter = undefined;
process.mainModule = undefined;
process.config = undefined;
//process.removeListener = undefined;
// process.on = undefined;
process.openStdin = undefined;
process.chdir = undefined;
process.cwd = undefined;
process.exit = undefined;
process.umask = undefined;
// GLOBAL = undefined;
// root = undefined;
// global = {};
setInterval = undefined;
//setTimeout = undefined;
//clearTimeout = undefined;
clearInterval = undefined;
// setImmediate = undefined;
clearImmediate = undefined;
module = undefined;
require = undefined;
msg = undefined;

// delete DataView;
delete DTRACE_NET_SERVER_CONNECTION;
// delete DTRACE_NET_STREAM_END;
delete DTRACE_NET_SOCKET_READ;
delete DTRACE_NET_SOCKET_WRITE;
delete DTRACE_HTTP_SERVER_REQUEST;
delete DTRACE_HTTP_SERVER_RESPONSE;
delete DTRACE_HTTP_CLIENT_REQUEST;
delete DTRACE_HTTP_CLIENT_RESPONSE;
delete COUNTER_NET_SERVER_CONNECTION;
delete COUNTER_NET_SERVER_CONNECTION_CLOSE;
delete COUNTER_HTTP_SERVER_REQUEST;
delete COUNTER_HTTP_SERVER_RESPONSE;
delete COUNTER_HTTP_CLIENT_REQUEST;
delete COUNTER_HTTP_CLIENT_RESPONSE;
delete process.argv;
delete process.exit;
delete process.versions;
// delete GLOBAL;
// delete root;
delete setInterval;
//delete setTimeout;
//delete clearTimeout;
delete clearInterval;
// delete setImmediate;
delete clearImmediate;
delete module;
delete require;
delete msg;

process.exit = function () {};";

        protected virtual string JsCodeEvaluation => @"
process.stdin.resume();
process.stdin.on('data', function(buf) { content += buf.toString(); });
process.stdin.on('end', function() {
    content = content.replace(new RegExp(EOL + '$'), '');
    let inputData = content.split(EOL);
    let result = adapterFunction(inputData, code.run);
    if (result !== undefined) {
        console.log(result);
    }
});";

        protected virtual string JsCodePostevaulationCode => string.Empty;

        protected virtual string JsCodeTemplate =>
            RequiredModules + @";" +
            NodeDisablePlaceholder +
            PreevaluationPlaceholder +
            EvaluationPlaceholder +
            PostevaluationPlaceholder;

        protected override async Task<IExecutionResult<TestResult>> ExecuteAgainstTestsInput(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutionResult<TestResult> result)
        {
            var codeSavePath = this.SaveCodeToTempFile(executionContext);

            var executor = this.CreateRestrictedExecutor();

            var checker = executionContext.Input.GetChecker();

            var testResults = await this.ProcessTests(executionContext, executor, checker, codeSavePath);

            result.Results.AddRange(testResults);

            return result;
        }

        protected override async Task<IExecutionResult<OutputResult>> ExecuteAgainstSimpleInput(
            IExecutionContext<SimpleInputModel> executionContext,
            IExecutionResult<OutputResult> result)
        {
            var codeSavePath = this.SaveCodeToTempFile(executionContext);

            var executor = this.CreateRestrictedExecutor();

            var processExecutionResult = await this.ExecuteCode(
                executionContext,
                executor,
                codeSavePath,
                executionContext.Input.Input);

            result.Results.Add(GetOutputResult(processExecutionResult));

            return result;
        }

        protected virtual async Task<List<TestResult>> ProcessTests(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutor executor,
            IChecker checker,
            string codeSavePath)
        {
            var testResults = new List<TestResult>();

            foreach (var test in executionContext.Input.Tests)
            {
                var testInput = PrepareTestInput(test.Input);
                var processExecutionResult = await this.ExecuteCode(
                    executionContext,
                    executor,
                    codeSavePath,
                    testInput);

                var testResult = CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    processExecutionResult.ReceivedOutput);

                testResults.Add(testResult);
            }

            return testResults;
        }

        protected virtual string PreprocessJsSubmission<TInput>(string template, IExecutionContext<TInput> context)
        {
            var problemSkeleton = !string.IsNullOrEmpty((context.Input as SimpleInputModel)?.TaskSkeletonAsString)
                ? (context.Input as SimpleInputModel)?.TaskSkeletonAsString
                : !string.IsNullOrEmpty((context.Input as TestsInputModel)?.TaskSkeletonAsString)
                    ? (context.Input as TestsInputModel)?.TaskSkeletonAsString
                    : DefaultAdapterFunctionCode;

            var code = context.Code.Trim(';');

            var processedCode = template
                .Replace(RequiredModules, this.JsCodeRequiredModules)
                .Replace(PreevaluationPlaceholder, JsCodePreEvaluationCodeProvider.GetPreEvaluationCode(this.Type))
                .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                .Replace(UserInputPlaceholder, code)
                .Replace(AdapterFunctionPlaceholder, problemSkeleton);

            return processedCode;
        }

        protected override string SaveCodeToTempFile<TInput>(IExecutionContext<TInput> executionContext)
        {
            // Preprocess the user submission
            var codeToExecute = this.PreprocessJsSubmission(
                this.JsCodeTemplate,
                executionContext);

            // Save the preprocessed submission which is ready for execution
            return FileHelpers.SaveStringToTempFile(this.WorkingDirectory, codeToExecute);
        }

        private Task<ProcessExecutionResult> ExecuteCode<TInput>(
            IExecutionContext<TInput> executionContext,
            IExecutor executor,
            string codeSavePath,
            string? inputData = null)
            => executor.Execute(
                this.Settings.NodeJsExecutablePath,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                inputData,
                new[] { LatestEcmaScriptFeaturesEnabledFlag, codeSavePath });
    }

    public record NodeJsPreprocessExecuteAndCheckExecutionStrategySettings(
        int BaseTimeUsed,
        int BaseMemoryUsed,
        string NodeJsExecutablePath,
        string UnderscoreModulePath)
        : BaseInterpretedCodeExecutionStrategySettings(BaseTimeUsed, BaseMemoryUsed);
}
