namespace OJS.Workers.ExecutionStrategies.NodeJs
{
    public static class NodeJsConstants
    {
        // Arguments
        public const string TestsReporterArgument = "-R"; // https://node-tap.org/docs/reporting/

        // JavaScript Flags
        public const string DelayFlag = "--delay";
        public const string LatestEcmaScriptFeaturesEnabledFlag = "--harmony";

        // TypeScript Flags
        public const string StrictFlag = "--strict";
        public const string TargetFlag = "--target";
        public const string TargetValue = "es2017";
        public const string ModuleFlag = "--module";
        public const string ModuleValue = "commonjs";
        public const string LibFlag = "--lib";
        public const string LibValue = "es2017,dom";

        // Other
        public const string JsonReportName = "json";

        public const string UserInputPlaceholder = "#userInput#";
        public const string AdapterFunctionPlaceholder = "#adapterFunctionCode#";
        public const string UserBaseDirectoryPlaceholder = "#userBaseDirectoryPlaceholder#";
        public const string NodeDisablePlaceholder = "#nodeDisableCode";
    }
}
