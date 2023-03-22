using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Cache.Common.Model;

namespace Minmaxdev.Cache.Common.AssemblyConfig
{
    public static partial class IServiceCollectionExtension
    {
        public static IServiceCollection RegisterServicesMinmaxdevCacheCommon(this IServiceCollection services)
        {
            return services
                .AddSingleton<ConfigFolderNone>()
                .AddSingleton<ConfigFolder>()
                ;
        }
    }
}