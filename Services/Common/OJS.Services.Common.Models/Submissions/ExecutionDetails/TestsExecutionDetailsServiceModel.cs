namespace OJS.Services.Common.Models.Submissions.ExecutionDetails
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using OJS.Data.Models.Problems;
    using OJS.Services.Common.Models.Mappings;
    using OJS.Services.Common.Models.Submissions.ExecutionContext.Mapping;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;
    using SoftUni.AutoMapper.Infrastructure.Models;

    public class TestsExecutionDetailsServiceModel
        : BaseExecutionDetailsServiceModel,
            IMapExplicitly
    {
        public short MaxPoints { get; set; }

        public string? TaskId { get; set; }

        public string? CheckerType { get; set; }

        public string? CheckerParameter { get; set; }

        public IEnumerable<TestContext> Tests { get; set; } = Enumerable.Empty<TestContext>();

        public void RegisterMappings(IProfileExpression configuration)
        {
            configuration.CreateMap<Problem, TestsExecutionDetailsServiceModel>()
                .ForMember(
                    d => d.CheckerType,
                    opt => opt.MapFrom(s => s.Checker!.ClassName))
                .ForMember(
                    d => d.CheckerParameter,
                    opt => opt.MapFrom(s => s.Checker!.Parameter))
                .ForMember(
                    d => d.TaskSkeleton,
                    opt => opt.MapFrom(s => s.SolutionSkeleton))
                .ForMember(
                    d => d.MaxPoints,
                    opt => opt.MapFrom(s => s.MaximumPoints))
                .ForMember(
                    d => d.Tests,
                    opt => opt.MapFrom(s => s.Tests))
                // Ignoring as TaskSkeleton should be mapped based on selected submission type
                .ForMember(
                    d => d.TaskSkeleton,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.TaskSkeletonAsString,
                    opt => opt.Ignore())
                .ForMember(
                    d => d.TaskId,
                    opt => opt.Ignore());

            configuration
                .CreateMap<TestsExecutionDetailsServiceModel, TestsInputModel>()
                .ForMember(m => m.Tests, opt => opt.MapFrom(s => s.Tests))
                .ForMember(
                    m => m.CheckerParameter,
                    opt => opt.MapFrom(y => y.CheckerParameter))
                .ForMember(
                    m => m.TaskSkeleton,
                    opt => opt.MapFrom<TaskSkeletonValueResolver<TestsExecutionDetailsServiceModel>>())
                .ForMember(
                    m => m.CheckerTypeName,
                    opt => opt.MapFrom<CheckerTypeNameValueResolver>())
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }
}
