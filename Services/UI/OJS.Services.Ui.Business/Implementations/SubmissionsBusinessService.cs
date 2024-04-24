namespace OJS.Services.Ui.Business.Implementations;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OJS.Common;
using OJS.Common.Enumerations;
using OJS.Common.Helpers;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Submissions;
using OJS.Data.Models.Tests;
using OJS.Services.Common;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Ui.Business.Extensions;
using OJS.Services.Ui.Business.Validations.Implementations.Contests;
using OJS.Services.Ui.Business.Validations.Implementations.Submissions;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Participants;
using OJS.Services.Ui.Models.Submissions;
using OJS.Services.Ui.Models.Submissions.PublicSubmissions;
using OJS.Workers.Common.Models;
using SoftUni.AutoMapper.Infrastructure.Extensions;
using SoftUni.Common.Extensions;
using SoftUni.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static OJS.Services.Common.PaginationConstants.Submissions;

public class SubmissionsBusinessService : ISubmissionsBusinessService
{
    private readonly ISubmissionsDataService submissionsData;
    private readonly ISubmissionsCommonDataService submissionsCommonData;
    private readonly ISubmissionsForProcessingCommonDataService submissionsForProcessingData;
    private readonly IUsersBusinessService usersBusiness;
    private readonly IParticipantScoresBusinessService participantScoresBusinessService;
    private readonly IParticipantsBusinessService participantsBusinessService;
    private readonly ISubmissionsCommonBusinessService submissionsCommonBusinessService;

    // TODO: https://github.com/SoftUni-Internal/exam-systems-issues/issues/624
    private readonly IParticipantsDataService participantsDataService;
    private readonly IProblemsDataService problemsDataService;
    private readonly IContestsDataService contestsDataService;
    private readonly IUserProviderService userProviderService;
    private readonly ILecturersInContestsBusinessService lecturersInContestsBusiness;
    private readonly ISubmissionDetailsValidationService submissionDetailsValidationService;
    private readonly IContestParticipationValidationService contestParticipationValidationService;
    private readonly ISubmitSubmissionValidationService submitSubmissionValidationService;
    private readonly ISubmissionResultsValidationService submissionResultsValidationService;
    private readonly ISubmissionFileDownloadValidationService submissionFileDownloadValidationService;
    private readonly IRetestSubmissionValidationService retestSubmissionValidationService;
    private readonly ISubmissionPublisherService submissionPublisher;
    private readonly ISubmissionsHelper submissionsHelper;
    private readonly ILogger<SubmissionsBusinessService> logger;

    public SubmissionsBusinessService(
        ISubmissionsDataService submissionsData,
        ISubmissionsCommonDataService submissionsCommonData,
        IUsersBusinessService usersBusiness,
        IProblemsDataService problemsDataService,
        IParticipantsBusinessService participantsBusinessService,
        IParticipantsDataService participantsDataService,
        ISubmissionsCommonBusinessService submissionsCommonBusinessService,
        IUserProviderService userProviderService,
        IParticipantScoresBusinessService participantScoresBusinessService,
        ILecturersInContestsBusinessService lecturersInContestsBusiness,
        ISubmissionDetailsValidationService submissionDetailsValidationService,
        IContestParticipationValidationService contestParticipationValidationService,
        ISubmitSubmissionValidationService submitSubmissionValidationService,
        ISubmissionResultsValidationService submissionResultsValidationService,
        ISubmissionFileDownloadValidationService submissionFileDownloadValidationService,
        IRetestSubmissionValidationService retestSubmissionValidationService,
        ISubmissionsForProcessingCommonDataService submissionsForProcessingData,
        ISubmissionPublisherService submissionPublisher,
        IContestsDataService contestsDataService,
        ISubmissionsHelper submissionsHelper,
        ILogger<SubmissionsBusinessService> logger)
    {
        this.submissionsData = submissionsData;
        this.submissionsCommonData = submissionsCommonData;
        this.usersBusiness = usersBusiness;
        this.problemsDataService = problemsDataService;
        this.participantsBusinessService = participantsBusinessService;
        this.lecturersInContestsBusiness = lecturersInContestsBusiness;
        this.submissionsCommonBusinessService = submissionsCommonBusinessService;
        this.participantsDataService = participantsDataService;
        this.userProviderService = userProviderService;
        this.participantScoresBusinessService = participantScoresBusinessService;
        this.submissionDetailsValidationService = submissionDetailsValidationService;
        this.contestParticipationValidationService = contestParticipationValidationService;
        this.submitSubmissionValidationService = submitSubmissionValidationService;
        this.submissionResultsValidationService = submissionResultsValidationService;
        this.submissionFileDownloadValidationService = submissionFileDownloadValidationService;
        this.retestSubmissionValidationService = retestSubmissionValidationService;
        this.submissionPublisher = submissionPublisher;
        this.submissionsForProcessingData = submissionsForProcessingData;
        this.contestsDataService = contestsDataService;
        this.submissionsHelper = submissionsHelper;
        this.logger = logger;
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

        await this.submissionPublisher.PublishRetest(submission.Id);
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
        var user = await this.usersBusiness.GetUserProfileByUsername(username);
        var loggedInUser = this.userProviderService.GetCurrentUser();
        var loggedInUserProfile = await this.usersBusiness.GetUserProfileById(loggedInUser.Id);

        if (!loggedInUser.IsAdminOrLecturer && loggedInUserProfile!.UserName != username)
        {
            throw new UnauthorizedAccessException("You are not authorized for this action");
        }

        var userParticipantsIds = await this.participantsDataService
            .GetAllByUser(user!.Id)
            .Select(p => p.Id)
                .ToEnumerableAsync();

        return await this.submissionsData
            .GetLatestSubmissionsByUserParticipations<TServiceModel>(
                userParticipantsIds.MapCollection<int?>(),
                itemsInPage,
                page);
    }

