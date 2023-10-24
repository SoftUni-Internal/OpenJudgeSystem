namespace OJS.Services.Business.ParticipantScores
{
    using System.Linq;
    using OJS.Common.Helpers;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using MissingFeatures;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Data.Models;
    using OJS.Services.Business.ParticipantScores.Models;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;
    using System.Text.RegularExpressions;
    using System.IO;
    using System.IO.Compression;
    using System.Text;

    public class ParticipantScoresBusinessService : IParticipantScoresBusinessService
    {
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IParticipantsDataService participantsData;
        private readonly IContestsDataService contestsDataService;

        public ParticipantScoresBusinessService(
            IParticipantScoresDataService participantScoresData,
            ISubmissionsDataService submissionsData,
            IParticipantsDataService participantsData,
            IContestsDataService contestsDataService)
        {
            this.participantScoresData = participantScoresData;
            this.submissionsData = submissionsData;
            this.participantsData = participantsData;
            this.contestsDataService = contestsDataService;
        }

        public void RecalculateForParticipantByProblem(int participantId, int problemId)
        {
            var submission = this.submissionsData.GetBestForParticipantByProblem(participantId, problemId);

            if (submission != null)
            {
                this.participantScoresData.ResetBySubmission(submission);
            }
            else
            {
                this.participantScoresData.DeleteForParticipantByProblem(participantId, problemId);
            }
        }

        public void NormalizeAllPointsThatExceedAllowedLimit()
        {
                this.NormalizeSubmissionPoints();
                this.NormalizeParticipantScorePoints();
        }

        public CategoryContestsParticipationSummary GetCategoryParticipationSummary(
            int categoryId, 
            bool showHidden,
            bool official)
        {
            return this.GetCategoryParticipationSummaryData(categoryId, showHidden, official);
        }

        public CategoryContestsParticipationSummary GetCategoryParticipationSummary(
            int categoryId, 
            bool showHidden)
        {
            return this.GetCategoryParticipationSummaryData(categoryId, showHidden);
        }
        private CategoryContestsParticipationSummary GetCategoryParticipationSummaryData(
            int categoryId,
            bool showHidden,
            bool official = true)
        {
            var contests = this.contestsDataService
                .GetAllNotDeletedByCategory(categoryId, showHidden)
                .Include(c => c.ProblemGroups)
                .Include(c => c.ProblemGroups.Select(pg => pg.Problems))
                .Where(c => c.Participants.Any(p => p.IsOfficial == official))
                .OrderByDescending(c => c.CreatedOn)
                .ToList();

            var participants = this.participantsData
                .GetAllByContestIdsAndIsOfficial(contests.Select(c => c.Id), official)
                .Include(p => p.User)
                .Include(p => p.Submissions)
                .Include(p => p.Scores)
                .Include(p => p.Problems)
                .Where(p => p.Scores.Count != 0)
                .OrderBy(p => p.Contest.OrderBy)
                .ToList();

            var results = new List<ParticipantScoresSummaryModel>();
            participants
                .ChunkBy(GlobalConstants.BatchOperationsChunkSize)
                .ForEach(batch =>
                    results.AddRange(this.GetStatisticsForParticipants(batch, StatisticsLevel.ProblemGroup)));

            var maxProblemsCount = results
                .Select(ps => ps.ProblemsCount)
                .Max();

            return new CategoryContestsParticipationSummary
            {
                MaxProblemsCount = maxProblemsCount,
                Results = results,
            };
        }
        public ContestParticipationSummary GetContestParticipationSummary(int contestId, bool official = true)
        {
            var participants = this.participantsData.GetAllByContestAndIsOfficial(contestId, official)
                .Include(p => p.User)
                .Include(p => p.Submissions)
                .Include(p => p.Scores)
                .Include(p => p.Problems)
                .Where(p => p.Scores.Count != 0)
                .ToList();

            var results = new List<ParticipantScoresSummaryModel>();
            participants
                .ChunkBy(GlobalConstants.BatchOperationsChunkSize)
                .ForEach(batch =>
                    results.AddRange(this.GetStatisticsForParticipants(batch, StatisticsLevel.ProblemName)));

            return new ContestParticipationSummary
            {
                Results = results,
            };
        }

        private IEnumerable<ParticipantScoresSummaryModel> GetStatisticsForParticipants(
            IEnumerable<Participant> participants,
            StatisticsLevel level)
        {
            var participantsTimeAndSubmissionsInfo = this.BuildStatisticsData(participants);

            return participantsTimeAndSubmissionsInfo
                .Where(i => i.MaximumPointsSubmissionsByProblems.Any())
                .Select(participantSummary => this.SummarizeParticipationWithMaximumPoints(participantSummary, level))
                .ToList();
        }

        private ParticipantScoresSummaryModel SummarizeParticipationWithMaximumPoints(
            ParticipantSummaryInfoServiceModel participantInfo,
            StatisticsLevel level)
        {
            var topScoreSubmissionsOrderedByCreatedOn = participantInfo
                .MaximumPointsSubmissionsByProblems
                .OrderBy(s => s.Submission.CreatedOn);

            var problemNameToTimeTakenBetweenBest = this.CalculateTimeTakenBetweenBestForProblems(
                topScoreSubmissionsOrderedByCreatedOn,
                level,
                participantInfo.UserStartTime);

            if (level == StatisticsLevel.ProblemGroup)
            {
                problemNameToTimeTakenBetweenBest = NormalizeProblemGroupIndexesAndEmptyValues(
                    problemNameToTimeTakenBetweenBest,
                    participantInfo.ProblemGroups.Select(pg => pg.OrderBy));
            }
            else
            {
                problemNameToTimeTakenBetweenBest = problemNameToTimeTakenBetweenBest
                    .OrderBy(v => v.Value.ProblemGroup)
                    .ToDictionary(k => k.Key, v => v.Value);
            }


            return new ParticipantScoresSummaryModel
            {
                ParticipantName = participantInfo.Participant.User.UserName,
                ContestId = participantInfo.Participant.Contest.Id,
                ContestName = participantInfo.Participant.Contest.Name,
                ProblemsCount = participantInfo.ProblemGroups.Count,
                ProblemToMinutesTakenToSolve = problemNameToTimeTakenBetweenBest,
                PointsTotal = participantInfo.Participant.Scores.Select(s => s.Points).Sum(),
                TimeTotal = participantInfo.TimeInContest,
            };
        }

        private Dictionary<string, ParticipantSummarySubmissionInfoServiceModel>
            CalculateTimeTakenBetweenBestForProblems(
                IEnumerable<MaximumResultSubmissionByProblemServiceModel> maxSubmissionsBySubmissionTime,
                StatisticsLevel level,
                DateTime userStartTime)
        {
            var maxSubmissionsBySubmissionTimeList = maxSubmissionsBySubmissionTime.ToList();

            var values = maxSubmissionsBySubmissionTimeList
                .Skip(1)
                .Select((laterSubmission, index) =>
                {
                    var earlierSubmission = maxSubmissionsBySubmissionTimeList[index];

                    return new ParticipationStatisticsEarlierAndLaterSubmissionModel
                    {
                        EarlierSubmission = earlierSubmission,
                        LaterSubmission = laterSubmission,
                    };
                });

            var result = this.GetTimeTakenAndLengthStatistics(values, level);

            var earliestSubmission = maxSubmissionsBySubmissionTimeList.First().Submission;
            result[this.GetSubmissionKey(level, earliestSubmission)] =
                this.GetSubmissionTimeTakenToSolveAndLength(
                    result,
                    earliestSubmission,
                    level,
                    userStartTime);

            return result;
        }

        private Dictionary<string, ParticipantSummarySubmissionInfoServiceModel> GetTimeTakenAndLengthStatistics(
            IEnumerable<ParticipationStatisticsEarlierAndLaterSubmissionModel> submissionPairsValues,
            StatisticsLevel level)
        {
            var resultDict = new Dictionary<string, ParticipantSummarySubmissionInfoServiceModel>();

            submissionPairsValues.ForEach(v =>
            {
                resultDict[this.GetSubmissionKey(level, v.LaterSubmission.Submission)] =
                    this.GetSubmissionTimeTakenToSolveAndLength(
                        resultDict,
                        v.LaterSubmission.Submission,
                        level,
                        v.EarlierSubmission.Submission.CreatedOn);
            });

            return resultDict;
        }

        private string GetSubmissionKey(StatisticsLevel level, Submission submission)
            => level == StatisticsLevel.ProblemName
                ? submission.Problem.Name
                : submission.Problem.ProblemGroup.OrderBy.ToString();

        private ParticipantSummarySubmissionInfoServiceModel GetSubmissionTimeTakenToSolveAndLength(
            Dictionary<string, ParticipantSummarySubmissionInfoServiceModel> resultDict,
            Submission submission,
            StatisticsLevel level,
            DateTime timeToCompareWith)
        {
            var timeTaken = submission.CreatedOn.GetMinutesDifferenceRounded(timeToCompareWith);

            var submissionLength = this.GetSubmissionCodeLength(submission);

            var problemIndex = this.GetSubmissionKey(level, submission);

            if (resultDict.ContainsKey(problemIndex) && level == StatisticsLevel.ProblemGroup)
            {
                (submissionLength, timeTaken) = this.CalculateSubmissionAverageTimeTakenAndLength(
                    resultDict[problemIndex],
                    timeTaken,
                    submissionLength);
            }

            return new ParticipantSummarySubmissionInfoServiceModel
            {
                ProblemGroup = submission.Problem.ProblemGroup.OrderBy,
                Length = submissionLength,
                TimeTaken = timeTaken
            };
        }

        private Tuple<int, double> CalculateSubmissionAverageTimeTakenAndLength(
            ParticipantSummarySubmissionInfoServiceModel existingValue,
            double timeTaken,
            int length)
            => new Tuple<int, double>(
                Convert.ToInt32((existingValue.Length + length) / 2),
                Math.Round((existingValue.TimeTaken + timeTaken) / 2));

        private int GetSubmissionCodeLength(Submission submission)
            => submission.IsBinaryFile
                ? this.GetFileSubmissionLength(submission.Content)
                : this.GetCodeSubmissionLength(submission.ContentAsString);

        private int GetFileSubmissionLength(byte[] submissionFileContent)
        {
            using (var zip = new ZipArchive(new MemoryStream(submissionFileContent), ZipArchiveMode.Read, true))
            {
                var eligibleFileEntries = zip
                    .Entries
                    .Where(e =>
                        GlobalConstants.ParticipationStatisticsFileSubmissionsAllowedExtensions
                            .Any(fe => e.FullName.EndsWith(fe)));

                var sumFileContentsLength = eligibleFileEntries
                    .Select(e =>
                    {
                        using (var reader = new StreamReader(e.Open(), Encoding.UTF8))
                        {
                            var fileContent = reader.ReadToEnd();

                            return this.GetCodeSubmissionLength(fileContent);
                        }
                    })
                    .Sum();

                return sumFileContentsLength;
            }
        }

        private int GetCodeSubmissionLength(string codeContentAsString)
            => codeContentAsString.CountNewLines();

        private IEnumerable<ParticipantSummaryInfoServiceModel> BuildStatisticsData(
            IEnumerable<Participant> participants)
        {
            return participants
                .Where(p => p.Submissions.Any())
                .Select(p => new ParticipantSummaryInfoServiceModel
                {
                    Participant = p,
                    UserStartTime = p.ParticipationStartTime ?? p.CreatedOn,
                    ProblemGroups = p.Contest.ProblemGroups
                        .Where(group => !group.IsDeleted && group.Problems.Any(problem => !problem.IsDeleted))
                        .ToList(),
                    TimeInContest = CalculateTimeInMinutesFromParticipationStartToLastSubmission(p),
                    MaximumPointsSubmissionsByProblems = p.Submissions
                        .Where(s => s.Points == s.Problem.MaximumPoints)
                        .GroupBy(s => s.ProblemId)
                        .Select(g
                            => new MaximumResultSubmissionByProblemServiceModel
                            {
                                ProblemId = g.Key.Value,
                                Submission = g.OrderBy(s => s.CreatedOn).FirstOrDefault()
                            })
                });
        }

        private double CalculateTimeInMinutesFromParticipationStartToLastSubmission(Participant participant)
            => participant.Submissions
                .Select(s => s.CreatedOn)
                .OrderByDescending(s => s)
                .FirstOrDefault()
                .GetMinutesDifferenceRounded(participant.ParticipationStartTime ?? participant.CreatedOn);

        private Dictionary<string, ParticipantSummarySubmissionInfoServiceModel>
            NormalizeProblemGroupIndexesAndEmptyValues(
                Dictionary<string, ParticipantSummarySubmissionInfoServiceModel> values,
                IEnumerable<int> problemGroupsOrderBy)
        {
            var sortedOrderedBy = problemGroupsOrderBy
                .OrderBy(n => n)
                .Distinct()
                .ToList();

            var valuesProblemGroups = values.Select(v => v.Value.ProblemGroup);

            return sortedOrderedBy
                .Select(n => new
                {
                    Key = sortedOrderedBy.IndexOf(n) + 1,
                    Value = valuesProblemGroups.Contains(n)
                        ? values[n.ToString()]
                        : new ParticipantSummarySubmissionInfoServiceModel
                        {
                            ProblemGroup = n,
                            TimeTaken = 0,
                            Length = 0
                        }
                })
                .ToDictionary(k => k.Key.ToString(), v => v.Value);
        }

        private void NormalizeSubmissionPoints()
        {
            var itemsToTake = 300;
            var itemsToSkip = 0;
            var submissionsWithProblemMaxPoints = this.submissionsData
                .GetAllHavingPointsExceedingLimit()
                .Select(s => new
                {
                    Submission = s,
                    ProblemMaxPoints = s.Problem.MaximumPoints,
                })
                .ToList();
            while (itemsToSkip < submissionsWithProblemMaxPoints.Count)
            {
                    submissionsWithProblemMaxPoints.Skip(itemsToSkip).Take(itemsToTake).ToList().ForEach(x =>
                    {
                        x.Submission.Points = x.ProblemMaxPoints;

                        this.submissionsData.Update(x.Submission, withSaveChanges:false);
                    });
                    this.submissionsData.SaveChanges();
                    itemsToSkip += itemsToTake;
            }
        }


        private void NormalizeParticipantScorePoints()
        {
            this.participantScoresData
                    .GetAllHavingPointsExceedingLimit()
                    .Select(ps => new
                    {
                        ParticipantScore = ps,
                        ProblemMaxPoints = ps.Problem.MaximumPoints,
                        Particinapnt = ps.Participant,
                    })
                    .ToList()
                    .ForEach(x =>
                        this.participantScoresData.UpdateBySubmissionAndPoints(
                            x.ParticipantScore,
                            x.ParticipantScore.SubmissionId,
                            x.ProblemMaxPoints,
                            x.Particinapnt,
                            withSaveChanges:false));
                this.participantsData.SaveChanges();
        }
    }
}