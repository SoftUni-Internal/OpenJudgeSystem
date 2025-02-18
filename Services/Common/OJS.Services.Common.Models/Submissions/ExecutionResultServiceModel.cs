namespace OJS.Services.Common.Models.Submissions
{
    using AutoMapper;
    using OJS.Workers.ExecutionStrategies.Models;
    using OJS.Services.Infrastructure.Models.Mapping;
    using System.Linq;

    public class ExecutionResultServiceModel : IMapExplicitly
    {
        public string Id { get; set; } = null!;

        public bool IsCompiledSuccessfully { get; set; }

        public string CompilerComment { get; set; } = string.Empty;

        public TaskResultServiceModel? TaskResult { get; set; }

        public OutputResult? OutputResult { get; set; }

        public void RegisterMappings(IProfileExpression configuration)
            => configuration
                .CreateMap(typeof(ExecutionResult<TestResult>), typeof(ExecutionResultServiceModel))
                .ForMember(
                    nameof(this.CompilerComment),
                    opt => opt.MapFrom(src => (src as ExecutionResult<TestResult>)!.CompilerComment))
                .ForMember(
                    nameof(this.IsCompiledSuccessfully),
                    opt => opt.MapFrom(src => (src as ExecutionResult<TestResult>)!.IsCompiledSuccessfully))
                .ForMember(
                    nameof(this.TaskResult),
                    opt => opt.MapFrom(src => (src as ExecutionResult<TestResult>)!))
                .ForMember(
                    nameof(this.OutputResult),
                    opt => opt.MapFrom(src => (src as ExecutionResult<OutputResult>)!.Results.FirstOrDefault()))
                .ForMember(nameof(this.Id), opt => opt.Ignore());
    }
}