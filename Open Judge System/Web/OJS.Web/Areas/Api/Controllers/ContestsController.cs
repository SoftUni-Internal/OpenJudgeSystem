namespace OJS.Web.Areas.Api.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using Newtonsoft.Json;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Infrastructure.Filters.Attributes;

    [ValidateRemoteDataApiKey]
    public class ContestsController : ApiController
    {
        private readonly IOjsDbContext dbContext;

        public ContestsController(IOjsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ActionResult Export(int id)
        {
            this.dbContext.DbContext.Configuration.LazyLoadingEnabled = false;
            this.dbContext.DbContext.Configuration.ProxyCreationEnabled = false;

            var contest = this.dbContext.Contests
                .Where(c => !c.IsDeleted)
                .Include(c => c.ProblemGroups)
                .Include(c => c.ProblemGroups.Select(pg => pg.Problems))
                .Include(c => c.ProblemGroups.Select(pg => pg.Problems.Select(p => p.Tests)))
                .Include(c => c.ProblemGroups.Select(pg => pg.Problems.Select(p => p.Checker)))
                .Include(c => c.ProblemGroups.Select(pg => pg.Problems.Select(p => p.SubmissionTypes)))
                .Include(c => c.ProblemGroups.Select(pg => pg.Problems.Select(p => p.ProblemSubmissionTypeExecutionDetails)))
                .Include(c => c.ProblemGroups.Select(pg => pg.Problems.Select(p => p.Resources)))
                .FirstOrDefault(c => c.Id == id);

            contest.ProblemGroups = contest.ProblemGroups
                .Where(pg => !pg.IsDeleted)
                .Select(pg =>
                {
                    pg.Problems = pg.Problems
                        .Where(p => !p.IsDeleted)
                        .Select(p =>
                        {
                            p.Resources = p.Resources.Where(r => !r.IsDeleted).ToList();
                            return p;
                        }).ToList();
                    return pg;
                })
                .ToList();

            if (contest == null)
            {
                return this.HttpNotFound($"Contest with id {id} not found.");
            }

            RemoveCircularReferences(contest);

            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
            };

            return this.Content(JsonConvert.SerializeObject(contest, jsonSettings));
        }

        public ActionResult GetExistingIds(IEnumerable<int> ids)
        {
            var existingContestIds = this.dbContext.Contests
                .Where(c => !c.IsDeleted && ids.Contains(c.Id))
                .Select(c => c.Id)
                .ToList();

            return this.Content(JsonConvert.SerializeObject(existingContestIds));
        }

        public ActionResult GetExistingIdsForCategory(int contestCategoryId)
        {
            var existingContestIds = this.dbContext.Contests
                .Where(c => !c.IsDeleted && c.CategoryId == contestCategoryId)
                .Select(c => c.Id)
                .ToList();

            return this.Content(JsonConvert.SerializeObject(existingContestIds));
        }

        private static void RemoveCircularReferences(Contest contest)
        {
            foreach (var problemGroup in contest.ProblemGroups)
            {
                problemGroup.Contest = null;

                foreach (var problem in problemGroup.Problems)
                {
                    problem.ProblemGroup = null;

                    foreach (var test in problem.Tests)
                    {
                        test.Problem = null;
                    }

                    foreach (var resource in problem.Resources)
                    {
                        resource.Problem = null;
                    }

                    foreach (var submissionType in problem.SubmissionTypes)
                    {
                        submissionType.Problems = null;
                    }

                    foreach (var executionDetail in problem.ProblemSubmissionTypeExecutionDetails)
                    {
                        executionDetail.Problem = null;
                    }
                }
            }
        }
    }
}