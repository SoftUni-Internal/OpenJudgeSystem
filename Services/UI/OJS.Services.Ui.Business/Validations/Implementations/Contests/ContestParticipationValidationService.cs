namespace OJS.Services.Ui.Business.Validations.Implementations.Contests;

using System.Threading.Tasks;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Common.Models.Users;
using OJS.Services.Infrastructure;
using OJS.Services.Ui.Data;
using OJS.Services.Infrastructure.Models;
using OJS.Services.Ui.Business.Cache;
using OJS.Services.Ui.Models.Contests;

public class ContestParticipationValidationService : IContestParticipationValidationService
{
    private readonly IDatesService datesService;
    private readonly IContestsActivityService activityService;
    private readonly ILecturersInContestsCacheService lecturersInContestsCache;
    private readonly IContestsDataService contestsData;

    public ContestParticipationValidationService(
        IDatesService datesService,
        IContestsActivityService activityService,
        ILecturersInContestsCacheService lecturersInContestsCache,
        IContestsDataService contestsData)
    {
        this.datesService = datesService;
        this.activityService = activityService;
        this.lecturersInContestsCache = lecturersInContestsCache;
        this.contestsData = contestsData;
    }

    public async Task<ValidationResult> GetValidationResult((ContestParticipationValidationServiceModel?, ContestCategoryServiceModel?, IParticipantForActivityServiceModel? participant, UserInfoModel?, bool) item)
    {
        var (contest, contestCategory, participant, user, official) = item;

        var userIsAdminOrLecturerInContest = await this.lecturersInContestsCache
            .IsUserAdminOrLecturerInContest(contest?.Id, contest?.CategoryId, user);

        var contestIsVisible = contest?.IsVisible == true || contest?.VisibleFrom <= this.datesService.GetUtcNow();

        if (contest == null ||
            user == null ||
            ((!contestIsVisible || contestCategory is not { IsVisible: true }) &&
            !userIsAdminOrLecturerInContest))
        {
            return ValidationResult.Invalid(ValidationMessages.Contest.NotFound);
        }

        if (userIsAdminOrLecturerInContest)
        {
            return ValidationResult.Valid();
        }

        var contestActivityEntity = this.activityService.GetContestActivity(contest, [participant]);

        if (official && !contestActivityEntity.CanBeCompeted)
        {
            return ValidationResult.Invalid(ValidationMessages.Contest.CanBeCompeted);
        }

        if (!official && !contestActivityEntity.CanBePracticed)
        {
            return ValidationResult.Invalid(ValidationMessages.Contest.CanBePracticed);
        }

        if (contest.IsOnlineExam &&
            official &&
            !userIsAdminOrLecturerInContest &&
            !await this.contestsData.IsUserInExamGroupByContestAndUser(contest.Id, user.Id))
        {
            return ValidationResult.Invalid(ValidationMessages.Participant.NotRegisteredForExam);
        }

        return ValidationResult.Valid();
    }
}