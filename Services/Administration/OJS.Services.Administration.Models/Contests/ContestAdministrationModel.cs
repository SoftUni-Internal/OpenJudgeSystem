namespace OJS.Services.Administration.Models.Contests;

using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Contests;
using OJS.Services.Common.Models;
using OJS.Services.Infrastructure.Models.Mapping;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class ContestAdministrationModel : BaseAdministrationModel<int>, IMapExplicitly
{
    public string? Name { get; set; }

    public string? Type { get; set; }

    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? Description { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }

    public string? ContestPassword { get; set; }

    public string? PracticePassword { get; set; }

    public int LimitBetweenSubmissions { get; set; }

    public bool AutoChangeLimitBetweenSubmissions { get; set; }

    public bool IsVisible { get; set; }

    public DateTime? VisibleFrom { get; set; }

    public TimeSpan? Duration { get; set; }

    [MaxLength(20)]
    public string? NewIpPassword { get; set; }

    public string? AllowedIps { get; set; }

    public bool AllowParallelSubmissionsInTasks { get; set; }

    public double OrderBy { get; set; }

    public int OfficialParticipants { get; set; }

    public bool IsOnlineExam => this.Type == ContestType.OnlinePracticalExam.ToString();

    public bool IsWithRandomTasks => this.IsOnlineExam ||
                                     this.Type == ContestType.OnsitePracticalExamWithRandomTasks.ToString();

    public int NumberOfProblemGroups { get; set; }
    // TODO : Add Automatically change test detailed feedback visiblity and Warn on missing author solutions
    public void RegisterMappings(IProfileExpression configuration)
    {
        configuration.CreateMap<Contest, ContestAdministrationModel>()
            .ForMember(crm => crm.CategoryName, opt
                => opt.MapFrom(c => c.Category!.Name))
            .ForMember(crm => crm.OperationType, opt
                => opt.Ignore())
            .ForMember(crm => crm.AllowedIps, opt
                => opt.MapFrom(c => string.Join(';', c.IpsInContests.Select(x => x.Ip.Value).ToHashSet())))
            .ForMember(crm => crm.OfficialParticipants, opt
                => opt.MapFrom(c => c.Participants.Count(p => p.IsOfficial)));

        configuration.CreateMap<ContestAdministrationModel, Contest>()
            //TODO Fix
            .ForMember(crm => crm.IpsInContests, opt
                => opt.Ignore())
            .ForMember(crm => crm.Category, opt
                => opt.Ignore())
            .ForMember(crm => crm.LecturersInContests, opt
                => opt.Ignore())
            .ForMember(crm => crm.ProblemGroups, opt
                => opt.Ignore())
            .ForMember(crm => crm.Participants, opt
                => opt.Ignore())
            .ForMember(crm => crm.DeletedOn, opt
                => opt.Ignore())
            .ForMember(crm => crm.ExamGroups, opt
                => opt.Ignore())
            .ForMember(crm => crm.IsDeleted, opt
                => opt.Ignore())
            .ForMember(crm => crm.CreatedOn, opt
                => opt.Ignore())
            .ForMember(crm => crm.ModifiedOn, opt
                => opt.Ignore())
            .ForMember(crm => crm.AutoChangeTestsFeedbackVisibility, opt
                => opt.Ignore());
    }
}