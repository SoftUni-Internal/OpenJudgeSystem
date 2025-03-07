namespace OJS.Services.Ui.Models.Submissions;

using AutoMapper;
using OJS.Data.Models.Submissions;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class SubmissionForSubmitSummaryServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? StrategyName { get; set; }

    public bool IsOfficial { get; set; }

    public int ProblemId { get; set; }

    public ResultForPublicSubmissionsServiceModel Result { get; set; } = null!;

    public bool IsCompiledSuccessfully { get; set; }

    public bool Processed { get; set; }

    public long? MaxMemoryUsed { get; set; }

    public int? MaxTimeUsed { get; set; }

    public string? TestRunsCache { get; set; }

    public int Points { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<Submission, SubmissionForSubmitSummaryServiceModel>()
            .ForMember(x => x.Result, opt => opt.MapFrom(s => new ResultForPublicSubmissionsServiceModel
            {
                Points = s.Points,
            }))
            .ForMember(x => x.StrategyName, opt => opt.MapFrom(s => s.SubmissionType != null ? s.SubmissionType.Name : null))
            .ForMember(x => x.IsOfficial, opt => opt.Ignore())
            .ForMember(x => x.MaxTimeUsed, opt => opt.Ignore())
            .ForMember(x => x.MaxMemoryUsed, opt => opt.Ignore());
}