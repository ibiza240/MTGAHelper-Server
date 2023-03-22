using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib.Exceptions;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using static MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish.DeckScraperMtgGoldfishBase;

namespace MTGAHelper.Lib.Scraping.DeckSources
{
    public interface IDeckScraper
    {
        string SiteUrl { get; }

        DeckScraperResult GetDecks(bool deleteFirst = false);
    }

    public abstract class DeckScraperBase : IDeckScraper
    {
        protected readonly object scraperLock = new object();

        protected readonly IMtgaTextDeckConverter converter;
        protected readonly IWriterDeck writerDeck;

        protected int counterProcessed;
        protected int nbTotal;

        protected int nbSuccess;  // Valid decks
        protected int nbWarning;  // Decks with cards not in Standard or  skipped (duplicate name, etc.)
        protected int nbError;    // Unexpected errors

        private List<string> errors = new List<string>();
        private List<string> warnings = new List<string>();

        protected List<ConfigModelDeck> decks = new List<ConfigModelDeck>();

        public abstract string SiteUrl { get; }

        protected abstract ScraperType ScraperType { get; }

        private BannedCardFormat bannedCardsFormat => ScraperType.Format == ScraperTypeFormatEnum.HistoricBo1 || ScraperType.Format == ScraperTypeFormatEnum.HistoricBo3 ? BannedCardFormat.Historic : BannedCardFormat.Standard;

        public DeckScraperBase(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter
            )
        {
            this.writerDeck = writerDeck;
            this.converter = converter;
        }

        protected void AddWarning(string message, int nbSkipped)
        {
            Log.Warning(message);
            lock (scraperLock)
            {
                warnings.Add(message);
                nbWarning += nbSkipped;
                nbTotal += nbSkipped;
            }
        }

        protected virtual ICollection<int> GetDecksSliceList()
        {
            // By default, assume that the downloader uses only 1 slice (lists all the decks at once)
            return new[] { 0 };
        }

        protected abstract ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart);

        protected virtual ICollection<DeckScraperDeckInputs> GetDeckVariants(DeckScraperDeckInputs input)
        {
            // By default, assume that the downloader uses only 1 variant per deck
            return new[] { input };
        }

