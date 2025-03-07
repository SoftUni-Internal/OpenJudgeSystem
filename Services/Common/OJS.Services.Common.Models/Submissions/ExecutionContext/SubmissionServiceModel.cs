namespace OJS.Services.Common.Models.Submissions.ExecutionContext
{
    using AutoMapper;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Common.Models.Submissions.ExecutionContext.Mapping;
    using OJS.Services.Common.Models.Submissions.ExecutionDetails;
    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Services.Infrastructure.Models.Mapping;
    using System;
    using System.Linq;

    public class SubmissionServiceModel : IMapExplicitly
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

        public DateTime? StartedExecutionOn { get; set; }

        public bool Verbosely { get; set; }

        public SimpleExecutionDetailsServiceModel? SimpleExecutionDetails { get; set; }

        public TestsExecutionDetailsServiceModel? TestsExecutionDetails { get; set; }

        public ExecutionOptionsServiceModel ExecutionOptions { get; set; } = new();

        public void RegisterMappings(IProfileExpression configuration)
        {
            configuration
                .CreateMap(typeof(SubmissionServiceModel), typeof(OjsSubmission<>))
                .ForMember(
                    nameof(OjsSubmission<object>.Id),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.Id)))
                .ForMember(
                    nameof(OjsSubmission<object>.Input),
                    opt => opt.MapFrom(typeof(SubmissionInputValueResolver)))
                .ForMember(
                    nameof(OjsSubmission<object>.ExecutionStrategyType),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.ExecutionStrategy)))
                .ForMember(
                    nameof(OjsSubmission<object>.ExecutionType),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.ExecutionType)))
                .ForMember(
                    nameof(OjsSubmission<object>.CompilerType),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.CompilerType)))
                .ForMember(
                    nameof(OjsSubmission<object>.Code),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.Code)))
                .ForMember(
                    nameof(OjsSubmission<object>.FileContent),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.FileContent)))
                .ForMember(
                    nameof(OjsSubmission<object>.AdditionalFiles),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.AdditionalFiles)))
                .ForMember(
                    nameof(OjsSubmission<object>.MemoryLimit),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.MemoryLimit)))
                .ForMember(
                    nameof(OjsSubmission<object>.ExecutionStrategyBaseMemoryLimit),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.ExecutionStrategyBaseMemoryLimit)))
                .ForMember(
                    nameof(OjsSubmission<object>.TimeLimit),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.TimeLimit)))
                .ForMember(
                    nameof(OjsSubmission<object>.ExecutionStrategyBaseTimeLimit),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.ExecutionStrategyBaseTimeLimit)))
                .ForMember(
                    nameof(OjsSubmission<object>.Verbosely),
                    opt => opt.MapFrom(nameof(SubmissionServiceModel.Verbosely)))
                .ForMember(nameof(OjsSubmission<object>.AdditionalCompilerArguments), opt => opt.Ignore())
                .ForMember(nameof(OjsSubmission<object>.ProcessingComment), opt => opt.Ignore())
                .ForMember(nameof(OjsSubmission<object>.AllowedFileExtensions), opt => opt.Ignore())
                .ForMember(nameof(OjsSubmission<object>.MaxPoints), opt => opt.Ignore())
                .ForMember(nameof(OjsSubmission<object>.ExceptionType), opt => opt.Ignore())
                .ForMember(nameof(OjsSubmission<object>.CompletedExecutionOn), opt => opt.Ignore());

            configuration.CreateMap<Submission, SubmissionServiceModel>()
                .ForMember(
                    d => d.Code,
                    opt => opt.MapFrom(s => s.IsBinaryFile ? string.Empty : s.ContentAsString))
                .ForMember(
                    d => d.FileContent,
                    opt => opt.MapFrom(s => s.IsBinaryFile ? s.Content : null))
                .ForMember(
                    d => d.AdditionalFiles,
                    opt => opt.MapFrom(s => s.IsBinaryFile ? s.Problem.AdditionalFiles : null))
                .ForMember(
                    d => d.ExecutionStrategy,
                    opt => opt.MapFrom(s =>
                        s.SubmissionType != null
                            ? s.SubmissionType.ExecutionStrategyType
                            : ExecutionStrategyType.NotFound))
                .ForMember(
                    d => d.ExecutionType,
                    opt => opt.MapFrom(s => ExecutionType.TestsExecution))
                .ForMember(dest => dest.TimeLimit, opt
                    => opt.MapFrom(s => s.SubmissionType!.SubmissionTypesInProblems
                        .Where(x => x.ProblemId == s.ProblemId)
                        .Any(x => x.TimeLimit.HasValue)
                        ? s.SubmissionType.SubmissionTypesInProblems
                            .Where(x => x.ProblemId == s.ProblemId).Select(x => x.TimeLimit).First()
                        : s.Problem.TimeLimit))
                .ForMember(dest => dest.MemoryLimit, opt
                    => opt.MapFrom(s => s.SubmissionType!.SubmissionTypesInProblems
                        .Where(x => x.ProblemId == s.ProblemId)
                        .Any(x => x.MemoryLimit.HasValue)
                        ? s.SubmissionType.SubmissionTypesInProblems
                            .Where(x => x.ProblemId == s.ProblemId).Select(x => x.MemoryLimit).First()
                        : s.Problem.MemoryLimit))
                .ForMember(
                    d => d.ExecutionStrategyBaseTimeLimit,
                    opt => opt.MapFrom(s =>
                        s.SubmissionType == null
                            ? null
                            : s.SubmissionType.BaseTimeUsedInMilliseconds))
                .ForMember(
                    d => d.ExecutionStrategyBaseMemoryLimit,
                    opt => opt.MapFrom(s =>
                        s.SubmissionType == null
                            ? null
                            : s.SubmissionType.BaseMemoryUsedInBytes))
                .ForMember(
                    d => d.TestsExecutionDetails,
                    opt => opt.MapFrom(s => s.Problem))
                .ForMember(
                    d => d.CompilerType,
                    opt => opt.MapFrom(s =>
                        s.SubmissionType != null
                            ? s.SubmissionType.CompilerType
                            : CompilerType.None))
                .ForMember(
                    d => d.SimpleExecutionDetails,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.ExecutionOptions,
                    opt => opt.Ignore())
                .ForMember(d => d.Verbosely, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    if (src.SubmissionType is { MaxAllowedTimeLimitInMilliseconds: not null } &&
                        dest.TimeLimit > src.SubmissionType.MaxAllowedTimeLimitInMilliseconds)
                    {
                        dest.TimeLimit = src.SubmissionType.MaxAllowedTimeLimitInMilliseconds.Value;
                    }

                    if (src.SubmissionType is { MaxAllowedMemoryLimitInBytes: not null } &&
                        dest.MemoryLimit > src.SubmissionType.MaxAllowedMemoryLimitInBytes)
                    {
                        dest.MemoryLimit = src.SubmissionType.MaxAllowedMemoryLimitInBytes.Value;
                    }
                });
        }
    }
}
