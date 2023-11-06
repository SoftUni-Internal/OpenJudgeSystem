namespace OJS.Web.Infrastructure.Seeders
{
    using System.Linq;
    using Microsoft.AspNet.Identity;
    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Services.Common.HttpRequester;
    using OJS.Services.Common.HttpRequester.Models.Users;

    public class AdministratorSeeder : ISeeder
    {
        private readonly UserManager<UserProfile> userManager;
        private readonly IHttpRequesterService httpRequester;

        public AdministratorSeeder(UserManager<UserProfile> userManager, IHttpRequesterService httpRequester)
        {
            this.userManager = userManager;
            this.httpRequester = httpRequester;
        }

        public void SeedData()
        {
            if (this.userManager.Users.Any())
            {
                return;
            }

            var result = this.httpRequester.GetAsync<ExternalUserInfoModel>(
                new { UserName = GlobalConstants.AdministratorUserName },
                string.Format(UrlConstants.GetUserInfoByUsernameApiFormat, Settings.SulsPlatformBaseUrl),
                Settings.SulsApiKey).GetAwaiter().GetResult();

            if (!result.IsSuccess)
            {
                return;
            }

            var user = result.Data.Entity;

            var createResult = this.userManager.Create(user);

            if (createResult.Succeeded)
            {
                this.userManager.AddToRoleAsync(user.Id, GlobalConstants.AdministratorRoleName);
            }
        }
    }
}