namespace OJS.Services.Ui.Models.Contests;

using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;
using System;
using System.Linq;

public class ContestRegistrationDetailsServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool RequirePassword { get; set; }

    public bool ShouldConfirmParticipation { get; set; }

    public bool IsRegisteredSuccessfully { get; set; }

    public TimeSpan? Duration { get; set; }

    public int NumberOfProblems { get; set; }

    public int? ParticipantId { get; set; }

    public bool IsOnlineExam { get; set; }

    public int? CategoryId { get; set; }

    public bool IsVisible { get; set; }

    public DateTime? VisibleFrom { get; set; }

    public string? ContestPassword { get; set; }

    public string? PracticePassword { get; set; }

    public ContestType Type { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }

    public bool HasContestPassword { get; set; }

    public bool HasPracticePassword { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<Contest, ContestRegistrationDetailsServiceModel>()
            .ForMember(
                d => d.NumberOfProblems,
                opt => opt.MapFrom(src => src.ProblemGroups.Count(pg => pg.Problems.Count > 0)))
            .ForMember(
                d => d.Duration,
                opt => opt.MapFrom(src =>
                    src.Duration ?? ((src.StartTime.HasValue && src.EndTime.HasValue)
                        ? (src.EndTime - src.StartTime)
                        : null)))
            .ForMember(
                opt => opt.HasContestPassword,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.ContestPassword)))
            .ForMember(
                opt => opt.HasPracticePassword,
                opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.PracticePassword)))
            .ForMember(
                opt => opt.RequirePassword,
                opt => opt.Ignore())
            .ForMember(
                opt => opt.ShouldConfirmParticipation,
                opt => opt.Ignore())
            .ForMember(
                opt => opt.IsRegisteredSuccessfully,
                opt => opt.Ignore())
            .ForMember(
                opt => opt.ParticipantId,
                src => src.Ignore());
}