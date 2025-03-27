namespace OJS.Servers.Ui.Models.Submissions.Details
{
    using AutoMapper;
    using System;
    using System.Collections.Generic;
    using OJS.Servers.Ui.Models.Submissions.Profile;
    using OJS.Servers.Ui.Models.Users;
    using OJS.Services.Ui.Models.Submissions;
    using OJS.Services.Infrastructure.Models.Mapping;
    using OJS.Workers.Common.Models;

    public class SubmissionDetailsResponseModel : IMapExplicitly
    {
        public int Id { get; set; }

        public ProblemResponseModel Problem { get; set; } = null!;

        public int Points { get; set; }

        public string? Content { get; set; }

        public IEnumerable<TestRunDetailsResponseModel> TestRuns { get; set; } = null!;

        public UserResponseModel User { get; set; } = null!;

        public bool UserIsInRoleForContest { get; set; }

        public SubmissionTypeForSubmissionDetailsResponseModel SubmissionType { get; set; } = null!;

        public double? MaxUsedTime { get; set; }

        public double? MaxUsedMemory { get; set; }

        public short MaxPoints { get; set; }

        public bool IsOfficial { get; set; }

        public bool IsCompiledSuccessfully { get; set; }

        public bool IsEligibleForRetest { get; set; }

        public string CompilerComment { get; set; } = null!;

        public bool IsProcessed { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public DateTime? StartedExecutionOn { get; set; }

        public DateTime? CompletedExecutionOn { get; set; }

        public string? WorkerName { get; set; }

        public string? ProcessingComment { get; set; }

        public int TotalTests { get; set; }

        public int ContestId { get; set; }

        public string? ContestName { get; set; }

        public int? ContestCategoryId { get; set; }

        public bool AllowMentor { get; set; }

        public ExceptionType? ExceptionType { get; set; }

        public void RegisterMappings(IProfileExpression configuration)
            => configuration
                .CreateMap<SubmissionDetailsServiceModel, SubmissionDetailsResponseModel>()
                .ForMember(m => m.Content, opt => opt.MapFrom(s => s.Code));
    }
}