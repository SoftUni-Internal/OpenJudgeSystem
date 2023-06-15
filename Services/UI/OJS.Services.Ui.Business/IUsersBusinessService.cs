﻿namespace OJS.Services.Ui.Business
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using OJS.Data.Models.Users;
    using OJS.Services.Ui.Models.Search;
    using OJS.Services.Ui.Models.Users;
    using SoftUni.Services.Infrastructure;

    public interface IUsersBusinessService : IService
    {
        public Task<UserProfileServiceModel?> GetUserProfileByUsername(string? username);

        public Task<UserProfileServiceModel?> GetUserProfileById(string userId);

        Task<UserSearchServiceResultModel> GetSearchUsersByUsername(SearchServiceModel model);

        Task<bool> IsLoggedInUserAdmin(ClaimsPrincipal userPrincipal);

        Task AddOrUpdateUser(UserProfile userEntity);

        Task<UserAuthInfoServiceModel?> GetAuthInfo();
    }
}