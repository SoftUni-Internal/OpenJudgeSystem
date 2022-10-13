namespace OJS.Servers.Ui.Models.Contests;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OJS.Services.Ui.Models.Contests;
using SoftUni.AutoMapper.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class ContestFiltersRequestModel : IMapExplicitly
{
    [BindProperty(Name = "category")]
    public int? CategoryId { get; set; }

    [BindProperty(Name = "status")]
    public IEnumerable<ContestStatus>? Statuses { get; set; }

    [BindProperty(Name = "strategy")]
    public IEnumerable<int>? SubmissionTypeIds { get; set; }

    [BindProperty(Name = "page")]
    public int? PageNumber { get; set; }

    public int? ItemsPerPage { get; set; }

    public string SortType { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<ContestFiltersRequestModel, ContestFiltersServiceModel>()
            .ForMember(
                m => m.CategoryIds,
                opt => opt.MapFrom(
                    src => src.CategoryId.HasValue
                        ? new List<int> { src.CategoryId.Value }
                        : Enumerable.Empty<int>()))
            .ForMember(m => m.SortType,
                opt => opt.MapFrom(
                    src => Enum.Parse<ContestSortType>(src.SortType)));

}