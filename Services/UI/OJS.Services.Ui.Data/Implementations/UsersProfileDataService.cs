﻿namespace OJS.Services.Ui.Data.Implementations
{
    using Microsoft.EntityFrameworkCore;
    using OJS.Data;
    using OJS.Data.Models.Users;
    using OJS.Services.Common.Data.Implementations;
    using OJS.Services.Infrastructure.Extensions;
    using System.Linq;
    using System.Threading.Tasks;

    public class UsersProfileDataService : DataService<UserProfile>, IUsersProfileDataService
    {
        public UsersProfileDataService(OjsDbContext db)
            : base(db)
        {
        }

        public IQueryable<UserProfile> GetAll()
            => this.GetQuery(u => !u.IsDeleted);

        public async Task<TServiceModel> AddOrUpdate<TServiceModel>(UserProfile user)
        {
            var existingUser = await this
                .GetByUsername(user.UserName)
                .FirstOrDefaultAsync();

            if (existingUser == null)
            {
                existingUser = user;
                await this.Add(existingUser);
            }
            else
            {
                existingUser.PasswordHash = user.PasswordHash;
                existingUser.SecurityStamp = user.SecurityStamp;
                existingUser.Email = user.Email;
                existingUser.IsDeleted = user.IsDeleted;
                existingUser.DeletedOn = user.DeletedOn;
                existingUser.ModifiedOn = user.ModifiedOn;
                existingUser.UserSettings = user.UserSettings;

                this.Update(existingUser);
            }

            await this.SaveChanges();

            return existingUser.Map<TServiceModel>();
        }

        public Task<TServiceModel?> GetByUsername<TServiceModel>(string? username)
            => this.GetByUsername(username)
                .AsNoTracking()
                .FirstOrDefaultAsync()
                .Map<TServiceModel?>();

        public IQueryable<UserProfile> GetByUsername(string? username)
            => this.GetQuery(u => u.UserName == username)
                .Include(up => up.UserSettings);
    }
}