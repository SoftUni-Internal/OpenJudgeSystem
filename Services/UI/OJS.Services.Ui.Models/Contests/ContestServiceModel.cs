namespace OJS.Services.Ui.Models.Contests;

using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Contests;
using OJS.Services.Ui.Models.SubmissionTypes;
using OJS.Services.Infrastructure.Models.Mapping;

public class ContestServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? CategoryId { get; set; }

    public int LimitBetweenSubmissions { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsVisible { get; set; }

    public DateTime? VisibleFrom { get; set; }

    public bool IsOnlineExam { get; set; }

    public IEnumerable<SubmissionTypeServiceModel> AllowedSubmissionTypes { get; set; } = null!;

    public IEnumerable<ContestProblemServiceModel> Problems { get; set; } = null!;

    public bool UserIsAdminOrLecturerInContest { get; set; }

    public ContestType Type { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }

    public void RegisterMappings(IProfileExpression configuration) =>
        configuration.CreateMap<Contest, ContestServiceModel>()
            .ForMember(
                d => d.AllowedSubmissionTypes,
                opt => opt.Ignore())
            .ForMember(
                d => d.Problems,
                opt => opt.MapFrom(s =>
                    s.ProblemGroups
                        .SelectMany(pg => pg.Problems)
                        .OrderBy(p => p.ProblemGroup.OrderBy)
                        .ThenBy(p => p.OrderBy)))
            .ForMember(d => d.UserIsAdminOrLecturerInContest, opt => opt.Ignore())
            .ReverseMap();
}