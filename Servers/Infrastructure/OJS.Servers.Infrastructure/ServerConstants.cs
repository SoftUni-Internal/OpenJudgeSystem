namespace OJS.Servers.Infrastructure
{
    public static class ServerConstants
    {
        public const string LokiHttpClientName = "Loki";

        public static class Authentication
        {
            public const string CanAccessAdministrationCookieName = "can_access_administration";
            public const string LoggedInUsername = "logged_in_username";
        }

        public static class Authorization
        {
            public const string ApiKeyPolicyName = "Api Key Requirement";
        }

        public static class UrlParameters
        {
            public const string ApiKey = "apiKey";
        }

        public static class ViewDataKeys
        {
            public const string ReturnUrl = "ReturnUrl";
        }

        public static class ExceptionHandling
        {
            public const string BadHttpRequestExceptionTitle = "Bad Request!";
            public const string ValidationExceptionTitle = "Invalid Request!";
            public const string UnhandledExceptionTitle = "An unexpected error occurred!";
        }
    }
}