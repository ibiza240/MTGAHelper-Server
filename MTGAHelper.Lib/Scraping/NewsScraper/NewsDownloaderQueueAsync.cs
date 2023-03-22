using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Lib.CacheLoaders;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.Config.News;
using MTGAHelper.Lib.Scraping.CalendarScraper;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MTGAHelper.Lib.Scraping.NewsScraper
{
    public class NewsDownloaderQueueAsync
    {
        private readonly IDataPath dataPath;
        private readonly NewsDownloader newsDownloader;
        private readonly CacheSingleton<IReadOnlyCollection<ConfigModelNews>> cache;
        private readonly CalendarScraperMtgaAssistant calendarScraperMtgaAssistant;
        private readonly CacheSingleton<IReadOnlyCollection<ConfigModelCalendarItem>> cacheCalendar;
        private readonly CacheCalendarImageBinder cacheCalendarImageBinder;

        public NewsDownloaderQueueAsync(
            IDataPath dataPath,
            NewsDownloader newsDownloader,
            CacheSingleton<IReadOnlyCollection<ConfigModelNews>> cache,
            CalendarScraperMtgaAssistant calendarScraperMtgaAssistant,
            CacheSingleton<IReadOnlyCollection<ConfigModelCalendarItem>> cacheCalendar,
            CacheCalendarImageBinder cacheCalendarImageBinder
            )
        {
            this.dataPath = dataPath;
            this.newsDownloader = newsDownloader;
            this.cache = cache;
            this.calendarScraperMtgaAssistant = calendarScraperMtgaAssistant;
            this.cacheCalendar = cacheCalendar;
            this.cacheCalendarImageBinder = cacheCalendarImageBinder;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await ContinuallyTryToUpdateNews(cancellationToken);
        }

        private Task ContinuallyTryToUpdateNews(CancellationToken cancellationToken)
        {
            Task task = null;

            // Start a task and return it
            task = Task.Run(async () =>
           {
               while (true)
               {
                   try
                   {
                       Log.Information("Updating news");
                       newsDownloader.UpdateNewsList();
                       Log.Information("Updated news, count: {nbNews}", cache.Get().Count);

                       // TODO: FIX SCRAPER
                       //// Also update the calendar...
                       //await UpdateCalendar();
                   }
                   catch (Exception ex)
                   {
                       Log.Error(ex, "Unexpected error in thread for DownloaderQueueAsync:");
                   }

                   // Update news again in 30 minutes
                   Thread.Sleep(30 * 60 * 1000);
               }
           });

            return task;
        }

        private async Task UpdateCalendar()
        {
            // Retrieve latest calendar
            Log.Information("Getting calendar");
            var calendar = await calendarScraperMtgaAssistant.GetCalendar();

            // Merge new calendar
            var latestCache = cacheCalendar.Get()
                .Union(calendar.Where(i => cacheCalendar.Get().Any(x => x.Title == i.Title && x.DateRange.Split(" to ")[0] == i.DateRange.Split(" to ")[0]) == false))
                .ToArray();

            // Persist
            var s = JsonConvert.SerializeObject(latestCache);
            File.WriteAllText(Path.Combine(dataPath.FolderData, "calendar.json"), s);

            // Update in-memory cache
            cacheCalendarImageBinder.Reload();
        }
    }
}