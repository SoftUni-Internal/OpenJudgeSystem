namespace OJS.Services.Ui.Business.Implementations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OJS.Common;
using OJS.Common.Enumerations;
using OJS.Data;
using OJS.Data.Models.Participants;
using OJS.Data.Models.Submissions;
using OJS.Data.Models.Tests;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Submissions;
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
using OJS.Data.Models.Problems;
using OJS.Services.Ui.Business.Cache;
using OJS.Workers.Common.Extensions;
using static OJS.Services.Common.Constants.PaginationConstants.Submissions;
using static OJS.Services.Infrastructure.Models.ModelHelpers;
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
    private readonly ITestRunsDataService testRunsDataService;
    private readonly IProblemsCacheService problemsCache;
    private readonly IContestCategoriesCacheService contestCategoriesCache;

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
        ITestRunsDataService testRunsDataService,
        IProblemsCacheService problemsCache,
        IContestCategoriesCacheService contestCategoriesCache)
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
        this.testRunsDataService = testRunsDataService;
        this.problemsCache = problemsCache;
        this.contestCategoriesCache = contestCategoriesCache;
    }

    public async Task Retest(int submissionId)
    {
        var submission = await this.submissionsData.GetSubmissionById<SubmissionForRetestServiceModel>(submissionId)
            ?? throw new BusinessServiceException(ValidationMessages.Submission.NotFound);

        var user = this.userProviderService.GetCurrentUser();

        var userIsAdminOrLecturerInContest = await this.lecturersInContestsBusiness
            .IsCurrentUserAdminOrLecturerInContest(submission.ContestId);

        var validationResult = await this.retestSubmissionValidationService.GetValidationResult((
                submission,
                user,
                userIsAdminOrLecturerInContest));

        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        await this.publisher.Publish(new RetestSubmissionPubSubModel { Id = submissionId });
    }

    public async Task<SubmissionDetailsServiceModel> GetDetailsById(int submissionId)
    {
        var submission = await this.submissionsData.GetSubmissionById<SubmissionDetailsServiceModel>(submissionId)
            ?? throw new BusinessServiceException(ValidationMessages.Submission.NotFound);

        var userAdminOrLecturerInContest = await this.lecturersInContestsBusiness
            .IsCurrentUserAdminOrLecturerInContest(submission.ContestId);

        var currentUser = this.userProviderService.GetCurrentUser();

        var validationResult = this.submissionDetailsValidationService
            .GetValidationResult((submission, currentUser, userAdminOrLecturerInContest));

        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        submission.UserIsInRoleForContest = userAdminOrLecturerInContest;

        var category = await this.contestCategoriesCache.GetById(submission.ContestCategoryId);
        submission.AllowMentor = category?.AllowMentor ?? false;

        if (!submission.IsCompiledSuccessfully)
        {
            // If the submission is not compiled successfully, we do not need to load the test runs.
            return submission;
        }

        var testRuns = await this.testRunsDataService
            .GetAllBySubmission(submissionId)
            .AsNoTracking()
            .MapCollection<TestRunDetailsServiceModel>()
            .ToListAsync();

        foreach (var testRun in testRuns)
        {
            var test = testRun.Test;
            submission.Tests.Add(test);

            testRun.ShowInput = submission.UserIsInRoleForContest || (test is { HideInput: false }
                               && (test.IsTrialTest
                                   || test.IsOpenTest
                                   || submission.Problem.ShowDetailedFeedback));

            var showExecutionComment = submission.UserIsInRoleForContest || (!string.IsNullOrEmpty(testRun.ExecutionComment)
                                   && (test.IsOpenTest
                                       || test.IsTrialTest
                                       || submission.Problem.ShowDetailedFeedback));

            if (!showExecutionComment)
            {
                testRun.ExecutionComment = string.Empty;
            }

            if (testRun.ShowInput)
            {
                testRun.Input = test.InputData?.Decompress();
            }
            else
            {
                testRun.Input = string.Empty;
                testRun.ExpectedOutputFragment = string.Empty;
                testRun.UserOutputFragment = string.Empty;
            }
        }

        submission.TestRuns = [.. testRuns.OrderBy(tr => !tr.IsTrialTest).ThenBy(tr => tr.OrderBy)];
        submission.IsEligibleForRetest = await this.submissionsHelper.IsEligibleForRetest(
            submissionId, submission.IsProcessed, submission.IsCompiledSuccessfully, submission.TestRuns.Count);

        submission.AllowMentor = submission.AllowMentor && submission.TestRuns.Any(tr => tr.ResultType != TestRunResultType.CorrectAnswer);

        return submission;
    }

    public async Task<SubmissionFileDownloadServiceModel> GetSubmissionFile(int submissionId)
    {
        var submissionDetailsServiceModel = await this.submissionsData
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

    public async Task<PagedResult<TServiceModel>> GetByUsername<TServiceModel>(
        string? username,
        int page,
        int itemsInPage = DefaultSubmissionsPerPage)
    {
        if (!await this.usersBusiness.IsUserInRolesOrProfileOwner(
                username,
                [GlobalConstants.Roles.Administrator, GlobalConstants.Roles.Lecturer]))
        {
            throw new UnauthorizedAccessException("You are not authorized for this action");
        }

        var userId = await this.usersBusiness.GetUserIdByUsername(username);

        var userParticipantsIds = await this.participantsDataService
            .GetAllByUser(userId)
            .Select(p => p.Id)
            .ToListAsync();

        return await this.submissionsData
            .GetLatestSubmissionsByUserParticipations<TServiceModel>(
                userParticipantsIds,
                itemsInPage,
                page);
    }

    public async Task<PagedResult<SubmissionForSubmitSummaryServiceModel>> GetUserSubmissionsByProblem(
        int problemId,
        bool isOfficial,
        int page)
    {
        var problem =
            await this.problemsDataService.OneByIdTo<ProblemForSubmissionDetailsServiceModel>(problemId);

        var user = this.userProviderService.GetCurrentUser();

        var participant = await this.participantsDataService
            .GetAllByContestByUserAndIsOfficial(problem?.ProblemGroupContestId ?? 0, user.Id, isOfficial)
            .AsNoTracking()
            .MapCollection<ParticipantServiceModel>()
            .FirstOrDefaultAsync();

        var validationResult =
            this.submissionResultsValidationService.GetValidationResult((user, problem, participant, isOfficial));

        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        var submissions = await this.submissionsData
            .GetAllByProblemAndParticipant(problemId, participant!.Id)
            .AsNoTracking()
            .MapCollection<SubmissionForSubmitSummaryServiceModel>()
            .ToPagedResultAsync(DefaultSubmissionResultsPerPage, page);

        foreach (var submission in submissions.Items)
        {
            var (maxMemoryUsed, maxTimeUsed) = GetMaxMemoryAndTimeUsed(submission.TestRunsCache);

            submission.IsOfficial = isOfficial;
            submission.MaxMemoryUsed = maxMemoryUsed;
            submission.MaxTimeUsed = maxTimeUsed;
            submission.Result = new ResultForPublicSubmissionsServiceModel
            {
                Points = submission.Points,
                MaxPoints = problem!.MaximumPoints,
            };
        }

        return submissions;
    }

    public async Task Submit(SubmitSubmissionServiceModel model)
    {
        var problem = await this.problemsCache.GetForSubmitById(model.ProblemId)
            ?? throw new BusinessServiceException(ValidationMessages.Problem.NotFound);

        var currentUser = this.userProviderService.GetCurrentUser();

        // Using .Select instead of AutoMapper due to conditional mapping, depending on model state.
        // ProblemsForParticipants are not needed in most cases, so they are not included in the query.
        var participant = await this.participantsDataService
            .GetAllByContestByUserAndIsOfficial(model.ContestId, currentUser.Id, model.Official)
            .AsNoTracking()
            .Select(p => new ParticipantSubmitServiceModel
            {
                Id = p.Id,
                IsInvalidated = p.IsInvalidated,
                IsOfficial = p.IsOfficial,
                ContestId = p.ContestId,
                ContestType = p.Contest.Type,
                LastSubmissionTime = p.LastSubmissionTime,
                ParticipationStartTime = p.ParticipationStartTime,
                ParticipationEndTime = p.ParticipationEndTime,
                ContestStartTime = p.Contest.StartTime,
                ContestEndTime = p.Contest.EndTime,
                ContestPracticeStartTime = p.Contest.PracticeStartTime,
                ContestPracticeEndTime = p.Contest.PracticeEndTime,
                ContestLimitBetweenSubmissions = p.Contest.LimitBetweenSubmissions,
                ContestAllowParallelSubmissionsInTasks = p.Contest.AllowParallelSubmissionsInTasks,
                Problems = model.Official && model.IsOnlineExam
                    ? p.ProblemsForParticipants
                        .Select(pfp => new ProblemForParticipantServiceModel
                        {
                            ProblemId = pfp.ProblemId,
                            ParticipantId = pfp.ParticipantId,
                        })
                    : null,
            })
            .FirstOrDefaultAsync();

        var submissionType = problem.SubmissionTypesInProblems
            .Select(p => p.SubmissionType)
            .MapCollection<SubmissionType>()
            .FirstOrDefault(st => st.Id == model.SubmissionTypeId);

        var submitSubmissionValidationServiceResult = await this.submitSubmissionValidationService.GetValidationResult(
            (problem, participant, model, submissionType));

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

        var updatedParticipant = new Participant
        {
            Id = participant.Id,
            LastSubmissionTime = this.dates.GetUtcNow(),
        };

        this.participantsDataService.Attach(updatedParticipant);
        this.participantsDataService
            .GetEntry(updatedParticipant)
            .Property(p => p.LastSubmissionTime).IsModified = true;

        if (submissionType!.ExecutionStrategyType
            is ExecutionStrategyType.NotFound
            or ExecutionStrategyType.DoNothing)
        {
            // Submission is just uploaded and should not be processed
            await this.AddNewDefaultProcessedSubmission(participant.Id, newSubmission);
            return;
        }

        if (problem.HasAdditionalFiles)
        {
            problem.AdditionalFiles = await this.problemsDataService
                .GetByIdQuery(model.ProblemId)
                .Select(p => p.AdditionalFiles)
                .FirstOrDefaultAsync();
        }

        SubmissionForProcessing? submissionForProcessing = null;
        await this.transactionsProvider.ExecuteInTransaction(async () =>
        {
            await this.submissionsData.Add(newSubmission);
            await this.submissionsData.SaveChanges();

            submissionForProcessing = await this.submissionsForProcessingData.Add(newSubmission.Id);
            await this.submissionsData.SaveChanges();
        });

        var submissionServiceModel =
            this.submissionsCommonBusinessService.BuildSubmissionForProcessing(newSubmission, problem.Map<Problem>(),
                submissionType);

        await this.submissionsCommonBusinessService.PublishSubmissionForProcessing(submissionServiceModel,
            submissionForProcessing!);
    }

    public async Task ProcessExecutionResult(SubmissionExecutionResult submissionExecutionResult)
    {
        var submission = await this.submissionsData
            .GetByIdQuery(submissionExecutionResult.SubmissionId)
            .IgnoreQueryFilters()
            .Include(s => s.TestRuns)
            .FirstOrDefaultAsync()
            ?? throw new BusinessServiceException(
                $"Submission with Id: \"{submissionExecutionResult.SubmissionId}\" not found.");

        var submissionForProcessing = await this.submissionsForProcessingData
            .GetBySubmission(submission.Id)
            ?? throw new BusinessServiceException(
                $"Submission for processing for Submission with ID {submission.Id} not found in the database.");

        var participant = await this.participantsDataService
            .GetByIdQuery(submission.ParticipantId)
            .IgnoreQueryFilters()
            .Include(p => p.User)
            .FirstOrDefaultAsync()
            ?? throw new BusinessServiceException(
                $"Participant with Id: \"{submission.ParticipantId}\" not found.");

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
                submission.ExceptionType = exception?.ExceptionType ?? ExceptionType.Other;
                submission.IsCompiledSuccessfully = false;
                var errorMessage = exception?.Message ?? "Invalid execution result received. Please contact an administrator.";
                submission.ProcessingComment = errorMessage;
                submission.CompilerComment = exception?.ExceptionType == ExceptionType.Solution ? exception.Message : ProcessingExceptionCompilerComment;
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

        if (!executionResult.IsCompiledSuccessfully)
        {
            submission.TestRuns.Clear();
            return;
        }

        submission.TestRuns = (executionResult.TaskResult?.TestResults ?? []).Select(testResult => new TestRun
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
        }).ToList();
    }


    private static void HandleProcessingException(Submission submission, Exception ex, string methodName)
    {
        submission.ProcessingComment = string.Format(ProcessingException, methodName, ex.Message);
        submission.IsCompiledSuccessfully = false;
        submission.CompilerComment = ProcessingExceptionCompilerComment;
        submission.TestRuns = [];
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

    private async Task AddNewDefaultProcessedSubmission(int participantId, Submission submission)
    {
        var participant = await this.participantsDataService
            .GetByIdQuery(participantId)
            .Include(p => p.User)
            .FirstOrDefaultAsync()
            ?? throw new BusinessServiceException($"Participant with Id: \"{participantId}\" not found.");

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
}