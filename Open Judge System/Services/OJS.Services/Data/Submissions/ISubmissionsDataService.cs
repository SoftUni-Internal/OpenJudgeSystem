namespace OJS.Services.Data.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsDataService : IService
    {
        Submission GetById(int id);

        Submission GetBestForParticipantByProblem(int participantId, int problemId);

        IQueryable<Submission> GetAll();

        IQueryable<Submission> GetAllWithDeleted();

        IQueryable<Submission> GetByIdQuery(int id);

        IQueryable<Submission> GetAllByProblem(int problemId);

        IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId);
        
        IQueryable<IGrouping<int, Submission>> GetFirstBestForProblemIdsByParticipantGroupedByProblemId(
            IEnumerable<int> problemIds, int participantId);
        
        IQueryable<IGrouping<int, Submission>> GetWithMaxPointsByParticipantIds(IEnumerable<int> participantIds);

        IQueryable<Submission> GetAllFromContestsByLecturer(string lecturerId);

        IQueryable<Submission> GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
            DateTime createdBeforeDate,
            DateTime nonBestCreatedBeforeDate);

        IQueryable<Submission> GetAllHavingPointsExceedingLimit();

        IEnumerable<int> GetIdsByProblem(int problemId);

        IQueryable<IGrouping<int, Submission>> GetLastSubmittedForParticipants(IEnumerable<int> participantId);
        
        bool IsOfficialById(int id);

        void SetAllToUnprocessedByProblem(int problemId);

        void DeleteByProblem(int problemId);

        void Update(Submission submission);
        void Update(Submission submission, bool withSaveChanges);

        void RemoveTestRunsCacheByProblem(int problemId);

        void SaveChanges();
    }
}