namespace OJS.Services.Administration.Business.ProblemGroups
{
    using FluentExtensions.Extensions;
    using Microsoft.EntityFrameworkCore;
    using OJS.Data.Models;
    using OJS.Data.Models.Problems;
    using OJS.Data.Models.Tests;
    using OJS.Services.Administration.Data;
    using OJS.Services.Administration.Models.ProblemGroups;
    using OJS.Services.Common.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Resource = OJS.Common.Resources.ProblemGroupsBusiness;
    using SharedResource = OJS.Common.Resources.ContestsGeneral;
    using OJS.Services.Infrastructure.Extensions;
    using OJS.Services.Infrastructure.Exceptions;

    public class ProblemGroupsBusinessService : AdministrationOperationService<ProblemGroup, int, ProblemGroupsAdministrationModel>, IProblemGroupsBusinessService
    {
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IContestsDataService contestsData;
        private readonly IProblemsDataService problemsData;
        private readonly ISubmissionTypesInProblemsDataService submissionTypesInProblemsData;
        private readonly IProblemResourcesDataService problemResourcesData;
        private readonly ITestsDataService testsData;
        private readonly IOrderableService<ProblemGroup> problemGroupsOrderableService;
        private readonly IOrderableService<Problem> problemsOrderableService;

        public ProblemGroupsBusinessService(
            IProblemGroupsDataService problemGroupsData,
            IContestsDataService contestsData,
            IProblemsDataService problemsData,
            IProblemResourcesDataService problemResourcesData,
            ISubmissionTypesInProblemsDataService submissionTypesInProblemsData,
            IOrderableService<Problem> problemsOrderableService,
            IOrderableService<ProblemGroup> problemGroupsOrderableService,
            ITestsDataService testsData)
        {
            this.problemGroupsData = problemGroupsData;
            this.contestsData = contestsData;
            this.submissionTypesInProblemsData = submissionTypesInProblemsData;
            this.problemsData = problemsData;
            this.problemsOrderableService = problemsOrderableService;
            this.problemGroupsOrderableService = problemGroupsOrderableService;
            this.testsData = testsData;
            this.problemResourcesData = problemResourcesData;
        }

        public override async Task<ProblemGroupsAdministrationModel> Get(int id)
        {
            var problemGroup = await this.problemGroupsData
                .GetByIdQuery(id)
                .Include(pg => pg.Contest)
                .FirstOrDefaultAsync();
            if (problemGroup == null)
            {
                throw new BusinessServiceException($"Problem group with id: {id} not found.");
            }

            return problemGroup.Map<ProblemGroupsAdministrationModel>();
        }

        public override async Task<ProblemGroupsAdministrationModel> Create(ProblemGroupsAdministrationModel model)
        {
            var problemGroup = model.Map<ProblemGroup>();

            await this.problemGroupsData.Add(problemGroup);

            await this.problemGroupsData.SaveChanges();

            return model;
        }

        public override async Task<ProblemGroupsAdministrationModel> Edit(ProblemGroupsAdministrationModel model)
        {
            var problemGroup = model.Map<ProblemGroup>();

            this.problemGroupsData.Update(problemGroup);

            await this.problemGroupsData.SaveChanges();

            return model;
        }

        public override async Task Delete(int id)
        {
            await this.problemGroupsData.DeleteById(id);

            await this.problemGroupsData.SaveChanges();
        }

        public async Task<ServiceResult> DeleteById(int id)
        {
            var problemGroup = await this.problemGroupsData.GetByIdQuery(id)
                .Include(p => p.Problems)
                .FirstOrDefaultAsync();

            if (problemGroup != null && !problemGroup.IsDeleted)
            {
                if (problemGroup.Problems.Any(p => !p.IsDeleted))
                {
                    return new ServiceResult(Resource.CannotDeleteProblemGroupWithProblems);
                }

                this.problemGroupsData.Delete(problemGroup);
                await this.problemGroupsData.SaveChanges();
            }

            return ServiceResult.Success;
        }

        public async Task<ServiceResult> CopyAllToContestBySourceAndDestinationContest(
            int sourceContestId,
            int destinationContestId)
        {
            if (sourceContestId == destinationContestId)
            {
                return new ServiceResult(Resource.CannotCopyProblemGroupsIntoSameContest);
            }

            if (!await this.contestsData.ExistsById(destinationContestId))
            {
                return new ServiceResult(SharedResource.ContestNotFound);
            }

            if (await this.contestsData.IsActiveById(destinationContestId))
            {
                return new ServiceResult(Resource.CannotCopyProblemGroupsIntoActiveContest);
            }

            var sourceContestProblemGroups = await this.problemGroupsData
                .GetAllByContestId(sourceContestId)
                .Include(pg => pg.Problems)
                    .ThenInclude(p => p.Tests)
                .Include(pg => pg.Problems)
                    .ThenInclude(p => p.Resources)
                .Include(pr => pr.Problems)
                    .ThenInclude(p => p.Checker)
                .ToListAsync();

            await sourceContestProblemGroups
                .ForEachSequential(async pg => await this.CopyProblemGroupToContest(pg, destinationContestId));

            return ServiceResult.Success;
        }

        public async Task ReevaluateProblemsAndProblemGroupsOrder(int contestId)
        {
            var problemGroups = this.problemGroupsData
                .GetAllByContestId(contestId)
                .Include(pr => pr.Problems);

            await this.problemGroupsOrderableService.ReevaluateOrder(problemGroups);

            // Assign new OrderBy values to the problems in the contest, in order to avoid duplicate values,
            // and ensure every problem is in the correct order, depending on its group order first.
            var problems = problemGroups.SelectMany(p => p.Problems)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.ProblemGroup.OrderBy)
                .ThenBy(p => p.OrderBy)
                .Mutate((p, i) => p.OrderBy = i);

