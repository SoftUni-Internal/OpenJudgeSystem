﻿namespace OJS.Web.Areas.Contests.ViewModels.Contests
{
    using System;
    using System.Linq.Expressions;

    using OJS.Data.Models;

    public class ContestProblemListViewModel
    {
        public static Expression<Func<Problem, ContestProblemListViewModel>> FromProblem =>
            pr => new ContestProblemListViewModel
                {
                    Id = pr.Id,
                    Name = pr.Name,
                    ShowResults = pr.ShowResults,
                    MaximumPoints = pr.MaximumPoints
                };

        public int Id { get; set; }

        public string Name { get; set; }
        
        public bool ShowResults { get; set; }

        public short MaximumPoints { get; set; }
    }
}