namespace OJS.Services.Ui.Business.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OJS.Services.Ui.Data;
using Microsoft.EntityFrameworkCore;
using OJS.Services.Infrastructure;
using OJS.Data.Models;
using OJS.Data.Models.Contests;
using OJS.Data.Models.Participants;
using OJS.Services.Common.Models;
using OJS.Common.Extensions;
using SharedResource = OJS.Common.Resources.ContestsGeneral;
using Resource = OJS.Common.Resources.ParticipantsBusiness;

public class ParticipantsBusinessService : IParticipantsBusinessService
{
    private readonly IParticipantsDataService participantsData;
    private readonly ISubmissionsDataService submissionsData;
    private readonly IContestsDataService contestsData;
    private readonly IDatesService datesService;
    private readonly IParticipantScoresDataService scoresDataService;

    public ParticipantsBusinessService(
        IParticipantsDataService participantsData,
        ISubmissionsDataService submissionsData,
        IContestsDataService contestsData,
        IDatesService datesService,
        IParticipantScoresDataService scoresDataService)
    {
        this.participantsData = participantsData;
        this.contestsData = contestsData;
        this.submissionsData = submissionsData;
        this.datesService = datesService;
        this.scoresDataService = scoresDataService;
    }

    public async Task<Participant> CreateNewByContestByUserByIsOfficialAndIsAdminOrLecturer(
        Contest contest,
        string userId,
        bool isOfficial,
        bool isAdminOrLecturerInContest)
    {
        var participant = new Participant(contest.Id, userId, isOfficial) { Contest = contest };

        var utcNow = DateTime.SpecifyKind(this.datesService.GetUtcNow(), DateTimeKind.Unspecified);
        if (isOfficial && contest.IsOnlineExam)
        {
            participant.ParticipationStartTime = utcNow;
            participant.ParticipationEndTime = utcNow + contest.Duration;

            if (!isAdminOrLecturerInContest)
            {
                AssignRandomProblemsToParticipant(participant, contest);
            }
        }

        await this.participantsData.Add(participant);
        await this.participantsData.SaveChanges();

        return participant;
    }

    public async Task<ServiceResult<string>> UpdateParticipationEndTimeByIdAndTimeInMinutes(int id, int minutes)
    {
        var participant = await this.participantsData
            .GetByIdQuery(id)
            .Include(p => p.Contest)
            .Include(p => p.User)
            .FirstOrDefaultAsync();

        if (participant == null)
        {
            throw new ArgumentException(Resource.ParticipantDoesNotExist);
        }

        if (!participant.Contest.Duration.HasValue)
        {
            return new ServiceResult<string>(Resource.ContestDurationNotSet);
        }

        if (!participant.ParticipationEndTime.HasValue ||
            !participant.ParticipationStartTime.HasValue)
        {
            throw new ArgumentException(Resource.ParticipantParticipationTimeNotSet);
        }

        var newEndTime = participant.ParticipationEndTime.Value.AddMinutes(minutes);
        var minAllowedEndTime = participant.ParticipationStartTime.Value
            .AddMinutes(participant.Contest.Duration.Value.TotalMinutes);

        if (newEndTime < minAllowedEndTime)
        {
            return new ServiceResult<string>(Resource.ParticipationTimeReduceBelowDurationWarning);
        }

        participant.ParticipationEndTime = newEndTime;

        this.participantsData.Update(participant);

        return ServiceResult<string>.Success(participant.User.UserName);
    }

    public async Task<ServiceResult<ICollection<string>>>
        UpdateParticipationsEndTimeByContestByParticipationStartTimeRangeAndTimeInMinutes(
            int contestId,
            int timeInMinutes,
            DateTime participationStartTimeRangeStart,
            DateTime participationStartTimeRangeEnd)
    {
        var contest = await this.contestsData.OneById(contestId);

        if (contest == null)
        {
            return new ServiceResult<ICollection<string>>(SharedResource.ContestNotFound);
        }

        if (!contest.Duration.HasValue)
        {
            return new ServiceResult<ICollection<string>>(Resource.ContestDurationNotSet);
        }

        var contestTotalDurationInMinutes = contest.Duration.Value.TotalMinutes;

        var invalidForUpdateParticipantUsernames =
            this.participantsData
                .GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
                    contestId,
                    participationStartTimeRangeStart,
                    participationStartTimeRangeEnd)
                .Where(p =>
                    p.ParticipationEndTime!.Value.AddMinutes(timeInMinutes) <
                    p.ParticipationStartTime!.Value.AddMinutes(contestTotalDurationInMinutes))
                .Select(p => p.User.UserName)
                .ToList();

        var participantsInTimeRange =
            this.participantsData.GetAllOfficialInOnlineContestByContestAndParticipationStartTimeRange(
                contestId,
                participationStartTimeRangeStart,
                participationStartTimeRangeEnd);

        await this.participantsData.Update(
            participantsInTimeRange
                .Where(p =>
                    p.ParticipationEndTime!.Value.AddMinutes(timeInMinutes) >=
                    p.ParticipationStartTime!.Value.AddMinutes(contestTotalDurationInMinutes)),
            p => new Participant { ParticipationEndTime = p.ParticipationEndTime!.Value.AddMinutes(timeInMinutes), });

        return ServiceResult<ICollection<string>>.Success(invalidForUpdateParticipantUsernames);
    }

    public Task<int> GetParticipantLimitBetweenSubmissions(int participantId, int contestLimitBetweenSubmissions)
        => contestLimitBetweenSubmissions != 0
            ? this.submissionsData
                .GetUserSubmissionTimeLimit(participantId, contestLimitBetweenSubmissions)
                .ToTask()
            : Task.FromResult(0);

    private static void AssignRandomProblemsToParticipant(Participant participant, Contest contest)
    {
        var random = new Random();

        var problemGroups = contest
            .ProblemGroups
            .Where(pg => !pg.IsDeleted && pg.Problems.Any(p => !p.IsDeleted));

        foreach (var problemGroup in problemGroups)
        {
            var problemsInGroup = problemGroup.Problems.Where(p => !p.IsDeleted).ToList();
            if (problemsInGroup.Any())
            {
                var randomProblem = problemsInGroup[random.Next(0, problemsInGroup.Count)];
                participant.ProblemsForParticipants.Add(new ProblemForParticipant
                {
                    Participant = participant, Problem = randomProblem,
                });
            }
        }
    }
}