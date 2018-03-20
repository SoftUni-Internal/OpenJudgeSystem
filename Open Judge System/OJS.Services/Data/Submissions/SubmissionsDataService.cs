﻿namespace OJS.Services.Data.Submissions
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsDataService : ISubmissionsDataService
    {
        private readonly IEfDeletableEntityRepository<Submission> submissions;

        public SubmissionsDataService(IEfDeletableEntityRepository<Submission> submissions) =>
            this.submissions = submissions;

        public Submission GetBestForParticipantByProblem(int participantId, int problemId) =>
            this.submissions
                .All()
                .Where(s => s.ParticipantId == participantId &&
                    s.ProblemId == problemId &&
                    !s.IsDeleted &&
                    s.Processed)
                .OrderByDescending(x => x.Points)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

        public IQueryable<Submission> GetByIdQuery(int id) =>
            this.submissions.All().Where(s => s.Id == id);
    }
}