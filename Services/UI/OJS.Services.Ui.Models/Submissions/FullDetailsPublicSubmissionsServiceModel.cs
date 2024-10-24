namespace OJS.Services.Ui.Models.Submissions;

using AutoMapper;
using FluentExtensions.Extensions;
using OJS.Data.Models.Submissions;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class FullDetailsPublicSubmissionsServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public DateTime CreatedOn { get; set; }

    public string StrategyName { get; set; } = null!;

    public bool IsOfficial { get; set; }

    public string User { get; set; } = null!;

    public ProblemForPublicSubmissionsServiceModel Problem { get; set; } = null!;

    public ResultForPublicSubmissionsServiceModel Result { get; set; } = null!;

    public bool IsCompiledSuccessfully { get; set; }

    public bool Processed { get; set; }

    public long? MaxMemoryUsed { get; set; }

    public int? MaxTimeUsed { get; set; }

    public string? TestRunsCache { get; set; }

    public int PageNumber { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<Submission, FullDetailsPublicSubmissionsServiceModel>()
            .ForMember(
                x => x.StrategyName,
                opt => opt.MapFrom(
                    y => y.SubmissionType!.Name))
            .ForMember(
                x => x.User,
                opt => opt.MapFrom(
                    y => y.Participant!.User.UserName))
            .ForMember(
                x => x.Result,
                opt => opt.MapFrom(
                    y => new ResultForPublicSubmissionsServiceModel
                    {
                        Points = y.Points,
                        MaxPoints =
                            y.Problem.IsNull()
                                ? 0
                                : y.Problem!.MaximumPoints,
                    }))
            .ForMember(
                x => x.IsOfficial,
                opt => opt.MapFrom(
                    y => y.Participant!.IsOfficial))
            .ForMember(
                x => x.PageNumber,
                opt => opt.Ignore())
            .ForMember(
                x => x.MaxTimeUsed,
                opt => opt.MapFrom(s => GetMaxMemoryAndTimeUsed(s.TestRunsCache).MaxTimeUsed))
            .ForMember(
                x => x.MaxMemoryUsed,
                opt => opt.MapFrom(s => GetMaxMemoryAndTimeUsed(s.TestRunsCache).MaxMemoryUsed));

    private static (long? MaxMemoryUsed, int? MaxTimeUsed) GetMaxMemoryAndTimeUsed(string? testRunsCache)
    {
        if (string.IsNullOrWhiteSpace(testRunsCache))
        {
            return (null, null);
        }

        var cacheParts = testRunsCache.Split('|');
        if (cacheParts.Length <= 1)
        {
            return (null, null);
        }

        var timeMemoryPart = cacheParts[1];
        var timeMemoryValues = timeMemoryPart.Split(',');

        if (timeMemoryValues.Length < 2)
        {
            return (null, null);
        }

        if (int.TryParse(timeMemoryValues[0], out var maxTimeUsed) &&
            long.TryParse(timeMemoryValues[1], out var maxMemoryUsed))
        {
            return (maxMemoryUsed, maxTimeUsed);
        }

        return (null, null);
    }
}