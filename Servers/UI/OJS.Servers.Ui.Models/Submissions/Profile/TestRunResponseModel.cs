﻿using OJS.Services.Common.Models.Submissions;
using OJS.Services.Ui.Models.Submissions;

namespace OJS.Servers.Ui.Models.Submissions.Profile
{
    using SoftUni.AutoMapper.Infrastructure.Models;

    public class TestRunResponseModel : IMapFrom<TestRunServiceModel>
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
    }
}