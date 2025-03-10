namespace OJS.Services.Ui.Data.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Common.Enumerations;
using OJS.Data;
using OJS.Data.Models.Contests;
using OJS.Data.Models.Problems;
using OJS.Services.Common.Data.Implementations;
using OJS.Services.Infrastructure;
using OJS.Services.Ui.Models.Contests;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

public class ContestsDataService : DataService<Contest>, IContestsDataService
{
    private readonly IDatesService dates;

    public ContestsDataService(
        OjsDbContext db,
        IDatesService dates)
        : base(db)
        => this.dates = dates;

    public async Task<IEnumerable<TServiceModel>> GetAllCompetable<TServiceModel>()
        => await this.GetAllCompetableQuery()
            .MapCollection<TServiceModel>()
            .ToListAsync();

    public async Task<IEnumerable<TServiceModel>> GetAllExpired<TServiceModel>()
        => await this.GetAllExpiredQuery()
            .MapCollection<TServiceModel>()
            .ToListAsync();

    public IQueryable<Contest> GetAllVisible()
        => this.GetQuery(c => c.IsVisible || c.VisibleFrom <= this.dates.GetUtcNow());

    public async Task<PagedResult<TServiceModel>> GetAllAsPageByFiltersAndSorting<TServiceModel>(
        ContestFiltersServiceModel model)
    {
        var contests = model.CategoryIds.Any()
            ? this.GetAllVisibleByCategories(model.CategoryIds)
            : this.GetAllVisible();

        return await this.ApplyFiltersSortAndPagination<TServiceModel>(contests, model);
    }

    public IQueryable<Contest> GetLatestForParticipantByUsername(string username)
        => this.GetQuery(c => c.Participants
                .Any(p => p.User.UserName == username))
            .OrderByDescending(c => c.Participants
                .Where(p => p.User.UserName == username)
                .Max(p => p.CreatedOn));

    public async Task<PagedResult<TServiceModel>> ApplyFiltersSortAndPagination<TServiceModel>(
        IQueryable<Contest> contests,
        ContestFiltersServiceModel model)
    {
        // TODO: Remove filter by status (not used anumorE)
        contests = this.FilterByStatus(contests, model.Statuses.ToList());
        contests = Sort(contests, model.SortType, model.CategoryIds.Count());

        if (model.SubmissionTypeIds.Any())
        {
            contests = contests
                .Where(ContainsSubmissionTypeIds(model.SubmissionTypeIds));
        }

        return await contests.Paginate<TServiceModel>(model.ItemsPerPage, model.PageNumber);
    }

    public IQueryable<Contest> GetAllByLecturer(string lecturerId)
        => this.GetQuery(c =>
                c.LecturersInContests.Any(l => l.LecturerId == lecturerId) ||
                c.Category!.LecturersInContestCategories.Any(l => l.LecturerId == lecturerId));

    public Task<int> GetMaxPointsForExportById(int id)
        => this.GetMaxPointsByIdAndProblemGroupsFilter(id, pg => pg.Type != ProblemGroupType.ExcludedFromHomework);

    public Task<bool> IsUserInExamGroupByContestAndUser(int id, string userId)
        => this.Exists(c =>
                c.Id == id &&
                c.ExamGroups.Any(eg => eg.UsersInExamGroups.Any(u => u.UserId == userId)));

    private static IQueryable<Contest> Sort(
        IQueryable<Contest> contests,
        ContestSortType? sorting,
        int categoriesCount)
    {
        // After removing the sorting menu for the user, we are using OrderBy as default sorting value
        // Logic for Name, StartDate and EndDate is not used anymore therefore it is commented out
        // if (sorting == ContestSortType.StartDate)
        // {
        //     return contests
        //         .OrderBy(c => c.StartTime)
        //         .ThenBy(c => c.PracticeStartTime)
        //         .ThenBy(c => c.Name);
        // }
        //
        // if (sorting == ContestSortType.EndDate)
        // {
        //     return contests
        //         .OrderBy(c => c.EndTime)
        //         .ThenBy(c => c.PracticeEndTime)
        //         .ThenBy(c => c.Name);
        // }
        //
        // if (sorting == ContestSortType.Name)
        // {
        //     return contests
        //         .OrderBy(c => c.Name)
        //         .ThenBy(c => c.StartTime)
        //         .ThenBy(c => c.PracticeStartTime);
        // }

        // By checking the number of categories we can determine if the contest is a parent or a child contest
        // based on this we display the contests differently
        // 0 categories - main contest page displays the contest which have the LEAST time left
        // 1 category - child contest and we order the by the order by property of the category
        // > 1 categories - we first order them by the Contest.Category's OrderBy and then by the Contest's OrderBy

        if (sorting == ContestSortType.OrderBy)
        {
            switch (categoriesCount)
            {
                // No category chosen - contests are ordered by most recent activity
                case 0:
                    return contests.OrderByActivity();

                // Inner most category
                case 1:
                    return contests.OrderByOrderBy();

                // Has child categories
                default:
                    return contests.OrderByCategoryAndContestOrderBy();
            }
        }

        return contests;
    }

