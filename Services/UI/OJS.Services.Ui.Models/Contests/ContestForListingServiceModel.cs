namespace OJS.Services.Ui.Models.Contests;

using AutoMapper;
using OJS.Common.Enumerations;
using System.Linq;
using OJS.Data.Models.Contests;
using OJS.Services.Common.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class ContestForListingServiceModel : IMapExplicitly, ICanBeCompetedAndPracticed, IContestForActivityServiceModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ContestType Type { get; set; }

    public bool IsVisible { get; set; }

    public DateTime? VisibleFrom { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }

    public bool CanBeCompeted { get; set; }

    public bool CanBePracticed { get; set; }

    public string Category { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public double OrderBy { get; set; }

    public TimeSpan? Duration { get; set; }

    public int NumberOfProblems { get; set; }

    public int CompeteResults { get; set; }

    public int PracticeResults { get; set; }

    public int CompeteMaximumPoints { get; set; }

    public int PracticeMaximumPoints { get; set; }

    public bool HasContestPassword { get; set; }

    public bool HasPracticePassword { get; set; }

    public bool RequirePasswordForCompete { get; set; }

    public bool RequirePasswordForPractice { get; set; }

    public ContestParticipantResultServiceModel? UserParticipationResult { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<Contest, ContestForListingServiceModel>()
            .ForMember(
                dest => dest.Category,
                opt => opt.MapFrom(src => src.Category!.Name))
            .ForMember(
                d => d.NumberOfProblems,
                opt => opt.MapFrom(src => src.ProblemGroups.Count(pg => pg.Problems.Count > 0)))
            .ForMember(
                d => d.Duration,
                opt => opt.MapFrom(src =>
                    src.Duration ?? ((src.StartTime.HasValue && src.EndTime.HasValue)
                        ? (src.EndTime - src.StartTime)
                        : null)))
            .ForMember(d => d.CanBeCompeted, opt => opt.Ignore())
            .ForMember(d => d.CanBePracticed, opt => opt.Ignore())
            .ForMember(
                d => d.PracticeMaximumPoints,
                opt => opt.MapFrom(src => src.ProblemGroups
                    .SelectMany(pg => pg.Problems)
                    .Where(x => !x.IsDeleted)
                    .Sum(pr => pr.MaximumPoints)))
            // For online contests:
            // In a problem group with multiple problems, compete points are derived from a single problem,
            // unlike practice mode where points can be accumulated from all problems across groups.
            // Onsite contests have only 1 problem per problem group
            .ForMember(
                d => d.CompeteMaximumPoints,
                opt => opt.MapFrom(src => src.ProblemGroups
                    .SelectMany(pg => pg.Problems
                        .Where(p => !p.IsDeleted)
                        .Take(1))
                    .Sum(p => p.MaximumPoints)))
            /*
             * Automapper doesn't deal well with mapping calculated properties, such as 'HasContestPassword' and 'HasPracticePassword'.
             * That is why it is better to manually map them.
             */
            .ForMember(d => d.HasContestPassword, opt => opt.MapFrom(s => s.ContestPassword != null))
            .ForMember(d => d.HasPracticePassword, opt => opt.MapFrom(s => s.PracticePassword != null))
            // Mapped from cache
            .ForMember(d => d.UserParticipationResult, opt => opt.Ignore())
            .ForMember(d => d.CompeteResults, opt => opt.Ignore())
            .ForMember(d => d.PracticeResults, opt => opt.Ignore())
            .ForMember(d => d.RequirePasswordForCompete, opt => opt.Ignore())
            .ForMember(d => d.RequirePasswordForPractice, opt => opt.Ignore());
}