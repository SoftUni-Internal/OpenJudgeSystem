using System;

namespace OJS.Services.Business.Contests.Models
{
    public class JudgeLoadResults
    {
        //Judge work
        public int ExpectedExamSubmissions { get; set; }

        public int ExpectedTotalJudgeWorkInMinutes  => (int)Math.Ceiling((this.ExpectedExamSubmissions * 2) / 60.0);

        public int ProcessedSubmissionsPerWorkerPerMinute { get; set; }

        public int MinimumWorkersRequired { get; set; }

        //Minimum required workers multiplied by safety factor with value 1.2;
        public int SuggestedWorkers => (int)Math.Round(this.MinimumWorkersRequired * 1.2);

        //Doomsday scenario

        // <summary>
        /// Judge work required for the doomsday scenario in minutes
        /// </summary>
        public int JudgeWorkInMnutes { get; set; }

        // <summary>
        /// Judge work required for the doomsday scenario per worker in minutes
        /// </summary>
        public int JudgeWorkPerWorkerInMinutes { get; set; }

        /// <summary>
        /// Seconds between submissions for doomsday scenario  on base performance scale without additional workers
        /// </summary>
        public int SecondsBetweenSubmissionsBase { get; set; }

        /// <summary>
        /// "Seconds between submissions for doomsday scenario  on high performance scale with actual workers
        /// </summary>
        public int SecondsBetweenSubmissionsHigh { get; set; }

        //Distribution Results
        public int MaxSubmissionsPerMinute { get; set; }

        public int MaxDistributedWorkersRequired { get; set; }

        public int JudgeWorkRequiredInMinutes { get; set; }

        public int JudgeWorkRequiredPerWorkerInSeconds { get; set; }

        public int SecondsBetweenSubmission { get; set; }

        public int MaxSecondsBetweenSubmissions { get; set; }

        public double MaxUsersAtSameTime { get; set; }

        public int PreviousContestParticipants { get; set; }

        public int PreviousContestSubmissions { get; set; }

        public int PreviousAverageProblemRunTimeInSeconds { get; set; }

        public int PreviousContestExpectedProblems { get; set; }
    }
}
