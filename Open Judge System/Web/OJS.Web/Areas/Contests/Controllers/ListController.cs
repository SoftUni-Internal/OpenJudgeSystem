namespace OJS.Web.Areas.Contests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Services.Cache;
    using OJS.Services.Cache.Models;
    using OJS.Services.Data.ContestCategories;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;
    using OJS.Web.Areas.Contests.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using X.PagedList;

    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ListController : BaseController
    {
        private const int DefaultContestsPerPage = 10;

        private readonly IContestsDataService contestsData;
        private readonly IContestCategoriesDataService contestCategoriesData;
        private readonly IParticipantsDataService participantsData;
        private readonly ICacheItemsProviderService cacheItems;

        public ListController(
            IOjsData data,
            IContestsDataService contestsData,
            IContestCategoriesDataService contestCategoriesData,
            IParticipantsDataService participantsData,
            ICacheItemsProviderService cacheItems)
            : base(data)
        {
            this.contestsData = contestsData;
            this.contestCategoriesData = contestCategoriesData;
            this.participantsData = participantsData;
            this.cacheItems = cacheItems;
        }

        public ActionResult Index() => this.View();

        public ActionResult ReadCategories(int? id) =>
            this.Json(this.cacheItems.GetContestSubCategoriesList(id), JsonRequestBehavior.AllowGet);

        public ActionResult GetParents(int id) =>
            this.Json(this.cacheItems.GetContestCategoryParentsList(id), JsonRequestBehavior.AllowGet);

        public ActionResult ByCategory(int? id, int? page)
        {
            var userId = this.UserProfile?.Id;
            var contestCategory = this.GetContestCategoryFromCache(id);

            if (contestCategory == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Category_not_found);
            }

            if (id.HasValue && this.contestCategoriesData.HasContestsById(id.Value))
            {
                page = page ?? 1;

                contestCategory.IsUserLecturerInContestCategory =
                    this.CheckIfUserHasContestCategoryPermissions(contestCategory.Id);

                var contests = this.GetContestsListPage(contestCategory.Id, userId, page.Value, DefaultContestsPerPage);
                var contestIds = contests.Select(c => c.Id).ToArray();
                var participantsCount =
                    this.cacheItems.GetParticipantsCountForContestsInCategoryPage(contestIds, contestCategory.Id, page);
                
                var userParticipants = this.participantsData.GetAllByManyContestsAndUserId(contestIds, userId)
                    .Select(ParticipantStatusModel.FromParticipant)
                    .ToList();
                
                // Operations in memory to speed up db query
                foreach (var contest in contests)
                {
                    var participantsCountForContest = participantsCount.GetValuerOrDefault(contest.Id, new ParticipantsCountCacheModel());
                    contest.OfficialParticipants = participantsCountForContest.Official;
                    contest.PracticeParticipants = participantsCountForContest.Practice;
                    var contestUserParticipants = userParticipants.Where(p => p.ContestId == contest.Id).ToList();
                    this.FillCurrentParticipantInfo(contest, contestUserParticipants);

                    contest.UserIsAdminOrLecturerInContest =
                        contest.UserIsAdminOrLecturerInContest || contestCategory.IsUserLecturerInContestCategory;
                }

                contestCategory.Contests = contests;
            }

            var isAjaxRequest = this.Request.IsAjaxRequest();

            this.ViewBag.IsAjax = isAjaxRequest;

            if (isAjaxRequest)
            {
                return this.PartialView(contestCategory);
            }

            return this.View(contestCategory);
        }

        public ActionResult BySubmissionType(int? id, string submissionTypeName)
        {
            SubmissionTypeViewModel submissionType;
            if (id.HasValue)
            {
                submissionType = this.Data.SubmissionTypes
                    .All()
                    .Where(st => st.Id == id.Value)
                    .Select(SubmissionTypeViewModel.FromSubmissionType)
                    .FirstOrDefault();
            }
            else
            {
                throw new HttpException((int)HttpStatusCode.BadRequest, Resource.Invalid_request);
            }

            if (submissionType == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Submission_type_not_found);
            }

            var contests = this.contestsData
                .GetAllVisibleBySubmissionType(submissionType.Id)
                .OrderBy(c => c.OrderBy)
                .Select(ContestSimpleViewModel.FromContest);

            this.ViewBag.SubmissionType = submissionType.Name;
            return this.View(contests);
        }

        private ContestCategoryViewModel GetContestCategoryFromCache(int? id)
        {
            var contestCategory = new ContestCategoryViewModel
            {
                Id = id ?? default(int),
                CategoryName = Resource.Main_categories
            };

            if (id.HasValue)
            {
                var categoryName = this.cacheItems.GetContestCategoryName(id.Value);

                if (categoryName == null)
                {
                    return null;
                }

                contestCategory.CategoryName = categoryName;
            }

            if (!contestCategory.SubCategories.Any())
            {
                contestCategory.SubCategories = this.cacheItems.GetContestSubCategoriesList(id);
            }

            return contestCategory;
        }

        private void FillCurrentParticipantInfo(
            ContestListViewModel contest,
            ICollection<ParticipantStatusModel> participantsForUser)
        {
            contest.UserIsParticipant = participantsForUser.Any();

            // Contest entry time can be reached, but a participant may still have individual time left for an Online Contest
            var shouldCheckParticipantIndividualEndTime = !contest.CanBeCompeted &&
                contest.Type == ContestType.OnlinePracticalExam;

            if (contest.UserIsParticipant && shouldCheckParticipantIndividualEndTime)
            {
                var participant = participantsForUser.FirstOrDefault(p => p.IsOfficial);
                contest.CanBeCompeted = participant?.ParticipationEndTime >= DateTime.Now;
            }
        }

        private IPagedList<ContestListViewModel> GetContestsListPage(int categoryId, string userId, int page, int pageSize) =>
            this.contestsData
                .GetAllVisibleByCategory(categoryId)
                .OrderBy(c => c.OrderBy)
                .ThenByDescending(c => c.EndTime ?? c.PracticeEndTime ?? c.PracticeStartTime)
                .Select(ContestListViewModel.FromContest(userId, this.User.IsAdmin()))
                .ToPagedList(page, pageSize);
    }
}