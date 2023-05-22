namespace OJS.Servers.Ui.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using OJS.Data.Models.Users;
    using OJS.Servers.Infrastructure.Controllers;
    using OJS.Servers.Infrastructure.Extensions;
    using OJS.Servers.Ui.Models;
    using System.Threading.Tasks;
    using static OJS.Servers.Infrastructure.ServerConstants;

    [Authorize]
    public class AccountController : BaseViewController
    {
        private readonly UserManager<UserProfile> userManager;
        private readonly SignInManager<UserProfile> signInManager;

        public AccountController(
            UserManager<UserProfile> userManager,
            SignInManager<UserProfile> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            this.ViewData[ViewDataKeys.ReturnUrl] = returnUrl;
            return this.RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody]LoginRequestModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction("Index", "Home");
            }

            var signInResult = await this.signInManager
                .PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

            if (!signInResult.Succeeded)
            {
                this.ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password");

                return this.Unauthorized(model);
            }

            var user = await this.userManager.FindByNameAsync(model.UserName);
            var roles = await this.userManager.GetRolesAsync(user);

            this.HttpContext.AppendAuthInfoCookies(roles, user.UserName);
            return this.RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            this.HttpContext.ClearAuthInfoCookies();
            return this.RedirectToAction("Index", "Home");
        }
    }
}