    private static Expression<Func<Contest, bool>> ContainsSubmissionTypeIds(IEnumerable<int> submissionTypeIds)
        => c => c.ProblemGroups
            .SelectMany(pg => pg.Problems)
            .SelectMany(p => p.SubmissionTypesInProblems)
            .Select(x => x.SubmissionTypeId)
            .Any(x => submissionTypeIds.Contains(x)); // Does not work if converted to method group

    private async Task<int> GetMaxPointsByIdAndProblemGroupsFilter(int id, Expression<Func<ProblemGroup, bool>> filter)
    {
        var contest = await this.GetByIdQuery(id)
            .Include(c => c.ProblemGroups)
                .ThenInclude(pg => pg.Problems)
            .FirstOrDefaultAsync();

        if (contest == null)
        {
            return 0;
        }

        var filteredProblemGroups = contest.ProblemGroups
            .Where(pg => pg.Problems.Any(p => !p.IsDeleted))
            .Where(filter.Compile());

        var maxPoints = filteredProblemGroups
            .Select(pg => pg.Problems
                .Where(p => !p.IsDeleted)
                .Max(p => (int?)p.MaximumPoints) ?? 0)
            .Sum();

        return maxPoints;
    }

    private IQueryable<Contest> GetAllVisibleByCategories(IEnumerable<int> categoryIds)
        => this.GetAllVisible()
            .Where(c => c.CategoryId.HasValue && categoryIds.Contains(c.CategoryId.Value));

    private IQueryable<Contest> GetAllCompetableQuery()
        => this.GetAllVisible()
            .Where(this.CanBeCompeted());

    private IQueryable<Contest> GetAllExpiredQuery()
        => this.GetAllVisible()
            .Where(this.IsExpired());

    private Expression<Func<Contest, bool>> CanBeCompeted()
        => c => c.StartTime <= this.dates.GetUtcNow()
                && (!c.EndTime.HasValue || c.EndTime >= this.dates.GetUtcNow());

    private Expression<Func<Contest, bool>> IsUpcoming()
        => c => c.StartTime > this.dates.GetUtcNow();

    private Expression<Func<Contest, bool>> IsExpired()
        => c => c.StartTime <= this.dates.GetUtcNow()
                && c.EndTime < this.dates.GetUtcNow();

    private Expression<Func<Contest, bool>> CanBePracticed()
        => c => c.PracticeStartTime <= this.dates.GetUtcNow()
                && (!c.PracticeEndTime.HasValue || c.PracticeEndTime > this.dates.GetUtcNow());

    private IQueryable<Contest> FilterByStatus(
        IQueryable<Contest> contests,
        ICollection<ContestStatus> statuses)
    {
        var active = statuses.Any(s => s == ContestStatus.Active);
        var past = statuses.Any(s => s == ContestStatus.Past);
        var upcoming = statuses.Any(s => s == ContestStatus.Upcoming);
        var practice = statuses.Any(s => s == ContestStatus.Practice);

        if (active && !past && !upcoming && !practice)
        {
            return contests.Where(this.CanBeCompeted());
        }

        if (past && !active && !upcoming && !practice)
        {
            return contests.Where(this.IsExpired());
        }

        if (upcoming && !active && !past && !practice)
        {
            return contests.Where(this.IsUpcoming());
        }

        if (practice && !active && !past && !upcoming)
        {
            return contests.Where(this.CanBePracticed());
        }

        if (active && past)
        {
            return contests
                .Where(this.CanBeCompeted())
                .Concat(contests.Where(this.IsExpired()));
        }

        return contests;
    }
}