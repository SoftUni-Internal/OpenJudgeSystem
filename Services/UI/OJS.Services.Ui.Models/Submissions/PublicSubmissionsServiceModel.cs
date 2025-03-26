namespace OJS.Services.Ui.Models.Submissions;

using AutoMapper;
using FluentExtensions.Extensions;
using OJS.Data.Models.Contests;
using OJS.Data.Models.Problems;
using OJS.Data.Models.Submissions;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class ProblemForPublicSubmissionsServiceModel
    : IMapExplicitly
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ContestForPublicSubmissionsServiceModel Contest { get; set; } = null!;

    public double OrderBy { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<Problem, ProblemForPublicSubmissionsServiceModel>()
    .ForMember(
        x => x.Contest,
        opt => opt.MapFrom(
            y => y.ProblemGroup.Contest));
}

public class ContestForPublicSubmissionsServiceModel : IMapFrom<Contest>
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}

public class ResultForPublicSubmissionsServiceModel : IMapExplicitly
{
    public int Points { get; set; }

    public short MaxPoints { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<Submission, ResultForPublicSubmissionsServiceModel>()
            .ForMember(dest => dest.Points, opt => opt.MapFrom(src => src.Points))
            .ForMember(dest => dest.MaxPoints, opt => opt.MapFrom(src => src.Problem!.MaximumPoints));
}

public class PublicSubmissionsServiceModel : IMapExplicitly
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

    public int PageNumber { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<Submission, PublicSubmissionsServiceModel>()
            .ForMember(
                x => x.Result,
                opt => opt.MapFrom(s => s))
            .ForMember(
                x => x.StrategyName,
                opt => opt.MapFrom(
                    y => y.SubmissionType!.Name))
            .ForMember(
                x => x.User,
                opt => opt.MapFrom(
                    y => y.Participant!.User.UserName))
            .ForMember(
                x => x.IsOfficial,
                opt => opt.MapFrom(
                    y => y.Participant!.IsOfficial))
            .ForMember(
                x => x.PageNumber,
                opt => opt.Ignore());
}