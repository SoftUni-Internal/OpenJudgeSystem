﻿namespace OJS.Services.Administration.Models.Users;

using AutoMapper;
using OJS.Data.Models.Users;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class UserInListModel : IMapExplicitly
{
    public string? Id { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? City { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public bool IsDeleted { get; set; }

    public int Age { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<UserProfile, UserInListModel>()
            .ForMember(uilm => uilm.Age, opt
                => opt.MapFrom(u => u.UserSettings.Age))
            .ForMember(uilm => uilm.DateOfBirth, opt
                => opt.MapFrom(u => u.UserSettings.DateOfBirth))
            .ForMember(uilm => uilm.FirstName, opt
                => opt.MapFrom(u => u.UserSettings.FirstName))
            .ForMember(uilm => uilm.LastName, opt
                => opt.MapFrom(u => u.UserSettings.LastName))
            .ForMember(uilm => uilm.City, opt
                => opt.MapFrom(u => u.UserSettings.City));
}