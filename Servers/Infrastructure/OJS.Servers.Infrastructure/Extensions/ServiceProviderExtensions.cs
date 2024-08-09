namespace OJS.Servers.Infrastructure.Extensions
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using OJS.Data.Models.Users;
    using System;
    using System.Threading.Tasks;
    using static OJS.Common.GlobalConstants.Roles;

    public static class ServiceProviderExtensions
    {
        public static async Task CreateOrUpdateRoles(this IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<UserProfile>>();
            string[] roleNames = { Administrator, Lecturer, Developer };
            bool shouldCreateAdminUser = false;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);

                if (!roleExist)
                {
                    await roleManager.CreateAsync(new Role(roleName));
                }

                if (roleName == Administrator && !roleExist)
                {
                    shouldCreateAdminUser = true;
                }
            }

            if (shouldCreateAdminUser)
            {
                var userProfile = new UserProfile
                {
                    UserName = "judge.admin",
                    Email = "judge.admin@softuni.org",
                };

                var userPassword = "1234QwERt)";
                var user = await userManager.FindByEmailAsync(userProfile.Email);

                if (user == null)
                {
                    var createPowerUser = await userManager.CreateAsync(userProfile, userPassword);
                    if (createPowerUser.Succeeded)
                    {
                        await userManager.AddToRoleAsync(userProfile, Administrator);
                    }
                }
            }
        }
    }
}