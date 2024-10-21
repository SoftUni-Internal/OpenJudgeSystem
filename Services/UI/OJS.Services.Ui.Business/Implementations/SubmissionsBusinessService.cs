namespace OJS.Services.Ui.Business.Implementations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OJS.Common;
using OJS.Common.Enumerations;
using OJS.Common.Helpers;
using OJS.Data;
using OJS.Data.Models.Participants;
using OJS.Data.Models.Submissions;
using OJS.Data.Models.Tests;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Ui.Business.Validations.Implementations.Contests;
using OJS.Services.Ui.Business.Validations.Implementations.Submissions;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Participants;
using OJS.Services.Ui.Models.Submissions;
using OJS.Workers.Common.Models;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Cache;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static OJS.Services.Common.Constants.PaginationConstants.Submissions;
using static OJS.Services.Ui.Business.Constants.Comments;

public class SubmissionsBusinessService : ISubmissionsBusinessService
{
    private readonly ILogger<SubmissionsBusinessService> logger;
    private readonly ISubmissionsDataService submissionsData;
    private readonly ISubmissionsCommonDataService submissionsCommonData;
    private readonly ISubmissionsForProcessingCommonDataService submissionsForProcessingData;
    private readonly IUsersBusinessService usersBusiness;
    private readonly IParticipantScoresBusinessService participantScoresBusinessService;
    private readonly ISubmissionsCommonBusinessService submissionsCommonBusinessService;

    // TODO: https://github.com/SoftUni-Internal/exam-systems-issues/issues/624
    private readonly IParticipantsDataService participantsDataService;
    private readonly IProblemsDataService problemsDataService;
    private readonly IUserProviderService userProviderService;
    private readonly ILecturersInContestsBusinessService lecturersInContestsBusiness;
    private readonly ISubmissionDetailsValidationService submissionDetailsValidationService;
    private readonly ISubmitSubmissionValidationService submitSubmissionValidationService;
    private readonly ISubmissionResultsValidationService submissionResultsValidationService;
    private readonly ISubmissionFileDownloadValidationService submissionFileDownloadValidationService;
    private readonly IRetestSubmissionValidationService retestSubmissionValidationService;
    private readonly IPublisherService publisher;
    private readonly ISubmissionsHelper submissionsHelper;
    private readonly IDatesService dates;
    private readonly ITransactionsProvider transactionsProvider;
    private readonly ICacheService cache;
    private readonly IContestsDataService contestsData;
    private readonly ISubmissionTypesDataService submissionTypesData;

    public SubmissionsBusinessService(
        ILogger<SubmissionsBusinessService> logger,
        ISubmissionsDataService submissionsData,
        ISubmissionsCommonDataService submissionsCommonData,
        IUsersBusinessService usersBusiness,
        IProblemsDataService problemsDataService,
        IParticipantsDataService participantsDataService,
        ISubmissionsCommonBusinessService submissionsCommonBusinessService,
        IUserProviderService userProviderService,
        IParticipantScoresBusinessService participantScoresBusinessService,
        ILecturersInContestsBusinessService lecturersInContestsBusiness,
        ISubmissionDetailsValidationService submissionDetailsValidationService,
        ISubmitSubmissionValidationService submitSubmissionValidationService,
        ISubmissionResultsValidationService submissionResultsValidationService,
        ISubmissionFileDownloadValidationService submissionFileDownloadValidationService,
        IRetestSubmissionValidationService retestSubmissionValidationService,
        ISubmissionsForProcessingCommonDataService submissionsForProcessingData,
        IPublisherService publisher,
        ISubmissionsHelper submissionsHelper,
        IDatesService dates,
        ITransactionsProvider transactionsProvider,
        ICacheService cache,
        IContestsDataService contestsData,
        ISubmissionTypesDataService submissionTypesData)
    {
        this.logger = logger;
        this.submissionsData = submissionsData;
        this.submissionsCommonData = submissionsCommonData;
        this.usersBusiness = usersBusiness;
        this.problemsDataService = problemsDataService;
        this.lecturersInContestsBusiness = lecturersInContestsBusiness;
        this.submissionsCommonBusinessService = submissionsCommonBusinessService;
        this.participantsDataService = participantsDataService;
        this.userProviderService = userProviderService;
        this.participantScoresBusinessService = participantScoresBusinessService;
        this.submissionDetailsValidationService = submissionDetailsValidationService;
        this.submitSubmissionValidationService = submitSubmissionValidationService;
        this.submissionResultsValidationService = submissionResultsValidationService;
        this.submissionFileDownloadValidationService = submissionFileDownloadValidationService;
        this.retestSubmissionValidationService = retestSubmissionValidationService;
        this.publisher = publisher;
        this.submissionsForProcessingData = submissionsForProcessingData;
        this.submissionsHelper = submissionsHelper;
        this.dates = dates;
        this.transactionsProvider = transactionsProvider;
        this.cache = cache;
        this.contestsData = contestsData;
        this.submissionTypesData = submissionTypesData;
    }

