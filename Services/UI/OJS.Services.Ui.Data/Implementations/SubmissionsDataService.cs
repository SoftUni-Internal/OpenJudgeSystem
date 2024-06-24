namespace OJS.Services.Ui.Data.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Common.Extensions;
using OJS.Data;
using OJS.Data.Models.Submissions;
using OJS.Services.Common.Data.Implementations;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SubmissionsDataService : DataService<Submission>, ISubmissionsDataService
{
    private readonly IDatesService datesService;

    public SubmissionsDataService(OjsDbContext db, IDatesService datesService)
        : base(db)
        => this.datesService = datesService;

    public TServiceModel? GetSubmissionById<TServiceModel>(int id)
        => this.GetByIdQuery(id)
            .AsNoTracking()
            .MapCollection<TServiceModel>()
            .FirstOrDefault();

    public async Task<IEnumerable<TServiceModel>> GetLatestSubmissions<TServiceModel>(int submissionsPerPage)
        => await this.GetQuery(
                orderBy: s => s.Id,
                descending: true,
                take: submissionsPerPage)
            .MapCollection<TServiceModel>()
            .ToEnumerableAsync();

    public async Task<PagedResult<TServiceModel>> GetLatestSubmissions<TServiceModel>(int submissionsPerPage, int pageNumber)
            => await this.GetQuery(
                    filter: s => !s.IsDeleted,
                    orderBy: s => s.Id,
                    descending: true)
                .MapCollection<TServiceModel>()
                .ToPagedResultAsync(submissionsPerPage, pageNumber);

    // TODO: https://github.com/SoftUni-Internal/exam-systems-issues/issues/903
    public async Task<PagedResult<TServiceModel>> GetLatestSubmissionsByUserParticipations<TServiceModel>(
        IEnumerable<int?> userParticipantsIds,
        int submissionsPerPage,
        int pageNumber)
            => await this.GetQuery(
                    filter: s => !s.IsDeleted && userParticipantsIds.Contains(s.ParticipantId!),
                    orderBy: s => s.Id,
                    descending: true)
                .MapCollection<TServiceModel>()
                .ToPagedResultAsync(submissionsPerPage, pageNumber);

    public Submission? GetBestForParticipantByProblem(int participantId, int problemId) =>
        this.GetAllByProblemAndParticipant(problemId, participantId)
            .Where(s => s.Processed)
            .OrderByDescending(s => s.Points)
            .ThenByDescending(s => s.Id)
            .FirstOrDefault();

    public IQueryable<Submission> GetAllByProblem(int problemId)
        => this.GetQuery(s => s.ProblemId == problemId);

    public IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId)
        => this.GetQuery(
            filter: s => s.ParticipantId == participantId && s.ProblemId == problemId,
            orderBy: q => q.CreatedOn,
            descending: true);

    public IQueryable<Submission> GetAllFromContestsByLecturer(string lecturerId)
        => this.GetQuery(s =>
                (s.IsPublic.HasValue && s.IsPublic.Value) ||
                s.Problem!.ProblemGroup.Contest.LecturersInContests.Any(l => l.LecturerId == lecturerId) ||
                s.Problem!.ProblemGroup.Contest.Category!.LecturersInContestCategories.Any(l =>
                    l.LecturerId == lecturerId))
            .Include(s => s.Problem!.ProblemGroup.Contest.LecturersInContests)
            .Include(s => s.Problem!.ProblemGroup.Contest.Category!.LecturersInContestCategories);

    public IQueryable<Submission> GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
        DateTime createdBeforeDate,
        DateTime nonBestCreatedBeforeDate) =>
        this.GetQuery(s => s.CreatedOn < createdBeforeDate ||
                           (s.CreatedOn < nonBestCreatedBeforeDate &&
                            s.Participant!.Scores.All(ps => ps.SubmissionId != s.Id)));

    public IQueryable<Submission> GetAllHavingPointsExceedingLimit()
        => this.GetQuery(s => s.Points > s.Problem!.MaximumPoints);

    public IQueryable<int> GetIdsByProblem(int problemId)
        => this.GetAllByProblem(problemId)
            .Select(s => s.Id);

    public IQueryable<Submission> GetAllForUserByContest(int contestId, string userId)
        => this.GetQuery(
        filter: s => s.Participant!.UserId == userId
                     && s.Problem.ProblemGroup.ContestId == contestId
                     && s.Problem.ShowResults,
        orderBy: s => s.Id,
        descending: true);

    public IQueryable<Submission> GetAllByIdsQuery(IEnumerable<int> ids)
        => this.GetQuery()
            .Where(s => ids.Contains(s.Id));

    public bool IsOfficialById(int id) =>
        this.GetByIdQuery(id)
            .Any(s => s.Participant!.IsOfficial);

    public Submission? GetLastSubmitForParticipant(int participantId) =>
        this.GetQuery(s => s.ParticipantId == participantId)
            .OrderByDescending(s => s.CreatedOn)
            .FirstOrDefault();

    public void SetAllToUnprocessedByProblem(int problemId) =>
        this.GetAllByProblem(problemId)
            .UpdateFromQueryAsync(s => new Submission { Processed = false });

    public void DeleteByProblem(int problemId) =>
        this.Delete(s => s.ProblemId == problemId);

    public void RemoveTestRunsCacheByProblem(int problemId) =>
        this.GetAllByProblem(problemId)
            .UpdateFromQueryAsync(s => new Submission { TestRunsCache = null });

    public int GetUserSubmissionTimeLimit(int participantId, int limitBetweenSubmissions)
    {
        if (limitBetweenSubmissions <= 0)
        {
            return 0;
        }

        var lastSubmission = this.GetLastSubmitForParticipant(participantId);

        if (lastSubmission != null)
        {
            // check if the submission was sent after the submission time limit has passed
            var latestSubmissionTime = lastSubmission.CreatedOn;
            var differenceBetweenSubmissions = this.datesService.GetUtcNow() - latestSubmissionTime;
            // Adding 5 seconds to compensate for potential difference between server and client time
            if (differenceBetweenSubmissions.TotalSeconds + 5 < limitBetweenSubmissions)
            {
                return limitBetweenSubmissions - differenceBetweenSubmissions.TotalSeconds.ToInt();
            }
        }

        return 0;
    }

    public bool HasUserNotProcessedSubmissionForProblem(int problemId, string userId) =>
        this.GetQuery().Any(s => s.ProblemId == problemId && s.Participant!.UserId == userId && !s.Processed);

    public bool HasUserNotProcessedSubmissionForContest(int contestId, string userId) =>
        this.GetQuery().Any(s => s.Problem.ProblemGroup.ContestId == contestId
                               && s.Participant!.UserId == userId && !s.Processed);

    public async Task<int> GetProblemIdBySubmission(int submissionId)
        => await this.GetByIdQuery(submissionId)
            .Select(p => p.Problem.Id)
            .FirstOrDefaultAsync();

    public async Task<int> GetSubmissionsPerDayCount()
        => await this.GetQuery().AnyAsync()
            ? await this.GetQuery().GroupBy(x => new { x.CreatedOn.Year, x.CreatedOn.DayOfYear })
                .Select(x => x.Count())
                .AverageAsync()
                .ToInt()
            : 0;

    public async Task<int> GetParticipantIdBySubmission(int submissionId)
        => await this.GetByIdQuery(submissionId)
            .Select(p => p.Participant!.Id)
            .FirstOrDefaultAsync();

    private IQueryable<Submission> GetByIdQuery(int id) =>
        this.GetQuery(s => s.Id == id);
}