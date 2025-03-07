namespace OJS.Workers.ExecutionStrategies.Helpers;

using OJS.Workers.Common.Models;
using static OJS.Workers.ExecutionStrategies.NodeJs.NodeJsConstants;

public static class JsCodePreEvaluationCodeProvider
{
    public static string GetPreEvaluationCode(ExecutionStrategyType executionStrategyType)
        => executionStrategyType switch
        {
            ExecutionStrategyType.NodeJsPreprocessExecuteAndCheck or
            ExecutionStrategyType.NodeJsV20PreprocessExecuteAndCheck or
            ExecutionStrategyType.TypeScriptV20PreprocessExecuteAndCheck
                => GetForNodeJsPreprocessExecuteAndCheck(),

            ExecutionStrategyType.NodeJsPreprocessExecuteAndRunUnitTestsWithMocha or
            ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunUnitTestsWithMocha
                => GetForNodeJsPreprocessExecuteAndRunUnitTestsWithMocha(),

            ExecutionStrategyType.NodeJsPreprocessExecuteAndRunJsDomUnitTests
                => GetForNodeJsPreprocessExecuteAndRunJsDomUnitTests(),
            ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunJsDomUnitTests
                => GetForNodeJsV20PreprocessExecuteAndRunJsDomUnitTests(),

            ExecutionStrategyType.NodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy
                => GetForNodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMocha(),
            ExecutionStrategyType.NodeJsV20PreprocessExecuteAndRunCodeAgainstUnitTestsWithMochaExecutionStrategy
                => GetForNodeJsV20PreprocessExecuteAndRunCodeAgainstUnitTestsWithMocha(),

            ExecutionStrategyType.NodeJsZipExecuteHtmlAndCssStrategy
                => GetForNodeJsZipExecuteHtmlAndCss(),
            ExecutionStrategyType.NodeJsV20ZipExecuteHtmlAndCssStrategy
                => GetForNodeJsV20ZipExecuteHtmlAndCss(),

            _ => throw new ArgumentOutOfRangeException(
                nameof(executionStrategyType),
                $"Pre-evaluation code for the given execution strategy type: {executionStrategyType} is not implemented."),
        };

    private static string GetForNodeJsPreprocessExecuteAndCheck() => @"
let content = '';
let code = {
    run: " + UserInputPlaceholder + @"
};
let adapterFunction = " + AdapterFunctionPlaceholder;

