namespace OJS.Services.Infrastructure.BackgroundJobs.Implementations
{
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using OJS.Common.Enumerations;
    using System;
    using System.Linq.Expressions;

    public class HangfireBackgroundJobsService : IHangfireBackgroundJobsService
    {
        public object AddFireAndForgetJob<T>(
            Expression<Action<T>> methodCall,
            string queueName)
        {
            var client = new BackgroundJobClient();
            var job = Job.FromExpression(methodCall);

            return client.Create(job, new EnqueuedState(queueName));
        }

        public void AddOrUpdateRecurringJob(
            object recurringJobId,
            Expression<Action> methodCall,
            string cronExpression,
            string queue)
            => RecurringJob.AddOrUpdate((string)recurringJobId, queue.ToLowerInvariant(), methodCall, cronExpression);

        public void AddOrUpdateRecurringJob<T>(
            object recurringJobId,
            Expression<Action<T>> methodCall,
            string cronExpression,
            string queue)
            => RecurringJob.AddOrUpdate((string)recurringJobId, queue.ToLowerInvariant(), methodCall, cronExpression);

        public void OnSucceededStateContinueWith<T>(
            string parentJobId,
            Expression<Action<T>> methodCall)
            => BackgroundJob.ContinueJobWith(parentJobId, methodCall, JobContinuationOptions.OnlyOnSucceededState);
    }
}