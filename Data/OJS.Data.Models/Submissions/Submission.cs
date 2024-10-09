namespace OJS.Data.Models.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text;
    using OJS.Data.Models.Participants;
    using OJS.Data.Models.Problems;
    using OJS.Data.Models.Tests;
    using OJS.Data.Validation;
    using OJS.Workers.Common.Models;
    using OJS.Data.Models.Common;
    using OJS.Workers.Common.Extensions;

    public class Submission : DeletableAuditInfoEntity<int>
    {
        public int ParticipantId { get; set; }

        public virtual Participant Participant { get; set; } = null!;

        public int ProblemId { get; set; }

        public virtual Problem Problem { get; set; } = null!;

        public int? SubmissionTypeId { get; set; }

        public virtual SubmissionType? SubmissionType { get; set; }

        /// <remarks>
        /// Using byte[] (compressed with deflate) to save database space for text inputs. For other file types the actual file content is saved in the field.
        /// </remarks>
        public byte[] Content { get; set; } = Array.Empty<byte>();

        /// <remarks>
        /// If the value of FileExtension is null, then compressed text file is written in Content.
        /// </remarks>
        public string? FileExtension { get; set; }

        public byte[]? SolutionSkeleton { get; set; }

        public DateTime? StartedExecutionOn { get; set; }

        public DateTime? CompletedExecutionOn { get; set; }

        [StringLength(ConstraintConstants.IpAddressMaxLength)]
        [Column(TypeName = "varchar")]
        public string? IpAddress { get; set; }

        [StringLength(ConstraintConstants.Submission.WorkerNameMaxLength)]
        public string? WorkerName { get; set; }

        [NotMapped]
        public bool IsBinaryFile => !string.IsNullOrWhiteSpace(this.FileExtension);

        [NotMapped]
        public string ContentAsString
        {
            get
            {
                if (this.IsBinaryFile)
                {
                    throw new InvalidOperationException("This is a binary file (not a text submission).");
                }

                return this.Content.Decompress();
            }

            set
            {
                if (this.IsBinaryFile)
                {
                    throw new InvalidOperationException("This is a binary file (not a text submission).");
                }

                this.Content = value.Compress();
            }
        }

        public bool IsCompiledSuccessfully { get; set; }

        public string? CompilerComment { get; set; }

        public bool? IsPublic { get; set; }

        public virtual ICollection<TestRun> TestRuns { get; set; } = new HashSet<TestRun>();

        /// <summary>
        /// Gets or sets a cache field for submission test runs representing each test run result as an integer equal to <see cref="TestRunResultType"/>.
        /// The first integer represent the number of trial tests associated with this submissions.
        /// This field optimized database queries.
        ///
        /// Example: 300011002 means:
        /// - Three trial tests runs with 0 result (Correct Answer)
        /// - Five normal test runs with:
        ///   - Two 1 results (Wrong Answer)
        ///   - Two 0 results (Correct Answer)
        ///   - One 2 result (Time Limit).
        /// </summary>
        public string? TestRunsCache { get; set; }

        public bool Processed { get; set; }

        public string? ProcessingComment { get; set; }

        /// <summary>
        /// Gets or sets a cache field for submissions points (to speed-up some of the database queries).
        /// </summary>
        public int Points { get; set; }

        [NotMapped]
        public int CorrectTestRunsCount
            => this.TestRuns.Count(x => x.ResultType == (int)TestRunResultType.CorrectAnswer);

        [NotMapped]
        public int CorrectTestRunsWithoutTrialTestsCount
            => this.TestRuns.Count(x => x.ResultType == (int)TestRunResultType.CorrectAnswer && !x.Test.IsTrialTest);

        [NotMapped]
        public int IncorrectTestRunsWithoutTrialTestsCount
            => this.TestRuns.Count(x => x.ResultType != (int)TestRunResultType.CorrectAnswer && !x.Test.IsTrialTest);

        [NotMapped]
        public int TestsWithoutTrialTestsCount
            => this.Problem?.Tests.Count(x => !x.IsTrialTest) ?? 0;

        // TODO: Should be moved to a data/business service
        public void CacheTestRuns()
        {
            if (this.TestRuns.Any())
            {
                var result = new StringBuilder();
                var trialTests = 0;

                var orderedTestRuns = this.TestRuns
                    .OrderByDescending(tr => tr.IsTrialTest)
                    .ThenBy(tr => tr.Id);

                foreach (var testRun in orderedTestRuns)
                {
                    if (testRun.IsTrialTest)
                    {
                        trialTests++;
                    }

                    result.Append((int)testRun.ResultType);
                }

                this.TestRunsCache = $"{trialTests}{result}";
            }
        }
    }
}