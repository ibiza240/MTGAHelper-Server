using Microsoft.Extensions.DependencyInjection;
using Minmaxdev.Cache.Common.Config;
using Minmaxdev.Data.Persistence.Common.Service;
using Minmaxdev.Data.Persistence.File.Service;
using Minmaxdev.DataHandling;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Mtga;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Entity.Scryfall;
using MtgaDecksPro.Cards.BootstrapCardsBuilding.Service;
using MtgaDecksPro.Cards.Data.Handling.AssemblyConfig;
using MtgaDecksPro.Cards.Entity.AssemblyConfig;
using MtgaDecksPro.Cards.Entity.Config;
using System.Collections.Generic;

namespace MtgaDecksPro.Cards.BootstrapCardsBuilding.AssemblyConfig
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection RegisterServicesBootstrapCardsBuilding(this IServiceCollection services)
        {
            return services
                .AddAutoMapper(typeof(MapperProfileCardsBuilding))

                .RegisterServicesCardsDataHandling()
                .RegisterServicesCardsEntity()

                .AddCacheHandler_CacheMemory_DocumentFile<List<ScryfallModelRootObjectExtended>, ConfigFolderDataCards>("cardsbuilder", "scryfall-default-cards.json")
                .AddCacheHandler_CacheMemory_DocumentFile<ScryfallSetRootObject, ConfigFolderDataCards>("cardsbuilder", "scryfall-sets.json")

                .AddSingleton<IDocumentPersister<List<MtgaDataCardsRootObjectExtended>>>(provider =>
                    provider.GetService<FilePersister<List<MtgaDataCardsRootObjectExtended>>>().WithPathParts(provider.GetService<IConfigFolderDataCards>().FolderDataCards, "cardsbuilder", "Raw_cards.mtga"))
                .AddSingleton<DocumentPersisterConfiguration<List<MtgaDataCardsRootObjectExtended>>>()
                .AddSingleton<FilePersisterConfiguration<List<MtgaDataCardsRootObjectExtended>>>()

                .AddSingleton<IDocumentPersister<List<MtgaDataLocRootObject>>>(provider =>
                    provider.GetService<FilePersister<List<MtgaDataLocRootObject>>>().WithPathParts(provider.GetService<IConfigFolderDataCards>().FolderDataCards, "cardsbuilder", "data_loc.mtga"))
                .AddSingleton<DocumentPersisterConfiguration<List<MtgaDataLocRootObject>>>()
                .AddSingleton<FilePersisterConfiguration<List<MtgaDataLocRootObject>>>()

                .AddSingleton<ReaderMtgaDataCards>()
                .AddSingleton<CardsBuilder>()
                .AddSingleton<SetsBuilder>()
                .AddSingleton<ScryfallSeeker>()
                .AddSingleton<MemoryCacheManualData>()
                .AddSingleton(CacheExpirationConfig<CacheManualDataModel>.DefaultNeverExpires())
                ;
        }
    }
}