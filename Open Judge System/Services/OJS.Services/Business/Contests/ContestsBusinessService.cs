namespace OJS.Services.Business.Contests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Business.Contests.Models;
    using OJS.Services.Common;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ParticipantScores;

    public class ContestsBusinessService : IContestsBusinessService
    {
        private readonly IEfDeletableEntityRepository<Contest> contests;
        private readonly IContestsDataService contestsData;
        private readonly IParticipantsDataService participantsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly IExamGroupsDataService examGroupsData;

        public ContestsBusinessService(
            IEfDeletableEntityRepository<Contest> contests,
            IContestsDataService contestsData,
            IParticipantsDataService participantsData,
            IParticipantScoresDataService participantScoresData,
            IExamGroupsDataService examGroupsData)
        {
            this.contests = contests;
            this.contestsData = contestsData;
            this.participantsData = participantsData;
            this.participantScoresData = participantScoresData;
            this.examGroupsData = examGroupsData;
        }

        public bool IsContestIpValidByContestAndIp(int contestId, string ip) =>
            this.contestsData
                .GetByIdQuery(contestId)
                .Any(c => !c.AllowedIps.Any() || c.AllowedIps.Any(ai => ai.Ip.Value == ip));

        public bool CanUserCompeteByContestByUserAndIsAdmin(
            int contestId,
            string userId,
            bool isAdmin,
            bool allowToAdminAlways = false)
        {
            var contest = this.contestsData.GetById(contestId);

            var isUserAdminOrLecturerInContest = isAdmin || this.contestsData
                .IsUserLecturerInByContestAndUser(contestId, userId);

            if (contest.IsOnline && !isUserAdminOrLecturerInContest)
            {
                var participant = contest.Participants.FirstOrDefault(p => p.UserId == userId && p.IsOfficial);

                if (participant == null)
                {
                    return contest.CanBeCompeted;
                }

                return participant.ParticipationEndTime >= DateTime.Now;
            }

            if (contest.CanBeCompeted || (isUserAdminOrLecturerInContest && allowToAdminAlways))
            {
                return true;
            }

            if (isUserAdminOrLecturerInContest && contest.IsActive)
            {
                return true;
            }

            return false;
        }

        // TODO: Extract different logic blocks in separate services
        public ServiceResult TransferParticipantsToPracticeById(int contestId)
        {
            var contest = this.contestsData.GetById(contestId);

            if (contest == null)
            {
                return new ServiceResult("Contest cannot be found");
            }

            if (contest.IsActive)
            {
                return new ServiceResult("The Contest is active and participants cannot be transferred");
            }

            var competeOnlyParticipants = contest.Participants
                .GroupBy(p => p.UserId)
                .Where(g => g.Count() == 1 && g.All(p => p.IsOfficial))
                .Select(gr => gr.FirstOrDefault());

            foreach (var participant in competeOnlyParticipants)
            {
                foreach (var participantScore in participant.Scores)
                {
                    participantScore.IsOfficial = false;
                }

                participant.IsOfficial = false;
            }

            var competeAndPracticeParticipants = contest.Participants
                .GroupBy(p => p.UserId)
                .Where(g => g.Count() == 2)
                .ToDictionary(grp => grp.Key, grp => grp.OrderBy(p => p.IsOfficial));

            var participantsForDeletion = new List<Participant>();

            foreach (var competeAndPracticeParticipant in competeAndPracticeParticipants)
            {
                var unofficialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key].First();
                var officialParticipant = competeAndPracticeParticipants[competeAndPracticeParticipant.Key].Last();
                participantsForDeletion.Add(officialParticipant);

                foreach (var officialParticipantSubmission in officialParticipant.Submissions)
                {
                    officialParticipantSubmission.Participant = unofficialParticipant;
                }

                var scoresForDeletion = new List<ParticipantScore>();

                foreach (var officialParticipantScore in officialParticipant.Scores)
                {
                    var unofficialParticipantScore = unofficialParticipant
                        .Scores
                        .FirstOrDefault(s => s.ProblemId == officialParticipantScore.ProblemId);

                    if (unofficialParticipantScore != null)
                    {
                        if (unofficialParticipantScore.Points < officialParticipantScore.Points ||
                            (unofficialParticipantScore.Points == officialParticipantScore.Points &&
                             unofficialParticipantScore.Id < officialParticipantScore.Id))
                        {
                            unofficialParticipantScore = officialParticipantScore;
                            unofficialParticipantScore.IsOfficial = false;
                            unofficialParticipantScore.Participant = unofficialParticipant;
                        }

                        scoresForDeletion.Add(officialParticipantScore);
                    }
                    else
                    {
                        officialParticipantScore.IsOfficial = false;
                        officialParticipantScore.Participant = unofficialParticipant;
                    }
                }

                this.participantScoresData.Delete(scoresForDeletion);
            }

            this.participantsData.Delete(participantsForDeletion);

            return ServiceResult.Success;
        }

        public void DeleteById(int id)
        {
            this.examGroupsData.RemoveContestByContest(id);

            this.contests.Delete(id);
            this.contests.SaveChanges();
        }

        public JudgeLoadResults CalculateLoadForContest(BaseContestBusinessModel model)
        {
            var responseModel = new JudgeLoadResults();

            responseModel.PreviousAverageProblemRunTimeInSeconds = model.PreviousAverageProblemRunTimeInSeconds.Value;
            responseModel.PreviousContestExpectedProblems = model.PreviousContestExpectedProblems.Value;
            responseModel.PreviousContestSubmissions = model.PreviousContestSubmissions.Value;
            responseModel.PreviousContestParticipants = model.PreviousContestParticipants.Value;

            this.GetJudgeLoadData(model, responseModel);
            GetDoomsDayScenario(model, responseModel);
            GetDistributionResults(model, responseModel);

            return responseModel;
        }

        public int GetContestSubmissionsAverageRunTimeSeconds(
            Contest contest,
            bool onlyOfficialSubmissions,
            int? maxAllowedSubmissionCompletionTime)
        {
            var contestProblems = contest.ProblemGroups
                .SelectMany(pg => pg.Problems)
                .SelectMany(p => p.Submissions);

            if (onlyOfficialSubmissions)
            {
                contestProblems.Where(s => s.Participant.IsOfficial);
            }

            var submissionsRunTimeSecondsList =
                contestProblems.Select(s =>
                {
                    if (s.StartedExecutionOn.HasValue)
                    {
                        return s.CompletedExecutionOn.HasValue
                            ? (s.CompletedExecutionOn.Value - s.StartedExecutionOn.Value).TotalSeconds
                            : (s.ModifiedOn.Value - s.StartedExecutionOn.Value).TotalSeconds;
                    }
                    else
                    {
                        return s.CompletedExecutionOn.HasValue
                            ? (s.CompletedExecutionOn.Value - s.CreatedOn).TotalSeconds
                            : (s.ModifiedOn.Value - s.CreatedOn).TotalSeconds;
                    }
                }).ToList();

            if (maxAllowedSubmissionCompletionTime.HasValue)
            {
                submissionsRunTimeSecondsList =
                    submissionsRunTimeSecondsList.Where(s => s <= maxAllowedSubmissionCompletionTime.Value).ToList();
            }

            if (submissionsRunTimeSecondsList.Count == 0)
            {
                return 0;
            }

            return (int)submissionsRunTimeSecondsList.Average();
        }

        private static void GetDistributionResults(BaseContestBusinessModel model, JudgeLoadResults responseModel)
        {
            var gaussianDistributionPeak = 0.341;

            responseModel.MaxSubmissionsPerMinute = (int)Math.Round(responseModel.ExpectedExamSubmissions *
                gaussianDistributionPeak / ((double)model.ExamLengthInHours / 8.0 * 60));

            if (responseModel.MaxSubmissionsPerMinute < 1)
            {
                responseModel.MaxSubmissionsPerMinute = 1;
            }

            responseModel.MaxDistributedWorkersRequired = responseModel.MaxSubmissionsPerMinute /
                responseModel.ProcessedSubmissionsPerWorkerPerMinute * model.SafetyFactor;

            if (responseModel.MaxDistributedWorkersRequired < 1)
            {
                responseModel.MaxDistributedWorkersRequired = 1;
            }

            responseModel.JudgeWorkRequiredInMinutes = (int)Math.Round(responseModel.MaxSubmissionsPerMinute *
                model.AverageProblemRunTimeInSeconds * (1 + (model.WorkerIdleTimeInPercentage / 100.0)) / 60);
            responseModel.JudgeWorkRequiredPerWorkerInSeconds = (int)Math.Round(
                (double)responseModel.JudgeWorkRequiredInMinutes / (double)responseModel.MaxDistributedWorkersRequired *
                60);
            responseModel.SecondsBetweenSubmission = (int)Math.Round((double)responseModel.JudgeWorkRequiredInMinutes /
                responseModel.MaxDistributedWorkersRequired * (1 - 1.0 / model.SafetyFactor) * 60);
            responseModel.MaxSecondsBetweenSubmissions = (int)Math.Round(
                (double)responseModel.JudgeWorkRequiredInMinutes / model.ActualWorkers *
                (1 - 1.0 / model.SafetyFactor) * 60);
            responseModel.MaxUsersAtSameTime = model.ExpectedStudentsCount * gaussianDistributionPeak;
        }

        private static void GetDoomsDayScenario(BaseContestBusinessModel model, JudgeLoadResults responseModel)
        {
            responseModel.JudgeWorkInMnutes = (int)Math.Round(model.ExpectedStudentsCount *
                                                              model.AverageProblemRunTimeInSeconds *
                                                              (1 + (model.WorkerIdleTimeInPercentage / 100.0))) / 60;
            responseModel.JudgeWorkPerWorkerInMinutes =
                (int)Math.Round((double)responseModel.JudgeWorkInMnutes / model.ActualWorkers);
            responseModel.SecondsBetweenSubmissionsBase =
                (int)Math.Round((responseModel.JudgeWorkInMnutes / 20.0) * (1 - 1.0 / model.SafetyFactor) * 60);
            responseModel.SecondsBetweenSubmissionsHigh = (int)Math.Round((double)responseModel.JudgeWorkInMnutes /
                model.ActualWorkers * (1 - 1.0 / model.SafetyFactor) * 60);
        }

        private void GetJudgeLoadData(BaseContestBusinessModel model, JudgeLoadResults responseModel)
        {
            double previousExamSubmissions = model.PreviousContestSubmissions.Value;
            double previousExamStudents = model.PreviousContestParticipants.Value;
            int previousExamProblems = model.PreviousContestExpectedProblems.Value;

            var currentContestData = (double)model.ExpectedExamProblemsCount * model.ExpectedStudentsCount;

            responseModel.ExpectedExamSubmissions = (int)Math.Round(previousExamSubmissions / previousExamStudents /
                previousExamProblems * currentContestData);

            if (responseModel.ExpectedExamSubmissions < 1)
            {
                responseModel.ExpectedExamSubmissions = 1;
            }

            responseModel.ProcessedSubmissionsPerWorkerPerMinute = (int)Math.Ceiling(
                60 / (1 + (model.WorkerIdleTimeInPercentage / 100.0)) / model.AverageProblemRunTimeInSeconds);

            var judgeParallelWorkInPercentage = model.MaxJudgeParalelWork / 100.0;
            responseModel.MinimumWorkersRequired = (int)Math.Ceiling(
                (responseModel.ExpectedExamSubmissions / (model.ExamLengthInHours * judgeParallelWorkInPercentage * 60 *
                                                          responseModel.ProcessedSubmissionsPerWorkerPerMinute)) *
                model.SafetyFactor);

            if (responseModel.MinimumWorkersRequired < 1)
            {
                responseModel.MinimumWorkersRequired = 1;
            }
        }
    }
}