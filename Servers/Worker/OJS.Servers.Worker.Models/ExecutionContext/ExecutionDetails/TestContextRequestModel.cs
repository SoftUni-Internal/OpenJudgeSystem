namespace OJS.Servers.Worker.Models.ExecutionContext.ExecutionDetails;

using AutoMapper;
using OJS.Workers.Common.Models;
using SoftUni.AutoMapper.Infrastructure.Models;

public class TestContextRequestModel : IMapExplicitly
{
    public int Id { get; set; }

    public string? Input { get; set; }

    public string? Output { get; set; }

    public bool IsTrialTest { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration.CreateMap<TestContextRequestModel, TestContext>()
            .ForMember(s => s.OrderBy, opt => opt.Ignore());
}
