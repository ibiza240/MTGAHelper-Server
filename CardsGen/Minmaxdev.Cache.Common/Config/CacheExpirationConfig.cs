namespace Minmaxdev.Cache.Common.Config
{
    public interface ICacheExpirationConfig<TModel>
    {
        int ExpirationInSeconds { get; }
        int AbsoluteExpirationInSeconds { get; }
        string DefaultCacheKey { get; }
    }

    public readonly record struct CacheExpirationConfig<TModel>(
        int ExpirationInSeconds = int.MaxValue,
        int AbsoluteExpirationInSeconds = int.MaxValue
        ) : ICacheExpirationConfig<TModel>
    {
        public string DefaultCacheKey => typeof(TModel).FullName;

        public static ICacheExpirationConfig<TModel> DefaultNeverExpires()
        {
            return new CacheExpirationConfig<TModel>
            {
                AbsoluteExpirationInSeconds = int.MaxValue,
                ExpirationInSeconds = int.MaxValue,
            };
        }

        public static ICacheExpirationConfig<TModel> DefaultExpires10Seconds()
        {
            return new CacheExpirationConfig<TModel>
            {
                AbsoluteExpirationInSeconds = 60,
                ExpirationInSeconds = 10,
            };
        }
    }
}