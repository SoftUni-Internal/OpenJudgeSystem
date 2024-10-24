namespace OJS.Services.Ui.Models.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoMapper;
    using OJS.Data.Models.Submissions;
    using OJS.Services.Ui.Models.Users;
    using OJS.Services.Infrastructure.Models.Mapping;

    public class SubmissionDetailsServiceModel : IMapExplicitly
    {
        public int Id { get; set; }

        public ProblemServiceModel Problem { get; set; } = null!;

        public int Points { get; set; }

        public string? Content { get; set; }

        public IEnumerable<TestRunDetailsServiceModel> TestRuns { get; set; } = [];

        public UserServiceModel User { get; set; } = null!;

        public double MaxUsedTime { get; set; }

        public double MaxUsedMemory { get; set; }

        public short MaxPoints { get; set; }

        public SubmissionTypeForSubmissionDetailsServiceModel SubmissionType { get; set; } = null!;

        public bool IsOfficial { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public bool IsProcessed { get; set; }

        public bool IsEligibleForRetest { get; set; }

        public bool UserIsInRoleForContest { get; set; }

        public string? CompilerComment { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public DateTime? StartedExecutionOn { get; set; }

        public DateTime? CompletedExecutionOn { get; set; }

        public string? WorkerName { get; set; }

        public string? ProcessingComment { get; set; }

        public int TotalTests => this.Tests.Count();

        public int ContestId { get; set; }

        public string? ContestName { get; set; }

        public int ContestCategoryId { get; set; }

        public IEnumerable<TestDetailsServiceModel> Tests { get; set; } = [];

        public void RegisterMappings(IProfileExpression configuration)
            => configuration
                .CreateMap<Submission, SubmissionDetailsServiceModel>()
                .ForMember(d => d.MaxUsedMemory, opt => opt.MapFrom(s =>
                    GetMaxMemoryAndTimeUsed(s.TestRunsCache).MaxMemoryUsed))
                .ForMember(d => d.MaxUsedTime, opt => opt.MapFrom(s =>
                    GetMaxMemoryAndTimeUsed(s.TestRunsCache).MaxTimeUsed))
                .ForMember(d => d.Content, opt => opt.MapFrom(s =>
                    s.IsBinaryFile
                        ? null
                        : s.ContentAsString))
                .ForMember(d => d.IsOfficial, opt => opt.MapFrom(s => s.Participant.IsOfficial))
                .ForMember(s => s.IsProcessed, opt => opt.MapFrom(s => s.Processed))
                .ForMember(d => d.ContestId, opt => opt.MapFrom(s => s.Participant.ContestId))
                .ForMember(d => d.ContestName, opt => opt.MapFrom(s => s.Participant.Contest.Name))
                .ForMember(d => d.ContestCategoryId, opt => opt.MapFrom(s => s.Participant.Contest.CategoryId))
                .ForMember(d => d.MaxPoints, opt => opt.MapFrom(s => s.Problem.MaximumPoints))
                .ForMember(d => d.TotalTests, opt => opt.Ignore())
                .ForMember(s => s.UserIsInRoleForContest, opt => opt.Ignore())
                .ForMember(s => s.IsEligibleForRetest, opt => opt.Ignore())
                .ForMember(s => s.User, opt => opt.Ignore())
                .ForMember(s => s.Tests, opt => opt.Ignore());

        private static (long? MaxMemoryUsed, int? MaxTimeUsed) GetMaxMemoryAndTimeUsed(string? testRunsCache)
        {
            if (string.IsNullOrWhiteSpace(testRunsCache))
            {
                return (null, null);
            }

            var cacheParts = testRunsCache.Split('|');
            if (cacheParts.Length <= 1)
            {
                return (null, null);
            }

            var timeMemoryPart = cacheParts[1];
            var timeMemoryValues = timeMemoryPart.Split(',');

            if (timeMemoryValues.Length < 2)
            {
                return (null, null);
            }

            if (int.TryParse(timeMemoryValues[0], out var maxTimeUsed) &&
                long.TryParse(timeMemoryValues[1], out var maxMemoryUsed))
            {
                return (maxMemoryUsed, maxTimeUsed);
            }

            return (null, null);
        }
    }
}