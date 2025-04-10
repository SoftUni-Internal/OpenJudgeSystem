namespace OJS.Services.Common.Models.Contests.Results;

using OJS.Common.Enumerations;
using OJS.Data.Models.Problems;
using System;
using System.Linq.Expressions;

public class ContestProblemListViewModel
{
    public static Expression<Func<Problem, ContestProblemListViewModel>> FromProblem =>
        pr => new ContestProblemListViewModel
        {
            Id = pr.Id,
            Name = pr.Name,
            MaximumPoints = pr.MaximumPoints,
            ProblemGroupId = pr.ProblemGroupId,
            IsExcludedFromHomework = pr.ProblemGroup.Type == ProblemGroupType.ExcludedFromHomework,
            OrderBy = pr.OrderBy,
        };

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public short MaximumPoints { get; set; }

    public int ProblemGroupId { get; set; }

    public bool IsExcludedFromHomework { get; set; }

    public double OrderBy { get; set; }
}