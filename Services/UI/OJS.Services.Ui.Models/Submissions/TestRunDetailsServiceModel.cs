namespace OJS.Services.Ui.Models.Submissions;

using AutoMapper;
using OJS.Data.Models.Tests;
using OJS.Services.Infrastructure.Models.Mapping;
using OJS.Workers.Common.Models;

public class TestRunDetailsServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public int TimeUsed { get; set; }

    public long MemoryUsed { get; set; }

    public int SubmissionId { get; set; }

    public string? ExecutionComment { get; set; }

    public string? CheckerComment { get; set; }

    public TestRunResultType ResultType { get; set; }

    public string? ExpectedOutputFragment { get; set; }

    public string? UserOutputFragment { get; set; }

    public bool IsTrialTest { get; set; }

    public string? Input { get; set; }

    public double OrderBy { get; set; }

    public bool ShowInput { get; set; } = true;

    public int TestId { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<TestRun, TestRunDetailsServiceModel>()
            .ForMember(d => d.IsTrialTest, opt => opt.MapFrom(s => s.Test.IsTrialTest))
            .ForMember(d => d.Input, opt => opt.MapFrom(s => s.Test.InputDataAsString))
            .ForMember(d => d.OrderBy, opt => opt.MapFrom(s => s.Test.OrderBy))
            .ForMember(d => d.ShowInput, opt => opt.Ignore());
}