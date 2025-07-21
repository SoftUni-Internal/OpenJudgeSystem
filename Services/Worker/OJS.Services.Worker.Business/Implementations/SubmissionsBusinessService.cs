namespace OJS.Services.Worker.Business.Implementations;

using System;
using System.Linq;
using OJS.Workers.Common;
using OJS.Workers.ExecutionStrategies.Models;
using OJS.Services.Worker.Business.Validation;
using OJS.Services.Worker.Models.Configuration;
using FluentExtensions.Extensions;
using OJS.Workers.ExecutionStrategies.Extensions;
using AutoMapper;
using Microsoft.Extensions.Options;
using OJS.Services.Infrastructure.Extensions;
using OJS.Services.Worker.Business.ExecutionContext;
using OJS.Services.Worker.Business.Extensions;
using OJS.Services.Infrastructure;
using OJS.Services.Common.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Workers.Common.Models;
using OJS.Common.Constants;
using System.Threading;
using System.Threading.Tasks;

public class SubmissionsBusinessService : ISubmissionsBusinessService
{
    private readonly IRandomProvider random;
    private readonly IExecutionContextBuilderService executionContextBuilder;
    private readonly ISubmissionsValidationService submissionsValidation;
    private readonly ISubmissionExecutor submissionExecutor;
    private readonly IMapper mapper;
    private readonly SubmissionExecutionConfig executionConfig;

    public SubmissionsBusinessService(
        IRandomProvider random,
        IOptions<SubmissionExecutionConfig> submissionExecutionConfigAccessor,
        IExecutionContextBuilderService executionContextBuilder,
        ISubmissionsValidationService submissionsValidation,
        ISubmissionExecutor submissionExecutor,
        IMapper mapper)
    {
        this.executionConfig = submissionExecutionConfigAccessor.Value;
        this.random = random;
        this.executionContextBuilder = executionContextBuilder;
        this.submissionsValidation = submissionsValidation;
        this.submissionExecutor = submissionExecutor;
        this.mapper = mapper;
    }

    public Task<ExecutionResultServiceModel> ExecuteSubmission(SubmissionServiceModel submission, CancellationToken cancellationToken = default)
    {
        this.submissionsValidation
            .GetValidationResult(submission)
            .VerifyResult();

        switch (submission.ExecutionType)
        {
            case ExecutionType.SimpleExecution:
                return this.InternalExecuteSubmission<SimpleInputModel, OutputResult>(
                    submission,
                    cancellationToken);

            case ExecutionType.TestsExecution:
                return this.InternalExecuteSubmission<TestsInputModel, TestResult>(
                    submission,
                    cancellationToken);

            default:
                throw new ArgumentException("Submission execution type not valid");
        }
    }

    private static void PreprocessLineEndings(SubmissionServiceModel submission)
    {
        if (submission.TestsExecutionDetails != null && submission.ExecutionOptions.EscapeLineEndings)
        {
            submission.TestsExecutionDetails.Tests = submission.TestsExecutionDetails.Tests
                .Mutate(tc =>
                {
                    tc.Input = tc.Input
                        .Replace("\r\n", "\n")
                        .Replace("\\r\\n", "\\n");
                    tc.Output = tc.Output
                        .Replace("\r\n", "\n")
                        .Replace("\\r\\n", "\\n");
                })
                .ToList();
        }
    }

    // This is done to assure no information can be extracted from the networks tab when returning hidden tests in the response
    private static void RemoveDetailsForHiddenTests(TaskResultServiceModel taskResult)
        => Enumerable.Where<TestResult>(taskResult.TestResults.MapCollection<TestResult>(), t => !t.IsTrialTest)
            .ForEach(t => t.CheckerDetails = new CheckerDetails());

    private static void PreprocessSubmission(SubmissionServiceModel submission)
    {
        if (submission.TestsExecutionDetails != null && submission.ExecutionOptions.EscapeTests)
        {
            submission.TestsExecutionDetails.Tests = submission.TestsExecutionDetails.Tests
                .Mutate(x => x.Input = x.Input
                    .Replace("\\[", "[")
                    .Replace("\\]", "]")
                    .Replace("\\<", "<")
                    .Replace("\\>", ">")
                    .Replace("\\{", "{")
                    .Replace("\\}", "}")
                    .Replace("\\\\$", "$")
                    .Replace("\\$", "$")
                    .Replace("\\`", "`")
                    .Replace("\\#", "#")
                    .Replace("\\|", "|"))
                .ToList();
        }
    }

    private async Task<ExecutionResultServiceModel> InternalExecuteSubmission<TInput, TResult>(
        SubmissionServiceModel submission,
        CancellationToken cancellationToken)
        where TResult : ISingleCodeRunResult, new()
    {
        PreprocessSubmission(submission);
        PreprocessLineEndings(submission);

        var ojsSubmission = this.executionContextBuilder.BuildOjsSubmission<TInput>(submission);

        var ojsWorkerExecutionResult = await this.submissionExecutor.Execute<TInput, TResult>(ojsSubmission, cancellationToken);

        var executionResult = this.mapper.Map<ExecutionResultServiceModel>(ojsWorkerExecutionResult);

        var taskMaxPoints = submission.TestsExecutionDetails?.MaxPoints ?? ServiceConstants.CodeExecutionContext.TaskDefaultMaxPoints;

        if (executionResult.TaskResult == null)
        {
            this.ProcessSimpleExecutionResult(executionResult);
        }
        else
        {
            this.ProcessTaskResult(submission, executionResult, taskMaxPoints);
        }

        return executionResult;
    }

    private void ProcessSimpleExecutionResult(ExecutionResultServiceModel executionResult)
        => executionResult.OutputResult
            ?.LimitLength(this.executionConfig.OutputResultMaxLength);

    private void ProcessTaskResult(
        SubmissionServiceModel submission,
        ExecutionResultServiceModel executionResult,
        int taskMaxPoints)
    {
        executionResult.Id = this.BuildUniqueId(submission.TestsExecutionDetails!.TaskId!);

        executionResult.TaskResult!.CalculatePoints(taskMaxPoints);

        if (!submission.ExecutionOptions.KeepDetails)
        {
            RemoveDetailsForHiddenTests(executionResult.TaskResult!);
        }
    }

    private string BuildUniqueId(string prefix)
        => $"{prefix}_{this.random.GetRandomString()}";
}