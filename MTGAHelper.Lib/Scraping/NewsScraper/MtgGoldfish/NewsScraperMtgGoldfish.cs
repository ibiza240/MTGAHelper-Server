using HtmlAgilityPack;
using MTGAHelper.Lib.Config.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MTGAHelper.Lib.Scraping.NewsScraper.MtgGoldfish
{
    public class NewsScraperMtgGoldfish : NewsScraperBase
    {
        protected override string UrlSiteRoot => "https://www.mtggoldfish.com";
        protected override string UrlSiteNews => $"{UrlSiteRoot}/articles";
        protected override string Type => "MtgGoldfish";

        public NewsScraperMtgGoldfish(Util util)
            : base(util)
        {
        }

        public override ICollection<ConfigModelNews> GetNews()
        {
            var result = new List<ConfigModelNews>();
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(UrlSiteNews);

            var articles = doc.DocumentNode.SelectNodes("//div[@class='articles-container']//div[@class='article-tile']");
            foreach (var a in articles.Reverse())
            {
                var nodeTitle = a.SelectSingleNode("div/div[@class='article-tile-title']/a");
                var title = WebUtility.HtmlDecode(nodeTitle.InnerText).Trim();

                if (IsArticleForArena(title))
                {
                    var url = UrlSiteRoot + nodeTitle.Attributes["href"].Value;
                    var img = a.SelectSingleNode(".//img");
                    var imgUrl = img.Attributes["src"].Value;
                    var id = util.To32BitFnv1aHash(url).ToString();
                    var description = WebUtility.HtmlDecode(a.SelectSingleNode("div/p").InnerText).Trim();

                    result.Add(new ConfigModelNews
                    {
                        Id = id,
                        Type = Type,
                        Title = title,
                        Description = description,
                        DatePosted = DateTime.UtcNow,
                        Url = url,
                        ImageUrl = imgUrl
                    });
                }
            }

            return result;
        }

        protected override bool IsArticleForArenaScraperSpecific(string title)
        {
            var t = title.ToLower();
            if (t.Contains("the fish tank")
                )
                return false;
            else
                return true;
        }
    }
}