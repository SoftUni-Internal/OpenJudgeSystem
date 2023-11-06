using System;
namespace OJS.Services.Business.Contests.Models
{
    public class SubmissionRunTimeCalculationModel
    {
        public DateTime? StartedExecutionOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? CompletedExecutionOn { get; set; }
    }
}
