using OJS.Common.Models;
using OJS.Workers.Common.Models;

namespace OJS.Data.Models
{
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SubmissionsForProcessing")]
    public class SubmissionForProcessing
    {
        public int Id { get; set; }

        public int SubmissionId { get; set; }

        public bool Processing { get; set; }

        public bool Processed { get; set; }
        
        public WorkerType WorkerType { get; set; }
        
        public int ExecutionFailuresCount { get; set; }
    }
}