    private static string GetForNodeJsPreprocessExecuteAndRunUnitTestsWithMocha() => @"
chai.use(sinonChai);
describe('TestScope', function() {
    let code = {
        run: " + UserInputPlaceholder + @"
    };

    let result = code.run;
    let bgCoderConsole = {};

    before(function() {
        Object.keys(console)
            .forEach(function (prop) {
                bgCoderConsole[prop] = console[prop];
                console[prop] = new Function('');
            });
    });

    after(function() {
        Object.keys(bgCoderConsole)
            .forEach(function (prop) {
                console[prop] = bgCoderConsole[prop];
            });
    });";

    private static string GetForNodeJsPreprocessExecuteAndRunJsDomUnitTests() => @"
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

    private static string GetForNodeJsV20PreprocessExecuteAndRunJsDomUnitTests() => @"
chai.use(sinonChai);
describe('TestDOMScope', function() {
    let bgCoderConsole = {};
    before(function(done) {
    const dom = new jsdom.JSDOM(``);
    const { window } = dom;

    // define innerText manually to work as textContent
    Object.defineProperty(window.Element.prototype, 'innerText', {
        get() { return this.textContent; },
        set(value) { this.textContent = value; }
    });

    global.window = window;
    global.document = window.document;
    global.$ = jq(window);
    global.handlebars = handlebars;

    Object.getOwnPropertyNames(window)
        .filter(function(prop) {
            return prop.toLowerCase().indexOf('html') >= 0;
        }).forEach(function(prop) {
            global[prop] = window[prop];
        });

    Object.keys(console)
        .forEach(function(prop) {
            bgCoderConsole[prop] = console[prop];
            console[prop] = function() {}; // replacing new Function('') with an empty function
        });

    done();
});
    after(function() {
        Object.keys(bgCoderConsole)
            .forEach(function (prop) {
                console[prop] = bgCoderConsole[prop];
            });
    });";

    private static string GetForNodeJsPreprocessExecuteAndRunCodeAgainstUnitTestsWithMocha() => @"
chai.use(sinonChai);
let bgCoderConsole = {};
before(function(done)
{
    jsdom.env({
        html: '',
        done: function(errors, window) {
            global.window = window;
            global.document = window.document;
            global.$ = jq(window);
            global.handlebars = handlebars;
            Object.getOwnPropertyNames(window)
                .filter(function(prop) {
                return prop.toLowerCase().indexOf('html') >= 0;
            }).forEach(function(prop) {
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

    private static string GetForNodeJsV20PreprocessExecuteAndRunCodeAgainstUnitTestsWithMocha() => @"
chai.use(sinonChai);
let bgCoderConsole = {};
before(function(done) {
        const dom = new jsdom.JSDOM('', {
            runScripts: 'dangerously',
            resources: 'usable'
        });

        global.window = dom.window;
        global.document = dom.window.document;
        global.$ = jq(dom.window);
        global.handlebars = handlebars; // Include this line only if you're using Handlebars

        Object.getOwnPropertyNames(dom.window)
            .filter(function(prop) {
                return prop.toLowerCase().indexOf('html') >= 0;
            }).forEach(function(prop) {
                global[prop] = dom.window[prop];
            });

        Object.keys(console)
            .forEach(function(prop) {
                bgCoderConsole[prop] = console[prop];
                console[prop] = new Function('');
            });

        done();
    });

after(function() {
    Object.keys(bgCoderConsole)
        .forEach(function (prop) {
            console[prop] = bgCoderConsole[prop];
        });
});";

    private static string GetForNodeJsZipExecuteHtmlAndCss() => $@"
describe('TestDOMScope', function() {{
    let bgCoderConsole = {{}};
    before(function(done) {{
        jsdom.env({{
            html: userCode,
            src:[bootstrap],
            done: function(errors, window) {{
                global.window = window;
                global.document = window.document;
                global.$ = global.jQuery = jq(window);
                Object.getOwnPropertyNames(window)
                    .filter(function (prop) {{
                        return prop.toLowerCase().indexOf('html') >= 0;
                    }}).forEach(function (prop) {{
                        global[prop] = window[prop];
                    }});

                let head = $(document.head);
                let style = document.createElement('style');
                style.type = 'text/css';
                style.innerHTML = bootstrapCss;
                head.append(style);

                let links = head.find('link');
                links.each((index, el)=>{{
                    let style = document.createElement('style');
                    style.type = 'test/css';
                    let path = '{UserBaseDirectoryPlaceholder}/' + el.href;
                    let css = fs.readFileSync(path, 'utf-8');
                    style.innerHTML = css;
                    head.append(style);
                }});

                links.remove();

                Object.keys(console)
                    .forEach(function (prop) {{
                        bgCoderConsole[prop] = console[prop];
                        console[prop] = new Function('');
                    }});

{NodeDisablePlaceholder}

                done();
            }}
        }});
    }});

    after(function() {{
        Object.keys(bgCoderConsole)
            .forEach(function (prop) {{
                console[prop] = bgCoderConsole[prop];
            }});
    }});";

    private static string GetForNodeJsV20ZipExecuteHtmlAndCss() => $@"
describe('TestDOMScope', function() {{
    let bgCoderConsole = {{}};
before(function(done) {{
    const dom = new jsdom.JSDOM(userCode, {{
        runScripts: ""dangerously"",
        resources: ""usable""
    }});

    const {{ window }} = dom;

    global.window = window;
    global.document = window.document;
    global.$ = global.jQuery = jq(window);

    Object.getOwnPropertyNames(window)
        .filter((prop) => prop.toLowerCase().indexOf('html') >= 0)
        .forEach((prop) => {{
            global[prop] = window[prop];
        }});

    let head = $(document.head);
    let style = document.createElement('style');
    style.type = 'text/css';
    style.innerHTML = bootstrapCss;
    head.append(style);

    let links = head.find('link');
                links.each((index, el)=>{{
                    let style = document.createElement('style');
                    style.type = 'text/css';
                    let path = '{UserBaseDirectoryPlaceholder}/' + el.href;
                    let css = fs.readFileSync(path, 'utf-8');
                    style.innerHTML = css;
                    head.append(style);
                }});

                links.remove();


   Object.keys(console)
                    .forEach(function (prop) {{
                        bgCoderConsole[prop] = console[prop];
                        console[prop] = new Function('');
                    }});

    {NodeDisablePlaceholder}

    done();
}});

    after(function() {{
        Object.keys(bgCoderConsole)
            .forEach(function (prop) {{
                console[prop] = bgCoderConsole[prop];
            }});
    }});";
}
