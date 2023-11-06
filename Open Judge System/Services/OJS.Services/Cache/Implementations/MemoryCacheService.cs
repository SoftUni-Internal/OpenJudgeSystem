using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace OJS.Services.Cache.Implementations
{
    public class MemoryCacheService : IMemoryCacheService
    {
        public T Get<T>(string cacheId, Func<T> getItemCallback)
        {
            if (HttpRuntime.Cache.Get(cacheId) is T item)
            {
                return item;
            }

            item = getItemCallback();

            this.Set(cacheId, item);

            return item;
        }

        public async Task<T> GetAsync<T>(string cacheId, Func<Task<T>> getItemCallback)
        {
            if (HttpRuntime.Cache.Get(cacheId) is T item)
            {
                return item;
            }

            item = await getItemCallback();

            await this.SetAsync(cacheId, item);

            return item;
        }

        public void Set<T>(string cacheId, T value)
        {
            SetCache<T>(cacheId, value);
        }

        public void Set<T>(string cacheId, T value, TimeSpan expiration)
        {
            var absoluteExpiration = DateTime.Now + expiration;
            SetCache(cacheId, value, absoluteExpiration);
        }

        public async Task SetAsync<T>(string cacheId, T value)
        {
            await Task.FromResult(HttpContext.Current.Cache.Add(
                cacheId,
                value,
                null,
                DateTime.MaxValue,
                TimeSpan.Zero,
                CacheItemPriority.Default,
                null));
        }

        public async Task SetAsync<T>(string cacheId, T value, TimeSpan expiration)
        {
            await Task.FromResult(HttpContext.Current.Cache.Add(
                cacheId,
                value,
                null,
                DateTime.Now + expiration,
                TimeSpan.Zero,
                CacheItemPriority.Default,
                null));
        }

        public T GetOrSet<T>(string cacheId, Func<T> getItemCallback)
        {
            if (HttpRuntime.Cache.Get(cacheId) is T item)
            {
                return item;
            }

            item = getItemCallback();

            this.Set(cacheId, item);

            return item;
        }

        public T GetOrSet<T>(string cacheId, Func<T> getItemCallback, TimeSpan expiration)
        {
            if (HttpRuntime.Cache.Get(cacheId) is T item)
            {
                return item;
            }

            item = getItemCallback();

            this.Set(cacheId, item, expiration);

            return item;
        }

        public async Task<T> GetOrSetAsync<T>(string cacheId, Func<Task<T>> getItemCallback)
        {
            if (HttpRuntime.Cache.Get(cacheId) is T item)
            {
                return item;
            }

            item = await getItemCallback();

            await this.SetAsync(cacheId, item);

            return item;
        }

        public async Task<T> GetOrSetAsync<T>(string cacheId, Func<Task<T>> getItemCallback, TimeSpan expiration)
        {
            if (HttpRuntime.Cache.Get(cacheId) is T item)
            {
                return item;
            }

            item = await getItemCallback();

            await this.SetAsync(cacheId, item, expiration);

            return item;
        }

        public void Remove(string cacheId) =>
            HttpRuntime.Cache.Remove(cacheId);

        public async Task RemoveAsync(string cacheId) =>
            await Task.FromResult(HttpRuntime.Cache.Remove(cacheId));

        public bool ContainsKey(string cacheId)
        {
            if (HttpRuntime.Cache[cacheId] != null)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> ContainsKeyAsync(string cacheId)
        {
            if (await Task.FromResult(HttpRuntime.Cache[cacheId] != null))
            {
                return true;
            }

            return false;
        }

        private static void SetCache<T>(string cacheId, T item, DateTime absoluteExpiration)
            => HttpContext.Current.Cache.Add(
                cacheId,
                item,
                null,
                absoluteExpiration,
                TimeSpan.Zero,
                CacheItemPriority.Default,
                null);

        private static void SetCache<T>(string cacheId, T item)
            => HttpContext.Current.Cache.Add(
                cacheId,
                item,
                null,
                DateTime.MaxValue,
                TimeSpan.Zero,
                CacheItemPriority.Default,
                null);
    }
}