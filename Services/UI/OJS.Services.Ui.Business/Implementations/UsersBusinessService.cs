namespace OJS.Services.Ui.Business.Implementations
{
    using FluentExtensions.Extensions;
    using Microsoft.EntityFrameworkCore;
    using OJS.Data.Models.Users;
    using OJS.Services.Common;
    using OJS.Services.Infrastructure.Exceptions;
    using OJS.Services.Ui.Data;
    using OJS.Services.Ui.Models.Search;
    using OJS.Services.Ui.Models.Users;
    using OJS.Services.Infrastructure.Extensions;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using X.PagedList;

    public class UsersBusinessService : IUsersBusinessService
    {
        private readonly IUsersProfileDataService usersProfileData;
        private readonly IUserProviderService userProvider;

        public UsersBusinessService(
            IUsersProfileDataService usersProfileData,
            IUserProviderService userProvider)
        {
            this.usersProfileData = usersProfileData;
            this.userProvider = userProvider;
        }

        public async Task<UserProfileServiceModel?> GetUserProfileByUsername(string? username)
        {
            var currentUser = this.userProvider.GetCurrentUser();

            if (currentUser.Id == null && (username.IsNull() || username!.IsEmpty()))
            {
                throw new BusinessServiceException("Empty username is not valid");
            }

            var userWithUsernameExists = await this.usersProfileData.Exists(p => p.UserName == username);

            if (!userWithUsernameExists)
            {
                throw new BusinessServiceException("User with this username does not exist");
            }

            if (currentUser.Id == null && userWithUsernameExists)
            {
                return await Task.FromResult(new UserProfileServiceModel
                {
                    UserName = username ?? string.Empty,
                });
            }

            bool isLoggedInUserAdminOrProfileOwner = this.IsUserAdminOrProfileOwner(username);

            var profile = await (isLoggedInUserAdminOrProfileOwner
                ? this.usersProfileData
                    .GetByUsername<UserProfileServiceModel>(username)
                : this.GetByUsernameAsShortProfile(username));

            profile!.Id = isLoggedInUserAdminOrProfileOwner ? profile.Id : string.Empty;

            return profile;
        }

        public async Task<string?> GetUserIdByUsername(string? username)
        {
            var currentUser = this.userProvider.GetCurrentUser();

            if (currentUser.Id == null && (username.IsNull() || username!.IsEmpty()))
            {
                throw new BusinessServiceException("Empty username is not valid");
            }

            var userWithUsernameExists = await this.usersProfileData.Exists(p => p.UserName == username);

            if (!userWithUsernameExists)
            {
                throw new BusinessServiceException("User with this username does not exist");
            }

            bool isLoggedInUserAdminLecturerOrProfileOwner = this.IsUserAdminLecturerOrProfileOwner(username);

            if (!isLoggedInUserAdminLecturerOrProfileOwner)
            {
                throw new BusinessServiceException("You are not authorized to view this information");
            }

            var profile = await this.GetByUsernameAsShortProfile(username);

            return profile!.Id;
        }

        public async Task<UserProfileServiceModel?> GetUserProfileById(string userId) =>
            await this.usersProfileData
                .GetByIdQuery(userId)
                .AsNoTracking()
                .FirstOrDefaultAsync()
                .Map<UserProfileServiceModel>();

        public async Task<UserSearchServiceResultModel> GetSearchUsersByUsername(SearchServiceModel model)
        {
            var modelResult = new UserSearchServiceResultModel();

            var allUsersQueryable = this.usersProfileData
                .GetAll()
                .Where(u => u.UserName!.Contains(model.SearchTerm!));

            var searchUsers = await allUsersQueryable
                .MapCollection<UserSearchServiceModel>()
                .ToPagedListAsync(model.PageNumber, model.ItemsPerPage);

            modelResult.Users = searchUsers;
            modelResult.TotalUsersCount = allUsersQueryable.Count();

            return modelResult;
        }

        public async Task<bool> IsLoggedInUserAdmin(ClaimsPrincipal userPrincipal)
            => await Task.FromResult(userPrincipal.IsInRole("Administrator"));

        public async Task AddOrUpdateUser(UserProfile user)
            => await this.usersProfileData.AddOrUpdate<UserProfileServiceModel>(user);

        public async Task<UserAuthInfoServiceModel?> GetAuthInfo()
        {
            var currentUser = this.userProvider.GetCurrentUser();

            if (currentUser.IsNull())
            {
                return null;
            }

            var profile = await this.usersProfileData
                .GetByIdQuery(currentUser.Id!)
                .Include(u => u.UsersInRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync();

            return profile?.Map<UserAuthInfoServiceModel>();
        }

        public bool IsUserAdminLecturerOrProfileOwner(string? profileUsername)
        {
            var currentUser = this.userProvider.GetCurrentUser();

            if (currentUser == null || string.IsNullOrEmpty(profileUsername))
            {
                return false;
            }

            return currentUser.IsAdminOrLecturer ||
                   this.usersProfileData.GetByIdQuery(currentUser.Id).Any(u => u.UserName == profileUsername);
        }

        private bool IsUserAdminOrProfileOwner(string? profileUsername)
        {
            var currentUser = this.userProvider.GetCurrentUser();

            if (currentUser.IsNull() || string.IsNullOrEmpty(profileUsername))
            {
                return false;
            }

            return currentUser.IsAdmin ||
                   this.usersProfileData.GetByIdQuery(currentUser.Id).Any(u => u.UserName == profileUsername);
        }

        //AsNoTracking() Method is added to prevent ''tracking query'' error.
        //Error is thrown when we map from UserSettings (owned entity) without including the
        //UserProfile (owner entity) in the query.
        private async Task<UserProfileServiceModel?> GetByUsernameAsShortProfile(string? username) =>
            await this.usersProfileData.GetByUsername(username)
                .AsNoTracking()
                .Select(user => new UserProfileServiceModel
                {
                    UserName = user.UserName!,
                    Id = user.Id,
                })
                .FirstOrDefaultAsync();
    }
}