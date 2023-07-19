﻿using OJS.Services.Common;
using System;
using System.Threading.Tasks;

namespace OJS.Services.Cache
{
    public interface IRedisCacheService
    {
        T Get<T>(string cacheId, Func<T> getItemCallback);

        Task<T> GetAsync<T>(string cacheId, Func<Task<T>> getItemCallback);

        void Set<T>(string cacheId, T value, TimeSpan? expiration);

        Task SetAsync<T>(string cacheId, T value, TimeSpan? expiration);

        T GetOrSet<T>(string cacheId, Func<T> getItemCallback, TimeSpan? expiration = null);

        Task<T> GetOrSetAsync<T>(string cacheId, Func<Task<T>> getItemCallback, TimeSpan? expiration);

        void Remove(string cacheId);

        Task RemoveAsync(string cacheId);

        bool ContainsKey(string cacheId);

        Task<bool> ContainsKeyAsync(string cacheId);
    }
}
