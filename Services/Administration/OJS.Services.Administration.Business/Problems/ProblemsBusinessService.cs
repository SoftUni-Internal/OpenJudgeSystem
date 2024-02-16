namespace OJS.Services.Administration.Business.Problems
{
    using FluentExtensions.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using OJS.Common;
    using OJS.Common.Enumerations;
    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Data.Models.Problems;
    using OJS.Services.Administration.Business.Contests;
    using OJS.Services.Administration.Business.ProblemGroups;
    using OJS.Services.Administration.Data;
    using OJS.Services.Administration.Models.Contests.Problems;
    using OJS.Services.Administration.Models.Problems;
    using OJS.Services.Common;
    using OJS.Services.Common.Data;
    using OJS.Services.Common.Models;
    using OJS.Services.Common.Models.Submissions.ExecutionContext;
    using OJS.Services.Infrastructure.Exceptions;
    using SoftUni.AutoMapper.Infrastructure.Extensions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Transactions;
    using IsolationLevel = System.Transactions.IsolationLevel;
    using Resource = OJS.Common.Resources.ProblemsBusiness;
    using SharedResource = OJS.Common.Resources.ContestsGeneral;

    public class ProblemsBusinessService : AdministrationOperationService<Problem, int, ProblemAdministrationModel>, IProblemsBusinessService
    {
        private readonly IContestsDataService contestsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly IProblemsDataService problemsData;
        private readonly IProblemResourcesDataService problemResourcesData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly ISubmissionsForProcessingCommonDataService submissionsForProcessingData;
        private readonly ITestRunsDataService testRunsData;
        private readonly ISubmissionTypesDataService submissionTypesData;
        private readonly IProblemGroupsBusinessService problemGroupsBusiness;
        private readonly IContestsBusinessService contestsBusiness;
        private readonly ISubmissionsCommonBusinessService submissionsCommonBusinessService;
        private readonly IProblemGroupsDataService problemGroupsDataService;
        private readonly IZippedTestsParserService zippedTestsParser;

        public ProblemsBusinessService(
            IContestsDataService contestsData,
            IParticipantScoresDataService participantScoresData,
            IProblemsDataService problemsData,
            IProblemResourcesDataService problemResourcesData,
            ISubmissionsDataService submissionsData,
            ISubmissionsForProcessingCommonDataService submissionsForProcessingData,
            ITestRunsDataService testRunsData,
            ISubmissionTypesDataService submissionTypesData,
            IProblemGroupsBusinessService problemGroupsBusiness,
            IContestsBusinessService contestsBusiness,
            ISubmissionsCommonBusinessService submissionsCommonBusinessService,
            IProblemGroupsDataService problemGroupsDataService,
            IZippedTestsParserService zippedTestsParser)
        {
            this.contestsData = contestsData;
            this.participantScoresData = participantScoresData;
            this.problemsData = problemsData;
            this.problemResourcesData = problemResourcesData;
            this.submissionsData = submissionsData;
            this.submissionsForProcessingData = submissionsForProcessingData;
            this.testRunsData = testRunsData;
            this.submissionTypesData = submissionTypesData;
            this.problemGroupsBusiness = problemGroupsBusiness;
            this.contestsBusiness = contestsBusiness;
            this.submissionsCommonBusinessService = submissionsCommonBusinessService;
            this.problemGroupsDataService = problemGroupsDataService;
            this.zippedTestsParser = zippedTestsParser;
        }

        public override async Task<ProblemAdministrationModel> Create(ProblemAdministrationModel model)
        {
            var contestId = model.ContestId;

            var problem = model.Map<Problem>();
            if (problem.ProblemGroupId == default)
            {
                var isValidGroupType = Enum.TryParse(model.ProblemGroupType, out ProblemGroupType problemGroupType);

                problem.ProblemGroup = new ProblemGroup
                {
                    ContestId = contestId,
                    OrderBy = problem.OrderBy,
                    Type = isValidGroupType ? problemGroupType : null,
                };
            }

            await this.problemsData.Add(problem);

            AddSubmissionTypes(problem, model);
            await this.TryAddTestsToProblem(problem, model.Tests);

            await this.problemsData.SaveChanges();

            return model;
        }

        public override async Task Delete(int id)
        {
            var problem = this.problemsData
                .GetByIdQuery(id)
                .Select(p => new { p.ProblemGroupId, p.ProblemGroup.ContestId, p.IsDeleted, })
                .FirstOrDefault();

            if (problem == null || problem.IsDeleted)
            {
                return;
            }

            using var scope = TransactionsHelper.CreateTransactionScope(
                IsolationLevel.RepeatableRead,
                TransactionScopeAsyncFlowOption.Enabled);
            if (!await this.contestsData.IsOnlineById(problem.ContestId))
            {
                await this.problemGroupsBusiness.DeleteById(problem.ProblemGroupId);
            }

            await this.problemsData.DeleteById(id);
            await this.problemsData.SaveChanges();
            await this.testRunsData.DeleteByProblem(id);

            this.problemResourcesData.DeleteByProblem(id);

            this.submissionsData.DeleteByProblem(id);

            scope.Complete();
        }

        public async Task DeleteByContest(int contestId) =>
            await this.problemsData
                .GetAllByContest(contestId)
                .Select(p => p.Id)
                .ToList()
                .ForEachSequential(async id => await this.Delete(id));

        public async Task<ServiceResult> CopyToContestByIdByContestAndProblemGroup(int id, int contestId, int? problemGroupId)
        {
            var problem = await this.problemsData
                .GetByIdQuery(id)
                .AsNoTracking()
                .Include(p => p.Tests)
                .Include(p => p.Resources)
                .Include(p => p.ProblemGroup)
                .SingleOrDefaultAsync();

            if (problem?.ProblemGroup.ContestId == contestId)
            {
                return new ServiceResult(Resource.CannotCopyProblemsIntoSameContest);
            }

            if (!await this.contestsData.ExistsById(contestId))
            {
                return new ServiceResult(SharedResource.ContestNotFound);
            }

            if (await this.contestsData.IsActiveById(contestId))
            {
                return new ServiceResult(Resource.CannotCopyProblemsIntoActiveContest);
            }

            await this.CopyProblemToContest(problem, contestId, problemGroupId);

            return ServiceResult.Success;
        }

        public async Task<bool> UserHasProblemPermissions(int problemId, string? userId, bool isUserAdmin)
        {
            var problem = await this.problemsData.OneByIdTo<ProblemShortDetailsServiceModel>(problemId);

            if (problem == null)
            {
                throw new BusinessServiceException("Problem cannot be null");
            }

            return await this.contestsBusiness.UserHasContestPermissions(problem.ContestId, userId, isUserAdmin);
        }

        public Task ReevaluateProblemsOrder(int contestId, Problem problem)
            => this.problemGroupsBusiness.ReevaluateProblemsAndProblemGroupsOrder(contestId, problem.ProblemGroup);

        public async Task<AdditionalFilesDownloadModel?> GetAdditionalFiles(int problemId)
        {
            var hasProblem = await this.problemsData.ExistsById(problemId);
            if (!hasProblem)
            {
                throw new BusinessServiceException("Problem not found.");
            }

            var file = await this.problemsData.GetByIdQuery(problemId).Select(p => p.AdditionalFiles)
                .FirstOrDefaultAsync();
            if (file == null)
            {
                return null;
            }

            return new AdditionalFilesDownloadModel
            {
                Content = file!,
                MimeType = GlobalConstants.MimeTypes.ApplicationOctetStream,
                FileName = string.Format($"Problem-{problemId}-Additional files.zip"),
            };
        }

        public override async Task<ProblemAdministrationModel> Get(int id)
        {
            var problem = await this.problemsData.GetByIdQuery(id)
                .Include(stp => stp.SubmissionTypesInProblems)
                .ThenInclude(stp => stp.SubmissionType)
                .Include(p => p.ProblemGroup)
                .FirstOrDefaultAsync();
            if (problem is null)
            {
                throw new InvalidOperationException();
            }

            return problem.Map<ProblemAdministrationModel>();
        }

        public override async Task<ProblemAdministrationModel> Edit(ProblemAdministrationModel model)
        {
            if (model.Id < 0)
            {
                throw new ArgumentNullException("Problem must have an Id");
            }

            var problem = await this.problemsData.GetByIdQuery(model.Id)
                .Include(s => s.SubmissionTypesInProblems)
                .Include(s => s.ProblemGroup)
                .FirstOrDefaultAsync();

            if (problem is null)
            {
                throw new ArgumentNullException($"Problem with id {model.Id} not found");
            }

            problem.MapFrom(model);
            problem.ProblemGroup = this.problemGroupsDataService.GetByProblem(problem.Id)!;
            problem.ProblemGroup.Type = (ProblemGroupType)Enum.Parse(typeof(ProblemGroupType), model.ProblemGroupType!);

            if (!problem.ProblemGroup.Contest.IsOnlineExam)
            {
                problem.ProblemGroup.OrderBy = model.OrderBy;
            }

            AddSubmissionTypes(problem, model);

            this.problemsData.Update(problem);
            await this.problemsData.SaveChanges();
            return model;
        }

        public async Task RetestById(int id)
        {
            var submissions = await this.submissionsData.GetAllNonDeletedByProblemId<SubmissionServiceModel>(id);

            var submissionIds = submissions.Select(s => s.Id).ToList();

            using (var scope = TransactionsHelper.CreateTransactionScope(
                       IsolationLevel.RepeatableRead,
                       TransactionScopeAsyncFlowOption.Enabled))
            {
                await this.testRunsData.DeleteInBatchesBySubmissionIds(submissionIds);

                await this.participantScoresData.DeleteAllByProblem(id);

                await this.submissionsData.SetAllToUnprocessedByProblem(id);

                scope.Complete();
            }

            await this.submissionsCommonBusinessService.PublishSubmissionsForProcessing(submissions);
        }

        private static void AddSubmissionTypes(Problem problem, ProblemAdministrationModel model)
        {
            var newSubmissionTypes = model.SubmissionTypes
                .Select(x => new SubmissionTypeInProblem
                {
                    ProblemId = problem.Id,
                    SubmissionTypeId = x.Id,
                    SolutionSkeleton = x.SolutionSkeleton != null
                        ? x.SolutionSkeleton.ToString().Compress()
                        : Array.Empty<byte>(),
                });

            problem.SubmissionTypesInProblems = new HashSet<SubmissionTypeInProblem>(newSubmissionTypes);
        }

        private async Task CopyProblemToContest(Problem? problem, int contestId, int? problemGroupId)
        {
            double orderBy;

            if (problem == null)
            {
                return;
            }

            if (problemGroupId.HasValue)
            {
                orderBy = await this.problemsData.GetNewOrderByProblemGroup(problemGroupId.Value);

                problem.ProblemGroup = null!;
                problem.ProblemGroupId = problemGroupId.Value;
            }
            else
            {
                orderBy = await this.problemsData.GetNewOrderByContest(contestId);

                problem.ProblemGroup = new ProblemGroup
                {
                    ContestId = contestId,
                    OrderBy = orderBy,
                };
            }

            problem.OrderBy = orderBy;
            problem.SubmissionTypesInProblems = await this.submissionTypesData
                .GetAllByProblem(problem.Id)
                .SelectMany(x => x.SubmissionTypesInProblems)
                .ToListAsync();

            await this.problemsData.Add(problem);
            await this.problemsData.SaveChanges();
        }

        private async Task TryAddTestsToProblem(Problem problem, IFormFile? tests)
        {
            if (tests == null)
            {
                return;
            }

            try
            {
                await this.AddTestsToProblem(problem, tests);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(string.Format(ex.Message), ex);
            }
        }

        private async Task AddTestsToProblem(Problem problem, IFormFile testsFile)
        {
            await using var memoryStream = new MemoryStream();
            await testsFile.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var parsedTests = await this.zippedTestsParser.Parse(memoryStream);

            if (!this.zippedTestsParser.AreTestsParsedCorrectly(parsedTests))
            {
                throw new ArgumentException("Invalid tests");
            }

            this.zippedTestsParser.AddTestsToProblem(problem, parsedTests);
        }
    }
}