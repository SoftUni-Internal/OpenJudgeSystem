﻿#nullable disable
namespace OJS.Workers.ExecutionStrategies.NodeJs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Helpers;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Workers.Executors;

    public class NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy
        : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy
    {
        public NodeJsPreprocessExecuteAndRunJsDomUnitTestsExecutionStrategy(
            IProcessExecutorFactory processExecutorFactory,
            StrategySettings settings) // TODO: make this modular by getting requires from test
            : base(processExecutorFactory, settings)
        {
            if (!Directory.Exists(settings.JsDomModulePath))
            {
                throw new ArgumentException(
                    $"jsDom not found in: {settings.JsDomModulePath}",
                    nameof(settings.JsDomModulePath));
            }

            if (!Directory.Exists(settings.JQueryModulePath))
            {
                throw new ArgumentException(
                    $"jQuery not found in: {settings.JQueryModulePath}",
                    nameof(settings.JQueryModulePath));
            }

            if (!Directory.Exists(settings.HandlebarsModulePath))
            {
                throw new ArgumentException(
                    $"Handlebars not found in: {settings.HandlebarsModulePath}",
                    nameof(settings.HandlebarsModulePath));
            }

            settings.JsDomModulePath = FileHelpers.ProcessModulePath(settings.JsDomModulePath);
            settings.JQueryModulePath = FileHelpers.ProcessModulePath(settings.JQueryModulePath);
            settings.HandlebarsModulePath = FileHelpers.ProcessModulePath(settings.HandlebarsModulePath);

            this.Settings = settings;
        }

        protected override StrategySettings Settings { get; }

        protected override string JsCodeRequiredModules => base.JsCodeRequiredModules + @",
    jsdom = require('" + this.Settings.JsDomModulePath + @"'),
    jq = require('" + this.Settings.JQueryModulePath + @"'),
    sinon = require('" + this.Settings.SinonModulePath + @"'),
    sinonChai = require('" + this.Settings.SinonChaiModulePath + @"'),
    handlebars = require('" + this.Settings.HandlebarsModulePath + @"')";

        protected override string JsCodePreevaulationCode => @"
chai.use(sinonChai);
describe('TestDOMScope', function() {
    let bgCoderConsole = {};
    before(function(done) {
        jsdom.env({
            html: '',
            done: function(errors, window) {
                // define innerText manually to work as textContent, as it is not supported in jsdom but used in judge
                Object.defineProperty(window.Element.prototype, 'innerText', {
                    get() { return this.textContent },
                    set(value) { this.textContent = value }
                });
                global.window = window;
                global.document = window.document;
                global.$ = jq(window);
                global.handlebars = handlebars;
                Object.getOwnPropertyNames(window)
                    .filter(function (prop) {
                        return prop.toLowerCase().indexOf('html') >= 0;
                    }).forEach(function (prop) {
                        global[prop] = window[prop];
                    });
                Object.keys(console)
                    .forEach(function (prop) {
                        bgCoderConsole[prop] = console[prop];
                        console[prop] = new Function('');
                    });
                done();
            }
        });
    });
    after(function() {
        Object.keys(bgCoderConsole)
            .forEach(function (prop) {
                console[prop] = bgCoderConsole[prop];
            });
    });";

        protected override string JsCodeEvaluation => TestsPlaceholder;

        protected override string TestFuncVariables => base.TestFuncVariables + ", '_'";

        protected virtual string BuildTests(IEnumerable<TestContext> tests)
        {
            var testsCode = string.Empty;
            var testsCount = 1;
            foreach (var test in tests)
            {
                var code = Regex.Replace(test.Input, "([\\\\`])", "\\$1");

                testsCode +=
                    $@"
it('Test{testsCount++}', function(done) {{
    let content = `{code}`;
    let inputData = content.trim();
    let code = {{
        run: {UserInputPlaceholder}
    }};
    let testFunc = new Function('result', {this.TestFuncVariables}, inputData);
    testFunc.call({{}}, code.run, {this.TestFuncVariables.Replace("'", string.Empty)});
    done();
}});";
            }

            return testsCode;
        }

        protected override async Task<List<TestResult>> ProcessTests(
            IExecutionContext<TestsInputModel> executionContext,
            IExecutor executor,
            IChecker checker,
            string codeSavePath)
        {
            var testResults = new List<TestResult>();
            var arguments = new List<string>();
            arguments.Add(this.Settings.MochaModulePath);
            arguments.Add(codeSavePath);
            arguments.AddRange(this.AdditionalExecutionArguments);

            var processExecutionResult = await executor.Execute(
                this.Settings.NodeJsExecutablePath,
                string.Empty,
                executionContext.TimeLimit,
                executionContext.MemoryLimit,
                arguments);

            var mochaResult = JsonExecutionResult.Parse(processExecutionResult.ReceivedOutput);
            var currentTest = 0;
            foreach (var test in executionContext.Input.Tests)
            {
                var message = "yes";
                if (!string.IsNullOrEmpty(mochaResult.Error))
                {
                    message = mochaResult.Error;
                }
                else if (mochaResult.TestErrors[currentTest] != null)
                {
                    message = $"Unexpected error: {mochaResult.TestErrors[currentTest]}";
                }

                var testResult = CheckAndGetTestResult(
                    test,
                    processExecutionResult,
                    checker,
                    message);
                currentTest++;
                testResults.Add(testResult);
            }

            return testResults;
        }

        protected override string PreprocessJsSubmission<TInput>(string template, IExecutionContext<TInput> context)
        {
            var code = context.Code.Trim(';');
            var processedCode = template
                .Replace(RequiredModules, this.JsCodeRequiredModules)
                .Replace(PreevaluationPlaceholder, this.JsCodePreevaulationCode)
                .Replace(EvaluationPlaceholder, this.JsCodeEvaluation)
                .Replace(PostevaluationPlaceholder, this.JsCodePostevaulationCode)
                .Replace(NodeDisablePlaceholder, this.JsNodeDisableCode)
                .Replace(TestsPlaceholder, this.BuildTests((context.Input as TestsInputModel)?.Tests))
                .Replace(UserInputPlaceholder, code);
            return processedCode;
        }

        public new class StrategySettings : NodeJsPreprocessExecuteAndRunUnitTestsWithMochaExecutionStrategy.StrategySettings
        {
            public string JsDomModulePath { get; set; } = string.Empty;
            public string JQueryModulePath { get; set; } = string.Empty;
            public string HandlebarsModulePath { get; set; } = string.Empty;
        }
    }
}
