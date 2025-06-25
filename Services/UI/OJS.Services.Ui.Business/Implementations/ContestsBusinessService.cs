namespace OJS.Services.Ui.Business.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;
using FluentExtensions.Extensions;
using Microsoft.EntityFrameworkCore;
using OJS.Data.Models.Participants;
using OJS.Services.Common;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Infrastructure.Models;
using OJS.Services.Ui.Business.Cache;
using OJS.Services.Ui.Business.Validations.Implementations.Contests;
using OJS.Services.Ui.Data;
using OJS.Services.Ui.Models.Contests;
using OJS.Services.Ui.Models.Participants;
using OJS.Services.Ui.Models.Search;
using static OJS.Common.Constants.ServiceConstants.ErrorCodes;
using static OJS.Services.Common.Constants.PaginationConstants.Contests;

public class ContestsBusinessService(
    IContestsDataService contestsData,
    IContestsActivityService activityService,
    IParticipantsDataService participantsData,
    IParticipantScoresDataService participantScoresData,
    IUserProviderService userProviderService,
    IParticipantsBusinessService participantsBusiness,
    IContestCategoriesCacheService contestCategoriesCache,
    IContestParticipationValidationService contestParticipationValidationService,
    IContestParticipantsCacheService contestParticipantsCacheService,
    IContestsCacheService contestsCacheService,
    ILecturersInContestsCacheService lecturersInContestsCache,
    IContestDetailsValidationService contestDetailsValidationService)
    : IContestsBusinessService
{
    public async Task<ServiceResult<ContestDetailsServiceModel>> GetContestDetails(int id)
    {
        var contest = await contestsCacheService.GetContestDetailsServiceModel(id);
        if (contest == null)
        {
            return ServiceResult.NotFound<ContestDetailsServiceModel>(nameof(contest));
        }

        var user = userProviderService.GetCurrentUser();
        var category = await contestCategoriesCache.GetById(contest.CategoryId);
        var isLecturerInContestOrAdmin = await lecturersInContestsCache.IsUserAdminOrLecturerInContest(contest.Id, contest.CategoryId, user);

        var validationResult = contestDetailsValidationService.GetValidationResult((
            contest,
            category,
            isLecturerInContestOrAdmin));

        if (!validationResult.IsValid)
        {
            return validationResult.ToServiceResult<ContestDetailsServiceModel>();
        }

        var userParticipants = await participantsData
            .GetAllByContestAndUser(id, user.Id)
            .AsNoTracking()
            .MapCollection<ParticipantForContestDetailsServiceModel>()
            .ToListAsync();

        var contestActivityEntity = activityService.GetContestActivity(contest, userParticipants);

        var competeParticipant = userParticipants.FirstOrDefault(p => p.IsOfficial);
        var practiceParticipant = userParticipants.FirstOrDefault(p => !p.IsOfficial);

        var participantToGetProblemsFrom = contestActivityEntity.CanBeCompeted
            ? competeParticipant
            : practiceParticipant;

        if (!isLecturerInContestOrAdmin && participantToGetProblemsFrom != null && contestActivityEntity.CanBeCompeted && contest.IsWithRandomTasks)
        {
            var problemsForParticipantIds = participantToGetProblemsFrom.ProblemsForParticipants.Select(x => x.ProblemId);
            contest.Problems = [.. contest.Problems.Where(p => problemsForParticipantIds.Contains(p.Id))];
        }

        var canShowProblemsInCompete =
            (contest is { HasContestPassword: false, IsWithRandomTasks: false } && contestActivityEntity is { CanBeCompeted: true, CompeteUserActivity: not null })
             || isLecturerInContestOrAdmin
             || contestActivityEntity.CompeteUserActivity?.IsActive == true;

        var canShowProblemsInPractice =
            (!contest.HasPracticePassword && contestActivityEntity.CanBePracticed)
            || isLecturerInContestOrAdmin
            || contestActivityEntity.PracticeUserActivity?.IsActive == true;

        var canShowProblemsForAnonymous = user.IsAuthenticated || !contestActivityEntity.CanBeCompeted;

        if ((!canShowProblemsInPractice && !canShowProblemsInCompete) || !canShowProblemsForAnonymous)
        {
            contest.Problems = [];
            contest.Resources = [];
        }

        if (isLecturerInContestOrAdmin || competeParticipant != null)
        {
            contest.CanViewCompeteResults = true;
        }

        if (isLecturerInContestOrAdmin || contestActivityEntity.CanBeCompeted || contestActivityEntity.CanBePracticed)
        {
            contest.CanViewPracticeResults = true;
        }

        var participantsCount = await contestParticipantsCacheService.GetParticipantsCountForContest(id);

        contest.CompeteParticipantsCount = participantsCount.Official;
        contest.PracticeParticipantsCount = participantsCount.Practice;

        contest.CanBeCompeted = contestActivityEntity.CanBeCompeted;
        contest.CanBePracticed = contestActivityEntity.CanBePracticed;

        contest.IsAdminOrLecturerInContest = isLecturerInContestOrAdmin;

        contest.IsActive = await activityService.IsContestActive(contest);

        return ServiceResult.Success(contest);
    }

    public async Task<ServiceResult<ContestRegistrationDetailsServiceModel>> GetContestRegistrationDetails(int id, bool isOfficial)
    {
        var contest = await contestsData.OneByIdTo<ContestRegistrationDetailsServiceModel>(id);

        if (contest == null)
        {
            return ServiceResult.NotFound<ContestRegistrationDetailsServiceModel>(nameof(contest));
        }

        var user = userProviderService.GetCurrentUser();
        var category = await contestCategoriesCache.GetById(contest.CategoryId);

        var participant = await participantsData
            .GetAllByContestByUserAndIsOfficial(id, user.Id, isOfficial)
            .AsNoTracking()
            .MapCollection<ParticipantForContestRegistrationServiceModel>()
            .FirstOrDefaultAsync();

        var validationResult = await contestParticipationValidationService.GetValidationResult((
            contest.Map<ContestParticipationValidationServiceModel>(),
            category,
            participant,
            user,
            isOfficial));

        if (!validationResult.IsValid)
        {
            return validationResult.ToServiceResult<ContestRegistrationDetailsServiceModel>();
        }

        var userIsAdminOrLecturerInContest = await lecturersInContestsCache.IsUserAdminOrLecturerInContest(contest.Id, contest.CategoryId, user);

        contest.RequirePassword = ShouldRequirePassword(contest.HasContestPassword, contest.HasPracticePassword, participant!, isOfficial);
        contest.ParticipantId = participant?.Id;
        contest.IsRegisteredSuccessfully = participant != null && !participant.IsInvalidated;
        contest.ShouldConfirmParticipation = ShouldConfirmParticipation(participant, isOfficial, contest.IsOnlineExam, userIsAdminOrLecturerInContest);

        return ServiceResult.Success(contest);
    }

    public async Task<bool> RegisterUserForContest(
        int id,
        string? password,
        bool? hasConfirmedParticipation,
        bool isOfficial)
    {
        var user = userProviderService.GetCurrentUser();
        var contest = await contestsData.OneByIdTo<ContestRegistrationDetailsServiceModel>(id);
        var category = await contestCategoriesCache.GetById(contest?.CategoryId);
        var participant = await participantsData.GetByContestByUserAndByIsOfficial(id, user.Id, isOfficial);

        var participantForActivity = participant?.Map<ParticipantForActivityServiceModel>();
        if (participantForActivity != null)
        {
            participantForActivity.ContestStartTime = contest?.StartTime;
            participantForActivity.ContestEndTime = contest?.EndTime;
            participantForActivity.ContestPracticeStartTime = contest?.PracticeStartTime;
            participantForActivity.ContestPracticeEndTime = contest?.PracticeEndTime;
        }

        var validationResult = await contestParticipationValidationService.GetValidationResult((
            contest?.Map<ContestParticipationValidationServiceModel>(),
            category,
            participantForActivity,
            user,
            isOfficial));

        if (!validationResult.IsValid)
        {
            throw new BusinessServiceException(validationResult.Message);
        }

        var userIsAdminOrLecturerInContest = await lecturersInContestsCache.IsUserAdminOrLecturerInContest(contest?.Id, contest?.CategoryId, user);
        var shouldRequirePassword = ShouldRequirePassword(contest!.HasContestPassword, contest.HasPracticePassword, participantForActivity, isOfficial);
        var shouldConfirmParticipation =
            ShouldConfirmParticipation(participantForActivity, isOfficial, contest.IsOnlineExam, userIsAdminOrLecturerInContest);

        var requiredPasswordIsValid = false;

        // Validate password if present
        if (password != null && !password.IsNullOrEmpty() && shouldRequirePassword)
        {
            var isPasswordValid = isOfficial
                ? contest.ContestPassword == password
                : contest.PracticePassword == password;

            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid contest password");
            }

            requiredPasswordIsValid = true;
        }

        if ((participant == null || participant.IsInvalidated) &&
            (!shouldRequirePassword || requiredPasswordIsValid) &&
            (!shouldConfirmParticipation || hasConfirmedParticipation == true))
        {
            participant = await this.AddNewParticipantToContestIfNotExistsOrResetExistingToValid(contest, isOfficial, user.Id, userIsAdminOrLecturerInContest);
        }

        return participant != null;
    }

    public async Task<ServiceResult<VoidResult>> ValidateContestPassword(int id, bool official, string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return ServiceResult.Failure<VoidResult>(Unauthorized, "Password is empty");
        }

        var contest = await contestsData.OneById(id);

        var isOfficialAndIsCompetePasswordCorrect =
            official && contest!.HasContestPassword && contest.ContestPassword == password;

        var isPracticeAndIsPracticePasswordCorrect =
            !official && contest!.HasPracticePassword && contest.PracticePassword == password;

        if (!isOfficialAndIsCompetePasswordCorrect && !isPracticeAndIsPracticePasswordCorrect)
        {
            return ServiceResult.Failure<VoidResult>(Unauthorized, "Incorrect password!");
        }

        return ServiceResult.EmptySuccess;
    }

    public async Task<ServiceResult<ContestParticipationServiceModel>> GetParticipationDetails(
        StartContestParticipationServiceModel model)
    {
        var user = userProviderService.GetCurrentUser();
        var participant = await participantsData
            .GetQuery(p => p.ContestId == model.ContestId && p.UserId == user.Id && p.IsOfficial == model.IsOfficial)
            .MapCollection<ContestParticipationServiceModel>()
            .FirstOrDefaultAsync();

        if (participant == null)
        {
            // Participant must be registered in previous steps
            return ServiceResult.Success(new ContestParticipationServiceModel
            {
                IsActiveParticipant = false,
                IsRegisteredParticipant = false,
                Contest = null,
            });
        }

        var contest = await contestsCacheService.GetContestDetailsServiceModel(model.ContestId);
        var category = await contestCategoriesCache.GetById(contest?.CategoryId);

        participant.Contest = contest;

        participant.AllowMentor = category is { AllowMentor: true };
        var participantForActivity = participant.Map<ParticipantForActivityServiceModel>();

        var validationResult = await contestParticipationValidationService.GetValidationResult((
            contest?.Map<ContestParticipationValidationServiceModel>(),
            category,
            participantForActivity,
            user,
            model.IsOfficial));

        if (!validationResult.IsValid)
        {
            return validationResult.ToServiceResult<ContestParticipationServiceModel>();
        }

        var userIsAdminOrLecturerInContest = await lecturersInContestsCache.IsUserAdminOrLecturerInContest(contest?.Id, contest?.CategoryId, user);

        participant.IsRegisteredParticipant = true;
        participant.Contest!.UserIsAdminOrLecturerInContest = userIsAdminOrLecturerInContest;

        var participantActivity = activityService.GetParticipantActivity(participantForActivity);
        participant.EndDateTimeForParticipantOrContest = participantActivity!.ParticipationEndTime;
        participant.IsActiveParticipant = participantActivity.IsActive || userIsAdminOrLecturerInContest;

        participant.ParticipantId = participant.Id;
        participant.UserSubmissionsTimeLimit = contest!.LimitBetweenSubmissions;

        var participantsList = new List<int> { participant.Id, };

        var maxParticipationScores = await participantScoresData
            .GetMaxByProblemIdsAndParticipation(
                participant.Contest.Problems.Select(x => x.Id),
                participantsList);

        if (!userIsAdminOrLecturerInContest && model.IsOfficial && contest.IsWithRandomTasks)
        {
            participant.Contest.Problems = [.. participant.Contest.Problems
                .Where(x => participant.ProblemsForParticipantIds.Contains(x.Id))
                .OrderBy(p => p.ProblemGroupOrderBy)
                .ThenBy(p => p.OrderBy)
                .ThenBy(p => p.Name)];
        }

        await participant.Contest.Problems.ForEachAsync(problem =>
        {
            problem.Points = maxParticipationScores.GetValueOrDefault(problem.Id);
        });

        var participantsCount =
            await contestParticipantsCacheService.GetParticipantsCountForContest(model.ContestId);

        participant.ParticipantsCount = model.IsOfficial
            ? participantsCount.Official
            : participantsCount.Practice;

        return ServiceResult.Success(participant);
    }

    public async Task<ContestSearchServiceResultModel> GetSearchContestsByName(
        SearchServiceModel model)
    {
        var modelResult = new ContestSearchServiceResultModel();

        var allContestsQueryable = contestsData.GetAllVisible()
            .Where(c => c.Name != null && c.Name.Contains(model.SearchTerm ?? string.Empty) &&
                        c.Category != null && c.Category.IsVisible);

        var searchContests = await allContestsQueryable
            .MapCollection<ContestForListingServiceModel>()
            .ToPagedListAsync(model.PageNumber, model.ItemsPerPage);

        modelResult.Contests = searchContests;
        modelResult.TotalContestsCount = allContestsQueryable.Count();

        return modelResult;
    }

    public async Task<PagedResult<ContestForListingServiceModel>> GetAllByFiltersAndSorting(
        ContestFiltersServiceModel? model)
    {
        model = await this.GetNestedFilterCategoriesIfAny(model);

        var pagedContests =
            await contestsData.GetAllAsPageByFiltersAndSorting<ContestForListingServiceModel>(model);

        var participantResultsByContest = new Dictionary<int, List<ParticipantResultServiceModel>>();
        var user = userProviderService.GetCurrentUser();
        if (user.IsAuthenticated)
        {
            participantResultsByContest = await this.GetUserParticipantResultsForContestInPage([.. pagedContests
                .Items
                .Select(c => c.Id)]);
        }

        return await this.PrepareActivityAndResults(pagedContests, participantResultsByContest);
    }

    public async Task<Dictionary<int, List<ParticipantResultServiceModel>>> GetUserParticipantResultsForContestInPage(
        ICollection<int> contestIds)
    {
        var user = userProviderService.GetCurrentUser();

        var userParticipants = participantsData
            .GetAllByUsernameAndContests(user.Username ?? string.Empty, contestIds);

        return await MapParticipationResultsToContestsInPage(userParticipants);
    }

    public async Task<PagedResult<ContestForListingServiceModel>> GetParticipatedByUserByFiltersAndSorting(
        string username,
        ContestFiltersServiceModel? sortAndFilterModel,
        int? contestId = null,
        int? categoryId = null)
    {
        sortAndFilterModel = await this.GetNestedFilterCategoriesIfAny(sortAndFilterModel);

        var participatedContests = contestsData.GetLatestForParticipantByUsername(username);

        if (contestId.HasValue)
        {
            participatedContests = participatedContests.Where(c => c.Id == contestId.Value);
        }

        if (categoryId.HasValue)
        {
            participatedContests = participatedContests.Where(c => c.CategoryId == categoryId.Value);
        }

        var participatedContestsInPage = await contestsData.ApplyFiltersSortAndPagination<ContestForListingServiceModel>(
            participatedContests,
            sortAndFilterModel);

        var participantResultsByContest = new Dictionary<int, List<ParticipantResultServiceModel>>();
        var loggedInUser = userProviderService.GetCurrentUser();

        if (loggedInUser.IsAuthenticated && (loggedInUser.Username == username || loggedInUser.IsAdmin))
        {
            var contestIds = participatedContestsInPage
                .Items
                .Select(c => c.Id)
                .ToList();

            // Lecturers should not see points
            var userParticipants = participantsData
                .GetAllByUsernameAndContests(username, contestIds);

            participantResultsByContest =
                await MapParticipationResultsToContestsInPage(userParticipants);
        }

        return await this.PrepareActivityAndResults(participatedContestsInPage, participantResultsByContest);
    }

    public async Task<ContestsForHomeIndexServiceModel> GetAllForHomeIndex()
    {
        var active = await this.GetAllCompetable()
            .ToListAsync();
        //set CanBeCompeted and CanBePracticed properties in each active contest
        await activityService.SetCanBeCompetedAndPracticed(active);

        var past = await this.GetAllPastContests()
            .ToListAsync();
        //set CanBeCompeted and CanBePracticed properties in each active contest
        await activityService.SetCanBeCompetedAndPracticed(past);

        return new ContestsForHomeIndexServiceModel { ActiveContests = active, PastContests = past, };
    }

    public async Task<IEnumerable<ContestForHomeIndexServiceModel>> GetAllCompetable()
        => await contestsData
            .GetAllCompetable<ContestForHomeIndexServiceModel>()
            .OrderByDescendingAsync(ac => ac.EndTime)
            .TakeAsync(DefaultContestsPerPage);

    public async Task<IEnumerable<ContestForHomeIndexServiceModel>> GetAllPastContests()
        => await contestsData
            .GetAllExpired<ContestForHomeIndexServiceModel>()
            .OrderByDescendingAsync(ac => ac.EndTime)
            .TakeAsync(DefaultContestsPerPage);

    /// <summary>
    /// Maps activity properties, total results count and user participant results if any.
    /// </summary>
    /// <param name="pagedContests">Contests.</param>
    /// <param name="participantResultsByContest">The user participant results, grouped by contestId.
    /// The values are of list type representing the participants associated with this contest id - usually practice and compete participants.</param>
    /// <returns>A paged collection of contests.</returns>
    public async Task<PagedResult<ContestForListingServiceModel>> PrepareActivityAndResults(
        PagedResult<ContestForListingServiceModel> pagedContests,
        Dictionary<int, List<ParticipantResultServiceModel>> participantResultsByContest)
    {
        var participantsCount = await contestParticipantsCacheService
            .GetParticipantsCount(
                [
                    ..pagedContests.Items
                        .Select(c => c.Id)
                        .Distinct(),
                ],
                pagedContests.PageNumber);

        activityService.SetCanBeCompetedAndPracticed(
            pagedContests.Items.ToList(),
            participantResultsByContest.Values.SelectMany(x => x).ToList());

        await pagedContests.Items.ForEachAsync(c =>
        {
            c.CompeteResults = participantsCount[c.Id].Official;
            c.PracticeResults = participantsCount[c.Id].Practice;

            ParticipantResultServiceModel? competeParticipant = null;
            ParticipantResultServiceModel? practiceParticipant = null;
            if (participantResultsByContest.Count != 0)
            {
                var participants = participantResultsByContest.GetValueOrDefault(c.Id);
                if (participants != null)
                {
                    competeParticipant = participants.SingleOrDefault(p => p.IsOfficial);
                    practiceParticipant = participants.SingleOrDefault(p => !p.IsOfficial);

                    c.UserParticipationResult = new ContestParticipantResultServiceModel
                    {
                        CompetePoints = competeParticipant?.Points,
                        PracticePoints = practiceParticipant?.Points,
                    };
                }
            }

            c.RequirePasswordForCompete = ShouldRequirePassword(c.HasContestPassword, c.HasPracticePassword, competeParticipant, true);
            c.RequirePasswordForPractice = ShouldRequirePassword(c.HasContestPassword, c.HasPracticePassword, practiceParticipant, false);
        });

        return pagedContests;
    }

    public async Task<IEnumerable<string?>> GetEmailsOfParticipantsInContest(int contestId)
        => await participantsData
            .GetAllOfficialByContest(contestId)
            .Select(participant => participant.User.Email!)
            .ToListAsync();

    public async Task<IEnumerable<ContestForListingServiceModel>> GetAllParticipatedContests(string username)
         => await contestsData
             .GetLatestForParticipantByUsername(username)
             .MapCollection<ContestForListingServiceModel>()
             .ToListAsync();

    private static async Task<Dictionary<int, List<ParticipantResultServiceModel>>> MapParticipationResultsToContestsInPage(
        IQueryable<Participant> participants)
        => await participants
            .MapCollection<ParticipantResultServiceModel>()
            .GroupBy(p => p.ContestId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

    private static bool ShouldRequirePassword(bool hasContestPassword, bool hasPracticePassword, IParticipantForActivityServiceModel? participant, bool official)
        => participant is not { IsInvalidated: false }
           && ((official && hasContestPassword) || (!official && hasPracticePassword));

    private static bool ShouldConfirmParticipation(IParticipantForActivityServiceModel? participant, bool official, bool contestIsOnlineExam, bool userIsAdminOrLecturerInContest)
        => contestIsOnlineExam &&
           official &&
           (participant == null || participant.IsInvalidated) &&
           !userIsAdminOrLecturerInContest;

    private async Task<ContestFiltersServiceModel> GetNestedFilterCategoriesIfAny(ContestFiltersServiceModel? model)
    {
        model ??= new ContestFiltersServiceModel();
        model.PageNumber ??= 1;
        model.ItemsPerPage ??= DefaultContestsPerPage;

        // TODO: This is repeated in GetAllByFiltersAndSorting
        if (model.CategoryIds.Count() == 1)
        {
            var subcategories = await contestCategoriesCache
                .GetContestSubCategoriesList(model.CategoryIds.First(), CacheConstants.OneHourInSeconds);

            model.CategoryIds = model.CategoryIds
                .Concat([.. subcategories.Select(cc => cc.Id)]);
        }

        return model;
    }

    private async Task<Participant?> AddNewParticipantToContestIfNotExistsOrResetExistingToValid(
        ContestRegistrationDetailsServiceModel contest,
        bool official,
        string userId,
        bool isUserAdminOrLecturerInContest)
    {
        var participant = await participantsData.GetByContestByUserAndByIsOfficial(contest.Id, userId, official);
        if (participant != null)
        {
            if (participant.IsInvalidated)
            {
                participant.IsInvalidated = false;
                participantsData.Update(participant);
                await participantsData.SaveChanges();
            }

            return participant;
        }

        return await participantsBusiness.CreateNewByContestByUserByIsOfficialAndIsAdminOrLecturer(
            contest,
            userId,
            official,
            isUserAdminOrLecturerInContest);
    }
}