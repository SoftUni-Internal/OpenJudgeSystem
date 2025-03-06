namespace OJS.Services.Infrastructure.Constants
{
    using System.Text;

    public static class CacheConstants
    {
        public const int OneMinuteInSeconds = 60;
        public const int OneHourInSeconds = OneMinuteInSeconds * 60;
        public const int OneDayInSeconds = OneHourInSeconds * 24;
        public const int TwoMinutesInSeconds = OneMinuteInSeconds * 2;
        public const int FiveMinutesInSeconds = OneMinuteInSeconds * 5;
        public const int TenMinutesInSeconds = OneMinuteInSeconds * 10;

        public const string ParticipantsCountByContest = "ParticipantsCountByContest:{0}";
        public const string ParticipantsCountByContestsPage = "ParticipantsCountByContestsPage:{0}:{1}";
        public const string ContestDetailsById = "Contest:{0}:Details";
        public const string ProblemForSubmit = "Problem:{0}:Submit";
        public const string ContestCategoriesTree = "ContestCategoriesTree";
        public const string ContestCategoryDetails = "ContestCategoryDetails:{0}";

        public const string ContestSubCategoriesFormat = "ContestSubCategories:{0}";

        public const string TotalSubmissionsCount = "TotalSubmissionsCount";
        public const string LatestPublicSubmissions = "PublicSubmissions";

        public const string SubmissionTypesByContestCategory = "SubmissionTypesByCategory:{0}";

        public const string MentorSystemMessage = "MentorSystemMessage:{0}:{1}";

        public static CompositeFormat SettingsFormat => CompositeFormat.Parse("Settings:{0}");
    }
}