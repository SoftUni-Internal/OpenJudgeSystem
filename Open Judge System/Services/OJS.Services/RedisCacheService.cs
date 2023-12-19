using Newtonsoft.Json;
using OJS.Services.Cache;
using OJS.Services.Common.Emails;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using OJS.Common.Constants;

namespace OJS.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase redisCache;
        private readonly double memoryCacheExpirationInMinutes = 60;
        private readonly IEmailSenderService emailSenderService;
        private readonly string devEmail;
        private readonly IMemoryCacheService memoryCacheService;

        public RedisCacheService(IDatabase redisCache,
            IEmailSenderService emailSenderService,
            string devEmail, IMemoryCacheService memoryCacheService)
        {
            this.redisCache = redisCache;
            this.emailSenderService = emailSenderService;
            this.devEmail = devEmail;
            this.memoryCacheService = memoryCacheService;
        }

        public T Get<T>(string cacheId, Func<T> getItemCallback) =>
            GetItemResult(
                () =>
                {
                    var valueAsString = this.redisCache.StringGet(this.AddKeyPrefix(cacheId));

                    if (valueAsString.IsNull)
                    {
                        return getItemCallback();
                    }

                    return JsonConvert.DeserializeObject<T>(valueAsString);
                },
                getItemCallback);

        public async Task<T> GetAsync<T>(string cacheId, Func<Task<T>> getItemCallback) =>
            await GetItemResultAsync(
                async () =>
                {
                    var valueAsString = await this.redisCache.StringGetAsync(this.AddKeyPrefix(cacheId));
                    if (valueAsString.IsNull)
                    {
                        return await getItemCallback();
                    }

                    return JsonConvert.DeserializeObject<T>(valueAsString);
                },
                getItemCallback);

        public T GetOrSet<T>(string cacheId, Func<T> getItemCallback, TimeSpan expiration) =>
            GetItemResult(
                () =>
                {
                    this.VerifyValueInCache(cacheId, getItemCallback, expiration);
                    
                    return this.Get(cacheId, getItemCallback);
                },
                getItemCallback);

        public T GetOrSet<T>(string cacheId, Func<T> getItemCallback) =>
            GetItemResult(
                () =>
                {
                    this.VerifyValueInCache(cacheId, getItemCallback, null);
                    
                    return this.Get(cacheId, getItemCallback);
                },
                getItemCallback);

        public async Task<T> GetOrSetAsync<T>(string cacheId, Func<Task<T>> getItemCallback, TimeSpan expiration) =>
            await GetItemResultAsync(
                async () =>
                {
                    await this.VerifyValueInCacheAsync(cacheId, getItemCallback, expiration);
                    
                    return await this.GetAsync(cacheId, getItemCallback);
                },
                getItemCallback);

        public async Task<T> GetOrSetAsync<T>(string cacheId, Func<Task<T>> getItemCallback) =>
            await GetItemResultAsync(
                async () =>
                {
                    await this.VerifyValueInCacheAsync(cacheId, getItemCallback, null);
                    
                    return await this.GetAsync(cacheId, getItemCallback);
                },
                getItemCallback);

        public void Set<T>(string cacheId, T value, TimeSpan expiration)
        {
            try
            {
                string serializedObject = JsonConvert.SerializeObject(value);
                this.redisCache.StringSet(this.AddKeyPrefix(cacheId), serializedObject, expiration);
            }
            catch (RedisConnectionException ex)
            {
                this.SendEmail(ex.GetType().ToString(), ex.Message);
            }
        }

        public void Set<T>(string cacheId, T value)
        {
            try
            {
                string serializedObject = JsonConvert.SerializeObject(value);
                this.redisCache.StringSet(this.AddKeyPrefix(cacheId), serializedObject);
            }
            catch (RedisConnectionException ex)
            {
                this.SendEmail(ex.GetType().ToString(), ex.Message);
            }
        }

        public async Task SetAsync<T>(string cacheId, T value, TimeSpan expiration)
        {
            try
            {
                string serializedObject = JsonConvert.SerializeObject(value);
                await this.redisCache.StringSetAsync(this.AddKeyPrefix(cacheId), serializedObject, expiration);
            }
            catch (RedisConnectionException ex)
            {
                await this.SendEmailAsync(ex.GetType().ToString(), ex.Message);
            }
        }

        public async Task SetAsync<T>(string cacheId, T value)
        {
            try
            {
                string serializedObject = JsonConvert.SerializeObject(value);
                await this.redisCache.StringSetAsync(this.AddKeyPrefix(cacheId), serializedObject);
            }
            catch (RedisConnectionException ex)
            {
                await this.SendEmailAsync(ex.GetType().ToString(), ex.Message);
            }
        }

        public void Remove(string cacheId)
        {
            try
            {
                this.redisCache.KeyDelete(this.AddKeyPrefix(cacheId));
            }
            catch (RedisConnectionException ex)
            {
                this.SendEmail(ex.GetType().ToString(), ex.Message);
            }
        }

        public async Task RemoveAsync(string cacheId)
        {
            try
            {
                await this.redisCache.KeyDeleteAsync(this.AddKeyPrefix(cacheId));
            }
            catch (RedisConnectionException ex)
            {
                await this.SendEmailAsync(ex.GetType().ToString(), ex.Message);
            }
        }

        public bool ContainsKey(string cacheId) => this.redisCache.KeyExists(this.AddKeyPrefix(cacheId));

        public async Task<bool> ContainsKeyAsync(string cacheId)
        {
            return await this.redisCache.KeyExistsAsync(this.AddKeyPrefix(cacheId));
        }

        #region private

        private async Task VerifyValueInCacheAsync<T>(
            string cacheId,
            Func<Task<T>> getItemCallback,
            TimeSpan? expiration)
        {
            if (await this.redisCache.KeyExistsAsync(cacheId))
            {
                return;
            }

            var result = await getItemCallback();

            if (expiration != null)
            {
                await this.SetAsync(cacheId, result, expiration.Value);
            }
            else
            {
                await this.SetAsync(cacheId, result);
            }
        }

        private void VerifyValueInCache<T>(
            string cacheId,
            Func<T> getItemCallback,
            TimeSpan? expiration)
        {
            if (this.redisCache.KeyExists(this.AddKeyPrefix(cacheId)))
            {
                return;
            }

            var result = getItemCallback();

            if (expiration != null)
            {
                this.Set(cacheId, result, expiration.Value);
            }
            else
            {
                this.Set(cacheId, result);
            }
        }

        private bool ShouldSendEmail(string key, string value)
        {
            if (this.memoryCacheService.ContainsKey(key))
            {
                return false;
            }

            this.memoryCacheService.Set(key, value, TimeSpan.FromMinutes(memoryCacheExpirationInMinutes));
            return true;
        }

        private void SendEmail(string exTypeAsString, string exMessage)
        {
            if (this.ShouldSendEmail(exTypeAsString, exMessage))
            {
                this.emailSenderService.SendEmail(this.devEmail, exTypeAsString, exMessage);
            }
        }

        private async Task SendEmailAsync(string exTypeAsString, string exMessage)
        {
            if (this.ShouldSendEmail(exTypeAsString, exMessage))
            {
                await this.emailSenderService.SendEmailAsync(this.devEmail, exTypeAsString, exMessage);
            }
        }

        private T GetItemResult<T>(Func<T> resultAction, Func<T> fallbackResultAction)
        {
            if (!this.RedisIsConnected())
            {
                SendEmail("RedisConnection", "Redis is not connected");
                return fallbackResultAction();
            }

            try
            {
                return resultAction();
            }
            catch (RedisConnectionException ex)
            {
                this.SendEmail(ex.GetType().ToString(), ex.Message);
                return fallbackResultAction();
            }
        }

        private async Task<T> GetItemResultAsync<T>(Func<Task<T>> resultAction, Func<Task<T>> fallbackResultAction)
        {
            if (!this.RedisIsConnected())
            {
                await SendEmailAsync("RedisConnection", "Redis is not connected");
                return await fallbackResultAction();
            }

            try
            {
                return await resultAction();
            }
            catch (RedisConnectionException ex)
            {
                await this.SendEmailAsync(ex.GetType().ToString(), ex.Message);
                return await fallbackResultAction();
            }
        }

        private bool RedisIsConnected() =>
            !redisCache.Multiplexer.IsConnecting && redisCache.Multiplexer.IsConnected;

        private string AddKeyPrefix(string key)
        {
            return $"{CacheConstants.KeysPrefix}{key}";
        }

    }
    
    #endregion
}