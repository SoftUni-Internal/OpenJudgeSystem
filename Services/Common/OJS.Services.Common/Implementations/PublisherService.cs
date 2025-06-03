namespace OJS.Services.Common.Implementations;

using System.Threading.Tasks;
using System.Collections.Generic;
using MassTransit;
using System;
using System.Threading;
using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

public class PublisherService(IPublishEndpoint publishEndpoint) : IPublisherService
{
    private const int DefaultTimeoutMilliseconds = 3000;
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public async Task Publish<T>(T obj, CancellationToken? cancellationToken = null)
        where T : class
    {
        var token = cancellationToken ?? new CancellationTokenSource(DefaultTimeoutMilliseconds).Token;

        await publishEndpoint.Publish(obj, InjectTraceContext, token);
    }

    /// <summary>
    /// Injects current trace context into message headers using a MessageProperties approach.
    /// </summary>
    private static void InjectTraceContext<T>(PublishContext<T> context) where T : class
    {
        var currentActivity = Activity.Current;
        if (currentActivity == null)
        {
            return;
        }

        // Get the ActivityContext to inject
        var activityContext = currentActivity.Context;
        var contextToInject = activityContext != default ? activityContext : Activity.Current?.Context ?? default;

        // Inject the ActivityContext into the message headers using OpenTelemetry propagator
        var propagationContext = new PropagationContext(contextToInject, Baggage.Current);
        Propagator.Inject(propagationContext, context, InjectTraceContextToMessageProperties);
        return;

        static void InjectTraceContextToMessageProperties(PublishContext<T> messageProperties, string key, string value)
        {
            messageProperties.Headers.Set(key, value);
        }
    }

    public async Task PublishBatch<T>(IReadOnlyCollection<T> objs, CancellationToken? cancellationToken = null)
        where T : class
    {
        // Calculate timeout based on batch size
        var objectsCountTimeoutMultiplier = (int)Math.Min(10, objs.Count * 0.1);
        var timeoutMultiplier = Math.Max(1, objectsCountTimeoutMultiplier);
        var token = cancellationToken ?? new CancellationTokenSource(DefaultTimeoutMilliseconds * timeoutMultiplier).Token;

        await publishEndpoint.PublishBatch(objs, InjectTraceContext, token);
    }
}