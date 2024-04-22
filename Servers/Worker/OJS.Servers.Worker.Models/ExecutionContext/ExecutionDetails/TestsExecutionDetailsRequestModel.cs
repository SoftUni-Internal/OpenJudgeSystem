namespace OJS.Servers.Worker.Models.ExecutionContext.ExecutionDetails
{
    using System.Collections.Generic;
    using System.Linq;
    using OJS.Services.Common.Models.Submissions.ExecutionDetails;
    using OJS.Workers.Common.Models;
    using SoftUni.AutoMapper.Infrastructure.Models;

    public class TestsExecutionDetailsRequestModel : IMapTo<TestsExecutionDetailsServiceModel>
    {
        public int? MaxPoints { get; set; }

        public string? TaskId { get; set; }

        public int? ExamParticipantId { get; set; }

        public string? CheckerType { get; set; }

        public string? CheckerParameter { get; set; }

        public byte[]? TaskSkeleton { get; set; }

        public string? TaskSkeletonAsString { get; set; }

        public IEnumerable<TestContext> Tests { get; set; } = Enumerable.Empty<TestContext>();
    }
}