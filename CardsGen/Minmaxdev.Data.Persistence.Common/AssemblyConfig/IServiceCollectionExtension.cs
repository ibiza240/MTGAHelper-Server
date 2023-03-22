using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Data.Persistence.Common.Service;

namespace Minmaxdev.Data.Persistence.Common.AssemblyConfig
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection RegisterServicesMinmaxdevDataPersistenceCommon(this IServiceCollection services)
        {
            return services
                .AddSingleton<DocumentPersisterDependencies>();
        }
    }
}