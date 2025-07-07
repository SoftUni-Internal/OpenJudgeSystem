namespace OJS.Servers.Administration.Consumers;

using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using OJS.PubSub.Worker.Models.Submissions;
using OJS.Services.Administration.Business.Submissions;
using OJS.Services.Common.Telemetry;
using OJS.Services.Infrastructure.Constants;
using static OJS.Servers.Infrastructure.Telemetry.OjsActivitySources;

public class RetestSubmissionConsumer(
    ISubmissionsBusinessService submissionsBusinessService,
    ILogger<RetestSubmissionConsumer> logger,
    ITracingService tracingService)
    : IConsumer<RetestSubmissionPubSubModel>
{
    public async Task Consume(ConsumeContext<RetestSubmissionPubSubModel> context)
        => await tracingService.TraceAsync(
            submissions,
            SubmissionActivities.Retest,
            async activity =>
            {
                logger.LogReceivedRetestSubmission(context.Message.Id);
                await submissionsBusinessService.Retest(context.Message.Id, context.Message.Verbosely);
                activity?.SetTag(SubmissionTags.Verbosely, context.Message.Verbosely);
                activity?.SetTag(SubmissionTags.RetestSuccess, true);
                logger.LogRetestedSubmission(context.Message.Id);
            },
            tags: null,
            BusinessContext.ForSubmission(context.Message.Id),
            continueFromMessageHeaders: context);
}