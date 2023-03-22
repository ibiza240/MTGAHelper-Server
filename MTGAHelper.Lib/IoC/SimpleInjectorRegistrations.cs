using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Lib.AllCards;
using MTGAHelper.Lib.AllCards.Scryfall;
using MTGAHelper.Lib.Analyzers.Archetypes;
using MTGAHelper.Lib.Analyzers.Cards;
using MTGAHelper.Lib.CacheLoaders;
using MTGAHelper.Lib.CollectionDecksCompare;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.Config.Articles;
using MTGAHelper.Lib.Config.Decks;
using MTGAHelper.Lib.Config.News;
using MTGAHelper.Lib.Config.Users;
using MTGAHelper.Lib.DraftBoostersCriticalPoint;
using MTGAHelper.Lib.InventoryUpdatedConverters;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.IO.Writer.WriterDeckTypes;
using MTGAHelper.Lib.JsonFixing;
using MTGAHelper.Lib.MasteryPass;
using MTGAHelper.Lib.Matches;
using MTGAHelper.Lib.MtgaDeckStats;
using MTGAHelper.Lib.Scraping.CalendarScraper;
using MTGAHelper.Lib.Scraping.NewsScraper;
using MTGAHelper.Lib.Scraping.NewsScraper.AetherHub;
using MTGAHelper.Lib.Scraping.NewsScraper.CardGameBase;
using MTGAHelper.Lib.Scraping.NewsScraper.MtgaZone;
using MTGAHelper.Lib.Scraping.NewsScraper.MtgGoldfish;
using MTGAHelper.Lib.TextDeck;
using MTGAHelper.Lib.UserHistory;
using MTGAHelper.Lib.UserStats;
using MTGAHelper.Lib.WebTesterConcurrent;
using MTGAHelper.Server.DataAccess;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using MTGAHelper.Server.DataAccess.Persist;
using SimpleInjector;
using System.Collections.Generic;

namespace MTGAHelper.Lib.IoC
{
    public static class SimpleInjectorRegistrations
    {
        public static Container RegisterServicesLib(this Container container)
        {
            container.RegisterServicesShared();

            container.RegisterSingleton<CacheDataGlobal<GetSeasonAndRankDetailRaw>>();
            //--------------------------------------------------------
            //container.RegisterSingleton<CacheUserHistoryGeneric<CachedDatesAvailable>>();
            //container.Register<UserHistoryRepositoryGeneric<CachedDatesAvailable>>();
            RegisterRepositories(container);

            ///////////
            ///// Singletons
            //container.RegisterSingleton<IUtilLib, UtilLib>();
            container.RegisterSingleton<ICacheLoader<IReadOnlyDictionary<string, AccountModel>>, CacheLoaderAccounts>();
            container.RegisterSingleton<ICacheLoader<IReadOnlySet<string>>, CacheLoaderMembers>();
            container.RegisterSingleton<CacheLoaderCalendar>();
            container.Register<CacheCalendarImageBinder>();
            container.RegisterSingleton<ICacheLoader<IReadOnlyCollection<ConfigModelCalendarItem>>, CacheLoaderCalendar>();
            container.RegisterSingleton<IClearUserCache, CompositeClearUserCache>();
            container.RegisterSingleton<ILogResultPersister, LogResultPersister>();
            container.RegisterSingleton<ActiveUserCounter>();
            //container.RegisterSingleton<ICollection<Set>>(() => container.GetInstance<CacheSingleton<IReadOnlyDictionary<int, Set>>>().Get().Values);
            container.Collection.Append<AutoMapper.Profile, MapperProfileLib>(Lifestyle.Singleton);

            //container.RegisterSingleton<AllCardsBuilder2>();
            container.RegisterSingleton<ReaderScryfallCards>();
            container.RegisterSingleton<ReaderMtgaDataCards2>();
            ///////////
            ///// Transient
            container.Register<EconomyReportGenerator>();
            container.Register<ISessionContainer, SessionContainer>();
            //container.Register<ReaderMtgaDataCards>();
            //container.Register<ReaderMtgaDataLoc>();
            //container.Register<AllCardsBuilder>();
            //container.Register<AllDecksLoader>();
            container.Register<UserHistoryLoader>();
            container.Register<UserHistoryLatestInventoryBuilder>();
            container.Register<IWriterDeck, WriterDeck>();
            container.Register<NewsDownloaderQueueAsync>();
            //container.Register<IWriterDeckAverageArchetype, WriterDeckAverageArchetype>();
            container.Register<IWriterDeckMtga, WriterDeckMtga>();
            container.Register<CardToCollectionMatcher>();
            container.Register<NewsDownloader>();
            container.Register<NewsScraperMtgGoldfish>();
            container.Register<NewsScraperWotc>();
            container.Register<NewsScraperAetherHub>();
            container.Register<NewsScraperMtgaZone>();
            container.Register<NewsScraperCardGameBase>();
            container.Register<PastDraftRetriever>();
            container.Register<IMtgaTextDeckConverter, MtgaTextDeckConverter>();
            //container.Register<IMtgoTextDeckConverter, MtgoTextDeckConverter>();
            container.Register<DraftRatingsSourceManager>();
            // ReaderMtgaOutputLog
            //container.Register<IReaderMtgaOutputLogOld, ReaderMtgaOutputLogOld>();
            //
            container.Register<ICardsMissingComparer, CardsMissingComparer>();
            //container.Register<UserHistoryCollector>();
            container.Register<UserManager>();
            container.Register<UserHistoryParser>();
            container.Register<ReaderUserHistoryInfo>();
            container.Register<LandsPreferenceManager>();
            container.Register<Tester>();
            container.Register<DecksAnalyzer>();
            //container.Register<DraftRatingsLoader>();
            container.Register<MatchesReader>();
            //container.Register<MatchesCacheManager>();
            container.Register<MtgaDeckSummaryBuilder>();
            container.Register<MtgaDeckDetailBuilder>();
            container.Register<MtgaDeckAnalysisBuilder>();
            container.Register<MtgaDeckLimitedResultsBuilder>();
            container.Register<ArchetypeIdentifier>();
            container.Register<MasteryPassContainer>();
            container.Register<MasteryPassCalculator>();
            container.Register<BoosterOpenConverter>();
            container.Register<CraftedCardsConverter>();
            container.Register<UserHistorySummaryBuilder>();
            container.Register<RankDeltaCalculator>();
            container.Register<DraftBoostersCriticalPointCalculator>();
            container.Register<JsonValidator>();
            container.Register<MassJsonFileFixer>();
            container.Register<UserCollectionFetcher>();
            container.Register<ConfigManagerCustomDraftRatings>();
            container.Register<JumpstartCollectionComparer>();
            container.Register<ConfigUserCleaner>();
            container.Register<DecksFinderByCards>();
            container.Register<CalendarScraperMtgaAssistant>();

            return container;
        }

