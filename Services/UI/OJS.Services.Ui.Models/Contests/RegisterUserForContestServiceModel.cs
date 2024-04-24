namespace OJS.Services.Ui.Models.Contests;

using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Contests;
using SoftUni.AutoMapper.Infrastructure.Models;
using System;
using System.Linq;

public class RegisterUserForContestServiceModel : IMapExplicitly
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

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<Contest, RegisterUserForContestServiceModel>()
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