    public async Task Retest(int id)
    {
        var user = this.userProviderService.GetCurrentUser();

        var submission = this.submissionsData
            .GetSubmissionById<SubmissionDetailsServiceModel>(id);

        if (submission == null)
        {
            throw new BusinessServiceException(ValidationMessages.Submission.NotFound);
        }

        var isUserInRoleForContest = await this.lecturersInContestsBusiness.IsCurrentUserAdminOrLecturerInContest(submission.ContestId);

        var validationResult =
            this.retestSubmissionValidationService.GetValidationResult((
                submission,
                user,
                isUserInRoleForContest));

        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        await this.publisher.Publish(new RetestSubmissionPubSubModel { Id = id });
    }

    public async Task<SubmissionDetailsServiceModel?> GetById(int submissionId)
        => await this.submissionsData
            .GetByIdQuery(submissionId)
            .MapCollection<SubmissionDetailsServiceModel>()
            .FirstOrDefaultAsync();

    public async Task<SubmissionDetailsServiceModel> GetDetailsById(int submissionId)
    {
        var currentUser = this.userProviderService.GetCurrentUser();

        //AsNoTracking() Method is added to prevent ''tracking query'' error.
        //Error is thrown when we map from UserSettings (owned entity) without including the
        //UserProfile (owner entity) in the query.
        var submissionDetailsServiceModel = await this.submissionsData
            .GetByIdQuery(submissionId)
            .AsSplitQuery()
            .AsNoTracking()
            .MapCollection<SubmissionDetailsServiceModel>()
            .FirstOrDefaultAsync();

        if (submissionDetailsServiceModel == null)
        {
            throw new BusinessServiceException(ValidationMessages.Submission.NotFound);
        }

        submissionDetailsServiceModel.TestRuns = submissionDetailsServiceModel
            .TestRuns
            .OrderBy(tr => !tr.IsTrialTest)
            .ThenBy(tr => tr.OrderBy);

        var userIsAdminOrLecturerInContest =
            await this.lecturersInContestsBusiness.IsCurrentUserAdminOrLecturerInContest(submissionDetailsServiceModel.ContestId);

        submissionDetailsServiceModel.UserIsInRoleForContest = userIsAdminOrLecturerInContest;
        submissionDetailsServiceModel.IsEligibleForRetest =
            this.submissionsHelper.IsEligibleForRetest(submissionDetailsServiceModel);

        var validationResult =
            this.submissionDetailsValidationService.GetValidationResult((submissionDetailsServiceModel, currentUser, userIsAdminOrLecturerInContest));

        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        if (!userIsAdminOrLecturerInContest)
        {
            submissionDetailsServiceModel.TestRuns = submissionDetailsServiceModel.TestRuns.Select(tr =>
            {
                var currentTestRunTest = submissionDetailsServiceModel.Tests.FirstOrDefault(t => t.Id == tr.TestId);

                var displayShowInput = currentTestRunTest != null
                                       && (!currentTestRunTest.HideInput
                                           && ((currentTestRunTest.IsTrialTest
                                                || currentTestRunTest.IsOpenTest)
                                               || submissionDetailsServiceModel.Problem.ShowDetailedFeedback));

                var showExecutionComment = currentTestRunTest != null
                                           && (!string.IsNullOrEmpty(tr.ExecutionComment)
                                               && (currentTestRunTest.IsOpenTest
                                                   || currentTestRunTest.IsTrialTest
                                                   || submissionDetailsServiceModel.Problem.ShowDetailedFeedback));

                if (!showExecutionComment)
                {
                    tr.ExecutionComment = string.Empty;
                }

                if (!displayShowInput)
                {
                    tr.ShowInput = false;
                    tr.Input = string.Empty;
                    tr.ExpectedOutputFragment = string.Empty;
                    tr.UserOutputFragment = string.Empty;
                }

                return tr;
            });
        }

        return submissionDetailsServiceModel!;
    }

