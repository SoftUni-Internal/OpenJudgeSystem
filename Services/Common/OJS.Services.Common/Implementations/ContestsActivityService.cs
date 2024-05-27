namespace OJS.Services.Common.Implementations;

using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Common.Enumerations;
using OJS.Data.Models.Contests;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Common.Validation.Helpers;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class ContestsActivityService : IContestsActivityService
{
    private readonly IDatesService dates;
    private readonly IParticipantsCommonDataService participantsCommonData;
    private readonly IDataService<Contest> contestsData;
    private readonly INotDefaultValueValidationHelper notDefaultValueValidationHelper;
    private readonly IUserProviderService userProvider;

    public ContestsActivityService(
        IDatesService dates,
        IParticipantsCommonDataService participantsCommonData,
        IDataService<Contest> contestsData,
        INotDefaultValueValidationHelper notDefaultValueValidationHelper,
        IUserProviderService userProvider)
    {
        this.dates = dates;
        this.participantsCommonData = participantsCommonData;
        this.contestsData = contestsData;
        this.notDefaultValueValidationHelper = notDefaultValueValidationHelper;
        this.userProvider = userProvider;
    }

    public async Task<IContestActivityServiceModel> GetContestActivity(IContestForActivityServiceModel contest)
        => this.GetContestActivity(contest, await this.GetCurrentUserParticipantsForContests(new[] { contest.Id }));

    public ParticipantActivityServiceModel GetParticipantActivity(IParticipantForActivityServiceModel participant)
    {
        var startTime = participant.IsOfficial
            ? participant.ParticipationStartTime ?? participant.ContestStartTime
            : participant.ContestPracticeStartTime;

        var endTime = participant.IsOfficial
            ? participant.ParticipationEndTime ?? participant.ContestEndTime
            : participant.ContestPracticeEndTime;

        var hasParticipationTimeLeft = this.TimeRangeAllowsParticipation(startTime, endTime);

        return new ParticipantActivityServiceModel(
            hasParticipationTimeLeft,
            participant.IsInvalidated,
            startTime,
            endTime);
    }

    public async Task SetCanBeCompetedAndPracticed<T>(ICollection<T> contestModels)
        where T : class, ICanBeCompetedAndPracticed, IContestForActivityServiceModel
    {
        var contests = contestModels
            .Cast<IContestForActivityServiceModel>()
            .DistinctBy(c => c.Id)
            .ToList();

        var contestActivities = await this.GetContestActivities(contests).ToListAsync();

        foreach (var contestModel in contestModels)
        {
            var contestActivity = contestActivities.Single(c => c.Id == (contestModel as IContestForActivityServiceModel).Id);
            contestModel.CanBeCompeted = contestActivity.CanBeCompeted;
            contestModel.CanBePracticed = contestActivity.CanBePracticed;
        }
    }

    public void SetCanBeCompetedAndPracticed<T>(
        ICollection<T> contestModels,
        IReadOnlyCollection<IParticipantForActivityServiceModel> participants)
        where T : class, ICanBeCompetedAndPracticed, IContestForActivityServiceModel
    {
        var contestActivities = this.GetContestActivities(contestModels, participants).ToList();

        foreach (var contestModel in contestModels)
        {
            var contestActivity = contestActivities.Single(c => c.Id == (contestModel as IContestForActivityServiceModel).Id);
            contestModel.CanBeCompeted = contestActivity.CanBeCompeted;
            contestModel.CanBePracticed = contestActivity.CanBePracticed;
        }
    }

    public async Task<bool> IsContestActive(IContestForActivityServiceModel contest)
        => this.CanBeCompeted(contest, null) ||
           (contest.Type == ContestType.OnlinePracticalExam &&
                await this.participantsCommonData
                    .GetAllByContestAndIsOfficial(contest.Id, true)
                    .AnyAsync(p =>
                        p.ParticipationEndTime.HasValue &&
                        p.ParticipationEndTime.Value >= this.dates.GetUtcNow()));

    public async Task<bool> IsContestActive(int contestId)
    {
        var contest = await this.contestsData.OneByIdTo<ContestForActivityServiceModel>(contestId);

        this.notDefaultValueValidationHelper
            .ValidateValueIsNotDefault(contest, nameof(contest))
            .VerifyResult();

        return await this.IsContestActive(contest!);
    }

    private async Task<IEnumerable<IContestActivityServiceModel>> GetContestActivities(
        ICollection<IContestForActivityServiceModel> contests)
    {
        var participants = await this.GetCurrentUserParticipantsForContests(contests.Select(c => c.Id));
        return contests.Select(contest => this.GetContestActivity(contest, participants));
    }

    private IEnumerable<IContestActivityServiceModel> GetContestActivities(
        IEnumerable<IContestForActivityServiceModel> contests,
        IReadOnlyCollection<IParticipantForActivityServiceModel> participants)
        => contests.Select(contest => this.GetContestActivity(contest, participants));

    private bool CanBeCompeted(IContestForActivityServiceModel contest, IParticipantForActivityServiceModel? participant)
    {
        var contestIsVisible = contest.IsVisible || contest.VisibleFrom <= this.dates.GetUtcNow();

        if (!contestIsVisible || contest.IsDeleted)
        {
            return false;
        }

        return participant != null
            ? this.GetParticipantActivity(participant).IsActive
            : this.TimeRangeAllowsParticipation(contest.StartTime, contest.EndTime);
    }

    private bool CanBePracticed(IContestForActivityServiceModel contest, IParticipantForActivityServiceModel? participant)
    {
        var contestIsVisible = contest.IsVisible || contest.VisibleFrom <= this.dates.GetUtcNow();

        if (!contestIsVisible || contest.IsDeleted)
        {
            return false;
        }

        return participant != null
            ? this.GetParticipantActivity(participant).IsActive
            : this.TimeRangeAllowsParticipation(contest.PracticeStartTime, contest.PracticeEndTime);
    }

    /// <summary>
    /// Checks if the given time range is valid for participation, compared to the current time in UTC.
    /// If start time is not set, participation is not possible.
    /// If end time is not set, but has start time, participation is allowed forever.
    /// </summary>
    /// <param name="startTime">Start time.</param>
    /// <param name="endTime">End time.</param>
    /// <returns>True if the time range is valid, otherwise false.</returns>
    private bool TimeRangeAllowsParticipation(DateTime? startTime, DateTime? endTime)
    {
        var utcNow = this.dates.GetUtcNow();
        return startTime <= utcNow && (endTime == null || utcNow <= endTime);
    }

    private async Task<IReadOnlyCollection<ParticipantForActivityServiceModel>> GetCurrentUserParticipantsForContests(
        IEnumerable<int> contestIds)
    {
        var user = this.userProvider.GetCurrentUser();
        return user.IsAuthenticated
            ? await this.participantsCommonData
                .AllTo<ParticipantForActivityServiceModel>(p => p.UserId == user.Id && contestIds.Contains(p.ContestId))
                .ToListAsync()
            : new List<ParticipantForActivityServiceModel>();
    }

    private IContestActivityServiceModel GetContestActivity(
        IContestForActivityServiceModel contest,
        IReadOnlyCollection<IParticipantForActivityServiceModel> participants)
    {
        var officialParticipant = participants.SingleOrDefault(p => p.ContestId == contest.Id && p.IsOfficial);
        var practiceParticipant = participants.SingleOrDefault(p => p.ContestId == contest.Id && !p.IsOfficial);
        var canBeCompeted = this.CanBeCompeted(contest, officialParticipant);
        var canBePracticed = this.CanBePracticed(contest, practiceParticipant);

        return new ContestActivityServiceModel
        {
            Id = contest.Id,
            Name = contest.Name,
            CanBeCompeted = canBeCompeted,
            CanBePracticed = canBePracticed,
        };
    }
}