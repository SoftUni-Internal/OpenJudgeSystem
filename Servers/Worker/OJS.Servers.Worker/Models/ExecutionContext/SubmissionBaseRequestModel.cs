namespace OJS.Servers.Worker.Models.ExecutionContext
{
    using System.ComponentModel.DataAnnotations;
    using AutoMapper;
    using FluentExtensions.Extensions;
    using OJS.Servers.Worker.Models.ExecutionContext.ExecutionDetails;
    using OJS.Services.Common.Mapping;
    using OJS.Services.Common.Models.Submissions.ExecutionContext;
    using OJS.Services.Infrastructure.Models.Mapping;

    public abstract class SubmissionBaseRequestModel<TSubmissionRequestModel, TExecutionDetails>
        : IMapExplicitly
        where TSubmissionRequestModel : SubmissionBaseRequestModel<TSubmissionRequestModel, TExecutionDetails>
    {
        public int Id { get; set; }

        [Required]
        public string? ExecutionType { get; set; }

        [Required]
        public string? ExecutionStrategy { get; set; }

        public string? CompilerType { get; set; }

        [Required]
        public TExecutionDetails ExecutionDetails { get; set; } = default!;

        public ExecutionOptionsRequestModel ExecutionOptions { get; set; } = new ExecutionOptionsRequestModel();

        public int TimeLimit { get; set; }

        public int? ExecutionStrategyBaseTimeLimit { get; set; }

        public int MemoryLimit { get; set; }

        public int? ExecutionStrategyBaseMemoryLimit { get; set; }

        public byte[]? FileContent { get; set; }

        public byte[]? AdditionalFiles { get; set; }

        public bool WithExceptionStackTrace { get; set; }

        public bool Verbosely { get; set; }

        public void RegisterMappings(IProfileExpression configuration)
        {
            var mapping = configuration
                .CreateMap<TSubmissionRequestModel, SubmissionServiceModel>()
                .ForMember(
                    m => m.ExecutionType,
                    opt => opt.MapFrom<ExecutionTypeMemberValueResolver, string>(src =>
                        src.ExecutionType ?? string.Empty))
                .ForMember(
                    m => m.ExecutionStrategy,
                    opt => opt.MapFrom<ExecutionStrategyValueResolver, string>(src
                        => src.ExecutionStrategy ?? string.Empty))
                .ForMember(
                    m => m.CompilerType,
                    opt => opt.MapFrom<CompilerTypeMemberValueResolver, string?>(src => src.CompilerType))
                .ForMember(
                    m => m.SimpleExecutionDetails,
                    opt => opt.MapFrom(src =>
                        (src.ExecutionDetails!.ToString() ?? string.Empty)
                        .FromJson<SimpleExecutionDetailsRequestModel>()))
                .ForMember(
                    m => m.TestsExecutionDetails,
                    opt => opt.MapFrom(src =>
                        (src.ExecutionDetails!.ToString() ?? string.Empty)
                        .FromJson<TestsExecutionDetailsRequestModel>()))
                .ForMember(
                    m => m.StartedExecutionOn,
                    opt => opt.Ignore());

            this.MapAdditionalMembers(mapping);
        }

        protected virtual void MapAdditionalMembers(
            IMappingExpression<TSubmissionRequestModel, SubmissionServiceModel> mapping)
        {
        }
    }
}