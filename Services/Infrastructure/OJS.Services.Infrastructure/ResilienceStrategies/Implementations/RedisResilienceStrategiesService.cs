namespace OJS.Services.Infrastructure.ResilienceStrategies.Implementations;

using OJS.Services.Infrastructure.Configurations;
using OJS.Services.Infrastructure.Emails;
using OJS.Services.Infrastructure.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OJS.Services.Infrastructure.Models;
using Polly;
using Polly.CircuitBreaker;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

public class RedisResilienceStrategiesService : IRedisResilienceStrategiesService
{
    private readonly int memoryCacheExpirationInSeconds = 10 * 60;

    private readonly IMemoryCache memoryCache;
    private readonly IDatesService datesService;
    private readonly IEmailService emailService;
    private readonly EmailServiceConfig emailConfig;

    public RedisResilienceStrategiesService(
        IMemoryCache memoryCache,
        IDatesService datesService,
        IEmailService emailService,
        IOptions<EmailServiceConfig> emailConfig)
    {
        this.memoryCache = memoryCache;
        this.datesService = datesService;
        this.emailService = emailService;
        this.emailConfig = emailConfig.Value;
    }

    public async Task<T> ProcessOutcome<T>(Func<Task<T>> fallback, Outcome<T> outcome)
    {
        if (outcome.Exception is BrokenCircuitException)
        {
            return await fallback();
        }

        if (outcome.Exception is RedisConnectionException or RedisCommandException)
        {
            await this.SendEmailAsync(outcome.Exception);

            // If the circuit is not yet open, we want to display an error message to the user. For example: the contest's compete / practice page.
            throw new CircuitBreakerNotOpenException("Temporary connectivity issue with the data server. The system is attempting to recover. Please try again in a few moments.");
        }

        if (outcome.Exception is not null)
        {
            throw outcome.Exception;
        }

        return outcome.Result!;
    }

    private async Task SendEmailAsync(Exception exception)
    {
        var exceptionName = exception.GetType().Name;

        if (this.ShouldSendExceptionEmail(exceptionName, exception.Message))
        {
            await this.emailService.SendEmailAsync(
                this.emailConfig.DevEmail,
                exceptionName,
                exception.Message);
        }
    }

    private bool ShouldSendExceptionEmail(string exceptionName, string exceptionValue)
    {
        var cacheKey = $"Exception_{exceptionName}";
        if (!this.memoryCache.TryGetValue(cacheKey, out ExceptionCacheEntry? cacheEntry))
        {
            cacheEntry = new ExceptionCacheEntry
            {
                Value = exceptionValue,
                LastEmailSentTime = DateTime.UtcNow,
            };

            this.memoryCache.Set(
                cacheKey,
                cacheEntry,
                this.datesService.GetAbsoluteExpirationBySeconds(this.memoryCacheExpirationInSeconds));

            return true;
        }

        if (DateTime.UtcNow - cacheEntry!.LastEmailSentTime >= TimeSpan.FromHours(1))
        {
            cacheEntry.LastEmailSentTime = DateTime.UtcNow;

            this.memoryCache.Set(
                cacheKey,
                cacheEntry,
                this.datesService.GetAbsoluteExpirationBySeconds(this.memoryCacheExpirationInSeconds));

            return true;
        }

        return false;
    }
}