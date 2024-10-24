namespace OJS.Services.Ui.Business.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Data.Models;
using OJS.Data.Models.Participants;
using OJS.Data.Models.Problems;
using OJS.Services.Common.Models;
using OJS.Services.Infrastructure;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Contests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Resource = OJS.Common.Resources.ParticipantsBusiness;
using SharedResource = OJS.Common.Resources.ContestsGeneral;

public class ParticipantsBusinessService : IParticipantsBusinessService
{
    private readonly IParticipantsDataService participantsData;
    private readonly IContestsDataService contestsData;
    private readonly IDatesService datesService;
    private readonly IProblemGroupsDataService problemGroupsData;

    public ParticipantsBusinessService(
        IParticipantsDataService participantsData,
        IContestsDataService contestsData,
        IDatesService datesService,
        IProblemGroupsDataService problemGroupsData)
    {
        this.participantsData = participantsData;
        this.contestsData = contestsData;
        this.datesService = datesService;
        this.problemGroupsData = problemGroupsData;
    }

    public async Task<Participant> CreateNewByContestByUserByIsOfficialAndIsAdminOrLecturer(
        ContestRegistrationDetailsServiceModel contest,
        string userId,
        bool isOfficial,
        bool isAdminOrLecturerInContest)
    {
        var participant = new Participant(contest.Id, userId, isOfficial);

        var utcNow = DateTime.SpecifyKind(this.datesService.GetUtcNow(), DateTimeKind.Unspecified);
        if (isOfficial && contest.IsOnlineExam)
        {
            participant.ParticipationStartTime = utcNow;
            participant.ParticipationEndTime = utcNow + contest.Duration;

            if (!isAdminOrLecturerInContest)
            {
                var problemGroups = await this.problemGroupsData
                    .GetAllByContest(contest.Id)
                    .Include(pg => pg.Problems)
                    .ToListAsync();

                AssignRandomProblemsToParticipant(participant, problemGroups);
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

        return ServiceResult<string>.Success(participant.User.UserName!);
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
                .Select(p => p.User.UserName!)
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

    private static void AssignRandomProblemsToParticipant(Participant participant, List<ProblemGroup> problemGroups)
    {
        var random = new Random();

        foreach (var problemGroup in problemGroups.Where(pg => pg.Problems.Any(p => !p.IsDeleted)))
        {
            var problemsInGroup = problemGroup.Problems.Where(p => !p.IsDeleted).ToList();
            if (problemsInGroup.Count != 0)
            {
                var randomProblem = problemsInGroup[random.Next(0, problemsInGroup.Count)];
                participant.ProblemsForParticipants.Add(new ProblemForParticipant
                {
                    Participant = participant,
                    Problem = randomProblem,
                });
            }
        }
    }
}