namespace OJS.Services.Ui.Models.Contests
{
    using AutoMapper;
    using OJS.Common.Enumerations;
    using OJS.Data.Models.Problems;
    using OJS.Services.Ui.Models.SubmissionTypes;
    using OJS.Services.Infrastructure.Models.Mapping;
    using System.Collections.Generic;
    using System.Linq;

    public class ContestProblemServiceModel : IMapExplicitly
    {
        public int Id { get; set; }

        public int ContestId { get; set; }

        public string Name { get; set; } = null!;

        public int? DefaultSubmissionTypeId { get; set; }

        public double OrderBy { get; set; }

        public double ProblemGroupOrderBy { get; set; }

        public short MaximumPoints { get; set; }

        public int? Points { get; set; }

        public bool IsExcludedFromHomework { get; set; }

        public double MemoryLimit { get; set; }

        public double TimeLimit { get; set; }

        public double? FileSizeLimit { get; set; }

        public string CheckerName { get; set; } = null!;

        public string CheckerDescription { get; set; } = null!;

        public IEnumerable<ContestProblemResourceServiceModel> Resources { get; set; } = null!;

        public IEnumerable<SubmissionTypeServiceModel> AllowedSubmissionTypes { get; set; } = null!;

        public void RegisterMappings(IProfileExpression configuration) =>
            configuration.CreateMap<Problem, ContestProblemServiceModel>()
                .ForMember(d => d.ContestId, opt => opt.MapFrom(s => s.ProblemGroup.ContestId))
                .ForMember(
                    d => d.IsExcludedFromHomework,
                    opt => opt.MapFrom(s => s.ProblemGroup.Type == ProblemGroupType.ExcludedFromHomework))
                .ForMember(
                    d => d.FileSizeLimit,
                    opt => opt.MapFrom(s => s.SourceCodeSizeLimit.HasValue ? (double)s.SourceCodeSizeLimit / 1024 : default))
                .ForMember(
                    d => d.AllowedSubmissionTypes,
                    opt => opt.MapFrom(s => s.SubmissionTypesInProblems))
                .ForMember(
                    d => d.Points,
                    opt => opt.MapFrom(s => 0))
                .ForMember(
                    d => d.MemoryLimit,
                    opt => opt.MapFrom(s => (double?)s.MemoryLimit))
                .ForMember(
                    d => d.Resources,
                    opt => opt.MapFrom(s => s.Resources.OrderBy(x => x.OrderBy)))
                .ForMember(
                    d => d.TimeLimit,
                    opt => opt.MapFrom(s => (double?)s.TimeLimit))
                .ReverseMap();
    }
}