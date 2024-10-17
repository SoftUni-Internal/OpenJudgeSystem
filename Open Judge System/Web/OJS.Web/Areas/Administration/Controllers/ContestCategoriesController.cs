namespace OJS.Web.Areas.Administration.Controllers
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using EntityFramework.Extensions;
    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;
    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Business.ContestCategories;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Common.HttpRequester;
    using OJS.Services.Data.Contests;
    using OJS.Web.Areas.Administration.Controllers.Common;
    using OJS.Web.Areas.Administration.ViewModels.ContestCategory;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Infrastructure.Filters.Attributes;
    using OJS.Web.ViewModels.Common;
    using ContestsResource = Resources.Areas.Administration.Contests.ContestsControllers;
    using DatabaseModelType = OJS.Data.Models.ContestCategory;
    using ViewModelType = OJS.Web.Areas.Administration.ViewModels.ContestCategory.ContestCategoryAdministrationViewModel;

    public class ContestCategoriesController : AdministrationBaseGridController
    {
        private readonly IHangfireBackgroundJobService backgroundJobs;
        private readonly IHttpRequesterService httpRequester;
        private readonly IContestsDataService contestsData;

        public ContestCategoriesController(
            IOjsData data,
            IHangfireBackgroundJobService backgroundJobs,
            IHttpRequesterService httpRequester,
            IContestsDataService contestsData)
            : base(data)
        {
            this.backgroundJobs = backgroundJobs;
            this.httpRequester = httpRequester;
            this.contestsData = contestsData;
        }

        public override IEnumerable GetData()
        {
            return this.Data.ContestCategories
                .All()
                .Where(cat => !cat.IsDeleted)
                .Select(ViewModelType.ViewModel);
        }

        public override object GetById(object id)
        {
            return this.Data.ContestCategories
                .All()
                .FirstOrDefault(o => o.Id == (int)id);
        }

        public override string GetEntityKeyName() =>
            this.GetEntityKeyNameByType(typeof(DatabaseModelType));

        public ActionResult Index() => this.View();

        [HttpPost]
        [ClearMainContestCategoriesCache]
        [ClearContestCategoryCache(queryKeyForCategoryId: nameof(ViewModelType.Id))]
        public ActionResult Create([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var databaseModel = model.GetEntityModel();
            model.Id = (int)this.BaseCreate(databaseModel);
            this.UpdateAuditInfoValues(model, databaseModel);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        [ClearMainContestCategoriesCache]
        [ClearContestCategoryCache(queryKeyForCategoryId: nameof(ViewModelType.Id))]
        public ActionResult Update([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var entity = this.GetById(model.Id) as DatabaseModelType;
            this.BaseUpdate(model.GetEntityModel(entity));
            this.UpdateAuditInfoValues(model, entity);
            return this.GridOperation(request, model);
        }

        [HttpPost]
        [ClearMainContestCategoriesCache]
        [ClearContestCategoryCache(queryKeyForCategoryId: nameof(ViewModelType.Id))]
        public ActionResult Destroy([DataSourceRequest]DataSourceRequest request, ViewModelType model)
        {
            var contest = this.Data.ContestCategories.GetById(model.Id.Value);
            this.CascadeDeleteCategories(contest);
            return this.Json(this.ModelState.ToDataSourceResult());
        }

        public ActionResult Hierarchy() => this.View();

        public ActionResult ReadCategories(int? id)
        {
            var categories =
                this.Data.ContestCategories.All()
                    .Where(x => x.IsVisible)
                    .Where(x => id.HasValue ? x.ParentId == id : x.ParentId == null)
                    .OrderBy(x => x.OrderBy)
                    .Select(x => new { id = x.Id, hasChildren = x.Children.Any(), x.Name, });

            return this.Json(categories, JsonRequestBehavior.AllowGet);
        }

        [ClearMainContestCategoriesCache]
        [ClearMovedContestCategoryCache("id", "to")]
        public void MoveCategory(int id, int? to)
        {
            if (id != to)
            {
                var category = this.Data.ContestCategories.GetById(id);
                category.ParentId = to;
                this.Data.SaveChanges();
            }
        }

        [HttpGet]
        public ActionResult EditAllContests(int categoryId)
        {
            if (!this.CheckIfUserHasContestCategoryPermissions(categoryId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            var category = this.Data.ContestCategories.GetById(categoryId) ?? throw new HttpException(404, "Category not found");

            var model = new EditAllContestsAdministrationViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
            };

            this.PrepareEditContestsViewBagData();

            this.ViewBag.ReturnUrl = GetReturnUrlToCategory(model.CategoryId, model.CategoryName);
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAllContests(EditAllContestsAdministrationViewModel model, string returnUrl)
        {
            if (!this.CheckIfUserHasContestCategoryPermissions(model.CategoryId))
            {
                return this.RedirectToContestsAdminPanelWithNoPrivilegesMessage();
            }

            this.ViewBag.ReturnUrl = returnUrl;

            if (model.StartTime >= model.EndTime)
            {
                this.ModelState.AddModelError(nameof(EditAllContestsAdministrationViewModel.StartTime), ContestsResource.Contest_start_date_before_end);
            }

            if (model.PracticeStartTime >= model.PracticeEndTime)
            {
                this.ModelState.AddModelError(nameof(EditAllContestsAdministrationViewModel.PracticeStartTime), ContestsResource.Practice_start_date_before_end);
            }

            if (!this.ModelState.IsValid)
            {
                this.PrepareEditContestsViewBagData();
                return this.View(model);
            }

            var category = this.Data.ContestCategories.GetById(model.CategoryId) ?? throw new HttpException(404, "Category not found");

            this.contestsData.GetAllNotDeletedByCategory(category.Id, true)
                .Update(c => new Contest
                {
                    // Preserve the original values if a new value is not selected.
                    Type = model.Type == null || model.Type == 0 ? c.Type : (ContestType)model.Type,
                    LimitBetweenSubmissions = model.LimitBetweenSubmissions ?? c.LimitBetweenSubmissions,

                    // Overwrite the original values with the new ones, even when they are null, as they are optional for a contest.
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    PracticeStartTime = model.PracticeStartTime,
                    PracticeEndTime = model.PracticeEndTime,
                });

            this.TempData.AddInfoMessage($"All contests in category {category.Name} are edited successfully.");

            return this.Redirect(returnUrl);
        }

        [HttpGet]
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public ActionResult ImportContests(int categoryId)
        {
            var category = this.Data.ContestCategories.GetById(categoryId) ?? throw new HttpException(404, "Category not found");

            var model = new ImportContestsViewModel
            {
                CategoryId = category.Id,
                CategoryName = category.Name,
            };

            this.ViewBag.ReturnUrl = GetReturnUrlToCategory(model.CategoryId, model.CategoryName);
            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public ActionResult ImportContests(ImportContestsViewModel model, string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var category = this.Data.ContestCategories.GetById(model.CategoryId) ?? throw new HttpException(404, "Category not found");

            var contestIdsToImport = model.ContestIdsToImport
                .Split(new[] { '\r', '\n', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .Distinct()
                .ToArray();

            var existingContestIds = this.httpRequester.Get<int[]>(
                new { ids = contestIdsToImport },
                $"{model.OjsPlatformUrl.TrimEnd('/')}/api/Contests/GetExistingIds",
                model.ApiKey);

            if (!existingContestIds.IsSuccess)
            {
                if (existingContestIds.ErrorMessage.Contains("Invalid API key"))
                {
                    this.ModelState.AddModelError(nameof(model.ApiKey), "Invalid user API key.");
                }
                else
                {
                    this.ModelState.AddModelError(nameof(model.OjsPlatformUrl), existingContestIds.ErrorMessage);
                }

                return this.View(model);
            }

            if (existingContestIds.Data.Length != contestIdsToImport.Length)
            {
                var invalidIds = contestIdsToImport.Except(existingContestIds.Data).ToArray();
                this.ModelState.AddModelError(
                    nameof(model.ContestIdsToImport),
                    $"Some contests do not exist in the provided OJS platform. The following IDs are invalid: {string.Join(", ", invalidIds)}");
                return this.View(model);
            }

            this.backgroundJobs.AddFireAndForgetJob<IContestCategoriesImportService>(
                x => x.ImportContestsIntoCategory(
                    category.Id,
                    model.OjsPlatformUrl,
                    model.ReplaceExistingContests,
                    model.ApiKey,
                    contestIdsToImport));

            this.TempData.AddInfoMessage(
                $"Contests are being imported into {category.Name}. This may take a while. " +
                "You can continue working in the meantime and check the results later.");

            return this.Redirect(returnUrl);
        }

        private static string GetReturnUrlToCategory(int categoryId, string categoryName) =>
            $"/Contests/#!/List/ByCategory/{categoryId}/{categoryName.ToUrl()}";

        private void PrepareEditContestsViewBagData()
        {
            this.ViewBag.TypeData = new[] { new DropdownViewModel { Name = string.Empty } }
                .Concat(DropdownViewModel.GetEnumValues<ContestType>());
        }

        private void CascadeDeleteCategories(DatabaseModelType contest)
        {
            foreach (var children in contest.Children.ToList())
            {
                this.CascadeDeleteCategories(children);
            }

            this.Data.ContestCategories.Delete(contest);
            this.Data.SaveChanges();
        }
    }
}