        public static Container RegisterRepositories(this Container container)
        {
            container.RegisterSingleton(typeof(UserHistoryRepositoryGeneric<>), typeof(UserHistoryRepositoryGeneric<>));
            container.RegisterSingleton<UserHistoryDatesAvailable>();
            container.RegisterSingleton<UserMtgaDeckRepository>();

            container.RegisterSingleton(typeof(CacheUserHistoryOld<>), typeof(CacheUserHistoryOld<>));
            container.RegisterSingleton(typeof(UserHistoryRepositoryOld<>), typeof(UserHistoryRepositoryOld<>));

            //--------------------------------------------------------
            container.RegisterSingleton(typeof(CacheUserHistory<>), typeof(CacheUserHistory<>));
            container.RegisterSingleton(typeof(UserHistoryRepository<>), typeof(UserHistoryRepository<>));
            //--------------------------------------------------------

            container.RegisterSingleton<StatsLimitedRepository>();

            container.RegisterSingleton(typeof(IQueryHandler<,>), typeof(IQueryHandler<,>).Assembly);

            return container;
        }

        public static Container RegisterConfigLib(this Container container, ConfigModelApp config)
        {
            container.RegisterInstance(config);
            container.RegisterInstance<IAccountPath>(config);
            container.RegisterInstance<IConfigUsersPath>(config);
            container.RegisterInstance<IDataPath>(config);

            container.RegisterSingleton<IConfigManagerUsers, ConfigManagerUsers>();
            container.RegisterSingleton<ConfigManagerDecks>();
            container.RegisterSingleton<ConfigManagerNews>();
            container.RegisterSingleton<ICacheLoader<IReadOnlyCollection<ConfigModelNews>>, ConfigManagerNews>();
            container.RegisterSingleton<ConfigManagerArticles>();
            //container.RegisterSingleton<ICacheLoader<IReadOnlyCollection<ConfigModelArticle>>, ConfigManagerArticles>();

            return container;
        }
    }
}