namespace OJS.Services.Infrastructure.Constants
{
    public static class CacheConstants
    {
        public const int OneHourInSeconds = 60 * 60;
        public const int OneDayInSeconds = 86400;
        public const int TwoMinutesInSeconds = 120;
        public const int FiveMinutesInSeconds = 300;

        public const string MainContestCategoriesDropDown = "MainContestCategoriesDropDown";
        public const string ParticipantsCountByContest = "ParticipantsCountByContest:{0}";
        public const string ParticipantsCountByContestsPage = "ParticipantsCountByContestsPage:{0}:{1}";
        public const string ContestDetails = "ContestDetails:{0}";
        public const string ContestDetailsForSubmit = "ContestDetailsForSubmit:{0}";
        public const string ContestCategoriesTree = "ContestCategoriesTree";

        public const string ContestSubCategoriesFormat = "ContestSubCategories:{0}";
        public const string ContestParentCategoriesFormat = "ContestParentCategories:{0}";
        public const string ContestCategoryNameFormat = "ContestCategoryName:{0}";

        public const string TotalSubmissionsCount = "TotalSubmissionsCount";

        public const string SubmissionTypesByUsage = "SubmissionTypesByUsage";
    }
}