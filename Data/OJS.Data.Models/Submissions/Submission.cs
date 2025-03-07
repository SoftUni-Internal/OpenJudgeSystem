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

        public ExceptionType? ExceptionType { get; set; }

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
        /// Gets or sets a cache field for submission test runs, which includes:
        /// - A representation of each test run result as an integer corresponding to the <see cref="TestRunResultType"/> enumeration.
        /// - The maximum time and memory used for the submission.
        ///
        /// The cache field is a string composed of two parts separated by a pipe character '|', following the format:
        /// <c>{test runs}|{max time used},{max memory used}</c>.
        ///
        /// The first integer in the test runs part represents the number of trial tests associated with this submission!
        /// This field optimizes database queries by reducing the need to join with the test runs' table.
        ///
        /// Example: 300011002|90,1519616
        /// Breakdown:
        /// -> First Part (Test Runs):
        ///   - Three trial test runs with result 0 (Correct Answer).
        ///   - Five normal test runs with results:
        ///     - Two results of 1 (Wrong Answer).
        ///     - Two results of 0 (Correct Answer).
        ///     - One result of 2 (Time Limit Exceeded).
        ///
        /// -> Second Part (Max Time and Memory):
        ///   - Time used: 90 milliseconds.
        ///   - Memory used: 1,519,616 bytes.
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
            => this.TestRuns.Count(x => x.ResultType == TestRunResultType.CorrectAnswer);

        [NotMapped]
        public int CorrectTestRunsWithoutTrialTestsCount
            => this.TestRuns.Count(x => x.ResultType == TestRunResultType.CorrectAnswer && !x.Test.IsTrialTest);

        [NotMapped]
        public int IncorrectTestRunsWithoutTrialTestsCount
            => this.TestRuns.Count(x => x.ResultType != TestRunResultType.CorrectAnswer && !x.Test.IsTrialTest);

        [NotMapped]
        public int TestsWithoutTrialTestsCount
            => this.Problem?.Tests.Count(x => !x.IsTrialTest) ?? 0;

        public void CacheTestRuns()
        {
            if (this.TestRuns.Count != 0)
            {
                var result = new StringBuilder();
                var trialTests = 0;
                double maxTimeUsed = 0;
                double maxMemoryUsed = 0;

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

                    if (testRun.TimeUsed > maxTimeUsed)
                    {
                        maxTimeUsed = testRun.TimeUsed;
                    }

                    if (testRun.MemoryUsed > maxMemoryUsed)
                    {
                        maxMemoryUsed = testRun.MemoryUsed;
                    }
                }

                this.TestRunsCache = $"{trialTests}{result}|{maxTimeUsed},{maxMemoryUsed}";
            }
        }
    }
}