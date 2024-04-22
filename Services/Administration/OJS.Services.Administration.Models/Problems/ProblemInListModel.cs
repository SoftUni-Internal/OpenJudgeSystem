namespace OJS.Services.Administration.Models.Problems;

using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Problems;
using SoftUni.AutoMapper.Infrastructure.Models;
using System;
using System.Linq;

public class ProblemInListModel : IMapExplicitly
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public ProblemGroupType ProblemGroupType { get; set; }

    public double ProblemGroupOrderBy { get; set; }

    public int ProblemGroupId { get; set; }
    public string? Contest { get; set; }

    public string? ContestId { get; set; }

    public int PracticeTestsCount { get; set; }

    public int CompeteTestsCount { get; set; }

    public short MaximumPoints { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public void RegisterMappings(IProfileExpression configuration) =>
        configuration.CreateMap<Problem, ProblemInListModel>()
            .ForMember(x => x.Id, opt
                => opt.MapFrom(p => p.Id))
            .ForMember(x => x.Name, opt
                => opt.MapFrom(p => p.Name))
            .ForMember(x => x.ProblemGroupId, opt
                => opt.MapFrom(p => p.ProblemGroup.Id))
            .ForMember(x => x.Contest, opt
                => opt.MapFrom(p => p.ProblemGroup.Contest.Name))
            .ForMember(x => x.ContestId, opt
                => opt.MapFrom(p => p.ProblemGroup.ContestId))
            .ForMember(x => x.PracticeTestsCount, opt
                => opt.MapFrom(p => p.Tests.Count(test => test.IsTrialTest)))
            .ForMember(x => x.CompeteTestsCount, opt
                => opt.MapFrom(p => p.Tests.Count(test => test.IsOpenTest)))
            .ForMember(x => x.MaximumPoints, opt
                => opt.MapFrom(p => p.MaximumPoints))
            .ForMember(x => x.IsDeleted, opt
                => opt.MapFrom(p => p.IsDeleted));
}