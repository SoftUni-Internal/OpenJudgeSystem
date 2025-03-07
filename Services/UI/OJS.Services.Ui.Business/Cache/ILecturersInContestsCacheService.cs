namespace OJS.Services.Ui.Business.Cache;

using OJS.Services.Common.Models.Users;
using OJS.Services.Infrastructure;
using System.Threading.Tasks;

public interface ILecturersInContestsCacheService : IService
{
    /// <summary>
    /// Check if user is admin or lecturer in contest, or in category for the contest.
    /// Uses in-memory cache for all the lecturers in contests and categories, which is cleared on a short interval.
    /// </summary>
    /// <param name="contestId">The contest to check for</param>
    /// <param name="categoryId">The category of the contest</param>
    /// <param name="user">The user to check</param>
    /// <returns>True if user is admin or lecturer in contest, or in category for the contest.</returns>
    Task<bool> IsUserAdminOrLecturerInContest(int? contestId, int? categoryId, UserInfoModel? user);
}