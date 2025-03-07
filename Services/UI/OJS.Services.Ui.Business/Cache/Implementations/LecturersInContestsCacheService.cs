namespace OJS.Services.Ui.Business.Cache.Implementations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OJS.Data.Models;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

public class LecturersInContestsCacheService(
    IMemoryCache memoryCache,
    IDataService<LecturerInContest> lecturerInContests,
    IDataService<LecturerInContestCategory> lecturerInContestCategories)
    : ILecturersInContestsCacheService
{
    private const string LecturersInContestsCacheKey = "LecturersInContests";
    private const string LecturersInContestCategoriesCacheKey = "LecturersInContestCategories";
    private readonly TimeSpan cacheExpiration = TimeSpan.FromMinutes(5);

    public async Task<bool> IsUserAdminOrLecturerInContest(int? contestId, int? categoryId, UserInfoModel? user)
    {
        if (contestId is null or 0 || categoryId is null or 0 || user is null)
        {
            return false;
        }

        if (user is { IsLecturer: false, IsAdmin: false })
        {
            return false;
        }

        if (user.IsAdmin)
        {
            return true;
        }

        var lecturersInContestCategories = await memoryCache.GetOrCreateAsync(
            LecturersInContestCategoriesCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = this.cacheExpiration;

                return await lecturerInContestCategories
                    .GetQuery()
                    .GroupBy(l => l.LecturerId)
                    .ToDictionaryAsync(g => g.Key, g => g.Select(l => l.ContestCategoryId));
            });

        var userIsLecturerInCategory = lecturersInContestCategories != null &&
                                       lecturersInContestCategories.TryGetValue(user.Id, out var categories) &&
                                       categories.Contains(categoryId.Value);

        if (userIsLecturerInCategory)
        {
            return true;
        }

        var lecturersInContests = await memoryCache.GetOrCreateAsync(
            LecturersInContestsCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = this.cacheExpiration;

                return await lecturerInContests
                    .GetQuery()
                    .GroupBy(l => l.LecturerId)
                    .ToDictionaryAsync(g => g.Key, g => g.Select(l => l.ContestId));
            });

        var userIsLecturerInContest = lecturersInContests != null &&
                                      lecturersInContests.TryGetValue(user.Id, out var contests) &&
                                      contests.Contains(contestId.Value);

        return userIsLecturerInContest;
    }
}