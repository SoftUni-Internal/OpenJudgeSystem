namespace OJS.Servers.Ui.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using OJS.Common;
    using OJS.Data.Models.Users;
    using OJS.Servers.Infrastructure.Controllers;
    using OJS.Servers.Ui.Models;
    using OJS.Services.Common.Models.Users;
    using OJS.Services.Infrastructure;
    using OJS.Services.Infrastructure.Constants;
    using OJS.Services.Infrastructure.HttpClients;
    using OJS.Services.Ui.Business;
    using static OJS.Common.GlobalConstants.Urls;

    [Authorize]
    public class AccountController : BaseApiController
    {
        private readonly IUsersBusinessService usersBusinessService;
        private readonly SignInManager<UserProfile> signInManager;
        private readonly UserManager<UserProfile> userManager;
        private readonly ISulsPlatformHttpClientService sulsPlatformHttpClient;
        private readonly ILogger<AccountController> logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AccountController(
            IUsersBusinessService usersBusinessService,
            SignInManager<UserProfile> signInManager,
            ISulsPlatformHttpClientService sulsPlatformHttpClient,
            ILogger<AccountController> logger,
            IWebHostEnvironment webHostEnvironment,
            UserManager<UserProfile> userManager)
        {
            this.usersBusinessService = usersBusinessService;
            this.signInManager = signInManager;
            this.sulsPlatformHttpClient = sulsPlatformHttpClient;
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
            this.userManager = userManager;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]LoginRequestModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            var platformCallResult = new ExternalDataRetrievalResult<ExternalUserInfoModel>();

            try
            {
                this.logger.LogStartingHttpRequest("GET", GetUserInfoByUsernamePath);
                platformCallResult = await this.sulsPlatformHttpClient.GetAsync<ExternalUserInfoModel>(
                    new { model.UserName },
                    GetUserInfoByUsernamePath);

                if (platformCallResult.IsSuccess)
                {
                    this.logger.LogPlatformDataReceived(model.UserName);
                }
                else
                {
                    this.logger.LogPlatformDataNotReceived(model.UserName, platformCallResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogErrorGettingPlatformData(model.UserName, ex);
            }

            if (platformCallResult.IsSuccess)
            {
                var externalUser = platformCallResult.Data;

                if (externalUser == null)
                {
                    return this.Unauthorized(GlobalConstants.ErrorMessages.InvalidUsernameOrPassword);
                }

                await this.usersBusinessService.AddOrUpdateUser(externalUser.Entity);
            }
            else if (this.webHostEnvironment.IsProduction())
            {
                var user = await this.userManager.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName);
                if (user != null && await this.userManager.IsInRoleAsync(user, GlobalConstants.Roles.Administrator))
                {
                    var signInResult = await this.signInManager
                        .PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                    if (signInResult.Succeeded)
                    {
                        return this.Ok(GlobalConstants.ErrorMessages.LoggedInThroughDatabase);
                    }
                }

                return this.Unauthorized(GlobalConstants.ErrorMessages.InactiveLoginSystem);
            }

            var result = await this.signInManager
                    .PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
            if (!result.Succeeded)
            {
                return this.Unauthorized(GlobalConstants.ErrorMessages.InvalidUsernameOrPassword);
            }

            return this.Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await this.signInManager.SignOutAsync();
            return this.Ok();
        }
    }
}