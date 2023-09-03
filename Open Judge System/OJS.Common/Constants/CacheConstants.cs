﻿namespace OJS.Common.Constants
{
    public static class CacheConstants
    {
        public const int OneHourInSeconds = 60 * 60;
        public const int OneDayInSeconds = 86400;

        public const string MainContestCategoriesDropDown = "MainContestCategoriesDropDown";
        public const string ContestCategoriesTree = "ContestCategoriesTree";
        public const string ContestSubCategoriesFormat = "ContestSubCategories_id_{0}";
        public const string ContestParentCategoriesFormat = "ContestParentCategories_id_{0}";
        public const string ContestCategoryNameFormat = "ContestCategoryName_id_{0}";

        public const string SubmissionsCountByMonthsForPastElevenMonthsKey = "Submissions_count_by_month_for_past_eleven_months";
        public const string SubmissionsCountForLastMonthKey = "Submissions_count_by_month_for_last_month";

        public const string ActiveContests = "ActiveContests";
        public const string PastContests = "PastContests";

        public const int DefaultPastContestsToTake = 15;
    }
}