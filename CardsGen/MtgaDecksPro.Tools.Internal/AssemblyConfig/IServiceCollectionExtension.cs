using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Data.Persistence.File.AssemblyConfig;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.AssemblyConfig;
using MtgaDecksPro.Cards.Entity.Config;
using MtgaDecksPro.Tools.Internal.Service;
using Serilog;

namespace MtgaDecksPro.Tools.Internal.AssemblyConfig
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection RegisterServicesToolsInternal(this IServiceCollection services)
        {
            return services
                .AddMemoryCache()
                .AddHttpClient()

                .RegisterServicesBootstrapCardsBuilding()
                .RegisterServicesMinmaxdevDataPersistenceFile()

                .AddSingleton<ILogger>(new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    //.MinimumLevel.Debug()
                    .CreateLogger()
                )

                .AddSingleton<ChoiceDispatcher>()
                .AddSingleton<CommandBuildSetsAndCards>()
                .AddSingleton<CommandDownloadMtgaAssets>()
                .AddSingleton<CommandDownloadScryfallCards>()

                .AddSingleton<IConfigFolderDataCards>(i => new ConfigFolderDataCards { FolderDataCards = @"..\..\..\..\cards" })
                .AddSingleton(new ConfigFolderDataCards { FolderDataCards = @"..\..\..\..\cards" })
                ;
        }
    }
}