namespace OJS.Services.Administration.Models.Contests;

using OJS.Data.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;
using System;
using AutoMapper;
using System.Linq;

public class ContestInListModel : IMapExplicitly
{
    public int Id { get; set; }

    public string? Category { get; set; }

    public string? Name { get; set; }

    public bool AllowParallelSubmissionsInTasks { get; set; }

    public int? CategoryId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? ContestPassword { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsVisible { get; set; }

    public DateTime? VisibleFrom { get; set; }

    public int LimitBetweenSubmissions { get; set; }

    public bool AutoChangeLimitBetweenSubmissions { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public int OfficialParticipants { get; set; }

    public void RegisterMappings(IProfileExpression configuration) =>
        configuration.CreateMap<Contest, ContestInListModel>()
            .ForMember(x => x.Id, opt
                => opt.MapFrom(c => c.Id))
            .ForMember(x => x.Category, opt
                => opt.MapFrom(c => c.Category!.Name))
            .ForMember(x => x.AllowParallelSubmissionsInTasks, opt
                => opt.MapFrom(c => c.AllowParallelSubmissionsInTasks))
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
                => opt.MapFrom(c => c.IsVisible || c.VisibleFrom <= DateTime.UtcNow))
            .ForMember(x => x.LimitBetweenSubmissions, opt
                => opt.MapFrom(c => c.LimitBetweenSubmissions))
            .ForMember(x => x.OfficialParticipants, opt
                => opt.MapFrom(c => c.Participants.Count(p => p.IsOfficial)));
}