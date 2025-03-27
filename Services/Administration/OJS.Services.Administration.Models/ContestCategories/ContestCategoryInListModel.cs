namespace OJS.Services.Administration.Models.ContestCategories;

using AutoMapper;
using OJS.Data.Models.Contests;
using OJS.Services.Infrastructure.Models.Mapping;
using System;

public class ContestCategoryInListModel : IMapExplicitly
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public double OrderBy { get; set; }

    public string? Parent { get; set; }

    public int? ParentId { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsVisible { get; set; }

    public bool HasChildren { get; set; }

    public bool AllowMentor { get; set; }

    public DateTime? DeletedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public DateTime? CreatedOn { get; set; }

    public void RegisterMappings(IProfileExpression configuration) =>
        configuration.CreateMap<ContestCategory, ContestCategoryInListModel>()
            .ForMember(d => d.Parent, opt
                => opt.MapFrom(c => c.Parent!.Name))
            .ForMember(d => d.HasChildren, opt
                => opt.MapFrom(c => c.Children.Count != 0));
}