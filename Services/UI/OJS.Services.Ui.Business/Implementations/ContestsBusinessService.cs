namespace OJS.Services.Ui.Business.Implementations
{
    using FluentExtensions.Extensions;
    using Microsoft.EntityFrameworkCore;
    using OJS.Common;
    using OJS.Data.Models.Contests;
    using OJS.Data.Models.Participants;
    using OJS.Services.Common;
    using OJS.Services.Common.Models.Contests;
    using OJS.Services.Infrastructure.Constants;
    using OJS.Services.Infrastructure.Exceptions;
    using OJS.Services.Ui.Business.Cache;
    using OJS.Services.Ui.Business.Validations.Implementations.Contests;
    using OJS.Services.Ui.Data;
    using OJS.Services.Ui.Models.Contests;
    using OJS.Services.Ui.Models.Search;
    using OJS.Services.Ui.Models.Submissions;
    using SoftUni.AutoMapper.Infrastructure.Extensions;
    using SoftUni.Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using X.PagedList;
    using static OJS.Services.Common.PaginationConstants.Contests;

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
        private readonly IUsersBusinessService usersBusinessService;
        private readonly IUserProviderService userProviderService;
        private readonly IContestParticipationValidationService contestParticipationValidationService;
        private readonly IContestParticipantsCacheService contestParticipantsCacheService;
        private readonly ILecturersInContestsBusinessService lecturersInContestsBusiness;
        private readonly IContestDetailsValidationService contestDetailsValidationService;

        public ContestsBusinessService(
            IContestsDataService contestsData,
            ISubmissionsDataService submissionsData,
            IContestsActivityService activityService,
            IExamGroupsDataService examGroupsData,
            IParticipantsDataService participantsData,
            IParticipantScoresDataService participantScoresData,
            IUsersBusinessService usersBusinessService,
            IUserProviderService userProviderService,
            IParticipantsBusinessService participantsBusiness,
            IContestCategoriesCacheService contestCategoriesCache,
            IContestParticipationValidationService contestParticipationValidationService,
            IContestParticipantsCacheService contestParticipantsCacheService,
            ILecturersInContestsBusinessService lecturersInContestsBusiness,
            IContestDetailsValidationService contestDetailsValidationService)
        {
            this.contestsData = contestsData;
            this.submissionsData = submissionsData;
            this.activityService = activityService;
            this.examGroupsData = examGroupsData;
            this.participantsData = participantsData;
            this.participantScoresData = participantScoresData;
            this.usersBusinessService = usersBusinessService;
            this.userProviderService = userProviderService;
            this.participantsBusiness = participantsBusiness;
            this.contestCategoriesCache = contestCategoriesCache;
            this.contestParticipationValidationService = contestParticipationValidationService;
            this.contestParticipantsCacheService = contestParticipantsCacheService;
            this.lecturersInContestsBusiness = lecturersInContestsBusiness;
            this.contestDetailsValidationService = contestDetailsValidationService;
        }

        public async Task<ContestDetailsServiceModel> GetContestDetails(int id)
        {
            var user = this.userProviderService.GetCurrentUser();
            var contest = await this.contestsData.GetByIdWithCategoryAndProblemsAndSubmissionTypes(id);
            var isLecturerOrAdmin = await this.lecturersInContestsBusiness.IsCurrentUserAdminOrLecturerInContest(contest?.Id);

            var validationResult = this.contestDetailsValidationService.GetValidationResult((
                contest,
                id,
                isLecturerOrAdmin));
            if (!validationResult.IsValid)
            {
                throw new BusinessServiceException(validationResult.Message);
            }

            var contestActivityEntity = this.activityService
                .GetContestActivity(contest!.Map<ContestForActivityServiceModel>());

            var participant = await this.participantsData
                .GetWithProblemsForParticipantsByContestByUserAndIsOfficial(
                    id,
                    user.Id,
                    contestActivityEntity.CanBeCompeted);

            var contestDetailsServiceModel = contest!.Map<ContestDetailsServiceModel>();

            // set CanBeCompeted and CanBePracticed properties in contest
            this.activityService.SetCanBeCompetedAndPracticed(contestDetailsServiceModel);

            contestDetailsServiceModel.IsAdminOrLecturerInContest = isLecturerOrAdmin;

            if (!isLecturerOrAdmin && participant != null && contestActivityEntity.CanBeCompeted)
            {
                var problemsForParticipant = participant.ProblemsForParticipants.Select(x => x.Problem);
                contestDetailsServiceModel.Problems = problemsForParticipant.Map<ICollection<ContestProblemServiceModel>>();
            }

            var canShowProblemsInCompete = (!contest!.HasContestPassword && !contest.IsOnlineExam && contestActivityEntity.CanBeCompeted) || isLecturerOrAdmin;
            var canShowProblemsInPractice = (!contest.HasPracticePassword && contestActivityEntity.CanBePracticed) || isLecturerOrAdmin;
            var canShowProblemsForAnonymous = user.Id != null || !contestActivityEntity.CanBeCompeted;

            if ((!canShowProblemsInPractice && !canShowProblemsInCompete) || !canShowProblemsForAnonymous)
            {
                contestDetailsServiceModel.Problems = new List<ContestProblemServiceModel>();
            }

            if (isLecturerOrAdmin || participant != null)
            {
                contestDetailsServiceModel.CanViewResults = true;
            }

            contestDetailsServiceModel.AllowedSubmissionTypes = contest.ProblemGroups
                .SelectMany(pg => pg.Problems)
                .AsQueryable()
                .SelectMany(p => p.SubmissionTypesInProblems)
                .GroupBy(st => st.SubmissionTypeId)
                .Select(g => g.First())
                .Select(x => new ContestDetailsSubmissionTypeServiceModel { Id = x.SubmissionTypeId, Name = x.SubmissionType.Name })
                .ToList();

            var participantsCount = await this.contestParticipantsCacheService.GetParticipantsCountForContest(id);

            contestDetailsServiceModel.CompeteParticipantsCount = participantsCount.Official;
            contestDetailsServiceModel.PracticeParticipantsCount = participantsCount.Practice;

            return contestDetailsServiceModel;
        }

        public async Task<RegisterUserForContestServiceModel> RegisterUserForContest(
            int id, string? password, bool? hasConfirmedParticipation, bool isOfficial)
        {
            var contest = this.contestsData
                .GetByIdQuery(id)
                .Include(c => c.Category)
                .Include(c => c.ProblemGroups)
                    .ThenInclude(pg => pg.Problems)
                .FirstOrDefault();

            var user = this.userProviderService.GetCurrentUser();
            var userProfile = await this.usersBusinessService.GetUserProfileById(user.Id);

            var validationResult = this.contestParticipationValidationService.GetValidationResult((
                contest,
                id,
                user,
                isOfficial)!);

            if (!validationResult.IsValid)
            {
                throw new BusinessServiceException(validationResult.Message);
            }

            var participant = await this.participantsData
                .GetByContestByUserAndByIsOfficial(
                    id,
                    userProfile!.Id,
                    isOfficial);

            var registerModel = contest!.Map<RegisterUserForContestServiceModel>();
            registerModel.RequirePassword = ShouldRequirePassword(contest!, participant!, isOfficial);
            registerModel.ParticipantId = participant?.Id;
            registerModel.IsRegisteredSuccessfully = participant != null;
            registerModel.ShouldConfirmParticipation = contest!.IsOnlineExam && isOfficial && participant == null;

            bool requiredPasswordIsValid = false;

            // Validate password if present
            if (password != null && !password.IsNullOrEmpty() && ShouldRequirePassword(contest!, participant, isOfficial))
            {
                var isPasswordValid = GetIsPasswordValid(contest!, password, isOfficial);

                if (!isPasswordValid)
                {
                    throw new UnauthorizedAccessException("Invalid contest password");
                }

                requiredPasswordIsValid = true;
            }

            bool shouldRequireConfirmParticipationAndHasConfirmed = registerModel.ShouldConfirmParticipation &&
                                                                    hasConfirmedParticipation.HasValue &&
                                                                    hasConfirmedParticipation.Value;

            bool shouldNotRequirePasswordAndIsNotOnline = !registerModel.RequirePassword && !contest.IsOnlineExam;
            bool requiredPasswordIsValidAndIsNotOnline = requiredPasswordIsValid && !contest.IsOnlineExam;

            if (participant == null && (shouldNotRequirePasswordAndIsNotOnline ||
                                        requiredPasswordIsValidAndIsNotOnline ||
                                        (requiredPasswordIsValid && shouldRequireConfirmParticipationAndHasConfirmed) ||
                                        (!registerModel.RequirePassword && shouldRequireConfirmParticipationAndHasConfirmed)))
            {
                var userIsAdminOrLecturerInContest = await this.lecturersInContestsBusiness.IsCurrentUserAdminOrLecturerInContest(contest?.Id);

                await this.AddNewParticipantToContestIfNotExists(contest!, isOfficial, user.Id, userIsAdminOrLecturerInContest);
                registerModel.IsRegisteredSuccessfully = true;
                registerModel.RequirePassword = false;
                registerModel.ShouldConfirmParticipation = false;
            }

            return registerModel;
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

        public async Task<ContestParticipationServiceModel> StartContestParticipation(
            StartContestParticipationServiceModel model)
        {
            var user = this.userProviderService.GetCurrentUser();

            var userProfile = await this.usersBusinessService.GetUserProfileById(user.Id!);

            var participant = await this.participantsData
                .GetWithContestAndSubmissionDetailsByContestByUserAndIsOfficial(
                    model.ContestId,
                    userProfile!.Id,
                    model.IsOfficial);

            if (participant == null)
            {
                return new ContestParticipationServiceModel
                {
                    IsActiveParticipant = false, IsRegisteredParticipant = false, Contest = null,
                };
            }

            var contest = await this.contestsData
                .GetByIdWithProblemsDetailsAndCategories(model.ContestId);

            var validationResult = this.contestParticipationValidationService.GetValidationResult((
                contest,
                model.ContestId,
                user,
                model.IsOfficial)!);

            if (!validationResult.IsValid)
            {
                throw new BusinessServiceException(validationResult.Message);
            }

            var userIsAdminOrLecturerInContest = await this.lecturersInContestsBusiness.IsCurrentUserAdminOrLecturerInContest(contest?.Id);

            var participationModel = participant!.Map<ContestParticipationServiceModel>();
            participationModel.IsRegisteredParticipant = true;
            participationModel.Contest!.UserIsAdminOrLecturerInContest = userIsAdminOrLecturerInContest;
            participationModel.IsActiveParticipant = true;

            // explicitly setting lastSubmissionTime to avoid including all submissions for participant
            var lastSubmissionTime = this.submissionsData
                .GetAllForUserByContest(contest!.Id, user.Id)
                .Select(x => (DateTime?)x.CreatedOn)
                .Max();
            participationModel.LastSubmissionTime = lastSubmissionTime;

            participationModel.Contest!.AllowedSubmissionTypes = participationModel
                .Contest
                .AllowedSubmissionTypes
                .DistinctBy(st => st.Id);

            participationModel.ParticipantId = participant!.Id;
            participationModel.ContestIsCompete = model.IsOfficial;
            participationModel.UserSubmissionsTimeLimit = await this.participantsBusiness.GetParticipantLimitBetweenSubmissions(
                    participant.Id,
                    contest!.LimitBetweenSubmissions);

            var participantsList = new List<int> { participant.Id, };

            var maxParticipationScores = await this.participantScoresData
                .GetMaxByProblemIdsAndParticipation(
                    participationModel.Contest.Problems.Select(x => x.Id),
                    participantsList);

            var isOfficialOnlineContest = model.IsOfficial && contest.IsOnlineExam;

            if (!userIsAdminOrLecturerInContest && isOfficialOnlineContest)
            {
                participationModel.Contest.Problems = participant
                    .ProblemsForParticipants
                    .Select(x => x.Problem)
                    .MapCollection<ContestProblemServiceModel>()
                    .OrderBy(p => p.OrderBy)
                    .ToList();
            }

            await participationModel.Contest.Problems.ForEachAsync(problem =>
            {
                problem.Points = maxParticipationScores
                    .Where(ps => ps.ProblemId == problem.Id)
                    .Select(x => x.Points)
                    .FirstOrDefault();
            });

            var participantsCount =
                await this.contestParticipantsCacheService.GetParticipantsCountForContest(model.ContestId);

            participationModel.ParticipantsCount = model.IsOfficial
                ? participantsCount.Official
                : participantsCount.Practice;

            return participationModel;
        }

        public async Task<ContestSearchServiceResultModel> GetSearchContestsByName(
            SearchServiceModel model)
        {
            var modelResult = new ContestSearchServiceResultModel();

            var allContestsQueryable = this.contestsData.GetAllNonDeletedContests()
                .Include(c => c.Category)
                .Where(c => (c.Name != null && c.Name.Contains(model.SearchTerm ?? string.Empty)) &&
                            (c.Category != null && c.Category.IsVisible));

            var searchContests = await allContestsQueryable
                .MapCollection<ContestSearchServiceModel>()
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
            var contestIds = pagedContests.Items.Select(c => c.Id).ToList();

            return await this.PrepareActivityAndResults(pagedContests);
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

            Dictionary<int, List<ParticipantResultServiceModel>>? participantResultsByContest = null;

            var loggedInUser = this.userProviderService.GetCurrentUser();
            var loggedInUserProfile = await this.usersBusinessService.GetUserProfileById(loggedInUser.Id);

            if (!loggedInUserProfile.IsNull() && (loggedInUserProfile!.UserName == username || loggedInUser.IsAdmin))
            {
                // Lecturers should not see points
                var userParticipants = this.participantsData
                    .GetAllByUsernameAndContests(username, participatedContestsInPage.Items.Select(c => c.Id));

                participantResultsByContest =
                    await MapParticipationResultsToContestsInPage(participatedContestsInPage, userParticipants);
            }

            return await this.PrepareActivityAndResults(participatedContestsInPage, participantResultsByContest);
        }

        public async Task<ContestsForHomeIndexServiceModel> GetAllForHomeIndex()
        {
            var active = await this.GetAllCompetable()
                .ToListAsync();
            //set CanBeCompeted and CanBePracticed properties in each active contest
            active.ForEach(c => this.activityService.SetCanBeCompetedAndPracticed(c));

            var past = await this.GetAllPastContests()
                .ToListAsync();
            //set CanBeCompeted and CanBePracticed properties in each active contest
            past.ForEach(c => this.activityService.SetCanBeCompetedAndPracticed(c));

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

        private static async Task<Dictionary<int, List<ParticipantResultServiceModel>>> MapParticipationResultsToContestsInPage(
            PagedResult<ContestForListingServiceModel> participatedContestsInPage,
            IQueryable<Participant> participants)
        {
            var participatedContestIds = participatedContestsInPage
                .Items
                .Select(c => c.Id)
                .Distinct();

            return (await participants
                .Where(p => participatedContestIds.Contains(p.ContestId))
                .MapCollection<ParticipantResultServiceModel>()
                .ToListAsync())
                .GroupBy(p => p.ContestId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static bool ShouldRequirePassword(Contest contest, Participant? participant, bool official)
        {
            if (participant != null && !participant.IsInvalidated)
            {
                return false;
            }

            return (official && contest.HasContestPassword) || (!official && contest.HasPracticePassword);
        }

        private static bool GetIsPasswordValid(Contest contest, string? password, bool isOfficial)
        {
            if (isOfficial)
            {
                return contest.ContestPassword == password;
            }

            return contest.PracticePassword == password;
        }

        /// <summary>
        /// Maps activity properties, total results count and user participant results if any.
        /// </summary>
        /// <param name="pagedContests">Contests.</param>
        /// <param name="participantResultsByContest">The user participant results, grouped by contestId.
        /// The values are of list type representing the participants associated with this contest id - usually practice and compete participants.</param>
        /// <returns>A paged collection of contests.</returns>
        private async Task<PagedResult<ContestForListingServiceModel>> PrepareActivityAndResults(
            PagedResult<ContestForListingServiceModel> pagedContests,
            Dictionary<int, List<ParticipantResultServiceModel>>? participantResultsByContest = null)
        {
            var participantsCount = await this.contestParticipantsCacheService
                .GetParticipantsCount(
                    pagedContests.Items
                        .Select(c => c.Id)
                        .Distinct()
                        .ToImmutableArray(),
                    pagedContests.PageNumber);

            pagedContests.Items.ForEach(c =>
            {
                this.activityService.SetCanBeCompetedAndPracticed(c);
                c.CompeteResults = participantsCount[c.Id].Official;
                c.PracticeResults = participantsCount[c.Id].Practice;

                if (participantResultsByContest != null)
                {
                    var participants = participantResultsByContest[c.Id];
                    var competePoints = participants.Where(p => p.IsOfficial).Select(p => p.Points).SingleOrDefault();
                    var practicePoints = participants.Where(p => !p.IsOfficial).Select(p => p.Points).SingleOrDefault();

                    c.UserParticipationResult = new ContestParticipantResultServiceModel
                    {
                        CompetePoints = competePoints,
                        PracticePoints = practicePoints,
                    };
                }
            });

            return pagedContests;
        }

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

        private async Task<Participant?> AddNewParticipantToContestIfNotExists(
            Contest contest,
            bool official,
            string userId,
            bool isUserAdminOrLecturerInContest)
        {
            var participant = await this.participantsData.GetByContestByUserAndByIsOfficial(contest.Id, userId, official);
            if (participant != null)
            {
                return participant;
            }

            if (contest.IsOnlineExam &&
                official &&
                !isUserAdminOrLecturerInContest &&
                !await this.contestsData.IsUserInExamGroupByContestAndUser(contest.Id, userId))
            {
                throw new BusinessServiceException(ValidationMessages.Participant.NotRegisteredForExam);
            }

            return await this.participantsBusiness.CreateNewByContestByUserByIsOfficialAndIsAdminOrLecturer(
                contest,
                userId,
                official,
                isUserAdminOrLecturerInContest);
        }

        private async Task<Participant> AddNewParticipantToContest(
            Contest contest,
            bool official,
            string userId,
            bool isUserAdminOrLecturer)
        {
            if (contest.IsExam &&
                official &&
                !isUserAdminOrLecturer &&
                !await this.contestsData.IsUserInExamGroupByContestAndUser(contest.Id, userId))
            {
                throw new BusinessServiceException("You are not registered for this exam!");
            }

            return await this.participantsBusiness.CreateNewByContestByUserByIsOfficialAndIsAdminOrLecturer(
                contest,
                userId,
                official,
                isUserAdminOrLecturer);
        }
    }
}