        protected abstract ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart);

        public DeckScraperResult GetDecks(bool deleteFirst = false)
        {
            errors = new List<string>();
            warnings = new List<string>();
            ICollection<int> slices = Array.Empty<int>();

            try
            {
                slices = TryGetResults(() => GetDecksSliceList());
            }
            catch (DeckScraperErrorException ex)
            {
                Log.Error(ex, "Scraper {ScraperType} - Problem while trying to download decks:", ScraperType);
                throw;
            }

#if DEBUG || DEBUGWITHSERVER
            foreach (var s in slices) GetDecksForSliceLoop(s);
#else
            System.Threading.Tasks.Parallel.ForEach(slices, s => GetDecksForSliceLoop(s));
#endif

            if (slices.Count > 1)
            {
                var decksByName = decks.GroupBy(i => i.Name);
                var decksDuplicate = decksByName.Where(i => i.Count() > 1).ToArray();

                if (decksDuplicate.Any())
                {
                    var totalSkipped = 0;
                    foreach (var i in decksDuplicate)
                    {
                        var nbSkipped = i.Count() - 1;
                        totalSkipped += nbSkipped;
                        Log.Debug("{ScraperType} found {nbSame} decks with name {deckName}. {nbSkipped} skipped (keeping only the most recent)", ScraperType, i.Count(), i.Key, nbSkipped);
                    }

                    decks = decksByName
                        .Select(i => i.First())
                        .ToList();

                    nbSuccess -= totalSkipped;
                    nbWarning += totalSkipped;

                    Log.Warning("{ScraperType} skipped a total of {totalIgnored} duplicates", ScraperType, totalSkipped);
                }
            }

            Log.Information("Scraper Summary: {nbSuccess} success, {nbError} errors, {nbWarning} warnings. Total: {nbTotal}", nbSuccess, nbError, nbWarning, nbTotal);
            var res = new DeckScraperResult(nbTotal, nbSuccess, nbError + nbWarning, decks)
            {
                Errors = errors,
                Warnings = warnings,
            };

            return res;
        }

        protected T TryGetResults<T>(Func<T> getResults) where T : class
        {
            var done = false;
            var iTry = 0;
            T results = null;

            do
            {
                try
                {
                    if (iTry > 0)
                        Log.Warning("Retry #{iTry} ({method})", iTry, getResults.Method);

                    iTry++;
                    results = getResults();

                    if (results != null)
                    {
                        done = true;
                    }
                    else if (iTry >= 5)
                    {
                        throw new DeckScraperErrorException($"{ScraperType} - Tried and failed 5 times (no exception thrown)");
                    }
                }
                catch (CardMissingException ex)
                {
                    throw new DeckScraperWarningException($"{ScraperType} - CardMissingException: {ex.Message}", ex);
                }
                catch (MtgGoldfishThrottlingException)
                {
                    Log.Warning("{ScraperType} - Request throttled, waiting 2 minutes...", ScraperType);
                    System.Threading.Thread.Sleep(120000);
                }
                catch (Exception ex) when (ex is DeckScraperWarningException || ex is DeckScraperErrorException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    if (iTry >= 5)
                        throw new DeckScraperErrorException($"{ScraperType} - Tried and failed 5 times", ex);
                    else
                        Log.Error(ex, "Error TryGetResults:");
                }
            }
            while (done == false);

            return results;
        }

        private void GetDecksForSliceLoop(int sliceStart)
        {
            ICollection<DeckScraperDeckInputs> decksToDownload = TryGetResults(() => GetDeckList(sliceStart));

            lock (scraperLock)
            {
                nbTotal += decksToDownload.Count;
            }

#if DEBUG || DEBUGWITHSERVER
            foreach (var deckInput in decksToDownload) GetDeckVariantsLoop(deckInput, sliceStart);
#else
            System.Threading.Tasks.Parallel.ForEach(decksToDownload, deckInput => GetDeckVariantsLoop(deckInput, sliceStart));
#endif
        }

        private void GetDeckVariantsLoop(DeckScraperDeckInputs deckInput, int sliceStart)
        {
            try
            {
                ICollection<DeckScraperDeckInputs> variants = TryGetResults(() => GetDeckVariants(deckInput));
                nbTotal += variants.Count - 1;
                if (variants.Count > 1)
                    Log.Information("{ScraperType} found {nbVariants} variations for deck {deckName}", ScraperType, variants.Count, deckInput.Name);

#if DEBUG || DEBUGWITHSERVER
                foreach (var v in variants) GetDeckLoop(v, sliceStart);
#else
                System.Threading.Tasks.Parallel.ForEach(variants, v => GetDeckLoop(v, sliceStart));
#endif
            }
            catch (DeckScraperWarningException ex)
            {
                lock (scraperLock)
                {
                    warnings.Add(ex.Message);
                    nbWarning++;
                }
                Log.Warning(ex.Message);//, ex.Data.Values);
            }
            catch (DeckFileAlreadyExistsException ex)
            {
                lock (scraperLock)
                {
                    warnings.Add(ex.Message);
                    nbWarning++;
                }
                Log.Warning("{ScraperType} [slice {sliceStart}] {deckFileAlreadyExistsException}", ScraperType, sliceStart, ex.Message);
            }
            catch (DeckScraperErrorException ex)
            {
                lock (scraperLock)
                {
                    errors.Add(ex.Message);
                    nbError++;
                }
                Log.Error(ex, ex.Message);//, ex.Data.Values);
            }
            catch (Exception ex)
            {
                lock (scraperLock)
                {
                    errors.Add(ex.Message);
                    nbError++;
                }
                Log.Fatal(ex, ex.Message);//, ex.Data.Values);
            }
        }

        private void GetDeckLoop(DeckScraperDeckInputs v, int sliceStart)
        {
            try
            {
                ConfigModelDeck configDeck = null;

                // Download the deck if required
                if (configDeck == null)
                    configDeck = TryGetResults(() => GetDeck(v, sliceStart));

                configDeck.ScraperTypeOrderIndex = v.OrderIndex;

                lock (scraperLock)
                {
                    if (decks.Any(i => i.Id == configDeck.Id))
                        throw new DeckScraperWarningException($"{ScraperType} processed deck {v.Name} [{configDeck.Id}] already in list (skipping 1)");

                    configDeck.Cards = configDeck.Deck.Cards.All
                        .Select(i => new DeckCardRaw
                        {
                            GrpId = i.Card.GrpId,
                            Amount = i.Amount,
                            Zone = i.Zone,
                        })
                        .ToArray();

                    //if (configDeck.DateCreatedUtc == default || configDeck.DateCreatedUtc >= new DateTime(2020,10,1))

                    var mustDownload = configDeck.ScraperTypeId.Contains("aetherhub") == false || configDeck.DateCreatedUtc > new DateTime(2020, 10, 12);
                    if (mustDownload)
                    {
                        //var bannedCard = configDeck.Deck.Cards.AllExceptBasicLands.FirstOrDefault(i => bannedCards.GetBannedCards(bannedCardsFormat).Contains(i.Card.name));
                        DeckCard bannedCard = null;

                        if (bannedCard == null)
                        {
                            Log.Information("{ScraperType} processed deck {deckName} [{deckId}] successfully", ScraperType, v.Name, configDeck.Id);
                            decks.Add(configDeck);
                            nbSuccess++;
                        }
                        else
                        {
                            Log.Information("{ScraperType} processed deck {deckName} [{deckId}], ignored because it contains Banned card {bannedCard}", ScraperType, v.Name, configDeck.Id, bannedCard.Card.Name);
                            nbWarning++;
                        }
                    }
                    else
                    {
                        Log.Information("{ScraperType} processed deck {deckName} [{deckId}], ignored because it is too old ({date})", ScraperType, v.Name, configDeck.Id, configDeck.DateCreatedUtc.ToString("yyyyMMdd"));
                        nbWarning++;
                    }
                }
            }
            catch (DeckScraperWarningException ex)
            {
                lock (scraperLock)
                {
                    warnings.Add(ex.Message);
                    nbWarning++;
                }
                Log.Warning(ex.Message);//, ex.Data.Values);
            }
            catch (DeckFileAlreadyExistsException ex)
            {
                lock (scraperLock)
                {
                    warnings.Add(ex.Message);
                    nbWarning++;
                }
                Log.Warning("{ScraperType} [slice {sliceStart}] {deckFileAlreadyExistsException}", ScraperType, sliceStart, ex.Message);
            }
            catch (DeckScraperErrorException ex)
            {
                lock (scraperLock)
                {
                    errors.Add(ex.Message);
                    nbError++;
                }
                Log.Error(ex, ex.Message);//, ex.Data.Values);
            }
            catch (Exception ex)
            {
                lock (scraperLock)
                {
                    errors.Add(ex.Message);
                    nbError++;
                }
                Log.Fatal(ex, ex.Message);//, ex.Data.Values);
            }
        }

        protected string DownloadDeckString(string deckName, string deckUrl)
        {
            var ret = TryGetResults(() =>
            {
                using (WebClient w = new WebClient())
                    return w.DownloadString(deckUrl);
            });

            return ret;
        }
    }
}