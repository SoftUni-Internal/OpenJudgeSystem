namespace OJS.Services.Worker.Business.Implementations
{
    using Microsoft.Extensions.Options;
    using OJS.Services.Worker.Models.Configuration;
    using System;

    using OJS.Workers.Common;
    using OJS.Workers.Common.Models;
    using OJS.Workers.ExecutionStrategies.Models;
    using System.Threading.Tasks;

    public class SubmissionExecutor(
        IExecutionStrategyFactory executionStrategyFactory,
        IOptions<SubmissionExecutionConfig> submissionExecutionConfigAccessor)
        : ISubmissionExecutor
    {
        private readonly SubmissionExecutionConfig executionConfig = submissionExecutionConfigAccessor.Value;

        public Task<IExecutionResult<TResult>> Execute<TInput, TResult>(
            OjsSubmission<TInput> submission)
            where TResult : ISingleCodeRunResult, new()
        {
            var executionStrategy = this.CreateExecutionStrategy(submission);

            var executionContext = this.CreateExecutionContext(submission);

            return this.ExecuteSubmission<TInput, TResult>(executionStrategy, executionContext, submission);
        }

        private IExecutionStrategy CreateExecutionStrategy(IOjsSubmission submission)
        {
            try
            {
                return executionStrategyFactory.CreateExecutionStrategy(submission);
            }
            catch (Exception ex)
            {
                submission.ProcessingComment = $"Exception in creating execution strategy: {ex.Message}";
                submission.ExceptionType = ExceptionType.Strategy;
                throw new Exception($"Exception in {nameof(this.CreateExecutionStrategy)}", ex);
            }
        }

        private IExecutionContext<TInput> CreateExecutionContext<TInput>(OjsSubmission<TInput> submission)
        {
            try
            {
                return new ExecutionContext<TInput>
                {
                    AdditionalCompilerArguments = submission.AdditionalCompilerArguments,
                    Code = submission.Code,
                    FileContent = submission.FileContent,
                    AdditionalFiles = submission.AdditionalFiles,
                    AllowedFileExtensions = submission.AllowedFileExtensions,
                    CompilerType = submission.CompilerType,
                    MemoryLimit = submission.MemoryLimit,
                    TimeLimit = submission.TimeLimit,
                    Input = submission.Input,
                    VerboseLogFileMaxBytes = this.executionConfig.SubmissionVerboseLogFileMaxBytes,
                };
            }
            catch (Exception ex)
            {
                submission.ProcessingComment = $"Exception in creating execution context: {ex.Message}";
                submission.ExceptionType = ExceptionType.Strategy;
                throw new Exception($"Exception in {nameof(this.CreateExecutionContext)}", ex);
            }
        }

        private Task<IExecutionResult<TResult>> ExecuteSubmission<TInput, TResult>(
            IExecutionStrategy executionStrategy,
            IExecutionContext<TInput> executionContext,
            IOjsSubmission submission)
            where TResult : ISingleCodeRunResult, new()
        {
            try
            {
                return executionStrategy.SafeExecute<TInput, TResult>(executionContext, submission);
            }
            catch (Exception ex)
            {
                submission.ProcessingComment = $"Exception in executing the submission: {ex.Message}";
                submission.ExceptionType = ExceptionType.Strategy;
                throw new Exception($"Exception in {nameof(this.ExecuteSubmission)}", ex);
            }
        }
    }
}