namespace OJS.Services.Administration.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Common.Data;

    public interface ISubmissionsDataService : IDataService<Submission>
    {
        Submission? GetBestForParticipantByProblem(int participantId, int problemId);

        IQueryable<Submission> GetByIdQuery(int id);

        IQueryable<Submission> GetAllWithParticipantProblemAndSubmissionType();

        IQueryable<Submission> GetAllByProblem(int problemId);

        IQueryable<Submission> GetAllByProblems(IEnumerable<int> problemIds);

        IQueryable<Submission> GetByIds(IEnumerable<int> ids);

        IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId);

        IQueryable<Submission> GetAllFromContestsByLecturer(string lecturerId);

        IQueryable<Submission> GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
            DateTime createdBeforeDate,
            DateTime nonBestCreatedBeforeDate);

        IQueryable<Submission> GetAllHavingPointsExceedingLimit();

        IQueryable<int> GetIdsByProblem(int problemId);

        bool IsOfficialById(int id);

        Task SetAllToUnprocessedByProblem(int problemId);

        void DeleteByProblem(int problemId);

        new void Update(Submission submission);

        void RemoveTestRunsCacheByProblem(int problemId);

        Task<IEnumerable<TServiceModel>> GetAllNonDeletedByProblemId<TServiceModel>(int problemId);

        Task<IEnumerable<int>> GetIdsByProblemId(int problemId);
    }
}