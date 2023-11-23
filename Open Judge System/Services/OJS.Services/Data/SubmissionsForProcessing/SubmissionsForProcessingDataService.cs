using System;
using System.Data.Entity;
using OJS.Common.Models;
using OJS.Services.Data.Submissions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;

namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Collections.Generic;
    using System.Linq;
    using MissingFeatures;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsForProcessingDataService : ISubmissionsForProcessingDataService
    {
        private readonly IEfGenericRepository<SubmissionForProcessing> submissionsForProcessing;
        private readonly IEfGenericRepository<Submission> submissions;

        public SubmissionsForProcessingDataService(
            IEfGenericRepository<SubmissionForProcessing> submissionsForProcessing,
            IEfGenericRepository<Submission> submissions)
        {
            this.submissionsForProcessing = submissionsForProcessing;
            this.submissions = submissions;
        }

        public SubmissionForProcessing GetBySubmission(int submissionId) =>
            this.submissionsForProcessing
                .All()
                .FirstOrDefault(sfp => sfp.SubmissionId == submissionId);

        public IQueryable<SubmissionForProcessing> GetAllUnprocessed() =>
            this.submissionsForProcessing
                .All()
                .Where(sfp => !sfp.Processed && !sfp.Processing);

        public ICollection<int> GetIdsOfAllProcessing() =>
            this.submissionsForProcessing
                .All()
                .Where(sfp => sfp.Processing && !sfp.Processed)
                .Select(sfp => sfp.Id)
                .ToList();

        public void AddOrUpdateBySubmissionIds(ICollection<int> submissionIds)
        {
            var newSubmissionsForProcessing = submissionIds
                .Select(sId => new SubmissionForProcessing
                {
                    SubmissionId = sId
                });

            var submissionsToGet = this.submissions
                .All()
                .Where(s => submissionIds.Contains(s.Id))
                .Include(s => s.Problem)
                .Include(s => s.Problem.ProblemGroup)
                .Include(s => s.Problem.ProblemGroup.Contest)
                .ToList();

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                newSubmissionsForProcessing.ForEach(sfp =>
                    this.AssignWorkerType(sfp, submissionsToGet.First(s => s.Id == sfp.SubmissionId)));

                submissionIds
                    .ChunkBy(GlobalConstants.BatchOperationsChunkSize)
                    .ForEach(chunk => this.submissionsForProcessing
                        .Delete(sfp => chunk.Contains(sfp.SubmissionId)));


                this.submissionsForProcessing.Add(newSubmissionsForProcessing);

                scope.Complete();
            }
        }

        public void AddOrUpdateBySubmission(Submission submission)
        {
            var submissionForProcessing = this.GetBySubmission(submission.Id);
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = false;
                submissionForProcessing.Processed = false;
                this.AssignWorkerType(submissionForProcessing, submission);
            }
            else
            {
                submissionForProcessing = new SubmissionForProcessing
                {
                    SubmissionId = submission.Id,
                };
                this.AssignWorkerType(submissionForProcessing, submission);
                this.submissionsForProcessing.Add(submissionForProcessing);
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void RemoveBySubmission(int submissionId)
        {
            var submissionForProcessing = this.GetBySubmission(submissionId);

            if (submissionForProcessing != null)
            {
                this.submissionsForProcessing.Delete(submissionId);
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void ResetProcessingStatusById(int id)
        {
            var submissionForProcessing = this.submissionsForProcessing.GetById(id);
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = false;
                submissionForProcessing.Processed = false;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void Clean() =>
            this.submissionsForProcessing.Delete(sfp => sfp.Processed && !sfp.Processing);

        public void Update(SubmissionForProcessing submissionForProcessing)
        {
            this.submissionsForProcessing.Update(submissionForProcessing);
            this.submissionsForProcessing.SaveChanges();
        }

        private void AssignWorkerType(SubmissionForProcessing submissionForProcessing, Submission submission)
        {
            submissionForProcessing.WorkerType = submission.WorkerType;
            if (submissionForProcessing.WorkerType != WorkerType.Default)
            {
                return;
            }

            var problem = submission.Problem;
            var strategyDetailsWorkerType = problem
                .ProblemSubmissionTypeExecutionDetails
                .Where(x => x.SubmissionTypeId == submission.SubmissionTypeId)
                .Select(x => x.WorkerType)
                .DefaultIfEmpty(WorkerType.Default)
                .FirstOrDefault();

            if (submissionForProcessing.WorkerType != WorkerType.Default)
            {
                submissionForProcessing.WorkerType = strategyDetailsWorkerType;
                return;
            }
            
            var contestWorkerType = problem.ProblemGroup.Contest.DefaultWorkerType;
            submissionForProcessing.WorkerType = contestWorkerType;
        }
    }
}