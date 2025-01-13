namespace OJS.Services.Ui.Models.Cache;

using AutoMapper;
using OJS.Data.Models.Problems;
using OJS.Services.Infrastructure.Models.Mapping;
using System.Collections.Generic;

public class ProblemForSubmitCacheModel : IMapExplicitly
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public short MaximumPoints { get; set; }

    public int TimeLimit { get; set; }

    public int MemoryLimit { get; set; }

    public byte[]? AdditionalFiles { get; set; }

    public int? SourceCodeSizeLimit { get; set; }

    public bool ShowResults { get; set; }

    public int? CheckerId { get; set; }

    public CheckerCacheModel? Checker { get; set; }

    public ICollection<SubmissionTypeInProblemCacheModel> SubmissionTypesInProblems { get; set; } = [];

    public ICollection<TestCacheModel> Tests { get; set; } = [];

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<Problem, ProblemForSubmitCacheModel>()
            .ForMember(m => m.Tests, opt => opt.Ignore())
            .ForMember(m => m.AdditionalFiles, opt => opt.Ignore())
            .ReverseMap();
}