﻿namespace OJS.Services.Administration.Models.Contests;

using OJS.Data.Models.Contests;
using SoftUni.AutoMapper.Infrastructure.Models;
using System;
using AutoMapper;

public class ContestInListModel : IMapExplicitly
{
    public int Id { get; set; }

    public string? Category { get; set; }

    public string? Name { get; set; }

    public bool AllowParallelSubmissionsInTasks { get; set; }

    public bool AutoChangeTestsFeedbackVisibility { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? ContestPassword { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsVisible { get; set; }

    public int LimitBetweenSubmissions { get; set; }

    public void RegisterMappings(IProfileExpression configuration) =>
        configuration.CreateMap<Contest, ContestInListModel>()
            .ForMember(x => x.Id, opt
                => opt.MapFrom(c => c.Id))
            .ForMember(x => x.Category, opt
                => opt.MapFrom(c => c.Category!.Name))
            .ForMember(x => x.AllowParallelSubmissionsInTasks, opt
                => opt.MapFrom(c => c.AllowParallelSubmissionsInTasks))
            .ForMember(x => x.AutoChangeTestsFeedbackVisibility, opt
                => opt.MapFrom(c => c.AutoChangeTestsFeedbackVisibility))
            .ForMember(x => x.CategoryId, opt
                => opt.MapFrom(c => c.CategoryId))
            .ForMember(x => x.StartTime, opt
                => opt.MapFrom(c => c.StartTime))
            .ForMember(x => x.EndTime, opt
                => opt.MapFrom(c => c.EndTime))
            .ForMember(x => x.ContestPassword, opt
                => opt.MapFrom(c => c.ContestPassword))
            .ForMember(x => x.Description, opt
                => opt.MapFrom(c => c.Description))
            .ForMember(x => x.IsDeleted, opt
                => opt.MapFrom(c => c.IsDeleted))
            .ForMember(x => x.IsVisible, opt
                => opt.MapFrom(c => c.IsVisible))
            .ForMember(x => x.LimitBetweenSubmissions, opt
                => opt.MapFrom(c => c.LimitBetweenSubmissions));
}