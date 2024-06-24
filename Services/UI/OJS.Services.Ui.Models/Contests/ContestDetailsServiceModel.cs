namespace OJS.Services.Ui.Models.Contests;

using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Contests;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;
using System.Collections.Generic;
using System.Linq;
using OJS.Services.Ui.Models.Submissions;
using System;

public class ContestDetailsServiceModel : IMapExplicitly, ICanBeCompetedAndPracticed, IMapTo<ContestForActivityServiceModel>
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool IsVisible { get; set; }

    public DateTime? VisibleFrom { get; set; }

    public bool IsDeleted { get; set; }

    public int? CategoryId { get; set; }

    public ContestDetailsCategoryServiceModel? Category { get; set; }

    public ContestType Type { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }

    public bool HasContestPassword { get; set; }

    public bool HasPracticePassword { get; set; }

    public string? Description { get; set; }

    public bool CanViewResults { get; set; }

    public bool IsOnlineExam { get; set; }

    public bool CanBeCompeted { get; set; }

    public bool CanBePracticed { get; set; }

    public bool IsAdminOrLecturerInContest { get; set; }

    public int CompeteParticipantsCount { get; set; }

    public int PracticeParticipantsCount { get; set; }

    public ICollection<ContestDetailsSubmissionTypeServiceModel> AllowedSubmissionTypes { get; set; } =
        new HashSet<ContestDetailsSubmissionTypeServiceModel>();

    public ICollection<ContestProblemServiceModel> Problems { get; set; } = new HashSet<ContestProblemServiceModel>();

    public void RegisterMappings(IProfileExpression configuration) =>
        configuration
            .CreateMap<Contest, ContestDetailsServiceModel>()
            .ForMember(
                d => d.Problems,
                opt => opt.MapFrom(s =>
                    s.ProblemGroups
                        .SelectMany(pg => pg.Problems)
                        .OrderBy(p => p.ProblemGroup.OrderBy)
                        .ThenBy(p => p.OrderBy)))
            .ForMember(d => d.IsAdminOrLecturerInContest, opt => opt.Ignore())
            .ForMember(d => d.CanViewResults, opt => opt.Ignore())
            .ForMember(d => d.AllowedSubmissionTypes, opt => opt.Ignore())
            .ForMember(d => d.CompeteParticipantsCount, opt => opt.Ignore())
            .ForMember(d => d.PracticeParticipantsCount, opt => opt.Ignore())
            .ForMember(d => d.CanBeCompeted, opt => opt.Ignore())
            .ForMember(d => d.CanBePracticed, opt => opt.Ignore());
}