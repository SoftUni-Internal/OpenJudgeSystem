﻿using OJS.Services.Data.ParticipantScores;

namespace OJS.Services.Business.Participants
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using OJS.Data.Models;
    using OJS.Services.Common;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;
    using SharedResource = Resources.Contests.ContestsGeneral;
    using Resource = Resources.Services.Participants.ParticipantsBusiness;

    public class ParticipantsBusinessService : IParticipantsBusinessService
    {
        private readonly IParticipantsDataService participantsData;
        private readonly IContestsDataService contestsData;
        private readonly IParticipantScoresDataService scoresDataService;

        public ParticipantsBusinessService(
            IParticipantsDataService participantsData,
            IContestsDataService contestsData,
            IParticipantScoresDataService scoresDataService)
        {
            this.participantsData = participantsData;
            this.contestsData = contestsData;
            this.scoresDataService = scoresDataService;
        }

        public Participant CreateNewByContestByUserByIsOfficialAndIsAdmin(
            Contest contest,
            string userId,
            bool isOfficial,
            bool isAdmin)
        {
            var participant = new Participant(contest.Id, userId, isOfficial);

            if (isOfficial)
            {
                if (contest.IsOnline)
                {
                    participant.ParticipationStartTime = DateTime.Now;
                    participant.ParticipationEndTime = DateTime.Now + contest.Duration;
                }

                if (contest.IsWithRandomTasks && !isAdmin &&
                    !this.contestsData.IsUserLecturerInByContestAndUser(contest.Id, userId))
                {
                    this.AssignRandomProblemsToParticipant(participant, contest);
                }
            }

            this.participantsData.Add(participant);
            return participant;
        }

        public ServiceResult<string> UpdateParticipationEndTimeByIdAndTimeInMinutes(int id, int minutes)
        {
            var participant = this.participantsData.GetById(id);
            if (participant == null)
            {
                throw new ArgumentException(Resource.Participant_does_not_exist);
            }

            if (!participant.Contest.Duration.HasValue)
            {
                return new ServiceResult<string>(Resource.Contest_duration_not_set);
            }

            if (!participant.ParticipationEndTime.HasValue ||
                !participant.ParticipationStartTime.HasValue)
            {
                throw new ArgumentException(Resource.Participant_participation_time_not_set);
            }

            var newEndTime = participant.ParticipationEndTime.Value.AddMinutes(minutes);
            var minAllowedEndTime = participant.ParticipationStartTime.Value
                .AddMinutes(participant.Contest.Duration.Value.TotalMinutes);

            if (newEndTime < minAllowedEndTime)
            {
                return new ServiceResult<string>(Resource.Participation_time_reduce_below_duration_warning);
            }

            participant.ParticipationEndTime = newEndTime;

            this.participantsData.Update(participant);

            return ServiceResult<string>.Success(participant.User.UserName);
        }

        public ServiceResult<ICollection<string>>
            UpdateParticipationsEndTimeByContestByParticipationStartTimeRangeAndTimeInMinutes(
                int contestId,
                int timeInMinutes,
                DateTime participationStartTimeRangeStart,
                DateTime participationStartTimeRangeEnd)
        {
            const string minuteParamName = "minute";

            var contest = this.contestsData.GetById(contestId);

            if (contest == null)
            {
                return new ServiceResult<ICollection<string>>(SharedResource.Contest_not_found);
            }

            if (!contest.Duration.HasValue)
            {
                return new ServiceResult<ICollection<string>>(Resource.Contest_duration_not_set);
            }

            var contestTotalDurationInMinutes = contest.Duration.Value.TotalMinutes;

            var invalidForUpdateParticipantUsernames =
                this.participantsData
                    .GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
                        contestId,
                        participationStartTimeRangeStart,
                        participationStartTimeRangeEnd)
                    .Where(p =>
                        SqlFunctions.DateAdd(minuteParamName, timeInMinutes, p.ParticipationEndTime) <
                        SqlFunctions.DateAdd(minuteParamName, contestTotalDurationInMinutes, p.ParticipationStartTime))
                    .Select(p => p.User.UserName)
                    .ToList();

            var participantsInTimeRange =
                this.participantsData.GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
                    contestId,
                    participationStartTimeRangeStart,
                    participationStartTimeRangeEnd);

            this.participantsData.Update(
                participantsInTimeRange
                    .Where(p =>
                        SqlFunctions.DateAdd(minuteParamName, timeInMinutes, p.ParticipationEndTime) >=
                        SqlFunctions.DateAdd(minuteParamName, contestTotalDurationInMinutes, p.ParticipationStartTime)),
                p => new Participant
                {
                    ParticipationEndTime = SqlFunctions.DateAdd(
                        minuteParamName,
                        timeInMinutes,
                        p.ParticipationEndTime)
                });

            return ServiceResult<ICollection<string>>.Success(invalidForUpdateParticipantUsernames);
        }

        public void UpdateTotalScoreSnapshotOfParticipants()
        {
            this.participantsData.UpdateTotalScoreSnapshot();
        }

        public void RemoveParticipantMultipleScores()
        {
            var participantScores =
                this.scoresDataService.GetAll()
                    .GroupBy(ps => new { ps.IsOfficial, ps.ProblemId, ps.ParticipantId })
                    .Where(ps => ps.Count() > 1)
                    .ToList();

            var participantScoresToRemove = new List<ParticipantScore>();
            foreach (var participantScoreGroup in participantScores)
            {
                participantScoresToRemove
                    .AddRange(participantScoreGroup.OrderByDescending(ps => ps.Points).Skip(1)
                    .ToList());
            }
            this.scoresDataService.Delete(participantScoresToRemove);
        }

        private void AssignRandomProblemsToParticipant(Participant participant, Contest contest)
        {
            var random = new Random();

            var problemGroups = contest.ProblemGroups
                .Where(pg => !pg.IsDeleted && pg.Problems.Any(p => !p.IsDeleted));

            foreach (var problemGroup in problemGroups)
            {
                var problemsInGroup = problemGroup.Problems.Where(p => !p.IsDeleted).ToList();
                if (problemsInGroup.Any())
                {
                    var randomProblem = problemsInGroup[random.Next(0, problemsInGroup.Count)];
                    participant.Problems.Add(randomProblem);
                }
            }
        }
    }
}