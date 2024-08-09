namespace OJS.Common
{
    public static class GlobalConstants
    {
        public const string ApplicationFullName = "OpenJudgeSystem";
        public const int BatchOperationsChunkSize = 3000;
        public const double ProblemDefaultOrderBy = 0;
        public const int NumberOfAutocompleteItemsShown = 20;

        public const int BestSubmissionEligibleForArchiveAgeInYears = 2;
        public const int NonBestSubmissionEligibleForArchiveAgeInYears = 1;

        // Tests
        public const string TestInputTxtFileExtension = FileExtensions.Input + FileExtensions.Txt;
        public const string TestOutputTxtFileExtension = FileExtensions.Output + FileExtensions.Txt;

        public const string ZeroTestStandardSignature = ".000.";
        public const string OpenTestStandardSignature = ".open.";

        //CORS
        public const string CorsDefaultPolicyName = "AllowFrontEndRequests";
        // Other
        public const string NewLineUnix = "\n";
        public const string NewLineWin = "\r\n";
        public const string ClassDelimiterWin = $"~~!!!==#==!!!~~{NewLineWin}";

        public static class BackgroundJobs
        {
            public const string AdministrationQueueName = "administration";
            public const string UiQueueName = "ui";
        }

        public static class ApplicationDateFormats
        {
            public const string GlobalShortDatePatternFormat = "dd/MM/yyyy";
        }

        public static class Assemblies
        {
            // Models
            public const string ModelsRegexPatternTemplate = @"^{0}\..+Models,";

            // Services
            public const string BusinessServicesRegexPatternTemplate = ServicesRegexPatternPrefix + "Business,";
            public const string DataServicesRegexPatternTemplate = ServicesRegexPatternPrefix + "Data,";
            public const string CommonServicesRegexPatternTemplate = ServicesRegexPatternPrefix + "Common,";
            public const string InfrastructureServicesRegexPatternTemplate = ServicesRegexPatternPrefix + "Infrastructure,";

            private const string ServicesRegexPatternPrefix = @"^{0}\.Services\..*\.?";
        }

        public static class Roles
        {
            public const string Administrator = "Administrator";
            public const string Lecturer = "Lecturer";
            public const string Developer = "Developer";

            public const string AdministratorOrLecturer = Administrator + ", " + Lecturer;
        }

        public static class Urls
        {
            public const string HangfirePath = "/hangfire";

            public const string GetUserInfoByIdPath = "/api/users/getjudgeuserinfobyuserid";
            public const string GetUserInfoByUsernamePath = "/api/users/getjudgeuserinfobyusername";
            public const string ExternalRegisterPath = "/identity/externaljudgeregister";
        }

        public static class MimeTypes
        {
            public const string ApplicationJson = "application/json";
            public const string ApplicationPdf = "application/pdf";
            public const string ApplicationZip = "application/zip";
            public const string ApplicationUnknown = "application/unknown";
            public const string ApplicationOctetStream = "application/octet-stream";
            public const string Csv = "text/csv";
            public const string Tsv = "text/tsv";
            public const string CsvExcelSheet = "application/vnd.ms-excel";
            public const string ExcelSheet = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            public const string TextHtml = "text/html";
            public const string Plain = "text/plain";
        }

        public static class HeaderKeys
        {
            public const string ContentDisposition = "Content-Disposition";
            public const string Origin = "Origin";
            public const string ApiKey = "X-Api-Key";
        }

        public static class HeaderValues
        {
            public const string ContentDispositionFileNameUtf8 = "filename*=UTF-8''";
        }

        public static class ErrorMessages
        {
            public const string GenericErrorMessage = "Something went wrong, please try again";
            public const string ModelCannotBeNull = "Model cannot be null";
            public const string FormCannotBeNull = "Form cannot be null";
            public const string UrlCannotBeNull = "URL cannot be null";
            public const string EntityDoesNotExistMessage = "Entity does not exist.";

            public const string EntityWithIdDoesNotExistTemplate = "{0} with Id \"{1}\" does not exist.";
            public const string CannotBeTemplate = "{0} cannot be {1}";
            public const string ValueCannotBeLessThanOrEqualToZero = "Value cannot be less than or equal to 0";
            public const string ValueCannotBeNullOrWhiteSpaceTemplate = "{0} cannot be null or white space";

            public const string AdministrationModelIdValidationMessage = "The provided id for the entity is invalid.";

            public const string InactiveLoginSystem =
                "We are sorry for the inconvenience, but the login system is currently unavailable";
            public const string NonExistentUser =
                "User with this username does not exist";
            public const string InvalidUsernameOrPassword =
                "Invalid username or password.";
            public const string ContestNotFound = "Contest not found";
            public const string LoggedInThroughDatabase = "We were unable to log you in through the SULS Platform. If you have profile changes, they won't take effect until you log out.";
        }

        public static class FileExtensions
        {
            public const string Json = ".json";
            public const string Txt = ".txt";
            public const string Html = ".html";
            public const string Excel = ".xlsx";
            public const string Xml = ".xml";
            public const string Csv = ".csv";
            public const string Zip = ".zip";
            public const string Sol = ".sol";
            public const string Input = ".in";
            public const string Output = ".out";
            public const string Docx = ".docx";
            public const string TestInputTxt = Input + Txt;
            public const string TestOutputTxt = Output + Txt;
            public const string TestInputZip = Input + Zip;
            public const string TestOutputZip = Output + Zip;
        }

        public static class Submissions
        {
            public const string SubmissionDownloadFileName = "Submission_{0}.{1}";
        }
    }
}