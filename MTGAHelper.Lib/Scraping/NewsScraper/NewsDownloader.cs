using MTGAHelper.Lib.Config.News;
using MTGAHelper.Lib.Scraping.NewsScraper.CardGameBase;
using MTGAHelper.Lib.Scraping.NewsScraper.MtgaZone;
using MTGAHelper.Lib.Scraping.NewsScraper.MtgGoldfish;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Lib.Scraping.NewsScraper
{
    public class NewsDownloader
    {
        private readonly ConfigManagerNews configNews;
        private readonly ICollection<INewsScraper> scrapers;
        private readonly CacheSingleton<IReadOnlyCollection<ConfigModelNews>> cache;

        public NewsDownloader(ConfigManagerNews configNews,
            CacheSingleton<IReadOnlyCollection<ConfigModelNews>> cache,
            NewsScraperMtgGoldfish newsScraperMtgGoldfish,
            NewsScraperWotc newsScraperWotc,
            //NewsScraperAetherHub newsScraperAetherHub,
            NewsScraperMtgaZone newsScraperMtgaZone,
            NewsScraperCardGameBase newsScraperCardGameBase
            )
        {
            this.configNews = configNews;
            this.cache = cache;
            scrapers = new INewsScraper[]
            {
                newsScraperMtgaZone,
                newsScraperMtgGoldfish,
                newsScraperWotc,
                //newsScraperAetherHub,
                newsScraperCardGameBase,
            };
        }

        public void UpdateNewsList()
        {
            IEnumerable<ConfigModelNews> tryManaged(Func<ICollection<ConfigModelNews>> getNews, Type type)
            {
                try
                {
                    return getNews();
                }
                catch (Exception ex)
                {
                    Log.Error("NEWS ERROR: Cannot load from source {newsSource}", type.ToString());
                    return Array.Empty<ConfigModelNews>();
                }
            }

            configNews.LoadData();
            var idsExisting = configNews.Values.Select(i => i.Id).ToArray();
            // Keep only new news
            var news = scrapers.SelectMany(i => tryManaged(i.GetNews, i.GetType()))
                .Where(i => idsExisting.Contains(i.Id) == false)
                .ToArray();

            foreach (var n in news)
                configNews.Set(n);

            configNews.Save();
            configNews.RefreshCache(cache);
        }

        public void Ignore(string[] ids, bool undo)
        {
            if (undo)
                configNews.ignored = configNews.ignored.Where(i => ids.Contains(i) == false).ToArray();
            else
                configNews.ignored = configNews.ignored.Union(ids).Distinct().ToArray();

            configNews.Save();
            configNews.RefreshCache(cache);
        }
    }
}