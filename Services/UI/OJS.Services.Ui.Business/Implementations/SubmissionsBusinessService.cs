namespace OJS.Services.Ui.Business.Implementations;

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Problems;
using FluentExtensions.Extensions;
using OJS.Data.Models.Tests;
using OJS.Common;
using OJS.Common.Helpers;
using OJS.Data.Models.Submissions;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Submissions;
using SoftUni.Judge.Common.Enumerations;
using SoftUni.AutoMapper.Infrastructure.Extensions;
using OJS.Services.Common;
using OJS.Services.Infrastructure.Exceptions;
using static OJS.Services.Ui.Business.Constants.PublicSubmissions;

public class SubmissionsBusinessService : ISubmissionsBusinessService
{
    private readonly ISubmissionsDataService submissionsData;

    private readonly IUsersBusinessService usersBusiness;
    private readonly IParticipantScoresBusinessService participantScoresBusinessService;
    private readonly IParticipantsDataService participantsDataService;
    private readonly IProblemsDataService problemsDataService;
    private readonly IUserProviderService userProviderService;
    private readonly IContestsBusinessService contestsBusinessService;
    private readonly IProblemsBusinessService problemsBusinessService;
    private readonly ISubmissionTypesBusinessService submissionTypesBusinessService;
    private readonly ISubmissionsDistributorCommunicationService submissionsDistributorCommunicationService;
    private readonly ITestRunsDataService testRunsDataService;

    public SubmissionsBusinessService(
        ISubmissionsDataService submissionsData,
        IUsersBusinessService usersBusiness,
        IProblemsDataService problemsDataService,
        IParticipantsDataService participantsDataService,
        IUserProviderService userProviderService,
        IContestsBusinessService contestsBusinessService,
        IProblemsBusinessService problemsBusinessService,
        ISubmissionTypesBusinessService submissionTypesBusinessService,
        ISubmissionsDistributorCommunicationService submissionsDistributorCommunicationService,
        ITestRunsDataService testRunsDataService,
        IParticipantScoresBusinessService participantScoresBusinessService)
    {
        this.submissionsData = submissionsData;
        this.usersBusiness = usersBusiness;
        this.problemsDataService = problemsDataService;
        this.participantsDataService = participantsDataService;
        this.userProviderService = userProviderService;
        this.contestsBusinessService = contestsBusinessService;
        this.problemsBusinessService = problemsBusinessService;
        this.submissionTypesBusinessService = submissionTypesBusinessService;
        this.submissionsDistributorCommunicationService = submissionsDistributorCommunicationService;
        this.testRunsDataService = testRunsDataService;
        this.participantScoresBusinessService = participantScoresBusinessService;
    }

    public async Task<SubmissionDetailsServiceModel?> GetById(int submissionId)
        => await this.submissionsData
            .GetByIdQuery(submissionId)
            .MapCollection<SubmissionDetailsServiceModel>()
            .FirstOrDefaultAsync();

    public async Task<SubmissionDetailsServiceModel?> GetDetailsById(int submissionId)
        => await this.submissionsData
            .GetByIdQuery(submissionId)
            .Include(s => s.Participant)
            .ThenInclude(p => p!.User)
            .Include(s => s.TestRuns)
            .ThenInclude(tr => tr.Test)
            .Include(s => s.SubmissionType)
            .MapCollection<SubmissionDetailsServiceModel>()
            .FirstOrDefaultAsync();

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

                if (!submissionResult.ParticipantId.HasValue)
                {
                    continue;
                }

                var participantId = submissionResult.ParticipantId.Value;

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

    public async Task<IEnumerable<SubmissionForProfileServiceModel>> GetForProfileByUser(string? username)
    {
        var user = await this.usersBusiness.GetUserProfileByUsername(username);
        var data = await this.submissionsData
            .GetQuery()
            .Include(s => s.Problem)
            .Include(s => s.TestRuns)
            .Include(s => s.SubmissionType)
            .Where(s => s.Participant!.UserId == user!.Id)
            .Take(40)
            .OrderByDescending(s => s.CreatedOn)
            .MapCollection<SubmissionForProfileServiceModel>()
            .ToListAsync();

        return data;
    }

    public async Task<IEnumerable<SubmissionResultsServiceModel>> GetSubmissionResultsByProblem(
        int problemId,
        bool isOfficial,
        int take = 0)
    {
        var problem = await this.problemsDataService.GetWithProblemGroupById(problemId);

        await this.ValidateUserCanViewResults(problem!, isOfficial);

        var participant =
            await this.participantsDataService.GetByContestByUserAndByIsOfficial(
                problem!.ProblemGroup.ContestId,
                this.userProviderService.GetCurrentUser().Id!,
                isOfficial);

        var userSubmissions = this.submissionsData
            .GetAllByProblemAndParticipant(problemId, participant!.Id)
            .MapCollection<SubmissionResultsServiceModel>();

        if (take != 0)
        {
            userSubmissions = userSubmissions.Take(take);
        }

        return await userSubmissions.ToListAsync();
    }

