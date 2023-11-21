namespace OJS.Services.Data.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EntityFramework.Extensions;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using System.Data.Entity;

    public class SubmissionsDataService : ISubmissionsDataService
    {
        private readonly IEfDeletableEntityRepository<Submission> submissions;
        private readonly IEfGenericRepository<ParticipantScore> participantScores;

        public SubmissionsDataService(
            IEfDeletableEntityRepository<Submission> submissions,
            IEfGenericRepository<ParticipantScore> participantScores)
        {
            this.submissions = submissions;
            this.participantScores = participantScores;
        }

        public Submission GetById(int id) => this.submissions.GetById(id);

        public Submission GetBestForParticipantByProblem(int participantId, int problemId) =>
            this.GetAllByProblemAndParticipant(problemId, participantId)
                .Where(s => s.Processed)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.Id)
                .FirstOrDefault();

        public IQueryable<Submission> GetAll() =>
            this.submissions.All();

        public IQueryable<Submission> GetAllWithDeleted() =>
            this.submissions.AllWithDeleted();

        public IQueryable<Submission> GetByIdQuery(int id) =>
            this.GetAll()
                .Where(s => s.Id == id);

        public IQueryable<Submission> GetAllByProblem(int problemId) =>
            this.GetAll()
                .Where(s => s.ProblemId == problemId);

        public IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId) =>
            this.GetAllByProblem(problemId)
                .Where(s => s.ParticipantId == participantId);

        public IQueryable<IGrouping<int, Submission>> GetFirstBestForProblemIdsByParticipantGroupedByProblemId(
            IEnumerable<int> problemIds,
            int participantId)
            => this.GetAll()
                .Include(s => s.Problem)
                .Where(s => problemIds.Contains(s.Problem.Id))
                .Where(s => s.ParticipantId == participantId)
                .Where(s => s.Points == s.Problem.MaximumPoints)
                .OrderBy(s => s.CreatedOn)
                .GroupBy(s => s.Problem.Id);

        public IQueryable<IGrouping<int, Submission>> GetWithMaxPointsByParticipantIds(IEnumerable<int> participantIds)
            => this.GetAll()
                .AsNoTracking()
                .Where(s => s.Points == s.Problem.MaximumPoints)
                .Where(s => participantIds.Contains(s.ParticipantId.Value))
                .GroupBy(s => s.ParticipantId.Value)
                .Select(g => g.OrderBy(s => s.CreatedOn).FirstOrDefault())
                .GroupBy(s => s.ParticipantId.Value);

        public IQueryable<Submission> GetAllFromContestsByLecturer(string lecturerId) =>
            this.GetAll()
                .Where(s =>
                    (s.IsPublic.HasValue && s.IsPublic.Value) ||
                    s.Problem.ProblemGroup.Contest.Lecturers.Any(l => l.LecturerId == lecturerId) ||
                    s.Problem.ProblemGroup.Contest.Category.Lecturers.Any(l => l.LecturerId == lecturerId));

        public IQueryable<Submission> GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
            DateTime createdBeforeDate,
            DateTime nonBestCreatedBeforeDate) =>
            //Optimized for SQL
            from s in this.submissions.AllWithDeleted()
            join ps in this.participantScores.All() on s.Id equals ps.SubmissionId
            into participanWithScores
            from ps in participanWithScores.DefaultIfEmpty()
            where (s.CreatedOn < createdBeforeDate || (s.CreatedOn < nonBestCreatedBeforeDate && ps == null))
            select s;

        public IQueryable<Submission> GetAllHavingPointsExceedingLimit() =>
            this.GetAll()
                .Where(s => s.Points > s.Problem.MaximumPoints);

        public IEnumerable<int> GetIdsByProblem(int problemId) =>
            this.GetAllByProblem(problemId)
                .Select(s => s.Id);

        public IQueryable<IGrouping<int, Submission>> GetLastSubmittedForParticipants(IEnumerable<int> participantIds)
            => this.submissions
                .All()
                .Where(s => !s.IsDeleted)
                .Where(s => participantIds.Contains(s.ParticipantId.Value))
                .OrderByDescending(s => s.CreatedOn)
                .GroupBy(s => s.ParticipantId)
                .Select(s => s.FirstOrDefault())
                .GroupBy(s => s.ParticipantId.Value);

        public bool IsOfficialById(int id) =>
            this.GetByIdQuery(id)
                .Any(s => s.Participant.IsOfficial);

        public void SetAllToUnprocessedByProblem(int problemId) =>
            this.GetAllByProblem(problemId)
                .Update(s => new Submission
                {
                    Processed = false,
                    StartedExecutionOn = null,
                    CompletedExecutionOn = null,
                    WorkerEndpoint = null,
                });

        public void DeleteByProblem(int problemId) =>
            this.submissions.Delete(s => s.ProblemId == problemId);

        public void Update(Submission submission)
        {
            this.submissions.Update(submission);
            this.submissions.SaveChanges();
        }

        public void Update(Submission submission, bool withSaveChanges)
        {
            if (withSaveChanges)
            {
                this.Update(submission);
            }
            else
            {
                this.submissions.Update(submission);
            }
        }
        
        public void RemoveTestRunsCacheByProblem(int problemId) =>
            this.GetAllByProblem(problemId)
                .Update(s => new Submission
                {
                    TestRunsCache = null
                });

        public void SaveChanges()
        {
            this.submissions.SaveChanges();
        }
    }
}