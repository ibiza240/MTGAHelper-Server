using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Cache.Common.Config;
using Minmaxdev.Cache.Memory.AssemblyConfig;
using Minmaxdev.Common.AssemblyConfig;
using Minmaxdev.Data.Persistence.File.AssemblyConfig;
using Minmaxdev.DataHandling;
using MtgaDecksPro.Cards.Data.Handling.Entity;
using MtgaDecksPro.Cards.Data.Handling.Service;
using MtgaDecksPro.Cards.Entity;
using MtgaDecksPro.Cards.Entity.Config;
using System.Collections.Generic;

namespace MtgaDecksPro.Cards.Data.Handling.AssemblyConfig
{
    public static partial class IServiceCollectionExtension
    {
        public static IServiceCollection RegisterServicesCardsDataHandling(this IServiceCollection services)
        {
            return services
                .RegisterServicesMinmaxdevCommon()
                .RegisterServicesMinmaxdevCacheMemory()
                .RegisterServicesMinmaxdevDataPersistenceFile()
                .AddAutoMapper(typeof(MapperProfileCardsDataHandling))
                .AddCacheObjects()
                ;
        }

        private static IServiceCollection AddCacheObjects(this IServiceCollection services)
        {
            return services

                // Simple handlers with 1 to 1 mapping from file content to cached data

                .AddCacheHandler_CacheMemory_DocumentFile<List<SetScryfall>, ConfigFolderDataCards>("sets.json")
                .AddCacheHandler_CacheMemory_DocumentFile<List<HistoricAnthologyCards>, ConfigFolderDataCards>("manual", "historic-anthologies.json")
                .AddCacheHandler_CacheMemory_DocumentFile<SetsManual, ConfigFolderDataCards>("manual", "sets-manual.json")

                // Handlers with complex document loading logic requiring a specific DocumentPersister

                .AddCacheHandler_CacheMemory_CustomDocumentPersister<SetsInfo, DocumentPersisterSetsInfo>()

                // Handlers that require a specific mapping between the file model and cached model

                .AddCacheHandler_CacheMemory_DocumentFile<CardsById, List<Card>, ConfigFolderDataCards>("cards.json")
                .WithCacheExpirationConfig(CacheExpirationConfig<CardsById>.DefaultNeverExpires())

                .AddCacheHandler_CacheMemory_DocumentFile<FormatsAndBans, FormatsFileModel, ConfigFolderDataCards>("manual", "formats.json")

                // Derived data built in-memory

                .AddCacheHandler_CacheMemory_Derived<CardsByName, MemoryPersisterCardsByName>()
                .AddCacheHandler_CacheMemory_Derived<CardSingleByName, MemoryPersisterPersisterCardSingleByName>()
                ;
        }

        public static IServiceCollection WithCacheExpirationConfig<TModel>(this IServiceCollection services, ICacheExpirationConfig<TModel> config)
        {
            return services
                .AddSingleton(config);
        }
    }
}