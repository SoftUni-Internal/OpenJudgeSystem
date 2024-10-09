namespace OJS.Servers.Worker.Models.ExecutionResult.Output;

using AutoMapper;
using OJS.Services.Infrastructure.Models.Mapping;
using OJS.Services.Common.Models.Submissions;
using OJS.Workers.Common.Models;

public class TestResultResponseModel : IMapExplicitly
{
    public int Id { get; set; }

    public TestRunResultType ResultType { get; set; }

    public string? ExecutionComment { get; set; }

    public string? Output { get; set; }

    public int TimeUsed { get; set; }

    public int MemoryUsed { get; set; }

    public CheckerDetailsResponseModel? CheckerDetails { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<TestResultServiceModel, TestResultResponseModel>()
            .ForMember(
                m => m.Output,
                opt => opt.MapFrom(src => src.CheckerDetails != null ? src.CheckerDetails.UserOutputFragment : null));
}