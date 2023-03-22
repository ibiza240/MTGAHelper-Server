using Microsoft.Extensions.Caching.Memory;
using Minmaxdev.Cache.Common;
using Minmaxdev.Cache.Common.Config;
using Minmaxdev.Common;
using System;

namespace Minmaxdev.Cache.Memory.Service
{
    public class MemoryCacheDictionaryWrapper<TModel> : ICacheDictionaryWrapper<TModel> where TModel : IId
    {
        private readonly IMemoryCache cache;
        private readonly ICacheExpirationConfig<TModel> expirationConfig;

        private string FullKey(Guid id) => $"{expirationConfig.DefaultCacheKey}_{id}";

        public MemoryCacheDictionaryWrapper(
            IMemoryCache cache,
            ICacheExpirationConfig<TModel> expirationConfig
            )
        {
            this.cache = cache;
            this.expirationConfig = expirationConfig;
        }

        public TModel Get(Guid id)
        {
            return cache.Get<TModel>(FullKey(id));
        }

        public bool TryGet(Guid id, out TModel data)
        {
            var isSuccess = cache.TryGetValue(FullKey(id), out data);
            return isSuccess;
        }

        public void Set(TModel data)
        {
            cache.Set(FullKey(data.Id), data, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(expirationConfig.ExpirationInSeconds),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationConfig.AbsoluteExpirationInSeconds),
            });
        }
    }
}