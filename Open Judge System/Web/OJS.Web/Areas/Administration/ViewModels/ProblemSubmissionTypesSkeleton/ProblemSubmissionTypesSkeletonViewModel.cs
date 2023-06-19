﻿namespace OJS.Web.Areas.Administration.ViewModels.ProblemSubmissionTypesSkeleton
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using OJS.Data.Models;
    using OJS.Workers.Common.Extensions;
    using static OJS.Common.Constants.EditorTemplateConstants;
    using Resource = Resources.Areas.Administration.Problems.ViewModels.DetailedProblem;
    using SharedResource = Resources.Areas.Administration.Contests.ViewModels.ContestAdministration;

    public class ProblemSubmissionTypesSkeletonViewModel
    {
        public static Expression<Func<ProblemSubmissionTypeExecutionDetails, ProblemSubmissionTypesSkeletonViewModel>> ViewModel
        {
            get
            {
                return pst => new ProblemSubmissionTypesSkeletonViewModel
                {
                    SolutionSkeletonData = pst.SolutionSkeleton,
                    ProblemId = pst.ProblemId,
                    SubmissionTypeId = pst.SubmissionTypeId,
                    TimeLimit = pst.TimeLimit.Value,
                };
            }
        }
        
        public int ProblemId { get; set; }
        
        public int SubmissionTypeId { get; set; }

        public int? TimeLimit { get; set; }

        [AllowHtml]
        [Display(Name = nameof(Resource.Solution_skeleton), ResourceType = typeof(Resource))]
        [UIHint(MultiLineText)]
        public string SolutionSkeleton
        {
            get => this.SolutionSkeletonData.Decompress();

            set => this.SolutionSkeletonData = !string.IsNullOrWhiteSpace(value) ? value.Compress() : null;
        }
        
        [AllowHtml]
        internal byte[] SolutionSkeletonData { get; set; }
    }
}