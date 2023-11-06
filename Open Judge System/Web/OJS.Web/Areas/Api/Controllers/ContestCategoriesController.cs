namespace OJS.Web.Areas.Api.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using Newtonsoft.Json;
    using OJS.Data;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Infrastructure.Filters.Attributes;

    [ValidateRemoteDataApiKey]
    public class ContestCategoriesController : ApiController
    {
        private readonly OjsDbContext dbContext;

        public ContestCategoriesController(OjsDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ActionResult ExportData(int id)
        {
            this.dbContext.Configuration.LazyLoadingEnabled = false;
            
            var contestCategory = this.dbContext.ContestCategories
                .Include(x => x.Contests)
                .Include(x => x.Contests.Select(
                        contest => contest.ProblemGroups))
                .Include(x => x.Contests.Select(
                    contest => contest.ProblemGroups.Select(
                        group => group.Contest)))
                .Include(x => x.Contests.Select(
                    contest => contest.ProblemGroups.Select(
                        group => group.Problems)))
                .Include(x => x.Contests.Select(
                    contest => contest.ProblemGroups.Select(
                        group => group.Problems.Select(problem => problem.Checker))))
                .Include(x => x.Contests.Select(
                    contest => contest.ProblemGroups.Select(
                        group => group.Problems.Select(problem => problem.Tests))))
                .Include(x => x.Contests.Select(
                    contest => contest.ProblemGroups.Select(
                        group => group.Problems.Select(problem => problem.SubmissionTypes))))
                .Include(x => x.Contests.Select(
                    contest => contest.ProblemGroups.Select(
                        group => group.Problems.Select(problem => problem.ProblemSubmissionTypeExecutionDetails))))
                .Where(x => x.Id == id)
                .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.IsVisible,
                        x.OrderBy,
                        x.IsDeleted,
                        x.DeletedOn,
                        Children = x.Children.Select(y => new { y.Id }),
                        Contests = x.Contests.Select(contest => new
                        {
                            contest.Name,
                            contest.IsVisible,
                            contest.VisibleFrom,
                            contest.AutoChangeTestsFeedbackVisibility,
                            contest.Type,
                            contest.Duration,
                            contest.StartTime,
                            contest.EndTime,
                            contest.ContestPassword,
                            contest.PracticePassword,
                            contest.NewIpPassword,
                            contest.PracticeStartTime,
                            contest.PracticeEndTime,
                            contest.LimitBetweenSubmissions,
                            contest.OrderBy,
                            contest.NumberOfProblemGroups,
                            contest.Description,
                            contest.UsersCantSubmitConcurrently,
                            contest.EnsureValidAuthorSubmisions,
                            contest.AllowedIps,
                            contest.IsDeleted,
                            contest.DeletedOn,
                            ProblemGroups = contest.ProblemGroups.Select(group => new
                            {
                                group.Id,
                                group.OrderBy,
                                group.Type,
                                group.IsDeleted,
                                group.DeletedOn,
                                Problems = group.Problems.Select(problem => new
                                {
                                    problem.Id,
                                    problem.Name,
                                    problem.MaximumPoints,
                                    problem.TimeLimit,
                                    problem.MemoryLimit,
                                    problem.SourceCodeSizeLimit,
                                    problem.OrderBy,
                                    problem.SolutionSkeleton,
                                    problem.AdditionalFiles,
                                    problem.ShowResults,
                                    problem.ShowDetailedFeedback,
                                    problem.IsDeleted,
                                    problem.DeletedOn,
                                    Checker = new
                                    {
                                        problem.Checker.Id,
                                        problem.Checker.Name,
                                    },
                                    Tests = problem.Tests.Select(test => new
                                    {
                                        test.Id,
                                        test.IsTrialTest,
                                        test.IsOpenTest,
                                        test.HideInput,
                                        test.OrderBy,
                                        test.InputData,
                                        test.OutputData
                                    }),
                                    SubmissionTypes = problem.SubmissionTypes.Select(submissionType => new
                                    {
                                        submissionType.Id,
                                        submissionType.Name,
                                    }),
                                    ProblemSubmissionTypesSkeletons =
                                        problem.ProblemSubmissionTypeExecutionDetails.Select(
                                            skeleton => new
                                            {
                                                skeleton.SolutionSkeleton,
                                                SubmissionType = new
                                                {
                                                    skeleton.SubmissionType.Name,
                                                    skeleton.SubmissionType.Id
                                                },
                                            }),
                                    Resources = problem.Resources.Select(resource => new
                                    {
                                        resource.Id,
                                        resource.Name,
                                        resource.Type,
                                        resource.File,
                                        resource.FileExtension,
                                        resource.Link,
                                        resource.OrderBy,
                                        resource.IsDeleted,
                                        resource.DeletedOn,
                                    })
                                })
                            })
                        })
                    })
                .ToList()
                .FirstOrDefault();

            if (contestCategory == null)
            {
                return this.JsonError($"Contest Category with id: {id} is not found!");
            }

            return this.Content(JsonConvert.SerializeObject(contestCategory));
        }
    }
}