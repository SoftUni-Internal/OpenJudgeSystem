namespace OJS.Services.Business.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Business.Submissions.Models;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.Submissions.ArchivedSubmissions;
    using OJS.Workers.Common.Models;

    public class SubmissionsBusinessService : ISubmissionsBusinessService
    {
        private readonly IEfDeletableEntityRepository<Submission> submissions;
        private readonly IEfGenericRepository<TestRun> testRuns;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IArchivedSubmissionsDataService archivedSubmissionsData;
        private readonly IParticipantScoresDataService participantScoresData;

        public SubmissionsBusinessService(
            IEfDeletableEntityRepository<Submission> submissions,
            IEfGenericRepository<TestRun> testRuns,
            ISubmissionsDataService submissionsData,
            IArchivedSubmissionsDataService archivedSubmissionsData,
            IParticipantScoresDataService participantScoresData)
        {
            this.submissions = submissions;
            this.testRuns = testRuns;
            this.submissionsData = submissionsData;
            this.archivedSubmissionsData = archivedSubmissionsData;
            this.participantScoresData = participantScoresData;
        }

        public IQueryable<Submission> GetAllForArchiving()
        {
            var archiveBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.BestSubmissionEligibleForArchiveAgeInYears);

            var archiveNonBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.NonBestSubmissionEligibleForArchiveAgeInYears);

            return this.submissionsData
                .GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
                    archiveBestSubmissionsLimit,
                    archiveNonBestSubmissionsLimit);
        }

        public void RecalculatePointsByProblem(int problemId)
        {
            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                var problemSubmissions = this.submissionsData
                    .GetAllByProblem(problemId)
                    .Include(s => s.TestRuns)
                    .Include(s => s.TestRuns.Select(tr => tr.Test))
                    .ToList();

                var submissionResults = this.GetSubmissionResults(problemSubmissions);

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

                    if (submissionResult.ParticipantId == null || submissionResult.ParticipantId == 0)
                    {
                        continue;
                    }

                    var participantId = submissionResult.ParticipantId;

                    if (!topResults.ContainsKey(participantId) || topResults[participantId].Points < points)
                    {
                        topResults[participantId] = new ParticipantScoreModel
                        {
                            Points = points,
                            SubmissionId = submission.Id
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

                this.submissions.SaveChanges();

                var participants = topResults.Keys.ToList();

                var existingScores = this.participantScoresData
                    .GetAllByProblem(problemId)
                    .Where(p => participants.Contains(p.ParticipantId))
                    .ToList();

                foreach (var existingScore in existingScores)
                {
                    var topScore = topResults[existingScore.ParticipantId];

                    existingScore.Points = topScore.Points;
                    existingScore.SubmissionId = topScore.SubmissionId;
                }

                this.submissions.SaveChanges();

                scope.Complete();
            }
        }

        private IEnumerable<dynamic> GetSubmissionResults(IEnumerable<Submission> problemSubmissions)
            => problemSubmissions
                .Select(s => new
                {
                    s.Id,
                    s.ParticipantId,
                    CorrectTestRuns = s.TestRuns.Count(t =>
                        t.ResultType == TestRunResultType.CorrectAnswer &&
                        !t.Test.IsTrialTest),
                    AllTestRuns = s.TestRuns.Count(t => !t.Test.IsTrialTest),
                    MaxPoints = s.Problem.MaximumPoints
                })
                .ToList();


        public int HardDeleteAllArchived()
        {
            return HardDeleteArchived();
        }

        /// <summary>
        /// Deletes archived submissions from OnlineJudgeSystem Db and marks them as Hard Deleted in OnlineJudgeSystemArchives.
        /// </summary>
        /// <param name="deleteCountLimit">Specifies a limit to the number of submissions deleted, if omitted or 0 is passed, delete all available, without limits.</param>
        /// <returns></returns>
        public int HardDeleteArchived(int deleteCountLimit = 0)
        {
            var hardDeletedCount = 0;

            IEnumerable<IQueryable<ArchivedSubmission>> submissionBatches = null;

            if (deleteCountLimit > 0)
            {
                submissionBatches = this.archivedSubmissionsData
                 .GetAllUndeletedFromMainDatabase()
                 .Distinct()
                 .OrderBy(x => x.Id)
                 .InSelfModifyingBatches(GlobalConstants.BatchOperationsChunkSize, deleteCountLimit);
            }
            else
            {
                submissionBatches = this.archivedSubmissionsData
                .GetAllUndeletedFromMainDatabase()
                .Distinct()
                .OrderBy(x => x.Id)
                .InSelfModifyingBatches(GlobalConstants.BatchOperationsChunkSize);
            }

            foreach (var submissionIdsBatch in submissionBatches)
            {
                var archivedIds = submissionIdsBatch
                    .Select(s => s.Id)
                    .ToSet();

                if(archivedIds.Count == 0)
                {
                    break;
                }

                // Get a batch of submissions marked for deletion in the Archives
                var idsSet = this.submissionsData.GetAllWithDeleted()
                    .Where(s => archivedIds.Contains(s.Id))
                    .Select(x => x.Id)
                    .ToSet();

                // This covers the case where Submissions were deleted from the Main DB, but were not marked as deleted in Archives
                if(idsSet.Count > 0)
                {
                    using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
                    {
                        this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(idsSet);

                        testRuns.Delete(
                            tr => idsSet.Contains(tr.SubmissionId),
                            batchSize: GlobalConstants.BatchOperationsChunkSize);

                        try
                        {
                            this.submissions.HardDelete(s => idsSet.Contains(s.Id));
                        }
                        catch (AggregateException ex)
                        {
                            Console.WriteLine(ex);
                            scope.Dispose();
                            continue;
                        }
                        scope.Complete();
                    }
                }
                
                // Set all selected submissions to `deleted from main db`
                foreach (var archivedIdsBatch in archivedIds.InBatches(GlobalConstants.BatchOperationsChunkSize / 10))
                {
                    this.archivedSubmissionsData.SetToHardDeletedFromMainDatabaseByIds(archivedIdsBatch);
                }

                hardDeletedCount += idsSet.Count;
            }

            return hardDeletedCount;
        }
    }
}