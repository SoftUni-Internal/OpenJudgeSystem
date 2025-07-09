namespace OJS.Services.Administration.Data.Implementations
{
    using OJS.Data;
    using OJS.Data.Models.Problems;
    using OJS.Services.Common.Models.Users;
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using OJS.Data.Models.Resources;

    public class ProblemResourcesDataService : AdministrationDataService<ProblemResource>, IProblemResourcesDataService
    {
        public ProblemResourcesDataService(OjsDbContext problemResources)
            : base(problemResources)
        {
        }

        public IQueryable<ProblemResource> GetByProblemQuery(int problemId)
            => this.GetQuery(pr => pr.ProblemId == problemId && !pr.IsDeleted);

        public void DeleteByProblem(int problemId)
            => this.Delete(pr => pr.ProblemId == problemId);

        protected override Expression<Func<ProblemResource, bool>> GetUserFilter(UserInfoModel user)
            => pr => user.IsAdmin ||
                     pr.Problem.ProblemGroup.Contest.Category!.LecturersInContestCategories.Any(cc => cc.LecturerId == user.Id) ||
                          pr.Problem.ProblemGroup.Contest.LecturersInContests.Any(l => l.LecturerId == user.Id);
    }
}