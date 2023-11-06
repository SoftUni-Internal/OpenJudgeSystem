using System.ComponentModel.DataAnnotations;

namespace OJS.Services.Business.Contests.Models
{
    public abstract class BaseContestBusinessModel
    {
        [Range(1, 1000)]
        public int ExamLengthInHours { get; set; }

        [Range(1, 100)]
        public int ExpectedExamProblemsCount { get; set; }

        [Range(1, 100000)]
        public int ExpectedStudentsCount { get; set; }

        [Range(1, 100000)]
        public int? PreviousContestParticipants { get; set; }

        [Range(1, 10000000)]
        public int? PreviousContestSubmissions { get; set; }

        [Range(1, 1000)]
        public int AverageProblemRunTimeInSeconds { get; set; }

        [Range(1, 100)]
        public int WorkerIdleTimeInPercentage { get; set; } = 20;

        [Range(1, 1000)]
        public int MaxJudgeParalelWork { get; set; } = 25;

        [Range(1, 100)]
        public int SafetyFactor { get; set; } = 2;

        [Range(1, 1000)]
        public int ActualWorkers { get; set; }

        [Range(1, 100)]
        public int? PreviousContestExpectedProblems { get; set; }

        [Range(1, int.MaxValue)]
        public int CurrentContestId { get; set; }

        public int? PreviousContestId { get; set; }

        public int? PreviousAverageProblemRunTimeInSeconds { get; set; }

        public int MaxAllowedTimeForSubmissionCompletion { get; set; }

    }
}
