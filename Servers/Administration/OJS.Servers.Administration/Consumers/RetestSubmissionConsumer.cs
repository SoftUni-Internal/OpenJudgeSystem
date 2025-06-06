namespace OJS.Servers.Administration.Consumers;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Servers.Infrastructure.Telemetry;
using OJS.Services.Administration.Business.Submissions;
using OJS.Services.Common.Telemetry;
using OJS.Services.Infrastructure.Constants;

public class RetestSubmissionConsumer(
    ISubmissionsBusinessService submissionsBusinessService,
    ILogger<RetestSubmissionConsumer> logger,
    ITracingService tracingService)
    : IConsumer<RetestSubmissionPubSubModel>
{
    public async Task Consume(ConsumeContext<RetestSubmissionPubSubModel> context)
        => await tracingService.TraceAsync(
            OjsActivitySources.submissions,
            OjsActivitySources.SubmissionActivities.Retest,
            async activity =>
            {
                logger.LogReceivedRetestSubmission(context.Message.Id);
                await submissionsBusinessService.Retest(context.Message.Id, context.Message.Verbosely);
                activity?.SetTag("submission.verbosely", context.Message.Verbosely);
                activity?.SetTag("retest.success", true);
                logger.LogRetestedSubmission(context.Message.Id);
            },
            tags: null,
            BusinessContext.ForSubmission(context.Message.Id),
            continueFromMessageHeaders: context);
}