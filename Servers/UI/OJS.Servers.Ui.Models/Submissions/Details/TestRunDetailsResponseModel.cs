﻿namespace OJS.Servers.Ui.Models.Submissions.Details
{
    using OJS.Services.Ui.Models.Submissions;
    using SoftUni.AutoMapper.Infrastructure.Models;

    public class TestRunDetailsResponseModel : IMapFrom<TestRunDetailsServiceModel>
    {
        public int Id { get; set; }

        public int TimeUsed { get; set; }

        public long MemoryUsed { get; set; }

        public int SubmissionId { get; set; }

        public string ExecutionComment { get; set; } = null!;

        public string CheckerComment { get; set; } = null!;

        public string ResultType { get; set; } = null!;

        public string? ExpectedOutputFragment { get; set; }

        public string? UserOutputFragment { get; set; }

        public bool IsTrialTest { get; set; }

        public string? Input { get; set; }

        public double OrderBy { get; set; }

        public bool ShowInput { get; set; }
    }
}