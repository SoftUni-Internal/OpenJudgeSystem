namespace OJS.Web.Areas.Administration.ViewModels.SubmissionType
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc;
    using OJS.Common.DataAnnotations;
    using OJS.Data.Models;
    using OJS.Web.Areas.Administration.ViewModels.Problem;
    using OJS.Workers.Common.Extensions;
    using OJS.Workers.Common.Models;
    using Resources.Areas.Administration.Problems.ViewModels;
    using static OJS.Common.Constants.EditorTemplateConstants;

    public class SubmissionTypeViewModel
    {
        [ExcludeFromExcel]
        public static Expression<Func<SubmissionType, SubmissionTypeViewModel>> ViewModel
        {
            get
            {
                return st => new SubmissionTypeViewModel
                {
                    Id = st.Id,
                    Name = st.Name,
                    ExecutionStrategyType = st.ExecutionStrategyType,
                };
            }
        }

        public int? Id { get; set; }

        public string Name { get; set; }

        public bool IsChecked { get; set; }

        [Range(1, 10000)]
        public int? TimeLimit { get; set; }

        [Range(1, int.MaxValue)]
        public int? MemoryLimit { get; set; }

        public ExecutionStrategyType ExecutionStrategyType { get; set; }
        
        [AllowHtml]
        [Display(Name = nameof(DetailedProblem.Solution_skeleton), ResourceType = typeof(DetailedProblem))]
        [UIHint(MultiLineText)]
        public string SolutionSkeleton
        {
            get => this.SolutionSkeletonData.Decompress();

            set => this.SolutionSkeletonData = !string.IsNullOrWhiteSpace(value) ? value.Compress() : null;
        }

        [AllowHtml]
        internal byte[] SolutionSkeletonData { get; set; }

        public static Action<SubmissionTypeViewModel> ApplySelectedTo(ProblemAdministrationViewModel problem)
        {
            return st =>
            {
                var submissionViewModel = new SubmissionTypeViewModel
                {
                    Id = st.Id,
                    Name = st.Name,
                    IsChecked = false,
                    ExecutionStrategyType = st.ExecutionStrategyType,
                    SolutionSkeleton = problem
                        .ProblemSubmissionTypesSkeletons
                        .FirstOrDefault(x => x.SubmissionTypeId == st.Id)?.SolutionSkeleton,
                };

                var selectedSubmission = problem.SelectedSubmissionTypes.FirstOrDefault(s => s.Id == st.Id);

                if (selectedSubmission != null)
                {
                    submissionViewModel.IsChecked = true;
                    submissionViewModel.TimeLimit = problem
                        .ProblemSubmissionTypesSkeletons
                        .FirstOrDefault(x => x.SubmissionTypeId == selectedSubmission.Id)?
                        .TimeLimit;

                    submissionViewModel.MemoryLimit = problem.ProblemSubmissionTypesSkeletons.FirstOrDefault(x => x.SubmissionTypeId == selectedSubmission.Id)?
                        .MemoryLimit;
                }

                problem.SubmissionTypes.Add(submissionViewModel);
            };
        }

        public SubmissionType GetEntityModel(SubmissionType model = null)
        {
            model = model ?? new SubmissionType();
            model.Id = this.Id.Value;
            return model;
        }
    }
}