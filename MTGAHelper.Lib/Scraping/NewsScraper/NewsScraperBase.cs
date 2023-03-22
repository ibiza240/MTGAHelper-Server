using MTGAHelper.Lib.Config.News;
using System.Collections.Generic;

namespace MTGAHelper.Lib.Scraping.NewsScraper
{
    public interface INewsScraper
    {
        ICollection<ConfigModelNews> GetNews();
    }

    public abstract class NewsScraperBase : INewsScraper
    {
        protected readonly Util util;

        protected abstract string UrlSiteRoot { get; }
        protected abstract string UrlSiteNews { get; }
        protected abstract string Type { get; }

        public NewsScraperBase(Util util)
        {
            this.util = util;
        }

        public abstract ICollection<ConfigModelNews> GetNews();

        protected virtual bool IsArticleForArenaScraperSpecific(string title)
        {
            return true;
        }

        protected bool IsArticleForArena(string title)
        {
            if (IsArticleForArenaScraperSpecific(title) == false)
                return false;

            var t = title.ToLower();
            if (
                // From MtgGoldfish
                t.Contains("legacy") || t.Contains("challenger") || t.Contains("modern") || t.Contains("vintage") || t.Contains("commander") || t.Contains("pioneer")
                 // From WotC
                 || t.Contains("magic online") || t.Contains("cube")
                 || t.Contains("edh")
                 ////////////////
                 || t.Contains("hearthstone")
                )
                return false;
            else
                return true;
        }
    }
}