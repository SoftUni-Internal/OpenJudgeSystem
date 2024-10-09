namespace OJS.Services.Common.Models.Submissions;

using AutoMapper;
using OJS.Workers.Common;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Services.Infrastructure.Models.Mapping;
using OJS.Workers.Common.Models;

public class TestResultServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public TestRunResultType ResultType { get; set; }

    public string ExecutionComment { get; set; } = string.Empty;

    public string Output { get; set; } = string.Empty;

    public CheckerDetails? CheckerDetails { get; set; }

    public int TimeUsed { get; set; }

    public int MemoryUsed { get; set; }

    public bool IsTrialTest { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<TestResult, TestResultServiceModel>()
            .ForMember(
                d => d.CheckerDetails,
                opt =>
                    opt.MapFrom(s => s.CheckerDetails))
            .ForMember(
                d => d.Output,
                opt =>
                    opt.MapFrom(s => s.CheckerDetails != null ? s.CheckerDetails.UserOutputFragment : null))
            .ReverseMap();
}