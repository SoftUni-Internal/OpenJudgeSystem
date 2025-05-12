namespace OJS.Servers.Worker.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OJS.Services.Common;
using OJS.Services.Worker.Business;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Extensions;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Constants;
using OJS.Services.Worker.Models.Configuration;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Helpers;
using OJS.Workers.Common.Models;
using ConfigurationException = OJS.Workers.Common.Exceptions.ConfigurationException;

public class SubmissionsForProcessingConsumer(
    ISubmissionsBusinessService submissionsBusiness,
    IPublisherService publisher,
    IHostInfoService hostInfoService,
    ILogger<SubmissionsForProcessingConsumer> logger,
    IDatesService dates,
    IOptions<SubmissionExecutionConfig> executionConfigAccessor)
    : IConsumer<SubmissionForProcessingPubSubModel>
{
    private readonly SubmissionExecutionConfig executionConfig = executionConfigAccessor.Value;

    public async Task Consume(ConsumeContext<SubmissionForProcessingPubSubModel> context)
    {
        var startedExecutionOn = dates.GetUtcNowOffset();
        var workerName = hostInfoService.GetHostIp();

        logger.LogStartingProcessingSubmission(context.Message.Id, workerName);

        var submissionStartedProcessingPubSubModel = new SubmissionStartedProcessingPubSubModel
        {
            SubmissionId = context.Message.Id,
            ProcessingStartedAt = startedExecutionOn,
        };

        await publisher.Publish(submissionStartedProcessingPubSubModel);

        var result = new ProcessedSubmissionPubSubModel(context.Message.Id)
        {
            WorkerName = workerName,
        };

        try
        {
            var submission = context.Message.Map<SubmissionServiceModel>();
            var asJson = JsonSerializer.Serialize(submission);
            logger.LogExecutingSubmission(submission.Id, submission.TrimDetails());
            var executionResult = await submissionsBusiness.ExecuteSubmission(submission);
            logger.LogProducedExecutionResult(submission.Id, executionResult);

            result.SetExecutionResult(executionResult);
        }
        catch (Exception exception)
        {
            var exceptionType = exception switch
            {
                StrategyException => ExceptionType.Strategy,
                SolutionException => ExceptionType.Solution,
                ConfigurationException => ExceptionType.Configuration,
                _ => ExceptionType.Other,
            };

            logger.LogErrorProcessingSubmission(context.Message.Id, result.WorkerName, exception);
            result.SetException(exception, true, exceptionType);
        }
        finally
        {
            result.SetStartedAndCompletedExecutionOn(startedExecutionOn.UtcDateTime, completedExecutionOn: DateTime.UtcNow);

            if (context.Message.Verbosely)
            {
                // If the submission is marked as verbose, try to read the log file and attach it to the result
                var logFilePath = FileHelpers.BuildSubmissionLogFilePath(context.Message.Id);
                if (FileHelpers.FileExists(logFilePath))
                {
                    result.VerboseLogFile = await FileHelpers.ReadFileUpToBytes(logFilePath, this.executionConfig.SubmissionVerboseLogFileMaxBytes);
                    FileHelpers.DeleteFile(logFilePath);
                }
            }
        }

        await publisher.Publish(result);
        logger.LogPublishedProcessedSubmission(context.Message.Id, result.WorkerName);
    }
}