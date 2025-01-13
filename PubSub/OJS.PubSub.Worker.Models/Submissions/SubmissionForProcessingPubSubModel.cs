namespace OJS.PubSub.Worker.Models.Submissions;

using AutoMapper;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Common.Models.Submissions.ExecutionDetails;
using OJS.Workers.Common.Models;
using OJS.Services.Infrastructure.Models.Mapping;

public class SubmissionForProcessingPubSubModel : IMapExplicitly
{
    public int Id { get; set; }

    public ExecutionType ExecutionType { get; set; }

    public ExecutionStrategyType ExecutionStrategy { get; set; }

    public CompilerType CompilerType { get; set; }

    public byte[]? FileContent { get; set; }

    public byte[]? AdditionalFiles { get; set; }

    public string? Code { get; set; }

    public int TimeLimit { get; set; }

    public int? ExecutionStrategyBaseTimeLimit { get; set; }

    public int MemoryLimit { get; set; }

    public int? ExecutionStrategyBaseMemoryLimit { get; set; }

    public SimpleExecutionDetailsServiceModel? SimpleExecutionDetails { get; set; }

    public TestsExecutionDetailsServiceModel? TestsExecutionDetails { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<SubmissionForProcessingPubSubModel, SubmissionServiceModel>()
            .ForMember(
                d => d.ExecutionOptions,
                opt => opt.Ignore())
            .ForMember(
                d => d.StartedExecutionOn,
                opt => opt.Ignore())
            .ReverseMap();
}