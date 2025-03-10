namespace OJS.Services.Administration.Business.Submissions
{
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using OJS.Common;
    using OJS.Services.Common.Data;
    using OJS.Services.Common.Models;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Administration.Data;
    using OJS.Services.Administration.Models.Submissions;
    using OJS.Services.Infrastructure;
    using OJS.Services.Infrastructure.Exceptions;
    using OJS.Services.Infrastructure.Extensions;

    public class SubmissionsBusinessService : AdministrationOperationService<Submission, int, SubmissionAdministrationServiceModel>, ISubmissionsBusinessService
    {
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
            this.submissionsForProcessingData = submissionsForProcessingData;
            this.testRunsData = testRunsData;
        }

        public async Task<ServiceResult> Retest(Submission submission, bool verbosely)
        {
            var submissionProblemId = submission.ProblemId;
            var submissionParticipantId = submission.ParticipantId;
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

            var submissionServiceModel = this.submissionsCommonBusinessService.BuildSubmissionForProcessing(submission, verbosely);
            await this.submissionsCommonBusinessService.PublishSubmissionForProcessing(submissionServiceModel, submissionForProcessing);

            return result;
        }

        public async Task<ServiceResult> Retest(int id, bool verbosely = false)
        {
            var submission = await this.submissionsData.GetNonDeletedWithNonDeletedProblemTestsAndSubmissionTypes(id);
            if (submission == null || submission.Id == 0)
            {
                throw new BusinessServiceException("Submission doesn't exist or either the submission or problem is deleted.");
            }

            return await this.Retest(submission, verbosely);
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