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

                    if (!submissionResult.ParticipantId.HasValue)
                    {
                        continue;
                    }

                    var participantId = submissionResult.ParticipantId.Value;

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

        public int HardDeleteAllArchived2()
        {
            var hardDeletedCount = 0;

            var submissionBatches = this.archivedSubmissionsData
                .GetAllUndeletedFromMainDatabase()
                .OrderBy(x => x.IsHardDeletedFromMainDatabase)
                .InBatches(GlobalConstants.BatchOperationsChunkSize);
            
            foreach (var submissionIdsBatch in submissionBatches)
            {
                var idsSet = submissionIdsBatch
                    .Select(s => s.Id)
                    .ToSet();

                using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
                {
                    this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(idsSet);

                    testRuns.Delete(
                        tr => idsSet.Contains(tr.SubmissionId), 
                        batchSize: GlobalConstants.BatchOperationsChunkSize);

                    try
                    {
                        this.submissions.HardDelete(
                            s => idsSet.Contains(s.Id),
                            batchSize: GlobalConstants.BatchOperationsChunkSize);
                    }
                    catch (AggregateException ex)
                    {
                        Console.WriteLine(ex);
                        scope.Dispose();
                        continue;
                    }

                    
                    this.archivedSubmissionsData.SetToHardDeletedFromMainDatabaseByIds(idsSet);

                    scope.Complete();
                }

                hardDeletedCount += idsSet.Count;
            }

            return hardDeletedCount;
        }
        
        
        public int HardDeleteAllArchived()
        {
            var hardDeletedCount = 0;

            var submissionBatches = this.archivedSubmissionsData
                .GetAllUndeletedFromMainDatabase()
                .Distinct()
                .OrderBy(x => x.IsHardDeletedFromMainDatabase)
                .InBatches(GlobalConstants.BatchOperationsChunkSize);
            
            foreach (var submissionIdsBatch in submissionBatches)
            {
                var archivedIds = submissionIdsBatch
                    .Select(s => s.Id)
                    .ToSet();

                // Some submissions are present in the Archives, but are not marked as `deleted from the main db`
                var idsSet = this.submissionsData.GetAll()
                    .Where(s => archivedIds.Contains(s.Id))
                    .Select(x => x.Id)
                    .ToSet();

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

                    // Set all selected submissions to `deleted from main db`
                    foreach (var archivedIdsBatch in archivedIds.InBatches(GlobalConstants.BatchOperationsChunkSize / 10))
                    {
                        this.archivedSubmissionsData.SetToHardDeletedFromMainDatabaseByIds(archivedIdsBatch);
                    }

                    scope.Complete();
                }

                hardDeletedCount += idsSet.Count;
            }

            return hardDeletedCount;
        }
    }
}