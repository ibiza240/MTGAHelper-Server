using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Common.Services;

namespace Minmaxdev.Common.AssemblyConfig
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection RegisterServicesMinmaxdevCommon(this IServiceCollection services)
        {
            return services
                .AddSingleton<StopwatchWrapper>();
        }
    }
}