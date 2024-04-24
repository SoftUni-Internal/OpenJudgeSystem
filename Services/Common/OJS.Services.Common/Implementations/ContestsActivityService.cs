namespace OJS.Services.Common.Implementations;

using Microsoft.EntityFrameworkCore;
using OJS.Common.Enumerations;
using OJS.Data.Models.Contests;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Common.Validation.Helpers;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Extensions;
using SoftUni.AutoMapper.Infrastructure.Extensions;
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

    public async Task<IContestActivityServiceModel> GetContestActivity(int id)
    {
        var contest = await this.contestsData.OneByIdTo<ContestForActivityServiceModel>(id);

        this.notDefaultValueValidationHelper
            .ValidateValueIsNotDefault(contest, nameof(contest))
            .VerifyResult();

        return new ContestActivityServiceModel
        {
            Id = contest!.Id,
            Name = contest.Name,
            CanBeCompeted = this.CanUserCompete(contest),
            CanBePracticed = this.CanBePracticed(contest),
        };
    }

    public IContestActivityServiceModel GetContestActivity(IContestForActivityServiceModel contest)
        => new ContestActivityServiceModel
        {
            Id = contest!.Id,
            Name = contest.Name,
            CanBeCompeted = this.CanUserCompete(contest),
            CanBePracticed = this.CanBePracticed(contest),
        };

    // Method is firstly checking if the Contest can be competed based in it's StartTime and EndTime
    // If this check returns false we have to check if the current user is a participant with remaining time
    // in an online contest
    public bool CanUserCompete(IContestForActivityServiceModel contest)
        => this.CanBeCompeted(contest) ||
           (contest.IsOnline && this.IsActiveParticipantInOnlineContest(contest.Id));

    // Usage: assign value to the CanBeCompeted/Practiced properties in the different Contest models sent to the UI
    // method must be called on model/collection after retrieving it from the db
    // and before sending it to the UI as response
    public void SetCanBeCompetedAndPracticed<T>(T contestModel)
        where T : ICanBeCompetedAndPracticed
    {
        var contestActivity = this.GetContestActivity(contestModel.Id).GetAwaiter().GetResult();
        contestModel.CanBeCompeted = contestActivity.CanBeCompeted;
        contestModel.CanBePracticed = contestActivity.CanBePracticed;
    }

    public bool CanBePracticed(IContestForActivityServiceModel contest)
    {
        if (!contest.IsVisible || contest.IsDeleted)
        {
            return false;
        }

        var currentTimeInUtc = this.dates.GetUtcNow();

        if (!contest.PracticeStartTime.HasValue)
        {
            // Cannot be practiced
            return false;
        }

        if (!contest.PracticeEndTime.HasValue)
        {
            // Practice forever
            return contest.PracticeStartTime <= currentTimeInUtc;
        }

        return contest.PracticeStartTime <= currentTimeInUtc && currentTimeInUtc <= contest.PracticeEndTime;
    }

    public async Task<bool> IsContestActive(IContestForActivityServiceModel contest)
        => this.CanBeCompeted(contest) ||
           (contest.IsOnline &&
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

    private bool IsActiveParticipantInOnlineContest(int contestId)
    {
        var currentUser = this.userProvider.GetCurrentUser();
        var participants = this.participantsCommonData.GetAllByUserAndContest(currentUser.Id, contestId);
        var currentTimeInUtc = this.dates.GetUtcNow();

        return participants.Any(p =>
            p.ParticipationEndTime.HasValue &&
            p.ParticipationEndTime.Value >= currentTimeInUtc);
    }

    private bool CanBeCompeted(IContestForActivityServiceModel contest)
    {
        if (!contest.IsVisible || contest.IsDeleted)
        {
            return false;
        }

        if (!contest.StartTime.HasValue)
        {
            // Cannot be competed
            return false;
        }

        var currentTimeInUtc = this.dates.GetUtcNow();

        if (!contest.EndTime.HasValue)
        {
            // Compete forever
            return contest.StartTime <= currentTimeInUtc;
        }

        return contest.StartTime <= currentTimeInUtc && currentTimeInUtc <= contest.EndTime;
    }
}