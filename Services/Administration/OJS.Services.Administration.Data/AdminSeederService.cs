namespace OJS.Services.Administration.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OJS.Common;
using OJS.Data;
using OJS.Data.Models.Users;
using System;
using System.Threading;
using System.Threading.Tasks;

public class AdminSeederService : IHostedService
{
    private readonly IServiceProvider serviceProvider;

    public AdminSeederService(IServiceProvider serviceProvider)
        => this.serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = this.serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<OjsDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserProfile>>();

        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            // Seed already exists
            return;
        }

        await CreateRoleIfNotExists(roleManager, GlobalConstants.Roles.Administrator);
        await CreateRoleIfNotExists(roleManager, GlobalConstants.Roles.Lecturer);

        // Seed admin user
        var adminUser = new UserProfile
        {
            UserName = "admin",
            Email = "judge.admin@softuni.bg",
        };

        var password = Environment.GetEnvironmentVariable("ADMIN_SEED_PASSWORD") ?? "Admin1234!";

        var result = await userManager.CreateAsync(adminUser, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, GlobalConstants.Roles.Administrator);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private static async Task CreateRoleIfNotExists(RoleManager<Role> roleManager, string roleName)
    {
        if (await roleManager.FindByNameAsync(roleName) == null)
        {
            await roleManager.CreateAsync(new Role(roleName));
        }
    }
}