    public SubmissionFileDownloadServiceModel GetSubmissionFile(int submissionId)
    {
        var submissionDetailsServiceModel = this.submissionsData
            .GetSubmissionById<SubmissionFileDetailsServiceModel>(submissionId);

        var currentUser = this.userProviderService.GetCurrentUser();

        var validationResult =
            this.submissionFileDownloadValidationService.GetValidationResult((submissionDetailsServiceModel!,
                currentUser));
        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        return new SubmissionFileDownloadServiceModel
        {
            Content = submissionDetailsServiceModel!.ByteContent,
            MimeType = GlobalConstants.MimeTypes.ApplicationOctetStream,
            FileName = string.Format(
                GlobalConstants.Submissions.SubmissionDownloadFileName,
                submissionDetailsServiceModel.Id,
                submissionDetailsServiceModel.FileExtension),
        };
    }

    public Task<Dictionary<SubmissionProcessingState, int>> GetAllUnprocessedCount()
        => this.submissionsForProcessingData
            .GetAllUnprocessed()
            .GroupBy(sfp => sfp.State)
            .ToDictionaryAsync(sfp => sfp.Key, sfp => sfp.Count());

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

    public async Task RecalculatePointsByProblem(int problemId)
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
                        t.ResultType == TestRunResultType.CorrectAnswer &&
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
                        Points = points, SubmissionId = submission.Id,
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

            await this.submissionsData.SaveChanges();

            var participants = topResults.Keys.ToList();

            var existingScores =
                await this.participantScoresBusinessService.GetByProblemForParticipants(participants, problemId);

            foreach (var existingScore in existingScores)
            {
                var topScore = topResults[existingScore.ParticipantId];

                existingScore.Points = topScore.Points;
                existingScore.SubmissionId = topScore.SubmissionId;
            }

            await this.submissionsData.SaveChanges();

