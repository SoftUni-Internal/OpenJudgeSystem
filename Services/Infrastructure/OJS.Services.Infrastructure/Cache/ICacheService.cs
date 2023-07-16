﻿namespace OJS.Services.Infrastructure.Cache
{
    using System;
    using System.Threading.Tasks;

    public interface ICacheService
    {
        T Get<T>(string cacheId, Func<T> getItemCallback);

        Task<T> Get<T>(string cacheId, Func<Task<T>> getItemCallback);

        T Get<T>(string cacheId, Func<T> getItemCallback, DateTime absoluteExpiration);

        T Get<T>(string cacheId, Func<T> getItemCallback, int cacheSeconds);

        Task<T> Get<T>(string cacheId, Func<Task<T>> getItemCallback, DateTime absoluteExpiration);

        Task<T> Get<T>(string cacheId, Func<Task<T>> getItemCallback, int cacheSeconds);

        void Remove(string cacheId);
    }
}