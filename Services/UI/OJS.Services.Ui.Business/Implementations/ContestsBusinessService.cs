namespace OJS.Services.Ui.Business.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using X.PagedList;
    using FluentExtensions.Extensions;
    using Microsoft.EntityFrameworkCore;
    using OJS.Common;
    using OJS.Data.Models.Participants;
    using OJS.Services.Common;
    using OJS.Services.Common.Models.Contests;
    using OJS.Services.Infrastructure.Constants;
    using OJS.Services.Infrastructure.Exceptions;
    using OJS.Services.Infrastructure.Extensions;
    using OJS.Services.Infrastructure.Models;
    using OJS.Services.Ui.Business.Cache;
    using OJS.Services.Ui.Business.Validations.Implementations.Contests;
    using OJS.Services.Ui.Data;
    using OJS.Services.Ui.Models.Contests;
    using OJS.Services.Ui.Models.Search;
    using static OJS.Services.Common.Constants.PaginationConstants.Contests;

    public class ContestsBusinessService : IContestsBusinessService
    {
        private readonly IContestsDataService contestsData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IContestsActivityService activityService;
        private readonly IExamGroupsDataService examGroupsData;
        private readonly IParticipantsDataService participantsData;
        private readonly IParticipantsBusinessService participantsBusiness;
        private readonly IContestCategoriesCacheService contestCategoriesCache;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly IUserProviderService userProviderService;
        private readonly IContestParticipationValidationService contestParticipationValidationService;
        private readonly IContestParticipantsCacheService contestParticipantsCacheService;
        private readonly IContestsCacheService contestsCacheService;
        private readonly ILecturersInContestsBusinessService lecturersInContestsBusiness;
        private readonly IContestDetailsValidationService contestDetailsValidationService;

        public ContestsBusinessService(
            IContestsDataService contestsData,
            ISubmissionsDataService submissionsData,
            IContestsActivityService activityService,
            IExamGroupsDataService examGroupsData,
            IParticipantsDataService participantsData,
            IParticipantScoresDataService participantScoresData,
            IUserProviderService userProviderService,
            IParticipantsBusinessService participantsBusiness,
            IContestCategoriesCacheService contestCategoriesCache,
            IContestParticipationValidationService contestParticipationValidationService,
            IContestParticipantsCacheService contestParticipantsCacheService,
            IContestsCacheService contestsCacheService,
            ILecturersInContestsBusinessService lecturersInContestsBusiness,
            IContestDetailsValidationService contestDetailsValidationService)
        {
            this.contestsData = contestsData;
            this.submissionsData = submissionsData;
            this.activityService = activityService;
            this.examGroupsData = examGroupsData;
            this.participantsData = participantsData;
            this.participantScoresData = participantScoresData;
            this.userProviderService = userProviderService;
            this.participantsBusiness = participantsBusiness;
            this.contestCategoriesCache = contestCategoriesCache;
            this.contestParticipationValidationService = contestParticipationValidationService;
            this.contestParticipantsCacheService = contestParticipantsCacheService;
            this.contestsCacheService = contestsCacheService;
            this.lecturersInContestsBusiness = lecturersInContestsBusiness;
            this.contestDetailsValidationService = contestDetailsValidationService;
        }

        public async Task<ContestDetailsServiceModel> GetContestDetails(int id)
        {
            var user = this.userProviderService.GetCurrentUser();
            var contest = await this.contestsCacheService.GetContestDetailsServiceModel(id);
            var category = await this.contestCategoriesCache.GetById(contest?.CategoryId);
            var isLecturerInContestOrAdmin = await this.lecturersInContestsBusiness.IsCurrentUserAdminOrLecturerInContest(contest?.Id);

            var validationResult = this.contestDetailsValidationService.GetValidationResult((
                contest,
                category,
                isLecturerInContestOrAdmin));

            if (!validationResult.IsValid)
            {
                throw new BusinessServiceException(validationResult.Message);
            }

            var activityServiceModel = contest!.Map<ContestForActivityServiceModel>();

            var contestActivityEntity = await this.activityService
                .GetContestActivity(activityServiceModel);

            var userParticipants = await this.participantsData
                .GetWithProblemsForParticipantsByContestAndUser(id, user.Id)
                .ToListAsync();

            var competeParticipant = userParticipants.FirstOrDefault(p => p.IsOfficial);
            var practiceParticipant = userParticipants.FirstOrDefault(p => !p.IsOfficial);

            var participantToGetProblemsFrom = contestActivityEntity.CanBeCompeted
                ? competeParticipant
                : practiceParticipant;

            if (!isLecturerInContestOrAdmin && participantToGetProblemsFrom != null && contestActivityEntity.CanBeCompeted)
            {
                var problemsForParticipant = participantToGetProblemsFrom.ProblemsForParticipants.Select(x => x.Problem);
                contest.Problems = problemsForParticipant.Map<ICollection<ContestProblemServiceModel>>();
            }

            var canShowProblemsInCompete = (!contest!.HasContestPassword && !contest!.IsOnlineExam && contestActivityEntity.CanBeCompeted && participantToGetProblemsFrom != null) || isLecturerInContestOrAdmin;
            var canShowProblemsInPractice = (!contest.HasPracticePassword && contestActivityEntity.CanBePracticed) || isLecturerInContestOrAdmin;
            var canShowProblemsForAnonymous = user.IsAuthenticated || !contestActivityEntity.CanBeCompeted;

            if ((!canShowProblemsInPractice && !canShowProblemsInCompete) || !canShowProblemsForAnonymous)
            {
                contest.Problems = new List<ContestProblemServiceModel>();
            }

            if (isLecturerInContestOrAdmin || competeParticipant != null)
            {
                contest.CanViewCompeteResults = true;
            }

            if (isLecturerInContestOrAdmin || contestActivityEntity.CanBeCompeted || contestActivityEntity.CanBePracticed)
            {
                contest.CanViewPracticeResults = true;
            }

            var participantsCount = await this.contestParticipantsCacheService.GetParticipantsCountForContest(id);

            contest.CompeteParticipantsCount = participantsCount.Official;
            contest.PracticeParticipantsCount = participantsCount.Practice;

            contest!.CanBeCompeted = contestActivityEntity.CanBeCompeted;
            contest.CanBePracticed = contestActivityEntity.CanBePracticed;

            contest.IsAdminOrLecturerInContest = isLecturerInContestOrAdmin;

            contest.IsActive = await this.activityService.IsContestActive(activityServiceModel);

            return contest;
        }

        public async Task<ContestRegistrationDetailsServiceModel> GetContestRegistrationDetails(int id, bool isOfficial)
        {
            var contest = await this.contestsData.OneByIdTo<ContestRegistrationDetailsServiceModel>(id);
            var user = this.userProviderService.GetCurrentUser();
            var category = await this.contestCategoriesCache.GetById(contest?.CategoryId);

            var validationResult = this.contestParticipationValidationService.GetValidationResult((
                contest?.Map<ContestParticipationValidationServiceModel>(),
                category,
                user,
                isOfficial));

            if (!validationResult.IsValid)
            {
                throw new BusinessServiceException(validationResult.Message);
            }

            var participant = await this.participantsData
                .GetByContestByUserAndByIsOfficial(
                    id,
                    user.Id,
                    isOfficial);

            var userIsAdminOrLecturerInContest = await this.lecturersInContestsBusiness.IsCurrentUserAdminOrLecturerInContest(contest?.Id);

            contest!.RequirePassword = ShouldRequirePassword(contest.HasContestPassword, contest.HasPracticePassword, participant!, isOfficial);
            contest.ParticipantId = participant?.Id;
            contest.IsRegisteredSuccessfully = participant != null && !participant.IsInvalidated;
            contest.ShouldConfirmParticipation = ShouldConfirmParticipation(participant, isOfficial, contest.IsOnlineExam, userIsAdminOrLecturerInContest);

            return contest;
        }

        public async Task<bool> RegisterUserForContest(
            int id,
            string? password,
            bool? hasConfirmedParticipation,
            bool isOfficial)
        {
            var user = this.userProviderService.GetCurrentUser();
            var contest = await this.contestsData.OneByIdTo<ContestRegistrationDetailsServiceModel>(id);
            var category = await this.contestCategoriesCache.GetById(contest?.CategoryId);

            var validationResult = this.contestParticipationValidationService.GetValidationResult((
                contest?.Map<ContestParticipationValidationServiceModel>(),
                category,
                user,
                isOfficial));

            if (!validationResult.IsValid)
            {
                throw new BusinessServiceException(validationResult.Message);
            }

            var participant = await this.participantsData
                .GetByContestByUserAndByIsOfficial(
                    id,
                    user.Id,
                    isOfficial);

            var userIsAdminOrLecturerInContest = await this.lecturersInContestsBusiness.IsCurrentUserAdminOrLecturerInContest(contest?.Id);
            var shouldRequirePassword = ShouldRequirePassword(contest!.HasContestPassword, contest!.HasPracticePassword, participant, isOfficial);
            var shouldConfirmParticipation =
                ShouldConfirmParticipation(participant, isOfficial, contest!.IsOnlineExam, userIsAdminOrLecturerInContest);

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

        public async Task<ContestServiceModel> GetContestByProblem(int problemId)
        {
           var contestServiceModel = await this.contestsData.GetByProblemId<ContestServiceModel>(problemId);
           if (contestServiceModel == null)
           {
               throw new BusinessServiceException(GlobalConstants.ErrorMessages.ContestNotFound);
           }

           contestServiceModel.AllowedSubmissionTypes = contestServiceModel.AllowedSubmissionTypes.DistinctBy(st => st.Id);

           return contestServiceModel;
        }

        public async Task ValidateContestPassword(int id, bool official, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new BusinessServiceException("Password is empty");
            }

            var contest = await this.contestsData.OneById(id);

            var isOfficialAndIsCompetePasswordCorrect =
                official && contest!.HasContestPassword && contest.ContestPassword == password;

            var isPracticeAndIsPracticePasswordCorrect =
                !official && contest!.HasPracticePassword && contest.PracticePassword == password;

            if (!isOfficialAndIsCompetePasswordCorrect && !isPracticeAndIsPracticePasswordCorrect)
            {
                throw new BusinessServiceException("Incorrect password!");
            }
        }

        public async Task<ContestParticipationServiceModel> GetParticipationDetails(
            StartContestParticipationServiceModel model)
        {
            var user = this.userProviderService.GetCurrentUser();

            if (!await this.participantsData.ExistsByContestByUserAndIsOfficial(model.ContestId, user.Id, model.IsOfficial))
            {
                // Participant must be registered in previous steps
                return new ContestParticipationServiceModel
                {
                    IsActiveParticipant = false,
                    IsRegisteredParticipant = false,
                    Contest = null,
                };
            }

            var participant = await this.participantsData
                .GetQuery(p => p.ContestId == model.ContestId && p.UserId == user.Id && p.IsOfficial == model.IsOfficial)
                .MapCollection<ContestParticipationServiceModel>()
                .FirstOrDefaultAsync() ?? throw new BusinessServiceException("Participant not found");

            var contest = await this.contestParticipantsCacheService.GetContestServiceModelForContest(model.ContestId);
            var category = await this.contestCategoriesCache.GetById(contest?.CategoryId);

            var validationResult = this.contestParticipationValidationService.GetValidationResult((
                contest?.Map<ContestParticipationValidationServiceModel>(),
                category,
                user,
                model.IsOfficial));

            if (!validationResult.IsValid)
            {
                throw new BusinessServiceException(validationResult.Message);
            }

            var userIsAdminOrLecturerInContest = await this.lecturersInContestsBusiness.IsCurrentUserAdminOrLecturerInContest(contest?.Id);

            participant.Contest = contest;
            participant.IsRegisteredParticipant = true;
            participant.Contest!.UserIsAdminOrLecturerInContest = userIsAdminOrLecturerInContest;

            var participantActivity = this.activityService.GetParticipantActivity(participant.Map<ParticipantForActivityServiceModel>());
            participant.EndDateTimeForParticipantOrContest = participantActivity.ParticipationEndTime;
            participant.IsActiveParticipant = participantActivity.IsActive || userIsAdminOrLecturerInContest;

            // explicitly setting lastSubmissionTime to avoid including all submissions for participant
            var lastSubmissionTime = this.submissionsData
                .GetAllForUserByContest(contest!.Id, user.Id)
                .Select(x => (DateTime?)x.CreatedOn)
                .Max();
            participant.LastSubmissionTime = lastSubmissionTime;

            participant.Contest!.AllowedSubmissionTypes = participant.Contest.Problems
                .SelectMany(p => p.AllowedSubmissionTypes)
                .DistinctBy(st => st.Id)
                .ToList();

            participant.ParticipantId = participant.Id;
            participant.UserSubmissionsTimeLimit = contest.LimitBetweenSubmissions;

            var participantsList = new List<int> { participant.Id, };

            var maxParticipationScores = await this.participantScoresData
                .GetMaxByProblemIdsAndParticipation(
                    participant.Contest.Problems.Select(x => x.Id),
                    participantsList);

            var isOfficialOnlineContest = model.IsOfficial && contest.IsOnlineExam;

            if (!userIsAdminOrLecturerInContest && isOfficialOnlineContest)
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
                await this.contestParticipantsCacheService.GetParticipantsCountForContest(model.ContestId);

            participant.ParticipantsCount = model.IsOfficial
                ? participantsCount.Official
                : participantsCount.Practice;

            return participant;
        }

        public async Task<ContestSearchServiceResultModel> GetSearchContestsByName(
            SearchServiceModel model)
        {
            var modelResult = new ContestSearchServiceResultModel();

            var allContestsQueryable = this.contestsData.GetAllVisible()
                .Include(c => c.Category)
                .Where(c => (c.Name != null && c.Name.Contains(model.SearchTerm ?? string.Empty)) &&
                            (c.Category != null && c.Category.IsVisible));

            var searchContests = await allContestsQueryable
                .MapCollection<ContestForListingServiceModel>()
                .ToPagedListAsync(model.PageNumber, model.ItemsPerPage);

            modelResult.Contests = searchContests;
            modelResult.TotalContestsCount = allContestsQueryable.Count();

            return modelResult;
        }

        public Task<bool> IsContestIpValidByContestAndIp(int contestId, string ip)
            => this.contestsData
                .Exists(c =>
                    c.Id == contestId &&
                    (!c.IpsInContests.Any() || c.IpsInContests.Any(ai => ai.Ip.Value == ip)));

        public async Task<PagedResult<ContestForListingServiceModel>> GetAllByFiltersAndSorting(
            ContestFiltersServiceModel? model)
        {
            model = await this.GetNestedFilterCategoriesIfAny(model);

            var pagedContests =
                await this.contestsData.GetAllAsPageByFiltersAndSorting<ContestForListingServiceModel>(model);

            var participantResultsByContest = new Dictionary<int, List<ParticipantResultServiceModel>>();
            var user = this.userProviderService.GetCurrentUser();
            if (user.IsAuthenticated)
            {
                participantResultsByContest = await this.GetUserParticipantResultsForContestInPage(pagedContests
                    .Items
                    .Select(c => c.Id)
                    .ToList());
            }

            return await this.PrepareActivityAndResults(pagedContests, participantResultsByContest);
        }

        public async Task<Dictionary<int, List<ParticipantResultServiceModel>>> GetUserParticipantResultsForContestInPage(
            ICollection<int> contestIds)
        {
            var user = this.userProviderService.GetCurrentUser();

            var userParticipants = this.participantsData
                .GetAllByUsernameAndContests(user.Username ?? string.Empty, contestIds);

            return await MapParticipationResultsToContestsInPage(contestIds, userParticipants);
        }

        public async Task<PagedResult<ContestForListingServiceModel>> GetParticipatedByUserByFiltersAndSorting(
            string username,
            ContestFiltersServiceModel? sortAndFilterModel)
        {
            sortAndFilterModel = await this.GetNestedFilterCategoriesIfAny(sortAndFilterModel);

            var participatedContestsInPage =
                await this.contestsData.ApplyFiltersSortAndPagination<ContestForListingServiceModel>(
                    this.contestsData.GetLatestForParticipantByUsername(username),
                    sortAndFilterModel);

            var participantResultsByContest = new Dictionary<int, List<ParticipantResultServiceModel>>();
            var loggedInUser = this.userProviderService.GetCurrentUser();

            if (loggedInUser.IsAuthenticated && (loggedInUser.Username == username || loggedInUser.IsAdmin))
            {
                var contestIds = participatedContestsInPage.Items.Select(c => c.Id).ToList();

                // Lecturers should not see points
                var userParticipants = this.participantsData
                    .GetAllByUsernameAndContests(username, contestIds);

                participantResultsByContest =
                    await MapParticipationResultsToContestsInPage(
                        contestIds,
                        userParticipants);
            }

            return await this.PrepareActivityAndResults(participatedContestsInPage, participantResultsByContest);
        }

        public async Task<ContestsForHomeIndexServiceModel> GetAllForHomeIndex()
        {
            var active = await this.GetAllCompetable()
                .ToListAsync();
            //set CanBeCompeted and CanBePracticed properties in each active contest
            await this.activityService.SetCanBeCompetedAndPracticed(active);

            var past = await this.GetAllPastContests()
                .ToListAsync();
            //set CanBeCompeted and CanBePracticed properties in each active contest
            await this.activityService.SetCanBeCompetedAndPracticed(past);

            return new ContestsForHomeIndexServiceModel { ActiveContests = active, PastContests = past, };
        }

        public async Task<IEnumerable<ContestForHomeIndexServiceModel>> GetAllCompetable()
            => await this.contestsData
                .GetAllCompetable<ContestForHomeIndexServiceModel>()
                .OrderByDescendingAsync(ac => ac.EndTime)
                .TakeAsync(DefaultContestsPerPage);

        public async Task<IEnumerable<ContestForHomeIndexServiceModel>> GetAllPastContests()
            => await this.contestsData
                .GetAllExpired<ContestForHomeIndexServiceModel>()
                .OrderByDescendingAsync(ac => ac.EndTime)
                .TakeAsync(DefaultContestsPerPage);

        public async Task DeleteById(int id)
        {
            await this.examGroupsData.RemoveContestByContest(id);

            await this.contestsData.DeleteById(id);
            await this.contestsData.SaveChanges();
        }

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
            var participantsCount = await this.contestParticipantsCacheService
                .GetParticipantsCount(
                    pagedContests.Items
                        .Select(c => c.Id)
                        .Distinct()
                        .ToImmutableArray(),
                    pagedContests.PageNumber);

            this.activityService.SetCanBeCompetedAndPracticed(
                pagedContests.Items.ToList(),
                participantResultsByContest.Values.SelectMany(x => x).ToList());

            await pagedContests.Items.ForEachAsync(c =>
            {
                c.CompeteResults = participantsCount[c.Id].Official;
                c.PracticeResults = participantsCount[c.Id].Practice;

                ParticipantResultServiceModel? competeParticipant = null;
                ParticipantResultServiceModel? practiceParticipant = null;
                if (participantResultsByContest.Any())
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

                c.RequirePasswordForCompete = ShouldRequirePassword(c.HasContestPassword, c.HasPracticePassword, competeParticipant?.Map<Participant>(), true);
                c.RequirePasswordForPractice = ShouldRequirePassword(c.HasContestPassword, c.HasPracticePassword, practiceParticipant?.Map<Participant>(), false);
            });

            return pagedContests;
        }

        private static async Task<Dictionary<int, List<ParticipantResultServiceModel>>> MapParticipationResultsToContestsInPage(
            IEnumerable<int> contestIds,
            IQueryable<Participant> participants)
        {
            var participatedContestIds = contestIds
                .Distinct();

            return (await participants
                .Where(p => participatedContestIds.Contains(p.ContestId))
                .MapCollection<ParticipantResultServiceModel>()
                .ToListAsync())
                .GroupBy(p => p.ContestId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static bool ShouldRequirePassword(bool hasContestPassword, bool hasPracticePassword, Participant? participant, bool official)
        {
            if (participant != null && !participant.IsInvalidated)
            {
                return false;
            }

            return (official && hasContestPassword) || (!official && hasPracticePassword);
        }

        private static bool ShouldConfirmParticipation(Participant? participant, bool official, bool contestIsOnlineExam, bool userIsAdminOrLecturerInContest)
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
                var subcategories = await this.contestCategoriesCache
                    .GetContestSubCategoriesList(model.CategoryIds.First(), CacheConstants.OneHourInSeconds);

                model.CategoryIds = model.CategoryIds
                    .Concat(subcategories
                        .Select(cc => cc.Id)
                        .ToList());
            }

            return model;
        }

        private async Task<Participant?> AddNewParticipantToContestIfNotExistsOrResetExistingToValid(
            ContestRegistrationDetailsServiceModel contest,
            bool official,
            string userId,
            bool isUserAdminOrLecturerInContest)
        {
            var participant = await this.participantsData.GetByContestByUserAndByIsOfficial(contest.Id, userId, official);
            if (participant != null)
            {
                if (participant.IsInvalidated)
                {
                    participant.IsInvalidated = false;
                    this.participantsData.Update(participant);
                    await this.participantsData.SaveChanges();
                }

                return participant;
            }

            return await this.participantsBusiness.CreateNewByContestByUserByIsOfficialAndIsAdminOrLecturer(
                contest,
                userId,
                official,
                isUserAdminOrLecturerInContest);
        }
    }
}