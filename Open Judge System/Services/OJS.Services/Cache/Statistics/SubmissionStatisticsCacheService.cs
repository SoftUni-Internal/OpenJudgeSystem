namespace OJS.Services.Cache.Statistics
{
    using OJS.Data.Models;
    using OJS.Services.Business.Submissions.Models;
    using OJS.Services.Data.Submissions;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using static OJS.Common.Constants.CacheConstants;

    public class SubmissionStatisticsCacheService : ISubmissionStatisticsCacheService
    {
        private readonly ISubmissionsDataService submissionsData;
        private readonly ICacheService cacheService;

        public SubmissionStatisticsCacheService(
            ISubmissionsDataService submissionsData,
            ICacheService cacheService)
        {
            this.submissionsData = submissionsData;
            this.cacheService = cacheService;
        }

        public IEnumerable<SubmissionCountByMonthStatisticsModel> GetSubsmissionsCountByMonthForPastYear()
        {
            var currentDate = DateTime.UtcNow;
            var elevenMonthsSubmissionsResult = this.GetSubsmissionsCountByMonthForPastYearExceptLastMonth(currentDate);
            var lastMonthSubmissionsResult = this.GetSubsmissionsCountByMonthForLastMonth(currentDate);

            var eleventhMonth = elevenMonthsSubmissionsResult.Last().MonthNumber;
            var lastMonth = lastMonthSubmissionsResult.Single().MonthNumber;

            if (eleventhMonth == lastMonth)
            {
                // Next month has just started before the cache for last month is expired
                this.cacheService.Remove(SubmissionsCountForLastMonthKey);
                lastMonthSubmissionsResult = this.GetSubsmissionsCountByMonthForLastMonth(currentDate);
            };

            var result = elevenMonthsSubmissionsResult.Concat(lastMonthSubmissionsResult);

            return result;
        }

        private IEnumerable<SubmissionCountByMonthStatisticsModel> GetSubsmissionsCountByMonthForPastYearExceptLastMonth(
            DateTime currentDate)
            => this.cacheService.GetOrSet(
                SubmissionsCountByMonthsForPastElevenMonthsKey,
                () => this.GetSubmissionsCountGroupsForPastYearExceptLastMonth(currentDate),
                TimeSpan.FromTicks(this.GetAbsoluteEndOfMonth(currentDate).Ticks));

        private IEnumerable<SubmissionCountByMonthStatisticsModel> GetSubsmissionsCountByMonthForLastMonth(DateTime currentDate)
            => this.cacheService.GetOrSet(
                SubmissionsCountForLastMonthKey,
                () => this.GetSubmissionsCountGroupForLastMonth(currentDate),
                TimeSpan.FromSeconds(OneHourInSeconds));

        private List<SubmissionCountByMonthStatisticsModel> GetSubmissionsCountGroupsByMonths(
            Expression<Func<Submission, bool>> filter,
            bool orderGroups,
            int defaultMonthIfEmpty)
        {
            var groups = this.submissionsData
                .GetAll()
                .Where(filter)
                .GroupBy(s => new { s.CreatedOn.Year, s.CreatedOn.Month });

            if (orderGroups)
            {
                groups = groups.OrderBy(g => g.Min(s => s.CreatedOn));
            }

            return groups
                .Select(g => new SubmissionCountByMonthStatisticsModel
                {
                    MonthNumber = g.Key.Month,
                    TotalSubmissionsCount = g.Count(),
                })
                .AsEnumerable()
                .DefaultIfEmpty(new SubmissionCountByMonthStatisticsModel
                {
                    MonthNumber = defaultMonthIfEmpty,
                    TotalSubmissionsCount = 0,
                })
                .ToList();
        }

        private IEnumerable<SubmissionCountByMonthStatisticsModel> GetSubmissionsCountGroupsForPastYearExceptLastMonth(
            DateTime currentDate)
        {
            var begginingOfYearSet = this.GetAbsoluteBeggingOfMonth(currentDate.AddMonths(-11));
            var begginingOfCurrentMonth = this.GetAbsoluteBeggingOfMonth(currentDate);

            return this.GetSubmissionsCountGroupsByMonths(
                s => s.CreatedOn >= begginingOfYearSet && s.CreatedOn < begginingOfCurrentMonth,
                orderGroups: true,
                defaultMonthIfEmpty: begginingOfYearSet.Month);
        }

        private IEnumerable<SubmissionCountByMonthStatisticsModel> GetSubmissionsCountGroupForLastMonth(
            DateTime currentDate)
        {
            var begginingOfCurrentMonth = this.GetAbsoluteBeggingOfMonth(currentDate);
            var endOfCurrentMonth = this.GetAbsoluteEndOfMonth(currentDate);

            return this.GetSubmissionsCountGroupsByMonths(
                s => s.CreatedOn >= begginingOfCurrentMonth && s.CreatedOn <= endOfCurrentMonth,
                orderGroups: false,
                defaultMonthIfEmpty: begginingOfCurrentMonth.Month);
        }

        private DateTime GetAbsoluteBeggingOfMonth(DateTime date)
            => new DateTime(date.Year, date.Month, 1).Date;

        private DateTime GetAbsoluteEndOfMonth(DateTime date)
            => new DateTime(
                date.Year,
                date.Month,
                DateTime.DaysInMonth(date.Year, date.Month))
            .AddDays(1)
            .AddTicks(-1);
    }
}
