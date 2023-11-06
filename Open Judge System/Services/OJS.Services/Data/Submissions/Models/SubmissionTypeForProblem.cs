using OJS.Data.Models;
using System.Collections;
using System.Collections.Generic;

namespace OJS.Services.Data.Submissions.Models
{
    public class SubmissionTypeForProblem
    {
        public int ProblemId { get; set; }

        public string ProblemName { get; set; }

        public int SubmissionTypeId { get; set; }

        public string SubmissionTypeName { get; set; }

        public bool HasAuthorSubmission { get; set; }
    }
}
