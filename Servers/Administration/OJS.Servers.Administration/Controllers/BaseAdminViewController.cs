namespace OJS.Servers.Administration.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using OJS.Servers.Administration.Extensions;
    using OJS.Servers.Infrastructure.Controllers;
    using static OJS.Common.GlobalConstants.Roles;
    using AdminResource = OJS.Common.Resources.AdministrationGeneral;

    [Authorize(Roles = AdministratorOrLecturer)]
    public class BaseAdminViewController : BaseViewController
    {
        protected IActionResult RedirectToContestsAdminPanelWithNoPrivilegesMessage()
        {
            this.TempData.AddDangerMessage(AdminResource.NoPrivilegesMessage);
            return this.RedirectToAction("Index", "Contests");
        }
    }
}