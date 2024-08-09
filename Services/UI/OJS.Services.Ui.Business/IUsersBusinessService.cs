namespace OJS.Services.Ui.Business
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using OJS.Data.Models.Users;
    using OJS.Services.Ui.Models.Search;
    using OJS.Services.Ui.Models.Users;
    using OJS.Services.Infrastructure;

    public interface IUsersBusinessService : IService
    {
        public Task<UserProfileServiceModel?> GetUserProfileByUsername(string? username);

        public Task<string?> GetUserIdByUsername(string? username);

        public Task<UserProfileServiceModel?> GetUserProfileById(string userId);

        Task<UserSearchServiceResultModel> GetSearchUsersByUsername(SearchServiceModel model);

        Task<bool> IsLoggedInUserAdmin(ClaimsPrincipal userPrincipal);

        bool IsUserAdminLecturerOrProfileOwner(string? profileUsername);

        Task AddOrUpdateUser(UserProfile userEntity);

        Task<UserAuthInfoServiceModel?> GetAuthInfo();
    }
}