    public async Task<PagedResult<SubmissionForProfileServiceModel>> GetForProfileByUserAndContest(string? username, int page, int contestId)
    {
        var user = await this.usersBusiness.GetUserProfileByUsername(username);

        return await this.submissionsData
            .GetAllForUserByContest(
                contestId,
                user!.Id)
            .MapCollection<SubmissionForProfileServiceModel>()
            .ToPagedResultAsync(DefaultSubmissionsPerPage, page);
    }

    public async Task<PagedResult<PublicSubmissionsServiceModel>> GetUserSubmissionsByProblem(
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

        return await this.GetUserSubmissions<PublicSubmissionsServiceModel>(problem.Id, participant.Id, page);
    }

    public async Task Submit(SubmitSubmissionServiceModel model)
    {
        var problem = await this.problemsDataService.GetWithProblemGroupCheckerAndTestsById(model.ProblemId);

        if (problem == null)
        {
            throw new BusinessServiceException(ValidationMessages.Problem.NotFound);
        }

        var currentUser = this.userProviderService.GetCurrentUser();
        var participant = await this.participantsDataService
            .GetWithContestAndSubmissionDetailsByContestByUserAndIsOfficial(
                problem.ProblemGroup.ContestId,
                currentUser.Id!,
                model.Official);

        var contestValidationResult = this.contestParticipationValidationService.GetValidationResult(
            (participant?.Contest,
                participant?.ContestId,
                currentUser,
                model.Official)!);

        var userSubmissionTimeLimit = await this.participantsBusinessService.GetParticipantLimitBetweenSubmissions(
            participant!.Id,
            participant.Contest.LimitBetweenSubmissions);

        var hasUserNotProcessedSubmissionForProblem =
            this.submissionsData.HasUserNotProcessedSubmissionForProblem(problem.Id, currentUser.Id!);

        var hasUserNotProcessedSubmissionForContest =
            this.submissionsData.HasUserNotProcessedSubmissionForContest(participant.ContestId, currentUser.Id!);

        var submitSubmissionValidationServiceResult = this.submitSubmissionValidationService.GetValidationResult(
            (problem,
                currentUser,
                participant,
                contestValidationResult,
                userSubmissionTimeLimit,
                hasUserNotProcessedSubmissionForProblem,
                hasUserNotProcessedSubmissionForContest,
                model));

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

        newSubmission.ParticipantId = participant.Id;
        newSubmission.IpAddress = "model.UserHostAddress";
        newSubmission.IsPublic = ((participant.IsOfficial && participant.Contest.ContestPassword == null) ||
                                  (!participant.IsOfficial && participant.Contest.PracticePassword == null)) &&
                                 participant.Contest.IsVisible &&
                                 !participant.Contest.IsDeleted &&
                                 problem.ShowResults;

        var submissionType = problem.SubmissionTypesInProblems
            .First(st => st.SubmissionTypeId == model.SubmissionTypeId)
            .SubmissionType;

        SubmissionServiceModel submissionServiceModel;
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        if (submissionType.ExecutionStrategyType is ExecutionStrategyType.NotFound or ExecutionStrategyType.DoNothing)
        {
            // Submission is just uploaded and should not be processed
            await this.AddNewDefaultProcessedSubmission(newSubmission);

            scope.Complete();
            return;
        }

        await this.submissionsData.Add(newSubmission);
        await this.submissionsData.SaveChanges();

        submissionServiceModel = this.BuildSubmissionForProcessing(newSubmission, problem, submissionType);
        await this.submissionsForProcessingData.Add(newSubmission.Id, submissionServiceModel.ToSerializedDetails());
        await this.submissionsData.SaveChanges();

        scope.Complete();
        scope.Dispose();

        await this.submissionsCommonBusinessService
            .PublishSubmissionForProcessing(submissionServiceModel);
    }

