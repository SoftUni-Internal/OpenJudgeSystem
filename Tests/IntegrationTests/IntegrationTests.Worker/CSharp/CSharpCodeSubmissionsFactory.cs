namespace IntegrationTests.Worker.CSharp;

using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionDetails;
using OJS.Workers.Common.Models;

public class CSharpCodeSubmissionFactory : BaseSubmissionFactory<CSharpCodeParameters>
{
    public override SubmissionForProcessingPubSubModel GetSubmission(CSharpCodeParameters strategyParameters)
    {
        var submission = new SubmissionForProcessingPubSubModel
        {
            Id = this.GetNextId(),
            ExecutionType = ExecutionType.TestsExecution,
            ExecutionStrategy = ExecutionStrategyType.DotNetCore6CompileExecuteAndCheck,
            CompilerType = CompilerType.CSharpDotNetCore,
            FileContent = null,
            AdditionalFiles = null,
            Code = strategyParameters.Code,
            TimeLimit = 100,
            ExecutionStrategyBaseTimeLimit = null,
            MemoryLimit = 16777216,
            ExecutionStrategyBaseMemoryLimit = null,
            Verbosely = false,
            SimpleExecutionDetails = null,
            TestsExecutionDetails = new TestsExecutionDetailsServiceModel
            {
                MaxPoints = 100,
                TaskId = null,
                CheckerType = "trim",
                CheckerParameter = null,
                Tests =
                [
                    new TestContext
                        { Id = 172459, Input = "0", Output = "Hello SoftUni", IsTrialTest = true, OrderBy = 0 },
                    new TestContext
                        { Id = 172460, Input = "0", Output = "Hello SoftUni", IsTrialTest = false, OrderBy = 0 }
                ],
                TaskSkeleton = null,
                TaskSkeletonAsString = null
            }
        };

        return submission;
    }
}