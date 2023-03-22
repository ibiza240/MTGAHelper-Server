using MTGAHelper.Lib.Scraping.DeckSources.Aetherhub;
using MTGAHelper.Lib.Scraping.DeckSources.Factory;
using MTGAHelper.Lib.Scraping.DeckSources.MtgaTool;
using MTGAHelper.Lib.Scraping.DeckSources.MtgDecks;
using MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish;
using MTGAHelper.Lib.Scraping.DeckSources.MtgTop8;
using MTGAHelper.Lib.Scraping.DeckSources.Streamdecker;
using SimpleInjector;

namespace MTGAHelper.Lib.Scraping.DeckSources.IoC
{
    public static class SimpleInjectorRegistrations
    {
        public static Container RegisterServicesDeckScrapers(this Container container)
        {
            container.RegisterSingleton<DecksDownloaderQueueAsync>();
            container.RegisterSingleton<IDateDeconstructor, DateDeconstructor>();

            container.Register<DeckScraperStreamDeckerFactory>();
            container.Register<DeckScraperMtgGoldfishMetaFactory>();
            container.Register<DeckScraperMtgGoldfishSingleScoopFactory>();
            container.Register<DeckScraperMtgGoldfishArticleFactory>();
            //container.Register<DeckScraperMtgDecksAverageArchetypeFactory>();
            container.Register<DeckScraperMtgDecksMetaFullFactory>();
            container.Register<DeckScraperAetherhubFactory>();
            container.Register<DeckScraperMtgTop8Factory>();
            container.Register<DeckScraperMtgaToolFactory>();
            container.Register<DeckScraperMtgGoldfishTournamentFactory>();
            container.Register<DeckScraperAetherhubByUrl>();
            container.Register<DeckScraperMtgGoldfishByUrl>();
            container.Register<DeckScraperMtgTop8DecksToBeat>();
            container.Register<DeckScraperMtgaTool>();
            container.Register<IDeckScraperStreamDecker, DeckScraperStreamDecker>();
            container.Register<IDeckScraperMtgDecksMetaFull, DeckScraperMtgDecksMetaFull>();
            //container.Register<IDeckScraperMtgDecksAverageArchetype, DeckScraperMtgDecksAverageArchetype>();
            container.Register<IDeckScraperMtgGoldfishMeta, DeckScraperMtgGoldfishMeta>();
            container.Register<IDeckScraperMtgGoldfishArticle, DeckScraperMtgGoldfishArticle>();
            container.Register<DeckScraperMtgGoldfishTournament>();
            container.Register<DeckScraperMtgGoldfishSingleScoop>();
            container.Register<DeckScraperAetherhubTierOne>();
            container.Register<DeckScraperAetherhubMetaPaper>();
            container.Register<DeckScraperAetherhubUserDecks>();
            container.Register<DeckScraperAetherhubTournamentBo3>();
            container.Register<IDecksDownloader, DecksDownloader>();

            return container;
        }
    }
}