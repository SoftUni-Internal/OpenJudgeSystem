namespace OJS.Services.Ui.Data;

using OJS.Data.Models.Submissions;
using OJS.Services.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoftUni.Common.Models;

public interface ISubmissionsDataService : IDataService<Submission>
{
    TServiceModel? GetSubmissionById<TServiceModel>(int id);

    Task<IEnumerable<TServiceModel>> GetLatestSubmissions<TServiceModel>(int submissionsPerPage);

    Task<PagedResult<TServiceModel>> GetLatestSubmissions<TServiceModel>(int submissionsPerPage, int pageNumber);

    Task<PagedResult<TServiceModel>> GetLatestSubmissionsByUserParticipations<TServiceModel>(
        IEnumerable<int?> userParticipantsIds,
        int submissionsPerPage,
        int pageNumber);

    Task<int> GetTotalSubmissionsCount();

    Task<TServiceModel> GetParticipantBySubmission<TServiceModel>(int submissionId);

    Task<TServiceModel> GetProblemBySubmission<TServiceModel>(int submissionId);

    Task<int> GetSubmissionsPerDayCount();

    Submission? GetBestForParticipantByProblem(int participantId, int problemId);

    IQueryable<Submission> GetAllByProblem(int problemId);

    IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId);

    IQueryable<Submission> GetAllFromContestsByLecturer(string lecturerId);

    IQueryable<Submission> GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
        DateTime createdBeforeDate,
        DateTime nonBestCreatedBeforeDate);

    IQueryable<Submission> GetAllHavingPointsExceedingLimit();
    IQueryable<Submission> GetAllByIdsQuery(IEnumerable<int> ids);

    IQueryable<int> GetIdsByProblem(int problemId);

    bool IsOfficialById(int id);

    void SetAllToUnprocessedByProblem(int problemId);

    void DeleteByProblem(int problemId);

    void RemoveTestRunsCacheByProblem(int problemId);

    int GetUserSubmissionTimeLimit(int participantId, int limitBetweenSubmissions);

    bool HasUserNotProcessedSubmissionForProblem(int problemId, string userId);
}