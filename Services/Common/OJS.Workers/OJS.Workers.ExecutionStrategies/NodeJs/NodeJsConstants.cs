namespace OJS.Workers.ExecutionStrategies.NodeJs
{
    public static class NodeJsConstants
    {
        // Arguments
        public const string TestsReporterArgument = "-R"; // https://node-tap.org/docs/reporting/

        // JavaScript Flags
        public const string DelayFlag = "--delay";
        public const string LatestEcmaScriptFeaturesEnabledFlag = "--harmony";

        // Other
        public const string JsonReportName = "json";

        public const string UserInputPlaceholder = "#userInput#";
        public const string AdapterFunctionPlaceholder = "#adapterFunctionCode#";
        public const string UserBaseDirectoryPlaceholder = "#userBaseDirectoryPlaceholder#";
        public const string NodeDisablePlaceholder = "#nodeDisableCode";
    }
}
