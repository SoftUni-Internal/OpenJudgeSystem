namespace OJS.Web.Areas.Administration.ViewModels.ContestCategory
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using static OJS.Common.Constants.EditorTemplateConstants;

    using Resource = Resources.Areas.Administration.Contests.ViewModels.ContestAdministration;

    public class EditAllContestsAdministrationViewModel
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        [UIHint(KendoDropDownList)]
        public int? Type { get; set; }

        [Display(Name = nameof(Resource.Start_time), ResourceType = typeof(Resource))]
        [UIHint(KendoDateAndTimePicker)]
        public DateTime? StartTime { get; set; }

        [Display(Name = nameof(Resource.End_time), ResourceType = typeof(Resource))]
        [UIHint(KendoDateAndTimePicker)]
        public DateTime? EndTime { get; set; }

        [Display(Name = nameof(Resource.Practice_start_time), ResourceType = typeof(Resource))]
        [UIHint(KendoDateAndTimePicker)]
        public DateTime? PracticeStartTime { get; set; }

        [Display(Name = nameof(Resource.Practice_end_time), ResourceType = typeof(Resource))]
        [UIHint(KendoDateAndTimePicker)]
        public DateTime? PracticeEndTime { get; set; }

        [Display(Name = nameof(Resource.Submissions_limit), ResourceType = typeof(Resource))]
        [UIHint(KendoPositiveInteger)]
        public int? LimitBetweenSubmissions { get; set; }
    }
}