            scope.Complete();
        }
    }

    // public async Task HardDeleteAllArchived() =>
    //     (await this.archivedSubmissionsData
    //         .GetAllUndeletedFromMainDatabase())
    //         .Select(s => s.Id)
    //         .AsEnumerable()
    //         .ChunkBy(GlobalConstants.BatchOperationsChunkSize)
    //         .ForEach(submissionIds =>
    //             this.HardDeleteByArchivedIds(new HashSet<int>(submissionIds)));

    // private Task HardDeleteByArchivedIds(ICollection<int> ids)
    // {
    //     using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
    //     {
    //         this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(ids);
    //         this.submissionsData.Delete(s => ids.Contains(s.Id));
    //
    //         this.archivedSubmissionsData.SetToHardDeletedFromMainDatabaseByIds(ids);
    //
    //         scope.Complete();
    //     }
    //
    //     return Task.CompletedTask;
    // }

    public async Task<PagedResult<TServiceModel>> GetByUsername<TServiceModel>(
        string? username,
        int page,
        int itemsInPage = DefaultSubmissionsPerPage)
    {
        if (!this.usersBusiness.IsUserInRolesOrProfileOwner(
                username,
                [GlobalConstants.Roles.Administrator, GlobalConstants.Roles.Lecturer]))
        {
            throw new UnauthorizedAccessException("You are not authorized for this action");
        }

        var userId = await this.usersBusiness.GetUserIdByUsername(username);

        var userParticipantsIds = await this.participantsDataService
            .GetAllByUser(userId)
            .Select(p => p.Id)
                .ToEnumerableAsync();

        return await this.submissionsData
            .GetLatestSubmissionsByUserParticipations<TServiceModel>(
                userParticipantsIds.MapCollection<int?>(),
                itemsInPage,
                page);
    }

    public async Task<PagedResult<TServiceModel>> GetUserSubmissionsByProblem<TServiceModel>(
        int problemId,
        bool isOfficial,
        int page)
    {
        var problem =
            await this.problemsDataService.GetWithProblemGroupById(problemId)
                .Map<ProblemForSubmissionDetailsServiceModel>();

        var user = this.userProviderService.GetCurrentUser();

        var participant = await this.participantsDataService
                .GetByContestByUserAndByIsOfficial(problem.ProblemGroup.ContestId, user.Id!, isOfficial)
                .Map<ParticipantServiceModel>();

        var validationResult =
            this.submissionResultsValidationService.GetValidationResult((user, problem, participant, isOfficial));

        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        return await this.GetUserSubmissions<TServiceModel>(problem.Id, participant.Id, page);
    }

    public async Task Submit(SubmitSubmissionServiceModel model)
    {
        var problem = await this.problemsDataService.GetWithProblemGroupCheckerAndTestsById(model.ProblemId)
            ?? throw new BusinessServiceException(ValidationMessages.Problem.NotFound);

        var submissionType = await this.cache.Get(
            string.Format(CacheConstants.SubmissionTypeById, model.SubmissionTypeId),
            async () => await this.submissionTypesData.OneById(model.SubmissionTypeId),
            CacheConstants.FiveMinutesInSeconds);

        var currentUser = this.userProviderService.GetCurrentUser();

        var participant = await this.participantsDataService
            .GetWithContestAndProblemsForParticipantByContestByUserAndIsOfficial(
                problem.ProblemGroup.ContestId,
                currentUser.Id!,
                model.Official);

        var contest = await this.contestsData.OneById(problem.ProblemGroup.ContestId);

        var submitSubmissionValidationServiceResult = this.submitSubmissionValidationService.GetValidationResult(
            (problem, participant, model, contest, submissionType));

        if (!submitSubmissionValidationServiceResult.IsValid)
        {
            throw new BusinessServiceException(
                submitSubmissionValidationServiceResult.Message,
                JsonConvert.SerializeObject(new { ProblemId = submitSubmissionValidationServiceResult.PropertyName }));
        }

        var newSubmission = model.Map<Submission>();
        if (model.StringContent != null)
        {
            newSubmission.ContentAsString = model.StringContent;
        }
        else
        {
            newSubmission.Content = model.ByteContent!;
        }

        newSubmission.ParticipantId = participant!.Id;
        newSubmission.IpAddress = "model.UserHostAddress";
        newSubmission.IsPublic = ((participant.IsOfficial && contest!.ContestPassword == null) ||
                                  (!participant.IsOfficial && contest!.PracticePassword == null)) &&
                                 (contest.IsVisible || contest.VisibleFrom <= this.dates.GetUtcNow()) &&
                                 !contest.IsDeleted &&
                                 problem.ShowResults;

        SubmissionServiceModel submissionServiceModel;
        var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        if (submissionType!.ExecutionStrategyType is ExecutionStrategyType.NotFound or ExecutionStrategyType.DoNothing)
        {
            // Submission is just uploaded and should not be processed
            await this.AddNewDefaultProcessedSubmission(participant, newSubmission);

            scope.Complete();
            scope.Dispose();
            return;
        }

        await this.submissionsData.Add(newSubmission);
        await this.submissionsData.SaveChanges();

        submissionServiceModel = this.submissionsCommonBusinessService.BuildSubmissionForProcessing(newSubmission, problem, submissionType);
        var submissionForProcessing = await this.submissionsForProcessingData.Add(newSubmission.Id);
        await this.submissionsData.SaveChanges();

        scope.Complete();
        // Should be disposed explicitly (not with using keyword), otherwise the next operation will fail with
        // "The current TransactionScope is already complete"
        scope.Dispose();

        await this.submissionsCommonBusinessService.PublishSubmissionForProcessing(submissionServiceModel, submissionForProcessing);
    }

    public async Task ProcessExecutionResult(SubmissionExecutionResult submissionExecutionResult)
    {
        var submission = await this.submissionsData
            .GetByIdQuery(submissionExecutionResult.SubmissionId)
            .IgnoreQueryFilters()
            .Include(s => s.Problem!.Tests)
            .Include(s => s.TestRuns)
            .FirstOrDefaultAsync();

        if (submission == null)
        {
            throw new BusinessServiceException(
                $"Submission with Id: \"{submissionExecutionResult.SubmissionId}\" not found.");
        }

        var submissionForProcessing = await this.submissionsForProcessingData
            .GetBySubmission(submission.Id);

        if (submissionForProcessing == null)
        {
            throw new BusinessServiceException(
                $"Submission for processing for Submission with ID {submissionExecutionResult.SubmissionId} not found in the database.");
        }

        var participant = await this.participantsDataService
            .GetByIdQuery(submission.ParticipantId)
            .IgnoreQueryFilters()
            .Include(p => p.User)
            .FirstOrDefaultAsync();

        if (participant == null)
        {
            throw new BusinessServiceException(
                $"Participant with Id: \"{submission.ParticipantId}\" not found.");
        }

        var exception = submissionExecutionResult.Exception;
        var executionResult = submissionExecutionResult.ExecutionResult;

        await this.transactionsProvider.ExecuteInTransaction(async () =>
        {
            submission.Processed = true;
            submission.ProcessingComment = null;
            submission.StartedExecutionOn = submissionExecutionResult.StartedExecutionOn;
            submission.CompletedExecutionOn = submissionExecutionResult.CompletedExecutionOn;
            submission.WorkerName = submissionExecutionResult.WorkerName;

            if (executionResult != null)
            {
                ProcessTestsExecutionResult(submission, executionResult);
                await this.SaveParticipantScore(participant, submission);
                CacheTestRuns(submission);
            }
            else
            {
                submission.IsCompiledSuccessfully = false;
                var errorMessage = exception?.Message
                    ?? "Invalid execution result received. Please contact an administrator.";
                submission.ProcessingComment = errorMessage;
                submission.CompilerComment = ProcessingExceptionCompilerComment;
            }

            await this.submissionsForProcessingData.SetProcessingState(submissionForProcessing, SubmissionProcessingState.Processed);
        });

        this.logger.LogSubmissionProcessedSuccessfully(submission.Id, submissionForProcessing);
    }

    public Task<int> GetTotalCount()
        => this.cache.Get(
            CacheConstants.TotalSubmissionsCount,
            async () => await this.submissionsData.IgnoreQueryFilters().Count(),
            CacheConstants.FiveMinutesInSeconds);

    public async Task<PagedResult<TServiceModel>> GetSubmissions<TServiceModel>(
        SubmissionStatus status,
        int page,
        int itemsPerPage = DefaultSubmissionsPerPage)
    {
        if (itemsPerPage <= 0)
        {
            throw new BusinessServiceException("Invalid submissions per page count");
        }

        IQueryable<Submission> query;

        switch (status)
        {
            case SubmissionStatus.Enqueued:
                query = this.submissionsCommonData.GetAllEnqueued();
                break;
            case SubmissionStatus.Processing:
                query = this.submissionsCommonData.GetAllProcessing();
                break;
            case SubmissionStatus.Pending:
                query = this.submissionsCommonData.GetAllPending();
                break;
            case SubmissionStatus.All:
            default:
                return this.userProviderService.GetCurrentUser().IsAdminOrLecturer
                    ? await this.submissionsData
                        .GetLatestSubmissions<TServiceModel>()
                        .ToPagedResultAsync(itemsPerPage, page)
                    : await this.cache.Get(
                        CacheConstants.LatestPublicSubmissions,
                        async () =>
                        {
                            var submissions = await this.submissionsData
                                .GetLatestSubmissions<TServiceModel>(DefaultSubmissionsPerPage)
                                .ToListAsync();

                            var totalItemsCount = await this.GetTotalCount();

                            // Public submissions do not have pagination, but PagedResult is used for consistency.
                            return new PagedResult<TServiceModel>
                            {
                                Items = submissions,
                                TotalItemsCount = totalItemsCount,
                                PageNumber = 1,
                            };
                        },
                        CacheConstants.TwoMinutesInSeconds);
        }

        return await query
            .OrderByDescending(s => s.Id)
            .MapCollection<TServiceModel>()
            .ToPagedResultAsync(itemsPerPage, page);
    }

    private static void ProcessTestsExecutionResult(
        Submission submission,
        ExecutionResultServiceModel executionResult)
    {
        submission.IsCompiledSuccessfully = executionResult.IsCompiledSuccessfully;
        submission.CompilerComment = executionResult.CompilerComment;
        submission.Points = executionResult.TaskResult!.Points;
        submission.TestRuns.Clear();

        if (!executionResult.IsCompiledSuccessfully)
        {
            return;
        }

        foreach (var testResult in executionResult.TaskResult?.TestResults ?? [])
        {
            submission.TestRuns.Add(
                new TestRun
                {
                    ResultType = testResult.ResultType,
                    CheckerComment = testResult.CheckerDetails?.Comment,
                    ExecutionComment = testResult.ExecutionComment,
                    ExpectedOutputFragment = testResult.CheckerDetails?.ExpectedOutputFragment,
                    UserOutputFragment = testResult.CheckerDetails?.UserOutputFragment,
                    IsTrialTest = testResult.IsTrialTest,
                    TimeUsed = testResult.TimeUsed,
                    MemoryUsed = testResult.MemoryUsed,
                    SubmissionId = submission.Id,
                    TestId = testResult.Id,
                });
        }
    }

    private static void HandleProcessingException(Submission submission, Exception ex, string methodName)
    {
        submission.ProcessingComment = string.Format(ProcessingException, methodName, ex.Message);
        submission.IsCompiledSuccessfully = false;
        submission.CompilerComment = ProcessingExceptionCompilerComment;
        submission.TestRuns = new List<TestRun>();
    }

    private static void CacheTestRuns(Submission submission)
    {
        try
        {
            submission.CacheTestRuns();
        }
        catch (Exception ex)
        {
            HandleProcessingException(submission, ex, nameof(CacheTestRuns));
        }
    }

    private async Task AddNewDefaultProcessedSubmission(Participant participant, Submission submission)
    {
        submission.Processed = true;
        submission.IsCompiledSuccessfully = true;
        submission.Points = 0;

        await this.submissionsData.Add(submission);
        await this.submissionsData.SaveChanges();

        await this.participantScoresBusinessService.SaveForSubmission(participant, submission);
    }

    private async Task SaveParticipantScore(Participant participant, Submission submission)
    {
        try
        {
            await this.participantScoresBusinessService.SaveForSubmission(participant, submission);
        }
        catch (Exception ex)
        {
            HandleProcessingException(submission, ex, nameof(this.SaveParticipantScore));
        }
    }

    private async Task<PagedResult<TServiceModel>> GetUserSubmissions<TServiceModel>(
        int problemId,
        int participantId,
        int page)
        => await this.submissionsData
            .GetAllByProblemAndParticipant(problemId, participantId)
            .MapCollection<TServiceModel>()
            .ToPagedResultAsync(DefaultSubmissionResultsPerPage, page);
}