namespace OJS.Services.Administration.Data.Implementations
{
    using OJS.Data;
    using OJS.Data.Models.Submissions;
    using OJS.Data.Models.Tests;
    using OJS.Services.Common.Models.Users;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class TestsDataService : AdministrationDataService<Test>, ITestsDataService
    {
        public TestsDataService(OjsDbContext tests)
            : base(tests)
        {
        }

        public IQueryable<Test> GetByIdQuery(int id)
            => this.GetQuery(t => t.Id == id);

        public IQueryable<Test> GetAllByProblem(int problemId)
            => this.GetQuery(t => t.ProblemId == problemId);

        public IQueryable<Test> GetAllNonTrialByProblem(int problemId)
            => this.GetAllByProblem(problemId)
                .Where(t => !t.IsTrialTest);

        public async Task DeleteByProblem(int problemId)
        {
            this.Delete(t => t.ProblemId == problemId);
            await this.SaveChanges();
        }

        public Task DeleteBySubmissions(IEnumerable<Submission> submissions) => throw new NotImplementedException();

        protected override Expression<Func<Test, bool>> GetUserFilter(UserInfoModel user)
            => test => user.IsAdmin ||
                          test.Problem.ProblemGroup.Contest.Category!.LecturersInContestCategories.Any(cc => cc.LecturerId == user.Id) ||
                          test.Problem.ProblemGroup.Contest.LecturersInContests.Any(l => l.LecturerId == user.Id);
    }
}