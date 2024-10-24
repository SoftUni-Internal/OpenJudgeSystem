namespace OJS.Services.Ui.Data.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using OJS.Data;
    using OJS.Data.Models.Contests;
    using OJS.Services.Common.Data.Implementations;
    using OJS.Services.Infrastructure.Extensions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ContestCategoriesDataService : DataService<ContestCategory>, IContestCategoriesDataService
    {
        public ContestCategoriesDataService(OjsDbContext db)
            : base(db)
        {
        }

        public IQueryable<ContestCategory> GetAllVisible()
            => this.GetQuery(cc => cc.IsVisible);

        public Task<IEnumerable<T>> GetAllVisible<T>()
            => this.GetQuery(cc => cc.IsVisible)
                .MapCollection<T>()
                .ToEnumerableAsync();

        public async Task<IEnumerable<T>> GetAllowedStrategyTypesById<T>(int id)
            => await this.GetQuery(cc => cc.Id == id)
                .SelectMany(c => c.Contests)
                    .Where(c => !c.IsDeleted && c.IsVisible)
                .SelectMany(pg => pg.ProblemGroups)
                    .Where(pg => !pg.IsDeleted)
                .SelectMany(p => p.Problems)
                    .Where(p => !p.IsDeleted)
                .SelectMany(stp => stp.SubmissionTypesInProblems)
                .Select(st => st.SubmissionType)
                .MapCollection<T>()
                .ToListAsync();

        public IQueryable<ContestCategory> GetAllVisibleByLecturer(string lecturerId)
            => this.GetAllVisible()
                .Where(cc =>
                    cc.LecturersInContestCategories.Any(l => l.LecturerId == lecturerId) ||
                    cc.Contests.Any(c => c.LecturersInContests.Any(l => l.LecturerId == lecturerId)));

        public Task<string?> GetNameById(int id)
            => this.GetQuery(cc => cc.Id == id)
                .Select(cc => cc.Name)
                .FirstOrDefaultAsync();

        public Task<bool> HasContestsById(int id)
            => this.GetAllVisible()
                .Where(cc => cc.Id == id)
                .AnyAsync(cc => cc.Contests.Any());

        public IQueryable<ContestCategory> GetAllVisibleOrdered() =>
            this.GetQuery(cc => cc.IsVisible)
                .OrderBy(x => x.OrderBy);

        public Task<IEnumerable<TServiceModel>> GetAllVisibleMainOrdered<TServiceModel>()
            => this.GetAllVisibleOrdered()
                .Where(x => !x.ParentId.HasValue)
                .MapCollection<TServiceModel>()
                .ToEnumerableAsync();
    }
}