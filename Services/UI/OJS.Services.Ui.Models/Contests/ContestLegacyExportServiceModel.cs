namespace OJS.Services.Ui.Models.Contests;

using AutoMapper;
using Data.Models;
using Data.Models.Checkers;
using Data.Models.Contests;
using Data.Models.Problems;
using Data.Models.Submissions;
using Data.Models.Tests;
using Infrastructure.Models.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

public class ContestLegacyExportServiceModel : IMapExplicitly
{
    public int Id { get; set; }

    public string Name { get; set; } = String.Empty;

    public bool IsVisible { get; set; }

    public DateTime? VisibleFrom { get; set; }

    public bool AutoChangeTestsFeedbackVisibility { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? NewIpPassword { get; set; }

    public DateTime? PracticeStartTime { get; set; }

    public DateTime? PracticeEndTime { get; set; }

    public int CategoryId { get; set; }

    public int Type { get; set; }

    public TimeSpan? Duration { get; set; }

    public string? ContestPassword { get; set; }

    public string? PracticePassword { get; set; }

    public int LimitBetweenSubmissions { get; set; }

    public int OrderBy { get; set; }

    public short NumberOfProblemGroups { get; set; }

    public string? Description { get; set; }

    public bool UsersCantSubmitConcurrently { get; set; }

    public int DefaultWorkerType { get; set; }

    public bool EnsureValidAuthorSubmisions { get; set; }

    public IEnumerable<LegacyProblemGroup> ProblemGroups { get; set; } = [];

    public void RegisterMappings(IProfileExpression configuration)
    {
        configuration.CreateMap<Contest, ContestLegacyExportServiceModel>()
            .ForMember(dest => dest.UsersCantSubmitConcurrently,
                opt => opt.MapFrom(src => !src.AllowParallelSubmissionsInTasks))
            .ForMember(dest => dest.DefaultWorkerType, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.EnsureValidAuthorSubmisions, opt => opt.MapFrom(src => false));

        configuration.CreateMap<Problem, LegacyProblem>()
            .ForMember(dest => dest.SubmissionTypes, opt => opt.MapFrom(src => src.SubmissionTypesInProblems.Select(stp => stp.SubmissionType)))
            .ForMember(dest => dest.DefaultSubmissionTypeId,
                opt => opt.MapFrom(src =>
                    src.SubmissionTypesInProblems.Count > 0 && src.SubmissionTypesInProblems.FirstOrDefault() != null
                        ? src.SubmissionTypesInProblems.FirstOrDefault()!.SubmissionTypeId
                        : (int?)null))
            .ForMember(dest => dest.DefaultSubmissionType,
                opt => opt.MapFrom(src =>
                    src.SubmissionTypesInProblems.Count > 0 && src.SubmissionTypesInProblems.FirstOrDefault() != null
                    ? src.SubmissionTypesInProblems.FirstOrDefault()!.SubmissionType
                    : null));

        configuration.CreateMap<SubmissionType, LegacySubmissionType>()
            .ForMember(dest => dest.AllowedFileExtensions,
                opt => opt.MapFrom(src => src.AllowedFileExtensions))
            .ForMember(dest => dest.ProblemSubmissionTypeExecutionDetails,
                opt => opt.MapFrom(src => src.SubmissionTypesInProblems));

        configuration.CreateMap<SubmissionTypeInProblem, LegacyProblemSubmissionTypeExecutionDetail>()
            .ForMember(dest => dest.WorkerType, opt => opt.MapFrom(src => 0));
    }
}

public class LegacyProblemGroup : IMapFrom<ProblemGroup>
{
    public int Id { get; set; }

    public int ContestId { get; set; }

    public int OrderBy { get; set; }

    public int? Type { get; set; }

    public IEnumerable<LegacyProblem> Problems { get; set; } = [];
}

public class LegacyProblem
{
    public int Id { get; set; }

    public string Name { get; set; } = String.Empty;

    public short MaximumPoints { get; set; }

    public int TimeLimit { get; set; }

    public int MemoryLimit { get; set; }

    public int? SourceCodeSizeLimit { get; set; }

    public byte[]? SolutionSkeleton { get; set; }

    public byte[]? AdditionalFiles { get; set; }

    public bool ShowResults { get; set; }

    public bool ShowDetailedFeedback { get; set; }

    public double OrderBy { get; set; }

    public LegacyChecker? Checker { get; set; }

    public IEnumerable<LegacyTest> Tests { get; set; } = [];

    public IEnumerable<LegacySubmissionType> SubmissionTypes { get; set; } = [];

    public int? DefaultSubmissionTypeId { get; set; }

    public LegacySubmissionType? DefaultSubmissionType { get; set; }

    public IEnumerable<LegacyResource> Resources { get; set; } = [];
}

public class LegacyChecker : IMapFrom<Checker>
{
    public int Id { get; set; }

    public string Name { get; set; } = String.Empty;

    public string? Description { get; set; }

    public string? DllFile { get; set; }

    public string? ClassName { get; set; }

    public string? Parameter { get; set; }
}

public class LegacyTest : IMapFrom<Test>
{
    public int Id { get; set; }

    public byte[] InputData { get; set; } = null!;

    public byte[] OutputData { get; set; } = null!;

    public bool IsTrialTest { get; set; }

    public bool IsOpenTest { get; set; }

    public int OrderBy { get; set; }

    public bool HideInput { get; set; }
}

public class LegacyResource : IMapFrom<ProblemResource>
{
    public int Id { get; set; }

    public string Name { get; set; } = String.Empty;

    public int Type { get; set; }

    public byte[]? File { get; set; }

    public string? FileExtension { get; set; }

    public string? Link { get; set; }

    public bool IsDeleted { get; set; }

    public int OrderBy { get; set; }
}

public class LegacySubmissionType
{
    public int Id { get; set; }

    public string Name { get; set; } = String.Empty;

    public int ExecutionStrategyType { get; set; }

    public int CompilerType { get; set; }

    public string AllowedFileExtensions { get; set; } = String.Empty;

    public IEnumerable<LegacyProblemSubmissionTypeExecutionDetail> ProblemSubmissionTypeExecutionDetails { get; set; } =
        [];
}

public class LegacyProblemSubmissionTypeExecutionDetail
{
    public int ProblemId { get; set; }

    public int SubmissionTypeId { get; set; }
    public int WorkerType { get; set; }
}