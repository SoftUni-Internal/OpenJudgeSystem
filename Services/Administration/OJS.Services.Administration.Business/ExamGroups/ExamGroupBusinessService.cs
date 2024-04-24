namespace OJS.Services.Administration.Business.ExamGroups;

using Microsoft.EntityFrameworkCore;
using OJS.Common.Enumerations;
using OJS.Data.Models;
using OJS.Data.Models.Contests;
using OJS.Data.Models.Users;
using OJS.Data.Validation;
using OJS.Services.Administration.Data;
using OJS.Services.Administration.Models.ExamGroups;
using OJS.Services.Common.Data;
using OJS.Services.Common.Models;
using OJS.Services.Common.Models.Users;
using OJS.Services.Infrastructure.BackgroundJobs;
using OJS.Services.Infrastructure.Exceptions;
using OJS.Services.Infrastructure.HttpClients;
using SoftUni.AutoMapper.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GlobalConstants = OJS.Common.GlobalConstants;

public class ExamGroupBusinessService : AdministrationOperationService<ExamGroup, int, ExamGroupAdministrationModel>,
    IExamGroupsBusinessService
{
    private readonly IExamGroupsDataService examGroupsDataService;
    private readonly IContestsDataService contestsDataService;
    private readonly IUsersDataService usersDataService;
    private readonly IHangfireBackgroundJobsService backgroundJobsService;
    private readonly ISulsPlatformHttpClientService httpClientService;
    private readonly IDataService<UserInExamGroup> userInExamGroupService;

    public ExamGroupBusinessService(
        IExamGroupsDataService examGroupsDataService,
        IContestsDataService contestsDataService,
        IUsersDataService usersDataService,
        IHangfireBackgroundJobsService backgroundJobsService,
        ISulsPlatformHttpClientService httpClientService,
        IDataService<UserInExamGroup> userInExamGroupService)
    {
        this.examGroupsDataService = examGroupsDataService;
        this.contestsDataService = contestsDataService;
        this.usersDataService = usersDataService;
        this.backgroundJobsService = backgroundJobsService;
        this.httpClientService = httpClientService;
        this.userInExamGroupService = userInExamGroupService;
    }

    public override Task<ExamGroupAdministrationModel> Get(int id) =>
        this.examGroupsDataService.GetByIdQuery(id).MapCollection<ExamGroupAdministrationModel>()
            .FirstAsync();

    public override async Task<ExamGroupAdministrationModel> Create(ExamGroupAdministrationModel model)
    {
        var examGroup = model.Map<ExamGroup>();

        await this.examGroupsDataService.Add(examGroup);
        await this.examGroupsDataService.SaveChanges();

        return model;
    }

    public override async Task<ExamGroupAdministrationModel> Edit(ExamGroupAdministrationModel model)
    {
        var examGroup = await this.examGroupsDataService.GetByIdQuery(model.Id).FirstAsync();

        examGroup.MapFrom(model);

        if (model.ContestId == 0)
        {
            examGroup.ContestId = null;
        }

        this.examGroupsDataService.Update(examGroup);
        await this.examGroupsDataService.SaveChanges();

        return model;
    }

    public async Task<UserToExamGroupModel> AddUserToExamGroup(UserToExamGroupModel model)
    {
        var examGroup = await this.examGroupsDataService.GetByIdWithUsersQuery(model.ExamGroupId)
            .FirstAsync();

        if (examGroup.UsersInExamGroups.Any(x => x.UserId == model.UserId))
        {
            throw new BusinessServiceException($"User is already in the exam group {examGroup.Name}.");
        }

        examGroup.UsersInExamGroups.Add(new UserInExamGroup
        {
            UserId = model.UserId!,
            ExamGroupId = examGroup.Id,
        });

        this.examGroupsDataService.Update(examGroup);
        await this.examGroupsDataService.SaveChanges();

        return model;
    }

    public async Task<MultipleUsersToExamGroupModel> AddMultipleUsersToExamGroup(MultipleUsersToExamGroupModel model)
    {
        var usernames = (model.UserNames ?? string.Empty)
            .Split(new[] { ",", " ", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            .Where(username => Regex.IsMatch(username, ConstraintConstants.User.UsernameRegEx));

        var examGroup = await this.examGroupsDataService.GetByIdWithUsersQuery(model.ExamGroupId).FirstAsync();

        var users = this.usersDataService.GetQuery(u => usernames.Contains(u.UserName));

        await this.AddUsersToExamGroup(examGroup, users);

        var externalUsernames = usernames
            .Except(users.Select(u => u.UserName), StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (externalUsernames.Any())
        {
            this.backgroundJobsService.AddFireAndForgetJob<IExamGroupsBusinessService>(
                x => x.AddExternalUsersByIdAndUsernames(examGroup.Id, externalUsernames),
                ApplicationName.Administration.ToString().ToLower());
        }

        return model;
    }

    public async Task<UserToExamGroupModel> RemoveUserFromExamGroup(UserToExamGroupModel model)
    {
        var examGroup = await this.examGroupsDataService.GetByIdQuery(model.ExamGroupId)
            .Include(x => x.UsersInExamGroups)
            .FirstAsync();

        var userExamGroup = examGroup.UsersInExamGroups.FirstOrDefault(x => x.UserId == model.UserId);
        if (userExamGroup == null)
        {
            throw new BusinessServiceException($"User does not present in the exam group {examGroup.Name}.");
        }

        this.userInExamGroupService.Delete(userExamGroup);
        this.examGroupsDataService.Update(examGroup);
        await this.examGroupsDataService.SaveChanges();

        return model;
    }

    public override async Task Delete(int id)
    {
        await this.examGroupsDataService.DeleteById(id);
        await this.examGroupsDataService.SaveChanges();
    }

    public async Task AddExternalUsersByIdAndUsernames(int examGroupId, IEnumerable<string> usernames)
    {
        var examGroup = await this.examGroupsDataService
            .GetByIdWithUsersQuery(examGroupId)
            .FirstOrDefaultAsync();

        if (examGroup == null)
        {
            throw new BusinessServiceException($"Exam group with id: {examGroupId} not found.");
        }

        foreach (var username in usernames)
        {
            await this.AddExternalUser(examGroup, null, username);
        }

        await this.examGroupsDataService.SaveChanges();
    }

    private async Task AddUsersToExamGroup(ExamGroup examGroup, IQueryable<UserProfile> users)
    {
        var usersToAdd = users
            .Where(u => u.UsersInExamGroups.All(eg => eg.ExamGroupId != examGroup.Id))
            .ToList();

        foreach (var user in usersToAdd)
        {
            if (user.IsDeleted)
            {
                user.IsDeleted = false;
            }

            examGroup.UsersInExamGroups.Add(new UserInExamGroup
            {
                UserId = user.Id,
                ExamGroupId = examGroup.Id,
            });
        }

        this.examGroupsDataService.Update(examGroup);
        await this.examGroupsDataService.SaveChanges();
    }

    private async Task AddExternalUser(ExamGroup examGroup, string? userId, string? username = null)
    {
        ExternalDataRetrievalResult<ExternalUserInfoModel> response;

        if (userId != null)
        {
            response = await this.httpClientService.GetAsync<ExternalUserInfoModel>(
                new { userId },
                GlobalConstants.Urls.GetUserInfoByIdPath);
        }
        else if (username != null)
        {
            response = await this.httpClientService.GetAsync<ExternalUserInfoModel>(
                new { username },
                string.Format(GlobalConstants.Urls.GetUserInfoByUsernamePath));
        }
        else
        {
            throw new ArgumentNullException(nameof(username));
        }

        if (response.IsSuccess)
        {
            if (response.Data == null)
            {
                return;
            }

            var user = response.Data.Entity;

            examGroup.UsersInExamGroups.Add(new UserInExamGroup
            {
                User = user,
                ExamGroupId = examGroup.Id,
            });

            this.examGroupsDataService.Update(examGroup);
        }
        else
        {
            throw new BusinessServiceException(response.ErrorMessage!);
        }
    }
}