    public async Task ProcessExecutionResult(SubmissionExecutionResult submissionExecutionResult)
    {
        var submission = await this.submissionsData
            .GetByIdQuery(submissionExecutionResult.SubmissionId)
            .Include(s => s.Problem!.Tests)
            .Include(s => s.TestRuns)
            .FirstOrDefaultAsync();

        if (submission == null)
        {
            throw new BusinessServiceException(
                $"Submission with Id: \"{submissionExecutionResult.SubmissionId}\" not found.");
        }

        submission.StartedExecutionOn = submissionExecutionResult.StartedExecutionOn;
        submission.CompletedExecutionOn = submissionExecutionResult.CompletedExecutionOn;

        var exception = submissionExecutionResult.Exception;
        var executionResult = submissionExecutionResult.ExecutionResult;

        submission.Processed = true;
        submission.ProcessingComment = null;

        var serializedExecutionResultServiceModel =
            submissionExecutionResult.Map<SerializedSubmissionExecutionResultServiceModel>();

        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        if (executionResult != null)
        {
            ProcessTestsExecutionResult(submission, executionResult);

            this.submissionsData.Update(submission);

            await this.SaveParticipantScore(submission);

            await this.submissionsForProcessingData.MarkProcessed(serializedExecutionResultServiceModel);
            CacheTestRuns(submission);
        }
        else
        {
            submission.IsCompiledSuccessfully = false;
            var errorMessage = exception?.Message
                               ?? "Invalid execution result received. Please contact an administrator.";
            submission.ProcessingComment = errorMessage;
            submission.CompilerComment = errorMessage;

            this.submissionsData.Update(submission);
            await this.submissionsForProcessingData.MarkProcessed(serializedExecutionResultServiceModel);
        }

        await this.submissionsData.SaveChanges();

        scope.Complete();
        scope.Dispose();
    }

    public async Task<PagedResult<SubmissionResultsServiceModel>> GetSubmissionResults(int submissionId, int page)
    {
        var problemId = await this.submissionsData.GetProblemIdBySubmission(submissionId);

        var participantId =
            await this.submissionsData.GetParticipantIdBySubmission(submissionId);

        if (problemId == 0 || participantId == 0)
        {
            return new PagedResult<SubmissionResultsServiceModel>();
        }

        return await this.GetUserSubmissions<SubmissionResultsServiceModel>(problemId, participantId, page);
    }

    public Task<int> GetTotalCount()
        => this.submissionsData.Count();

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

        if (status == SubmissionStatus.Processing)
        {
            query = this.submissionsCommonData.GetAllProcessing();
        }
        else if (status == SubmissionStatus.Pending)
        {
            query = this.submissionsCommonData.GetAllPending();
        }
        else
        {
            var user = this.userProviderService.GetCurrentUser();
            if (user.IsAdminOrLecturer)
            {
                return await this.submissionsData.GetLatestSubmissions<TServiceModel>(itemsPerPage, page);
            }

            var submissions = await this.submissionsData.GetLatestSubmissions<TServiceModel>(itemsPerPage, 1);
            return new PagedResult<TServiceModel>
            {
                Items = submissions.Items,
                TotalItemsCount = submissions.TotalItemsCount,
            };
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

        if (!executionResult.IsCompiledSuccessfully)
        {
            submission.TestRuns.Clear();
            return;
        }

        var testResults =
            executionResult.TaskResult?.TestResults ?? Enumerable.Empty<TestResultServiceModel>();

        submission.TestRuns.AddRange(
            testResults.Select(testResult => testResult.Map<TestRun>()));
    }

    private static void CacheTestRuns(Submission submission)
    {
        try
        {
            submission.CacheTestRuns();
        }
        catch (Exception ex)
        {
            submission.ProcessingComment = $"Exception in CacheTestRuns: {ex.Message}";
        }
    }

    private SubmissionServiceModel BuildSubmissionForProcessing(
        Submission submission,
        Problem problem,
        SubmissionType submissionType)
    {
        // We detach the existing entity, in order to avoid tracking exception on Update.
        this.submissionsData.Detach(submission);

        // Needed to map execution details
        submission.Problem = problem;
        submission.SubmissionType = submissionType;

        var serviceModel = submission.Map<SubmissionServiceModel>();

        serviceModel.TestsExecutionDetails!.TaskSkeleton = problem.SubmissionTypesInProblems
            .Where(x => x.SubmissionTypeId == submission.SubmissionTypeId)
            .Select(x => x.SolutionSkeleton)
            .FirstOrDefault();

        return serviceModel;
    }

    private async Task AddNewDefaultProcessedSubmission(Submission submission)
    {
        submission.Processed = true;
        submission.Points = 0;

        await this.submissionsData.Add(submission);
        await this.submissionsData.SaveChanges();

        await this.participantScoresBusinessService.SaveForSubmission(submission);
    }

    private async Task SaveParticipantScore(Submission submission)
    {
        try
        {
            await this.participantScoresBusinessService.SaveForSubmission(submission);
        }
        catch (Exception ex)
        {
            submission.ProcessingComment = $"Exception in SaveParticipantScore: {ex.Message}";
        }
    }

    private async Task<PagedResult<TServiceModel>> GetUserSubmissions<TServiceModel>(
        int problemId,
        int participantId,
        int page)
    {
        var userSubmissions = this.submissionsData
            .GetAllByProblemAndParticipant(problemId, participantId);

        return await userSubmissions
            .MapCollection<TServiceModel>()
            .ToPagedResultAsync(DefaultSubmissionResultsPerPage, page);
    }
}