namespace OJS.Common.Constants
{
    public static class CacheConstants
    {
        public const int OneHourInSeconds = 60 * 60;
        public const int OneDayInSeconds = 86400;

        public const string MainContestCategoriesDropDown = "MainContestCategoriesDropDown";
        public const string ContestCategoriesTree = "ContestCategoriesTree";
        public const string ContestSubCategoriesFormat = "ContestSubCategories:{0}";
        public const string ContestParentCategoriesFormat = "ContestParentCategories:{0}";
        public const string ContestCategoryNameFormat = "ContestCategoryName:{0}";

        public const string SubmissionsCountByMonthsForPastElevenMonthsKey = "SubmissionsCountByMonthForPastElevenMonths";
        public const string SubmissionsCountForLastMonthKey = "SubmissionsCountByMonthForLastMonth";

        public const string ActiveContests = "ActiveContests";
        public const string PastContests = "PastContests";

        public const int DefaultPastContestsToTake = 15;

        public static readonly string ContestView = "ContestView:{0}";

        public static readonly string KeysPrefix = "OJS:";

        public static readonly string ParticipantsCountByContest = "ParticipantsCountByContest:{0}";
    }
}