            await this.problemsOrderableService.ReevaluateOrder(problems);
        }

        public ICollection<ProblemGroupDropdownModel> GetOrderByContestId(int contestId)
            => this.problemGroupsData.GetAllByContest(contestId)
                .OrderBy(x => x.OrderBy)
                .MapCollection<ProblemGroupDropdownModel>()
                .ToHashSet();

        public async Task GenerateNewProblem(
            Problem problem,
            ProblemGroup currentNewProblemGroup,
            ICollection<Problem> problemsToAdd)
        {
            var currentNewProblem = new Problem
            {
                ProblemGroupId = currentNewProblemGroup.Id,
                ProblemGroup = currentNewProblemGroup,
                Name = problem.Name,
                MaximumPoints = problem.MaximumPoints,
                TimeLimit = problem.MaximumPoints,
                MemoryLimit = problem.MemoryLimit,
                SourceCodeSizeLimit = problem.SourceCodeSizeLimit,
                CheckerId = problem.CheckerId,
                Checker = problem.Checker,
                OrderBy = problem.OrderBy,
                SolutionSkeleton = problem.SolutionSkeleton,
                ShowResults = problem.ShowResults,
                ShowDetailedFeedback = problem.ShowDetailedFeedback,
            };

            await this.problemsData.Add(currentNewProblem);
            await this.problemsData.SaveChanges();

            var newSubmissionTypeInSourceProblemsToAdd = new List<SubmissionTypeInProblem>();
            var newResourcesToAdd = new List<ProblemResource>();
            var newTestsToAdd = new List<Test>();

            await this.GenerateNewTests(problem.Id, newTestsToAdd, currentNewProblem);

            await this.GenerateNewResources(problem.Id, newResourcesToAdd, currentNewProblem);

            this.submissionTypesInProblemsData
                .GetAllByProblem(problem.Id)
                .AsNoTracking()
                .ForEach(stp =>
                    this.GenerateNewSubmissionTypesInProblem(stp, newSubmissionTypeInSourceProblemsToAdd, currentNewProblem));

            currentNewProblem.Resources = newResourcesToAdd;
            currentNewProblem.Tests = newTestsToAdd;
            currentNewProblem.SubmissionTypesInProblems = newSubmissionTypeInSourceProblemsToAdd;
            problemsToAdd.Add(currentNewProblem);

            this.problemsData.Update(currentNewProblem);
        }

        private async Task CopyProblemGroupToContest(ProblemGroup problemGroup, int contestId)
        {
            var currentNewProblemGroup = new ProblemGroup
            {
                ContestId = contestId,
                OrderBy = problemGroup.OrderBy,
                Type = problemGroup.Type,
            };

            await this.problemGroupsData.Add(currentNewProblemGroup);
            await this.problemGroupsData.SaveChanges();

            var problemsToAdd = new List<Problem>();

            if (problemGroup.Problems.Count > 0)
            {
               await problemGroup.Problems
                    .Where(p => !p.IsDeleted)
                    .ToList()
                    .ForEachSequential(async problem =>
                       await this.GenerateNewProblem(problem, currentNewProblemGroup, problemsToAdd));

               currentNewProblemGroup.Problems = problemsToAdd;

               await this.submissionTypesInProblemsData.SaveChanges();
               await this.problemsData.SaveChanges();

               this.problemGroupsData.Update(currentNewProblemGroup);
               await this.problemGroupsData.SaveChanges();

               await this.ReevaluateProblemsAndProblemGroupsOrder(contestId);
            }
        }

        private async Task GenerateNewTests(
            int problemId,
            ICollection<Test> newTestsToAdd,
            Problem currentNewProblem)
        {
            var problemTests = await this.testsData
                .GetAllByProblem(problemId)
                .AsNoTracking()
                .ToListAsync();

            foreach (var test in problemTests)
            {
                var newTest = test;
                newTest.Id = 0;
                newTest.ProblemId = currentNewProblem.Id;
                newTest.Problem = currentNewProblem;

                await this.testsData.Add(newTest);
                newTestsToAdd.Add(newTest);
            }

            await this.testsData.SaveChanges();
        }

        private async Task GenerateNewResources(
            int problemId,
            ICollection<ProblemResource> newResourcesToAdd,
            Problem currentNewProblem)
        {
            var problemResources = await this.problemResourcesData
                .GetByProblemQuery(problemId)
                .AsNoTracking()
                .ToListAsync();

            foreach (var problemResource in problemResources)
            {
                var newResource = problemResource;
                newResource.Id = 0;
                newResource.ProblemId = currentNewProblem.Id;
                newResource.Problem = currentNewProblem;
                newResource.ModifiedOn = null;

                await this.problemResourcesData.Add(newResource);
                newResourcesToAdd.Add(newResource);
            }

            await this.problemResourcesData.SaveChanges();
        }

        private void GenerateNewSubmissionTypesInProblem(
            SubmissionTypeInProblem submissionTypeInProblem,
            ICollection<SubmissionTypeInProblem> submissionTypeInSourceProblems,
            Problem currentNewProblem)
        {
            var newSubmissionTypeInProblem = submissionTypeInProblem;
            newSubmissionTypeInProblem.ProblemId = currentNewProblem.Id;

            submissionTypeInSourceProblems.Add(newSubmissionTypeInProblem);

            this.submissionTypesInProblemsData.Add(newSubmissionTypeInProblem);
        }
    }
}
