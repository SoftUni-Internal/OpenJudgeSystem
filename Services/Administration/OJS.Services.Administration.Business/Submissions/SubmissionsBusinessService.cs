namespace OJS.Services.Administration.Business.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using OJS.Common;
    using OJS.Common.Helpers;
    using OJS.Services.Common;
    using OJS.Services.Common.Data;
    using OJS.Services.Common.Models;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Administration.Data;
    using OJS.Services.Administration.Models;
    using OJS.Services.Administration.Models.Submissions;
    using OJS.Services.Infrastructure;
    using OJS.Services.Infrastructure.Exceptions;
    using OJS.Workers.Common.Models;
    using OJS.Services.Infrastructure.Extensions;
    using OJS.Data;

    public class SubmissionsBusinessService : AdministrationOperationService<Submission, int, SubmissionAdministrationServiceModel>, ISubmissionsBusinessService
    {
        private readonly IParticipantScoresBusinessService participantScoresBusiness;
        private readonly ISubmissionsDataService submissionsData;
        private readonly ISubmissionsForProcessingCommonDataService submissionsForProcessingDataService;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly IParticipantScoresBusinessService participantScoresBusinessService;
        private readonly ISubmissionsCommonBusinessService submissionsCommonBusinessService;
        private readonly ISubmissionsForProcessingCommonDataService submissionsForProcessingData;
        private readonly ITestRunsDataService testRunsData;
        private readonly ITransactionsProvider transactions;
        private readonly IDatesService dates;
        private readonly ITestRunsDataService testRunsDataService;

        public SubmissionsBusinessService(
            ISubmissionsDataService submissionsData,
            IParticipantScoresBusinessService participantScoresBusiness,
            IParticipantScoresDataService participantScoresData,
            ITransactionsProvider transactions,
            ISubmissionsForProcessingCommonDataService submissionsForProcessingDataService,
            IParticipantScoresBusinessService participantScoresBusinessService,
            ISubmissionsForProcessingCommonDataService submissionsForProcessingData,
            ITestRunsDataService testRunsData,
            IDatesService dates,
            ISubmissionsCommonBusinessService submissionsCommonBusinessService,
            ITestRunsDataService testRunsDataService)
        {
            this.submissionsData = submissionsData;
            this.participantScoresData = participantScoresData;
            this.transactions = transactions;
            this.submissionsForProcessingDataService = submissionsForProcessingDataService;
            this.participantScoresBusinessService = participantScoresBusinessService;
            this.dates = dates;
            this.submissionsCommonBusinessService = submissionsCommonBusinessService;
            this.testRunsDataService = testRunsDataService;
            this.participantScoresBusiness = participantScoresBusiness;
            this.submissionsForProcessingData = submissionsForProcessingData;
            this.testRunsData = testRunsData;
        }

        public Task<IQueryable<Submission>> GetAllForArchiving()
        {
            var archiveBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.BestSubmissionEligibleForArchiveAgeInYears);

            var archiveNonBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.NonBestSubmissionEligibleForArchiveAgeInYears);

            return Task.FromResult(this.submissionsData
                .GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
                    archiveBestSubmissionsLimit,
                    archiveNonBestSubmissionsLimit));
        }

        public Task RecalculatePointsByProblem(int problemId)
        {
            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                var problemSubmissions = this.submissionsData
                    .GetAllByProblem(problemId)
                    .Include(s => s.TestRuns)
                    .Include(s => s.TestRuns.Select(tr => tr.Test))
                    .ToList();

                var submissionResults = problemSubmissions
                    .Select(s => new
                    {
                        s.Id,
                        s.ParticipantId,
                        CorrectTestRuns = s.TestRuns.Count(t =>
                            t.ResultType == (int)TestRunResultType.CorrectAnswer &&
                            !t.Test.IsTrialTest),
                        AllTestRuns = s.TestRuns.Count(t => !t.Test.IsTrialTest),
                        MaxPoints = s.Problem!.MaximumPoints,
                    })
                    .ToList();

                var problemSubmissionsById = problemSubmissions.ToDictionary(s => s.Id);
                var topResults = new Dictionary<int, ParticipantScoreModel>();

                foreach (var submissionResult in submissionResults)
                {
                    var submission = problemSubmissionsById[submissionResult.Id];
                    var points = 0;
                    if (submissionResult.AllTestRuns != 0)
                    {
                        points = (submissionResult.CorrectTestRuns * submissionResult.MaxPoints) /
                            submissionResult.AllTestRuns;
                    }

                    submission.Points = points;
                    submission.CacheTestRuns();

                    var participantId = submissionResult.ParticipantId;

                    if (!topResults.ContainsKey(participantId) || topResults[participantId].Points < points)
                    {
                        topResults[participantId] = new ParticipantScoreModel
                        {
                            Points = points,
                            SubmissionId = submission.Id,
                        };
                    }
                    else if (topResults[participantId].Points == points)
                    {
                        if (topResults[participantId].SubmissionId < submission.Id)
                        {
                            topResults[participantId].SubmissionId = submission.Id;
                        }
                    }
                }

                this.submissionsData.SaveChanges();

                var participants = topResults.Keys.ToList();

                var existingScores = this.participantScoresData
                    .GetAllByProblem(problemId)
                    .Where(p => participants.Contains(p.ParticipantId))
                    .ToList();

                foreach (var existingScore in existingScores)
                {
                    var topScore = topResults[existingScore.ParticipantId];

                    existingScore.Points = topScore.Points;
                    existingScore.SubmissionId = topScore.SubmissionId;
                }

                this.submissionsData.SaveChanges();

                scope.Complete();
            }

            return Task.CompletedTask;
        }

        public async Task<ServiceResult> Retest(Submission submission)
        {
            var submissionProblemId = submission.ProblemId;
            var submissionParticipantId = submission.ParticipantId;
            var submissionServiceModel = this.submissionsCommonBusinessService.BuildSubmissionForProcessing(submission);
            SubmissionForProcessing submissionForProcessing = new();

            var result = await this.transactions.ExecuteInTransaction(async () =>
            {
                submission.Processed = false;
                submission.ModifiedOn = this.dates.GetUtcNow();
                this.submissionsData.Update(submission);

                var submissionIsBestSubmission = await this.IsBestSubmission(
                    submissionProblemId,
                    submissionParticipantId,
                    submission.Id);

                if (submissionIsBestSubmission)
                {
                    await this.participantScoresBusinessService.RecalculateForParticipantByProblem(
                        submissionParticipantId,
                        submissionProblemId);
                }

                await this.testRunsDataService.DeleteBySubmission(submission.Id);

                submissionForProcessing = await this.submissionsForProcessingDataService.AddOrUpdate(submission.Id);
                await this.submissionsData.SaveChanges();

                return ServiceResult.Success;
            });

            await this.submissionsCommonBusinessService.PublishSubmissionForProcessing(submissionServiceModel, submissionForProcessing);

            return result;
        }

        public async Task<ServiceResult> Retest(int id)
        {
            var submission = this.submissionsData.GetByIdQuery(id)
                .Include(s => s.SubmissionType!)
                .Include(s => s.Problem)
                    .ThenInclude(p => p.Checker)
                .Include(s => s.Problem)
                    .ThenInclude(p => p.Tests)
                .Include(s => s.Problem)
                .ThenInclude(p => p.SubmissionTypesInProblems)
                .FirstOrDefault();

            if (submission == null || submission.Id == 0)
            {
                return new ServiceResult("Submission doesn't exist");
            }

            return await this.Retest(submission!);
        }

        public override async Task Delete(int id)
        {
            var submission = await this.submissionsData
                .GetByIdQuery(id)
                .FirstOrDefaultAsync();

            if (submission == null)
            {
                throw new BusinessServiceException($"Submission with Id:{id} not found.");
            }

            await this.transactions.ExecuteInTransaction(async () =>
            {
                await this.testRunsData.DeleteBySubmission(submission.Id);
                await this.submissionsData.DeleteById(submission.Id);
                await this.submissionsData.SaveChanges();
                await this.submissionsForProcessingData.RemoveBySubmission(submission.Id);
            });
        }

        public async Task<SubmissionAdministrationServiceModel> Download(int id)
        {
            if (id <= 0)
            {
                throw new BusinessServiceException(Resources.SubmissionsController.SubmissionNotFound);
            }

            var submission = await this.submissionsData
                .GetByIdQuery(id)
                .FirstOrDefaultAsync();

            if (submission == null)
            {
                throw new BusinessServiceException(Resources.SubmissionsController.SubmissionNotFound);
            }

            if (!submission.IsBinaryFile)
            {
                throw new BusinessServiceException(Resources.SubmissionsController.SubmissionNotFileUpload);
            }

            return submission.Map<SubmissionAdministrationServiceModel>();
        }

        public async Task<bool> IsBestSubmission(int problemId, int participantId, int submissionId)
        {
            var bestScore = await this.participantScoresData.GetByParticipantIdAndProblemId(participantId, problemId);

            return bestScore?.SubmissionId == submissionId;
        }
    }
}