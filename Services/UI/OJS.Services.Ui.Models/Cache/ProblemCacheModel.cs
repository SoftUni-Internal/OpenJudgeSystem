﻿namespace OJS.Services.Ui.Models.Cache;

using System;
using System.Collections.Generic;
using AutoMapper;
using OJS.Data.Models.Problems;
using OJS.Services.Infrastructure.Models.Mapping;

public class ProblemCacheModel : IMapExplicitly
{
    public int Id { get; set; }

    public int ProblemGroupId { get; set; }

    public virtual ProblemGroupCacheModel ProblemGroup { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public short MaximumPoints { get; set; }

    public int TimeLimit { get; set; }

    public int MemoryLimit { get; set; }

    public int? SourceCodeSizeLimit { get; set; }

    public int? CheckerId { get; set; }

    public virtual CheckerCacheModel? Checker { get; set; }

    public double OrderBy { get; set; }

    public byte[]? SolutionSkeleton { get; set; }

    public bool ShowResults { get; set; }

    public bool ShowDetailedFeedback { get; set; }

    public virtual ICollection<ProblemResourceCacheModel> Resources { get; set; } = [];

    public virtual ICollection<SubmissionTypeInProblemCacheModel> SubmissionTypesInProblems { get; set; } = [];

    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }

    public void RegisterMappings(IProfileExpression configuration)
        => configuration
            .CreateMap<Problem, ProblemCacheModel>()
            .ForMember(m => m.Checker, opt => opt.Ignore()) // Checker is mapped separately, as it's cached separately.
            .ReverseMap();
}