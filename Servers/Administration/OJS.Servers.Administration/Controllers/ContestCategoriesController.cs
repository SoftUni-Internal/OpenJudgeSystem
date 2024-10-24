namespace OJS.Servers.Administration.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OJS.Common;
using OJS.Data.Models.Contests;
using OJS.Servers.Administration.Attributes;
using OJS.Servers.Infrastructure.Extensions;
using OJS.Services.Administration.Business;
using OJS.Services.Administration.Business.ContestCategories;
using OJS.Services.Administration.Business.ContestCategories.GridData;
using OJS.Services.Administration.Business.ContestCategories.Validators;
using OJS.Services.Administration.Business.Users.Permissions;
using OJS.Services.Administration.Models.ContestCategories;
using OJS.Services.Infrastructure.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ContestCategoriesController : BaseAdminApiController<ContestCategory, int, ContestCategoryInListModel, ContestCategoryAdministrationModel>
{
    private readonly IContestCategoriesBusinessService contestCategoriesBusinessService;

    public ContestCategoriesController(
        IContestCategoriesBusinessService contestCategoriesBusinessService,
        ContestCategoryAdministrationModelValidator validator,
        IContestCategoriesGridDataService contestCategoryGridDataService)
    : base(
        contestCategoryGridDataService,
        contestCategoriesBusinessService,
        validator) => this.contestCategoriesBusinessService = contestCategoriesBusinessService;

    [HttpGet]
    public IActionResult GetForContestDropdown()
        => this.Ok(
             this.contestCategoriesBusinessService
            .GetAllVisible()
            .Where(x => !x.IsDeleted)
            .ToHashSet()
            .MapCollection<ContestCategoriesInContestView>());

    [HttpGet]
    [ProtectedEntityAction(nameof(userId), typeof(UserIdPermissionService))]
    public async Task<IActionResult> GetForLecturerInContestCategory(string userId)
        => await this.contestCategoriesBusinessService
            .GetForLecturerInContestCategory(userId)
            .ToOkResult();

    [HttpGet]
    [Authorize(Roles = GlobalConstants.Roles.Administrator)]
    public async Task<IActionResult> GetHierarchy()
        => await this.contestCategoriesBusinessService
            .GetHierarchy()
            .ToOkResult();

    [HttpPatch]
    [ProtectedEntityAction(false)]
    [Authorize(Roles = GlobalConstants.Roles.Administrator)]
    public async Task<IActionResult> EditHierarchy(Dictionary<int, ContestCategoriesHierarchyEditModel> categoriesToUpdate)
    {
        await this.contestCategoriesBusinessService.EditHierarchy(categoriesToUpdate);

        return this.Ok("The hierarchy has been updated successfully.");
    }
}