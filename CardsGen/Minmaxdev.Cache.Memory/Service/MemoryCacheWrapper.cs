using Microsoft.Extensions.Caching.Memory;
using Minmaxdev.Cache.Common;
using Minmaxdev.Cache.Common.Config;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Minmaxdev.Cache.Memory.Service
{
    public class MemoryCacheWrapper<TModel> : ICacheWrapper<TModel>
    {
        private readonly ILogger logger;
        private readonly IMemoryCache cache;
        private readonly ICacheExpirationConfig<TModel> expirationConfig;

        public MemoryCacheWrapper(
            ILogger logger,
            IMemoryCache cache,
            ICacheExpirationConfig<TModel> expirationConfig
            )
        {
            this.logger = logger;
            this.cache = cache;
            this.expirationConfig = expirationConfig;
        }

        public Task<TModel> Get()
        {
            return Task.FromResult(cache.Get<TModel>(expirationConfig.DefaultCacheKey));
        }

        public bool TryGet(out TModel data)
        {
            var isSuccess = cache.TryGetValue(expirationConfig.DefaultCacheKey, out data);
            return isSuccess;
        }

        public void Set(TModel data)
        {
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(expirationConfig.ExpirationInSeconds),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expirationConfig.AbsoluteExpirationInSeconds),
            };

            options.RegisterPostEvictionCallback(PostEvictionCallback);

            cache.Set(expirationConfig.DefaultCacheKey, data, options);
        }

        private void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
        {
            logger.Debug($"Removed from cache - key:{key as string} reason:{reason}");
        }

        public async Task ForceExpire()
        {
            cache.Remove(expirationConfig.DefaultCacheKey);
        }
    }
}