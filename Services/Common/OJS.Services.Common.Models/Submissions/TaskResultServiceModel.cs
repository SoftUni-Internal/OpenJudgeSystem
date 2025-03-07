namespace OJS.Services.Common.Models.Submissions;

using AutoMapper;
using OJS.Services.Infrastructure.Models.Mapping;
using System.Collections.Generic;
using OJS.Workers.ExecutionStrategies.Models;

public class TaskResultServiceModel : IMapExplicitly
{
    public int Points { get; set; }

    public IEnumerable<TestResultServiceModel> TestResults { get; set; } = [];

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap(typeof(ExecutionResult<TestResult>), typeof(TaskResultServiceModel))
            .ForMember(
                nameof(this.Points),
                // Calculated later on in code
                opt => opt.Ignore())
            .ForMember(
                nameof(this.TestResults),
                opt => opt.MapFrom(src => ((src as ExecutionResult<TestResult>)!).Results));
}