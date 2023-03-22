using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Data.Persistence.Common.AssemblyConfig;
using Minmaxdev.Data.Persistence.File.Service;

namespace Minmaxdev.Data.Persistence.File.AssemblyConfig
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection RegisterServicesMinmaxdevDataPersistenceFile(this IServiceCollection services)
        {
            return services
                .RegisterServicesMinmaxdevDataPersistenceCommon()
                .AddSingleton(typeof(FilePersister<>))
                .AddSingleton(typeof(FilePersister<,>))
                //.AddSingleton(typeof(FilePersisterStartEmpty<>))
                //.AddSingleton(typeof(FilePersisterStartEmpty<,>))
                .AddSingleton(typeof(FileManipulator<>))
                ;
        }
    }
}