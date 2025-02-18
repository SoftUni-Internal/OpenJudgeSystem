namespace OJS.Servers.Worker.Consumers;

using MassTransit;
using Microsoft.Extensions.Logging;
using OJS.Services.Common;
using OJS.Services.Worker.Business;
using OJS.Services.Infrastructure.Extensions;
using System;
using System.Threading.Tasks;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Common.Extensions;
using OJS.Services.Common.Models.Submissions;
using OJS.Services.Common.Models.Submissions.ExecutionContext;
using OJS.Services.Infrastructure;
using OJS.Services.Infrastructure.Constants;
using OJS.Workers.Common.Exceptions;
using OJS.Workers.Common.Models;
using ConfigurationException = OJS.Workers.Common.Exceptions.ConfigurationException;

public class SubmissionsForProcessingConsumer : IConsumer<SubmissionForProcessingPubSubModel>
{
    private readonly ISubmissionsBusinessService submissionsBusiness;
    private readonly IPublisherService publisher;
    private readonly IHostInfoService hostInfoService;
    private readonly ILogger<SubmissionsForProcessingConsumer> logger;
    private readonly IDatesService dates;

    public SubmissionsForProcessingConsumer(
        ISubmissionsBusinessService submissionsBusiness,
        IPublisherService publisher,
        IHostInfoService hostInfoService,
        ILogger<SubmissionsForProcessingConsumer> logger,
        IDatesService dates)
    {
        this.submissionsBusiness = submissionsBusiness;
        this.publisher = publisher;
        this.hostInfoService = hostInfoService;
        this.logger = logger;
        this.dates = dates;
    }

    public async Task Consume(ConsumeContext<SubmissionForProcessingPubSubModel> context)
    {
        var startedExecutionOn = this.dates.GetUtcNowOffset();
        var workerName = this.hostInfoService.GetHostIp();

        this.logger.LogStartingProcessingSubmission(context.Message.Id, workerName);

        var submissionStartedProcessingPubSubModel = new SubmissionStartedProcessingPubSubModel
        {
            SubmissionId = context.Message.Id,
            ProcessingStartedAt = startedExecutionOn,
        };

        await this.publisher.Publish(submissionStartedProcessingPubSubModel);

        var result = new ProcessedSubmissionPubSubModel(context.Message.Id)
        {
            WorkerName = workerName,
        };

        try
        {
            var submission = context.Message.Map<SubmissionServiceModel>();
            this.logger.LogExecutingSubmission(submission.Id, submission.TrimDetails());
            var executionResult = await this.submissionsBusiness.ExecuteSubmission(submission);
            this.logger.LogProducedExecutionResult(submission.Id, executionResult);

            result.SetExecutionResult(executionResult.Map<ExecutionResultServiceModel>());
        }
        catch (Exception exception)
        {
            var exceptionType = exception switch
            {
                StrategyException => ExceptionType.Strategy,
                SolutionException => ExceptionType.Solution,
                ConfigurationException => ExceptionType.Configuration,
                _ => ExceptionType.Other
            };

            this.logger.LogErrorProcessingSubmission(context.Message.Id, result.WorkerName, exception);
            result.SetException(exception, true, exceptionType);
        }

        finally
        {
            result.SetStartedAndCompletedExecutionOn(startedExecutionOn.UtcDateTime, completedExecutionOn: DateTime.UtcNow);
        }

        await this.publisher.Publish(result);
        this.logger.LogPublishedProcessedSubmission(context.Message.Id, result.WorkerName);
    }
}