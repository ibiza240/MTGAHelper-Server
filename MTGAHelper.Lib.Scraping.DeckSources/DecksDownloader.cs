using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Scraping.DeckSources.Factory;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Lib.Scraping.DeckSources
{
    public class TupleSectionAndDownloader
    {
        public ScraperType scraperType { get; private set; }
        public Func<DeckScraperResult> downloader { get; private set; }

        public TupleSectionAndDownloader(ScraperType scraperType, Func<DeckScraperResult> downloader)
        {
            this.scraperType = scraperType;
            this.downloader = downloader;
        }
    }

    public interface IDecksDownloader
    {
        DeckScraperResult DownloadDecks(ScraperType scraperType);

        IDecksDownloader Init(Dictionary<(ScraperTypeEnum Type, ScraperTypeFormatEnum Format), ICollection<string>> scraperUsers);

        (bool isValid, string error) Validate(string id);

        void AddDownloader(ScraperType newType);
    }

    public class DecksDownloader : IDecksDownloader
    {
        private Dictionary<ScraperTypeEnum, List<TupleSectionAndDownloader>> validValues;

        ICollection<string> allCombos =>
            // All top-level keywords (ie. mtggoldfish)
            validValues.Select(i => i.Key.ToString().ToLower())
            // All sections keywords (ie. mtggoldfish-meta)
            .Union(validValues.SelectMany(i => i.Value.Select(x => x.scraperType.Id)))
            .OrderBy(i => i)
            .ToArray();

        private readonly DeckScraperStreamDeckerFactory scraperStreamDeckerFactory;

        //DeckScraperMtgDecksAverageArchetypeFactory scraperMtgDecksAverageArchetypeFactory;
        // private DeckScraperMtgDecksMetaFullFactory scraperMtgDecksMetaFullFactory;

        private readonly DeckScraperMtgGoldfishMetaFactory scraperMtgGoldfishMetaFactory;
        private readonly DeckScraperMtgGoldfishArticleFactory scraperMtgGoldfishArticleFactory;
        private readonly DeckScraperMtgGoldfishSingleScoopFactory scraperMtgGoldfishSingleScoopFactory;
        private readonly DeckScraperMtgGoldfishTournamentFactory deckScraperMtgGoldfishTournamentFactory;
        private readonly DeckScraperAetherhubFactory scraperAetherhubFactory;
        private readonly DeckScraperMtgTop8Factory deckScraperMtgTop8Factory;
        private readonly DeckScraperMtgaToolFactory deckScraperMtgaToolFactory;
        private readonly DecksDownloaderQueueAsync downloaderQueue;

        public DecksDownloader(
            // ConfigModelApp configApp,
            DecksDownloaderQueueAsync downloaderQueue,
            DeckScraperStreamDeckerFactory scraperStreamDeckerFactory,
            //DeckScraperMtgDecksAverageArchetypeFactory scraperMtgDecksAverageArchetypeFactory,
            // DeckScraperMtgDecksMetaFullFactory scraperMtgDecksMetaFullFactory,
            DeckScraperMtgGoldfishMetaFactory scraperMtgGoldfishMetaFactory,
            DeckScraperMtgGoldfishArticleFactory scraperMtgGoldfishArticleFactory,
            DeckScraperAetherhubFactory scraperAetherhubFactory,
            DeckScraperMtgTop8Factory deckScraperMtgTop8Factory,
            DeckScraperMtgGoldfishSingleScoopFactory scraperMtgGoldfishSingleScoopFactory,
            DeckScraperMtgaToolFactory deckScraperMtgaToolFactory,
            DeckScraperMtgGoldfishTournamentFactory deckScraperMtgGoldfishTournamentFactory)
        {
            this.scraperStreamDeckerFactory = scraperStreamDeckerFactory;
            //this.scraperMtgDecksAverageArchetypeFactory = scraperMtgDecksAverageArchetypeFactory;
            // this.scraperMtgDecksMetaFullFactory = scraperMtgDecksMetaFullFactory;
            this.scraperMtgGoldfishMetaFactory = scraperMtgGoldfishMetaFactory;
            this.scraperMtgGoldfishArticleFactory = scraperMtgGoldfishArticleFactory;
            this.scraperAetherhubFactory = scraperAetherhubFactory;
            this.downloaderQueue = downloaderQueue;
            this.deckScraperMtgTop8Factory = deckScraperMtgTop8Factory;
            this.scraperMtgGoldfishSingleScoopFactory = scraperMtgGoldfishSingleScoopFactory;
            this.deckScraperMtgaToolFactory = deckScraperMtgaToolFactory;
            this.deckScraperMtgGoldfishTournamentFactory = deckScraperMtgGoldfishTournamentFactory;
        }

        public IDecksDownloader Init(Dictionary<(ScraperTypeEnum Type, ScraperTypeFormatEnum Format), ICollection<string>> scraperUsers)
        {
            validValues = new Dictionary<ScraperTypeEnum, List<TupleSectionAndDownloader>>
            {
                {  ScraperTypeEnum.MtgGoldfish, new List<TupleSectionAndDownloader>
                    {
                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.Meta.ToString().ToLower(), ScraperTypeFormatEnum.Standard),
                            () => TryDownload(() => scraperMtgGoldfishMetaFactory.Create(ScraperTypeFormatEnum.Standard).GetDecks(true))),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.Meta.ToString().ToLower(), ScraperTypeFormatEnum.HistoricBo3),
                            () => TryDownload(() => scraperMtgGoldfishMetaFactory.Create(ScraperTypeFormatEnum.HistoricBo3).GetDecks(true))),

                        //new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.Meta.ToString().ToLower(), ScraperTypeFormatEnum.ArenaStandard),
                        //    () => TryDownload(() => scraperMtgGoldfishMetaFactory.Create(ScraperTypeFormatEnum.ArenaStandard, allCards, lastSet).GetDecks(true))),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.AgainstTheOdds.ToString().ToLower()),
                            () => TryDownload(() => scraperMtgGoldfishArticleFactory.Create(MtgGoldfishArticleEnum.AgainstTheOdds).GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.BudgetMagic.ToString().ToLower()),
                            () => TryDownload(() => scraperMtgGoldfishArticleFactory.Create(MtgGoldfishArticleEnum.BudgetMagic).GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.GoldfishGladiators.ToString().ToLower()),
                            () => TryDownload(() => scraperMtgGoldfishArticleFactory.Create(MtgGoldfishArticleEnum.GoldfishGladiators).GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.InstantDeckTech.ToString().ToLower()),
                            () => TryDownload(() => scraperMtgGoldfishArticleFactory.Create(MtgGoldfishArticleEnum.InstantDeckTech).GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.FishFiveO.ToString().ToLower()),
                            () => TryDownload(() => scraperMtgGoldfishArticleFactory.Create(MtgGoldfishArticleEnum.FishFiveO).GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.StreamHighlights.ToString().ToLower()),
                            () => TryDownload(() => scraperMtgGoldfishArticleFactory.Create(MtgGoldfishArticleEnum.StreamHighlights).GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.MuchAbrew.ToString().ToLower()),
                            () => TryDownload(() => scraperMtgGoldfishArticleFactory.Create(MtgGoldfishArticleEnum.MuchAbrew).GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.BudgetArena.ToString().ToLower()),
                            () => TryDownload(() => scraperMtgGoldfishArticleFactory.Create(MtgGoldfishArticleEnum.BudgetArena).GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.SingleScoop.ToString().ToLower()),
                            () => TryDownload(() => scraperMtgGoldfishSingleScoopFactory.Create().GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.Tournaments.ToString().ToLower(), ScraperTypeFormatEnum.Standard),
                            () => TryDownload(() => deckScraperMtgGoldfishTournamentFactory.Create(ScraperTypeFormatEnum.Standard).GetDecks())),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.Tournaments.ToString().ToLower(), ScraperTypeFormatEnum.HistoricBo3),
                            () => TryDownload(() => deckScraperMtgGoldfishTournamentFactory.Create(ScraperTypeFormatEnum.HistoricBo3).GetDecks())),
                    }
                },
                //{  ScraperTypeEnum.MtgDecks, new List<TupleSectionAndDownloader>
                //    {
                //        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgDecks, "meta"), () => TryDownload(() => scraperMtgDecksMetaFullFactory.Create(allCards, lastSet).GetDecks())),
                //        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgDecks, "averagearchetype"), () => TryDownload(() => scraperMtgDecksAverageArchetypeFactory.Create(allCards, lastSet).GetDecks())),
                //    }
                //},
                {  ScraperTypeEnum.Aetherhub, new List<TupleSectionAndDownloader>
                    {
                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.Aetherhub, AetherhubListingEnum.Meta.ToString().ToLower(), ScraperTypeFormatEnum.Standard),
                            () => TryDownload(() => scraperAetherhubFactory.Create(AetherhubListingEnum.Meta, ScraperTypeFormatEnum.Standard).GetDecks(true))),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.Aetherhub, AetherhubListingEnum.Meta.ToString().ToLower(), ScraperTypeFormatEnum.ArenaStandard),
                            () => TryDownload(() => scraperAetherhubFactory.Create(AetherhubListingEnum.Meta, ScraperTypeFormatEnum.ArenaStandard).GetDecks(true))),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.Aetherhub, AetherhubListingEnum.Meta.ToString().ToLower(), ScraperTypeFormatEnum.HistoricBo1),
                            () => TryDownload(() => scraperAetherhubFactory.Create(AetherhubListingEnum.Meta, ScraperTypeFormatEnum.HistoricBo1).GetDecks(true))),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.Aetherhub, AetherhubListingEnum.Meta.ToString().ToLower(), ScraperTypeFormatEnum.HistoricBo3),
                            () => TryDownload(() => scraperAetherhubFactory.Create(AetherhubListingEnum.Meta, ScraperTypeFormatEnum.HistoricBo3).GetDecks(true))),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.Aetherhub, AetherhubListingEnum.Tier1.ToString().ToLower()),
                            () => TryDownload(() => scraperAetherhubFactory.Create(AetherhubListingEnum.Tier1, ScraperTypeFormatEnum.Unknown).GetDecks(true))),

                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.Aetherhub, AetherhubListingEnum.TournamentBo3.ToString().ToLower()),
                            () => TryDownload(() => scraperAetherhubFactory.Create(AetherhubListingEnum.TournamentBo3, ScraperTypeFormatEnum.Unknown).GetDecks(true))),
                    }
                },
                {  ScraperTypeEnum.Streamdecker, new List<TupleSectionAndDownloader>()
                },
                {  ScraperTypeEnum.MtgTop8, new List<TupleSectionAndDownloader>
                    {
                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgTop8, "deckstobeat"), () => TryDownload(() => deckScraperMtgTop8Factory.Create().GetDecks())),
                    }
                },
                {  ScraperTypeEnum.MtgaTool, new List<TupleSectionAndDownloader>
                    {
                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgaTool, "meta", ScraperTypeFormatEnum.ArenaStandard), () => TryDownload(() => deckScraperMtgaToolFactory.Create(ScraperTypeFormatEnum.ArenaStandard).GetDecks())),
                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgaTool, "meta", ScraperTypeFormatEnum.Standard), () => TryDownload(() => deckScraperMtgaToolFactory.Create(ScraperTypeFormatEnum.Standard).GetDecks())),
                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgaTool, "meta", ScraperTypeFormatEnum.HistoricBo1), () => TryDownload(() => deckScraperMtgaToolFactory.Create(ScraperTypeFormatEnum.HistoricBo1).GetDecks())),
                        new TupleSectionAndDownloader(new ScraperType(ScraperTypeEnum.MtgaTool, "meta", ScraperTypeFormatEnum.HistoricBo3), () => TryDownload(() => deckScraperMtgaToolFactory.Create(ScraperTypeFormatEnum.HistoricBo3).GetDecks())),
                    }
                },
            };

            var tuplesScraperUsers = scraperUsers
                .SelectMany(i => i.Value.Select(x => new { i.Key, Value = x }))

                //// DISABLE STREAMDECKER SCRAPING FOR NOW
                //.Where(i => i.Key.Type != ScraperTypeEnum.Streamdecker)

                .Select(d => new TupleSectionAndDownloader(
                    new ScraperType(d.Key.Type, d.Value, d.Key.Format),
                    () => TryDownload(() =>
                    {
                        switch (d.Key.Type)
                        {
                            case ScraperTypeEnum.Aetherhub:
                                return scraperAetherhubFactory.Create(AetherhubListingEnum.User, d.Key.Format, d.Value).GetDecks();

                            case ScraperTypeEnum.Streamdecker:
                                return scraperStreamDeckerFactory.Create(d.Value).GetDecks();

                            default:
                                throw new ApplicationException("Downloader by user not supported");
                        }
                    })))
                .GroupBy(i => i.scraperType.Type);

            foreach (var tuple in tuplesScraperUsers)
                validValues[tuple.Key].AddRange(tuple);

            return this;
        }

        private Func<DeckScraperResult> GetDownloader(ScraperType scraperType)
        {
            Func<DeckScraperResult> downloader = null;
            switch (scraperType.Type)
            {
                case ScraperTypeEnum.Aetherhub:
                    downloader = () => TryDownload(() => scraperAetherhubFactory.Create(AetherhubListingEnum.User, scraperType.Format, scraperType.Name).GetDecks());
                    break;
                //case ScraperTypeEnum.MtgGoldfish:
                //    downloader = (directory) => TryDownload(() => scraperMtgGoldfishArticleFactory.Create(scraperType.Name, allCards, directory).GetDecks());
                //    break;
                case ScraperTypeEnum.Streamdecker:
                    downloader = () => TryDownload(() => scraperStreamDeckerFactory.Create(scraperType.Name).GetDecks());
                    break;
            }

            return downloader;
        }

        public void AddDownloader(ScraperType newType)
        {
            if (validValues[newType.Type].Any(i => i.scraperType.Id == newType.Id) == false)
            {
                Func<DeckScraperResult> downloader = GetDownloader(newType);
                var tuple = new TupleSectionAndDownloader(newType, downloader);

                if (tuple.scraperType.Name != null)
                    validValues[newType.Type].Add(tuple);
            }
            else
            {
                Log.Warning("Tried to add an existing scraper: {ScraperId}", newType.Id);
            }
        }

        public (bool isValid, string error) Validate(string id)
        {
            var scraperType = new ScraperType(id);
            if (scraperType.IsByUser)
            {
                if (scraperType.Name != null && validValues[scraperType.Type].Any(i => i.scraperType.Id == id) == false)
                {
                    Func<DeckScraperResult> downloader = GetDownloader(scraperType);
                    var newTuple = new TupleSectionAndDownloader(scraperType, downloader);

                    validValues[scraperType.Type].Add(newTuple);
                }

                return (true, null);
            }

            var isValid = allCombos.Any(i => i == id);
            string error = isValid ? null : $"Invalid keyword '{id}'. Use one of: {string.Join(", ", allCombos)}";
            return (isValid, error);
        }

        public DeckScraperResult DownloadDecks(ScraperType scraperType)
        {
            try
            {
                var downloaders = BuildDownloaders(scraperType);
                downloaderQueue.AddRange(downloaders);

                //return Download(downloaders);
                return new DeckScraperResult();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                throw;
            }
        }

        private ICollection<TupleSectionAndDownloader> BuildDownloaders(ScraperType scraperType)
        {
            var res = new List<TupleSectionAndDownloader>();

            if (scraperType.Name == null)
            {
                // Top-level
                var downloaders = validValues[scraperType.Type];
                res.AddRange(downloaders);
            }
            else
            {
                // Section specific
                var downloader = validValues[scraperType.Type].First(i => i.scraperType.Id == scraperType.Id);
                res.Add(downloader);
            }

            return res;
        }

        private DeckScraperResult TryDownload(Func<DeckScraperResult> downloadDeck)
        {
            try
            {
                return downloadDeck();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected Error while downloading files, see exception:");
                return new DeckScraperResult();
            }
        }

        //private DeckScraperResult Download(ICollection<Task<DeckScraperResult>> DownloadDecks)
        //{
        //    ICollection<DeckScraperResult> results = new DeckScraperResult[0];

        //    Task.WhenAll(DownloadDecks)
        //        .ContinueWith((x) => results = x.Result)
        //        .Wait();

        //    int nbTotal = 0;
        //    int nbSuccess = 0;
        //    int nbIgnored = 0;
        //    var decks = new List<ConfigModelDeck>();
        //    var errors = new List<string>();
        //    var warnings = new List<string>();

        //    var problems = results.Where(i => i.Decks == null);
        //    foreach (var p in problems)
        //        Log.Error("Problem! NULL results: " + DownloadDecks.ToString());

        //    foreach (var r in results.Where(i => i.Decks != null))
        //    {
        //        nbTotal += r.NbTotal;
        //        nbSuccess += r.NbSuccess;
        //        nbIgnored += r.NbIgnored;
        //        decks.AddRange(r.Decks);

        //        errors.AddRange(r.Errors);
        //        warnings.AddRange(r.Warnings);
        //    }

        //    return new DeckScraperResult(nbTotal, nbSuccess, nbIgnored, decks)
        //    {
        //        Errors = errors,
        //        Warnings = warnings
        //    };
        //}
    }
}