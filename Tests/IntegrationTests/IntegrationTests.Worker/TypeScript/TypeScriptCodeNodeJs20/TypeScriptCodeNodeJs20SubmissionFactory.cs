namespace IntegrationTests.Worker.TypeScript.TypeScriptCodeNodeJs20;

using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionDetails;
using OJS.Workers.Common.Models;

public class TypeScriptCodeNodeJs20SubmissionFactory : BaseSubmissionFactory<TypeScriptCodeNodeJs20Parameters>
{
    public override SubmissionForProcessingPubSubModel GetSubmission(TypeScriptCodeNodeJs20Parameters strategyParameters)
    {
        var submission = new SubmissionForProcessingPubSubModel
        {
            Id = this.GetNextId(),
            ExecutionType = ExecutionType.TestsExecution,
            ExecutionStrategy = ExecutionStrategyType.TypeScriptV20PreprocessExecuteAndCheck,
            CompilerType = CompilerType.TypeScriptCompiler,
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
                TaskSkeleton = null,
                TaskSkeletonAsString = null,
                Tests =
                [
                    new TestContext { Id = 102739, Input = "3\n0\n0\n4", Output = "{3, 0} to {0, 0} is valid\n{0, 4} to {0, 0} is valid\n{3, 0} to {0, 4} is valid\n", IsTrialTest = true, OrderBy = 1 },
                    new TestContext { Id = 102740, Input = "2\n1\n1\n1", Output = "{2, 1} to {0, 0} is invalid\n{1, 1} to {0, 0} is invalid\n{2, 1} to {1, 1} is valid\n", IsTrialTest = true, OrderBy = 2 },
                    new TestContext { Id = 102741, Input = "10\n10\n10\n10", Output = "{10, 10} to {0, 0} is invalid\n{10, 10} to {0, 0} is invalid\n{10, 10} to {10, 10} is valid\n", IsTrialTest = false, OrderBy = 1 },
                    new TestContext { Id = 102742, Input = "0\n0\n0\n0", Output = "{0, 0} to {0, 0} is valid\n{0, 0} to {0, 0} is valid\n{0, 0} to {0, 0} is valid\n", IsTrialTest = false, OrderBy = 2 },
                    new TestContext { Id = 102743, Input = "0\n5\n0\n-12", Output = "{0, 5} to {0, 0} is valid\n{0, -12} to {0, 0} is valid\n{0, 5} to {0, -12} is valid\n", IsTrialTest = false, OrderBy = 3 },
                    new TestContext { Id = 102744, Input = "13\n7\n1\n-1", Output = "{13, 7} to {0, 0} is invalid\n{1, -1} to {0, 0} is invalid\n{13, 7} to {1, -1} is invalid\n", IsTrialTest = false, OrderBy = 4 },
                    new TestContext { Id = 102745, Input = "-2\n5\n-6\n8", Output = "{-2, 5} to {0, 0} is invalid\n{-6, 8} to {0, 0} is valid\n{-2, 5} to {-6, 8} is valid\n", IsTrialTest = false, OrderBy = 5 }
                ]
            }
        };


        return submission;
    }
}