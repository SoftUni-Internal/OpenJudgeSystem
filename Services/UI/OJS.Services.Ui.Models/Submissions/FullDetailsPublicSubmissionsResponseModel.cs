namespace OJS.Services.Ui.Models.Submissions;

using AutoMapper;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class FullDetailsPublicSubmissionsResponseModel : IMapFrom<FullDetailsPublicSubmissionsServiceModel>, IMapExplicitly
{
    public int Id { get; set; }

    public DateTime CreatedOn { get; set; }

    public string? StrategyName { get; set; }

    public bool IsOfficial { get; set; }

    public string? User { get; set; }

    public ProblemForPublicSubmissionsServiceModel Problem { get; set; } = null!;

    public ResultForPublicSubmissionsServiceModel Result { get; set; } = null!;

    public bool IsCompiledSuccessfully { get; set; }

    public bool Processed { get; set; }

    public long? MaxMemoryUsed { get; set; }

    public int? MaxTimeUsed { get; set; }

    public string? TestRunsCache { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<SubmissionForSubmitSummaryServiceModel, FullDetailsPublicSubmissionsResponseModel>()
            .ForMember(m => m.User, opt => opt.Ignore())
            .ForMember(m => m.Problem, opt => opt.MapFrom(src => new ProblemForPublicSubmissionsServiceModel
            {
                Id = src.ProblemId,
            }));
}