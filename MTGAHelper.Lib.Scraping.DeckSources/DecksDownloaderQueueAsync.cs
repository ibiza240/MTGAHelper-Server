using MTGAHelper.Lib.Config.Decks;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.Scraping.DeckSources
{
    public class DecksDownloaderQueueAsync
    {
        private readonly ConfigManagerDecks configDecks;

        private readonly object lockQueue = new object();
        private readonly Queue<TupleSectionAndDownloader> downloaders = new Queue<TupleSectionAndDownloader>();

        public ICollection<string> IdsInQueue { get { lock (lockQueue) return downloaders.Select(i => i.scraperType.Id).ToArray(); } }

        public DecksDownloaderQueueAsync(ConfigManagerDecks configDecks)
        {
            this.configDecks = configDecks;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await ContinuallyTryToDownloadWhatIsInQueue(cancellationToken);
            //Console.WriteLine("Result {0}", result);
        }

        private Task ContinuallyTryToDownloadWhatIsInQueue(CancellationToken cancellationToken)
        {
            Task task = null;

            // Start a task and return it
            task = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested == true)
                            throw new TaskCanceledException(task);

                        bool mustDownload;
                        lock (lockQueue)
                            mustDownload = downloaders.Any();

                        if (mustDownload)
                        {
                            TupleSectionAndDownloader d = null;
                            lock (lockQueue)
                                d = downloaders.Peek();

                            //var dir = Path.Combine(configApp.FolderDataDecks, ConfigModelDeck.SOURCE_SYSTEM);
                            var result = d.downloader();
                            Log.Information("{scraperType} results: {nbSuccess} success, {nbIgnored} ignored, {nbTotal} total",
                                d.scraperType.Id, result.NbSuccess, result.NbIgnored, result.NbTotal, result.Decks.Count);

                            int remaining = 0;
                            lock (lockQueue)
                                remaining = downloaders.Count - 1;

                            Log.Information("Download queue: {nbDownloaders} more requests queued", remaining);

                            var decksGrouped = result.Decks.GroupBy(i => i.Deck.Id);
                            var decksSameNameToKeep = decksGrouped.Select(i => i.First()).ToArray();
                            var decksSameNameToFilter = decksGrouped.Select(i => i.Skip(1)).SelectMany(i => i).ToArray();
                            if (decksSameNameToFilter.Length > 0)
                            {
                                Log.Warning("Removing {nbDecks} decks downloaded from {scraperType}. Keeping only 1 of each ({nbTypes})",
                                    decksSameNameToFilter.Length, d.scraperType, decksSameNameToKeep.Length);
                            }

                            var dateThreshold = DateTime.Now.AddMonths(-4);
                            var decksToKeep = decksSameNameToKeep
                                .Where(i => i.DateCreatedUtc == DateTime.MinValue || i.DateCreatedUtc >= dateThreshold)
                                .ToArray();
                            var nbDecksTooOld = decksSameNameToKeep.Length - decksToKeep.Length;

                            Log.Warning("Removing {nbDecks} decks downloaded because too old.", nbDecksTooOld);

                            configDecks.AddDecks(decksToKeep);

                            if (remaining == 0)
                            {
                                //container.ReloadMasterData(true);
                                configDecks.ReloadDecks();
                            }

                            lock (lockQueue)
                                downloaders.Dequeue();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Unexpected error in thread for DownloaderQueueAsync:");
                    }

                    Thread.Sleep(1000);
                }
            });

            return task;
        }

        internal void AddRange(ICollection<TupleSectionAndDownloader> downloaders)
        {
            lock (lockQueue)
                foreach (var d in downloaders)
                    this.downloaders.Enqueue(d);
        }
    }
}