using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Cache.Common.AssemblyConfig;

namespace Minmaxdev.Cache.Memory.AssemblyConfig
{
    /// <summary>
    /// Helpers for defining cache handlers
    /// </summary>
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection RegisterServicesMinmaxdevCacheMemory(this IServiceCollection services)
        {
            return services
                .RegisterServicesMinmaxdevCacheCommon()
                ;
        }
    }
}