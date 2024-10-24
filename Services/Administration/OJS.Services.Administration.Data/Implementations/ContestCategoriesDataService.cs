namespace OJS.Services.Administration.Data.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using OJS.Data;
    using OJS.Data.Models.Contests;
    using OJS.Services.Common.Models.Users;
    using OJS.Services.Infrastructure.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class ContestCategoriesDataService : AdministrationDataService<ContestCategory>, IContestCategoriesDataService
    {
        private readonly OjsDbContext dbContext;

        public ContestCategoriesDataService(OjsDbContext db)
            : base(db) =>
            this.dbContext = db;

        public Task<IEnumerable<ContestCategory>> GetContestCategoriesByParentId(int? parentId)
            => this.GetQuery(cc => cc.ParentId == parentId)
                 .ToEnumerableAsync();

        public Task<IEnumerable<ContestCategory>> GetContestCategoriesWithoutParent()
            => this.GetQuery(cc => !cc.ParentId.HasValue)
                .ToEnumerableAsync();

        public async Task<bool> UserHasContestCategoryPermissions(int categoryId, string? userId, bool isAdmin)
            => !string.IsNullOrWhiteSpace(userId) &&
               (isAdmin || await this.Exists(x =>
                   x.Id == categoryId &&
                   x.LecturersInContestCategories.Any(y => y.LecturerId == userId)));

        public IQueryable<ContestCategory> GetAllVisible()
            => this.GetQuery(cc => cc.IsVisible);

        public IQueryable<ContestCategory> GetAllVisibleAndNotDeleted()
            => this.GetQuery(cc => cc.IsVisible && !cc.IsDeleted);

        public IQueryable<ContestCategory> GetAllVisibleByLecturer(string? lecturerId)
            => this.GetAllVisible()
                .Where(cc =>
                    cc.LecturersInContestCategories.Any(l => l.LecturerId == lecturerId) ||
                    cc.Contests.Any(c => c.LecturersInContests.Any(l => l.LecturerId == lecturerId)));

        public async Task<ContestCategory> GetById(int? id)
            => await this.GetByIdQuery(id!)
                .FirstAsync();

        public async Task<ContestCategory?> GetByIdWithParent(int? id)
            => await this.GetByIdQuery(id!)
                .Include(c => c.Parent)
                .FirstOrDefaultAsync();

        public Task<string?> GetNameById(int id)
            => this.GetQuery(cc => cc.Id == id)
                .Select(cc => cc.Name)
                .FirstOrDefaultAsync();

        public Task<bool> HasContestsById(int id)
            => this.GetAllVisible()
                .Where(cc => cc.Id == id)
                .AnyAsync(cc => cc.Contests.Any());

        public async Task LoadChildrenRecursively(ContestCategory category)
        {
            await this.dbContext
                .Entry(category)
                .Collection(c => c.Children)
                .LoadAsync();

            foreach (var child in category.Children)
            {
                await this.LoadChildrenRecursively(child);
            }
        }

        protected override Expression<Func<ContestCategory, bool>> GetUserFilter(UserInfoModel user)
            => category => user.IsAdmin ||
                           category.LecturersInContestCategories.Any(cc => cc.LecturerId == user.Id);
    }
}