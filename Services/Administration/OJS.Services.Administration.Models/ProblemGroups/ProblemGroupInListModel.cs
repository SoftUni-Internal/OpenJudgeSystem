namespace OJS.Services.Administration.Models.ProblemGroups;

using AutoMapper;
using OJS.Common.Enumerations;
using OJS.Data.Models.Problems;
using SoftUni.AutoMapper.Infrastructure.Models;
using System;

public class ProblemGroupInListModel : IMapExplicitly
{
    public int Id { get; set; }

    public string? Contest { get; set; }

    public bool IsDeleted { get; set; }

    public double OrderBy { get; set; }

    public ProblemGroupType Type { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public void RegisterMappings(IProfileExpression configuration) =>
        configuration.CreateMap<ProblemGroup, ProblemGroupInListModel>()
            .ForMember(x => x.Contest, opt
                => opt.MapFrom(p => p.Contest.Name));
}