    public async Task<IEnumerable<SubmissionResultsServiceModel>> GetSubmissionResultsByProblemAndUser(
        int problemId,
        bool isOfficial,
        string userId)
    {
        var problem = await this.problemsDataService.GetWithProblemGroupById(problemId);

        await this.ValidateUserCanViewResults(problem!, isOfficial);

        var userSubmissions = await this.submissionsData
            .GetAllByProblemAndUser<SubmissionResultsServiceModel>(problemId, userId);

        return userSubmissions;
    }

    public async Task Submit(SubmitSubmissionServiceModel model)
    {
        var problem = await this.problemsDataService.GetWithProblemGroupCheckerAndTestsById(model.ProblemId);
        if (problem == null)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.ProblemNotFound);
        }

        var currentUser = this.userProviderService.GetCurrentUser();

        var participant = await this.participantsDataService
            .GetWithContestByContestByUserAndIsOfficial(
                problem.ProblemGroup.ContestId,
                currentUser.Id!,
                model.Official);
        if (participant == null)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.UserIsNotRegisteredForExam);
        }

        await this.contestsBusinessService.ValidateContest(
            participant.Contest,
            currentUser.Id!,
            currentUser.IsAdmin,
            model.Official);

        this.problemsBusinessService.ValidateProblemForParticipant(
            participant,
            participant.Contest,
            model.ProblemId,
            model.Official);

        // if (official &&
        //     !this.contestsBusinessService.IsContestIpValidByContestAndIp(problem.ProblemGroup.ContestId, this.Request.UserHostAddress))
        // {
        //     return this.RedirectToAction("NewContestIp", new { id = problem.ProblemGroup.ContestId });
        // }

        this.submissionTypesBusinessService.ValidateSubmissionType(model.SubmissionTypeId, problem);

        if (this.submissionsData.GetUserSubmissionTimeLimit(
                participant.Id,
                participant.Contest.LimitBetweenSubmissions) != 0)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.SubmissionWasSentTooSoon);
        }

        if (problem.SourceCodeSizeLimit < model.Content.Length)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.SubmissionTooLong);
        }

        if (this.submissionsData.HasUserNotProcessedSubmissionForProblem(problem.Id, currentUser.Id!))
        {
            throw new BusinessServiceException(Resources.ContestsGeneral
                .UserHasNotProcessedSubmissionForProblem);
        }

        var contest = participant.Contest;

        var newSubmission = new Submission
        {
            ContentAsString = model.Content,
            ProblemId = model.ProblemId,
            SubmissionTypeId = model.SubmissionTypeId,
            ParticipantId = participant.Id,
            IpAddress = "model.UserHostAddress",
            IsPublic = ((participant.IsOfficial && contest.ContestPassword == null) ||
                        (!participant.IsOfficial && contest.PracticePassword == null)) &&
                       contest.IsVisible &&
                       !contest.IsDeleted &&
                       problem.ShowResults,
        };

        await this.submissionsData.Add(newSubmission);
        await this.submissionsData.SaveChanges();

        newSubmission.Problem = problem;
        newSubmission.SubmissionType =
            problem.SubmissionTypesInProblems
                .First(st => st.SubmissionTypeId == model.SubmissionTypeId)
                .SubmissionType;

        var response = await this.submissionsDistributorCommunicationService
            .AddSubmissionForProcessing(newSubmission);
    }

    public async Task SubmitFileSubmission(SubmitFileSubmissionServiceModel model)
    {
        if (model.Content == null)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.UploadFile);
        }

        var problem = await this.problemsDataService.GetWithProblemGroupById(model.ProblemId);
        if (problem == null)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.ProblemNotFound);
        }

        var currentUser = this.userProviderService.GetCurrentUser();

        var participant = await this.participantsDataService
            .GetWithContestByContestByUserAndIsOfficial(problem.ProblemGroup.ContestId, currentUser.Id!, model.Official);
        if (participant == null)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.UserIsNotRegisteredForExam);
        }

        await this.contestsBusinessService.ValidateContest(participant.Contest, currentUser.Id!, currentUser.IsAdmin, model.Official);

        this.problemsBusinessService.ValidateProblemForParticipant(
            participant,
            participant.Contest,
            model.ProblemId,
            model.Official);

        // if (participantSubmission.Official &&
        //     !this.contestsBusinessService.IsContestIpValidByContestAndIp(problem.ProblemGroup.ContestId, this.Request.UserHostAddress))
        // {
        //     return this.RedirectToAction("NewContestIp", new { id = problem.ProblemGroup.ContestId });
        // }

        if (this.submissionsData.GetUserSubmissionTimeLimit(
                participant.Id,
                participant.Contest.LimitBetweenSubmissions) != 0)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.SubmissionWasSentTooSoon);
        }

        if (problem.SourceCodeSizeLimit < model.Content.Length)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.SubmissionTooLong);
        }

        this.submissionTypesBusinessService.ValidateSubmissionType(model.SubmissionTypeId, problem, true);

        var submissionType = await this.submissionTypesBusinessService
            .GetById(model.SubmissionTypeId);

        // Validate file extension
        if (!submissionType.AllowedFileExtensions.Contains(
            model.FileExtension))
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.InvalidExtention);
        }

        var newSubmission = new Submission
        {
            Content = model.Content,
            FileExtension = model.FileExtension,
            ProblemId = model.ProblemId,
            SubmissionTypeId = model.SubmissionTypeId,
            ParticipantId = participant.Id,
        };

        await this.submissionsData.Add(newSubmission);
        await this.submissionsData.SaveChanges();

        newSubmission.Problem = problem;
        newSubmission.SubmissionType =
            problem.SubmissionTypesInProblems
                .First(st => st.SubmissionTypeId == model.SubmissionTypeId)
                .SubmissionType;

        var response = await this.submissionsDistributorCommunicationService
            .AddSubmissionForProcessing(newSubmission);
    }

    public async Task ProcessExecutionResult(SubmissionExecutionResult submissionExecutionResult)
    {
        var submission = await this.submissionsData
            .GetByIdQuery(submissionExecutionResult.SubmissionId)
            .Include(s => s.Problem!.Tests)
            .FirstOrDefaultAsync();

        if (submission == null)
        {
            throw new BusinessServiceException(
                $"Submission with Id: \"{submissionExecutionResult.SubmissionId}\" not found.");
        }

        var exception = submissionExecutionResult.Exception;
        var executionResult = submissionExecutionResult.ExecutionResult;

        submission.ProcessingComment = null;
        await this.testRunsDataService.DeleteBySubmission(submission.Id);

        if (exception != null)
        {
            submission.ProcessingComment = exception.Message + Environment.NewLine + exception.StackTrace;
            return;
        }

        if (executionResult == null)
        {
            submission.ProcessingComment = "Invalid execution result received. Please contact an administrator.";
            return;
        }

        await this.ProcessTestsExecutionResult(submission, executionResult);
    }

    public Task<IEnumerable<SubmissionForPublicSubmissionsServiceModel>> GetPublicSubmissions()
        => this.submissionsData.GetLatestSubmissions<SubmissionForPublicSubmissionsServiceModel>(DefaultCount);

    public Task<int> GetTotalCount()
        => this.submissionsData.Count();

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

    private async Task ValidateUserCanViewResults(Problem problem, bool isOfficial)
    {
        if (problem == null)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.ProblemNotFound);
        }

        var user = this.userProviderService.GetCurrentUser();

        var userHasParticipation = await this.participantsDataService
            .ExistsByContestByUserAndIsOfficial(problem.ProblemGroup.ContestId, user.Id!, isOfficial);

        if (!userHasParticipation)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.UserIsNotRegisteredForExam);
        }

        if (!problem.ShowResults)
        {
            throw new BusinessServiceException(Resources.ContestsGeneral.ProblemResultsNotAvailable);
        }
    }

    private async Task ProcessTestsExecutionResult(
        Submission submission,
        ExecutionResultResponseModel executionResult)
    {
        submission.IsCompiledSuccessfully = executionResult.IsCompiledSuccessfully;
        submission.CompilerComment = executionResult.CompilerComment;
        submission.Points = executionResult.TaskResult.Points;

        if (!executionResult.IsCompiledSuccessfully)
        {
            await this.UpdateResults(submission);
        }

        var testResults = executionResult
                              .TaskResult
                              ?.TestResults
                          ?? Enumerable.Empty<TestResultResponseModel>();

        submission.TestRuns.AddRange(testResults.Select(testResult =>
            new TestRun
            {
                CheckerComment = testResult.CheckerDetails.Comment,
                ExpectedOutputFragment = testResult.CheckerDetails.ExpectedOutputFragment,
                UserOutputFragment = testResult.CheckerDetails.UserOutputFragment,
                ExecutionComment = testResult.ExecutionComment,
                MemoryUsed = testResult.MemoryUsed,
                ResultType = (TestRunResultType)Enum.Parse(typeof(TestRunResultType), testResult.ResultType),
                TestId = testResult.Id,
                TimeUsed = testResult.TimeUsed,
            }));

        submission.Processed = true;
        this.submissionsData.Update(submission);
        await this.submissionsData.SaveChanges();

        await this.UpdateResults(submission);
    }

    private async Task UpdateResults(Submission submission)
    {
        await this.SaveParticipantScore(submission);

        CacheTestRuns(submission);
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
}