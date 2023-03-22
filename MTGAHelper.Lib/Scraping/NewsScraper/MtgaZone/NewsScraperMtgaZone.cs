using HtmlAgilityPack;
using MTGAHelper.Lib.Config.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace MTGAHelper.Lib.Scraping.NewsScraper.MtgaZone
{
    public class NewsScraperMtgaZone : NewsScraperBase
    {
        protected override string UrlSiteRoot => "https://mtgazone.com";
        protected override string UrlSiteNews => $"{UrlSiteRoot}/category/news/";
        private string UrlSiteGuides => $"{UrlSiteRoot}/guides/";
        protected override string Type => "MTG Arena Zone";

        public NewsScraperMtgaZone(Util util)
            : base(util)
        {
        }

        public override ICollection<ConfigModelNews> GetNews()
        {
            var result = new List<ConfigModelNews>();

            //using (var client = new HttpClient())
            //{
            //    AddArticles(result, client.GetStringAsync(UrlSiteNews).Result);
            //    AddArticleCells(result, client.GetStringAsync(UrlSiteGuides).Result);
            //}

            using (var client = new HttpClient())
            {
                ScrapeNews(result, client.GetStringAsync(UrlSiteRoot).Result);
            }

            return result;
        }

        private void ScrapeNews(List<ConfigModelNews> result, string response)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var news = doc.DocumentNode.SelectNodes("//div[@id=\"grid-wrapper\"]//article");
            foreach (var a in news.Reverse())
            {
                var url = a.SelectSingleNode(".//a").Attributes["href"].Value;
                var id = util.To32BitFnv1aHash(url).ToString();
                var imgUrl = WebUtility.HtmlDecode(a.SelectSingleNode(".//img").Attributes["src"].Value);
                var title = WebUtility.HtmlDecode(a.SelectSingleNode(".//h2").InnerText).Trim();
                var description = WebUtility.HtmlDecode(a.SelectNodes(".//p").Last().InnerText).Trim();

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

        //private void AddArticles(List<ConfigModelNews> result, string response)
        //{
        //    // Weird because loading directly from HtmlWeb doesn't work
        //    //HtmlWeb hw = new HtmlWeb();
        //    //HtmlDocument doc = hw.Load(UrlSiteNews);
        //    var doc = new HtmlDocument();
        //    doc.LoadHtml(response);

        //    var articles = doc.DocumentNode.SelectNodes("//article");
        //    foreach (var a in articles.Reverse())
        //    {
        //        //var nodeTitle = a.SelectSingleNode("div/h2/a | div/div/h3/a");
        //        var nodeTitle = a.SelectSingleNode("div/h2/a");
        //        var title = WebUtility.HtmlDecode(nodeTitle.InnerText).Trim();

        //        if (IsArticleForArena(title))
        //        {
        //            var url = nodeTitle.Attributes["href"].Value;
        //            var imgUrl = WebUtility.HtmlDecode(a.SelectSingleNode("div//img").Attributes["src"].Value);
        //            var id = util.To32BitFnv1aHash(url).ToString();
        //            var description = "";

        //            result.Add(new ConfigModelNews
        //            {
        //                Id = id,
        //                Type = Type,
        //                Title = title,
        //                Description = description,
        //                DatePosted = DateTime.UtcNow,
        //                Url = url,
        //                ImageUrl = imgUrl
        //            });
        //        }
        //    }
        //}

        //private void AddArticleCells(List<ConfigModelNews> result, string response)
        //{
        //    // Weird because loading directly from HtmlWeb doesn't work
        //    //HtmlWeb hw = new HtmlWeb();
        //    //HtmlDocument doc = hw.Load(UrlSiteNews);
        //    var doc = new HtmlDocument();
        //    doc.LoadHtml(response);

        //    var articles = doc.DocumentNode.SelectNodes("//article/div");
        //    foreach (var a in articles.Reverse())
        //    {
        //        //var nodeTitle = a.SelectSingleNode("div/h2/a | div/div/h3/a");
        //        var nodeTitle = a.SelectSingleNode("h2/a");
        //        var title = WebUtility.HtmlDecode(nodeTitle.InnerText).Trim();

        //        if (IsArticleForArena(title))
        //        {
        //            var url = nodeTitle.Attributes["href"].Value;
        //            var imgUrl = WebUtility.HtmlDecode(a.SelectSingleNode("div/a/img").Attributes["src"].Value);
        //            var id = util.To32BitFnv1aHash(url).ToString();
        //            var description = "";

        //            result.Add(new ConfigModelNews
        //            {
        //                Id = id,
        //                Type = Type,
        //                Title = title,
        //                Description = description,
        //                DatePosted = DateTime.UtcNow,
        //                Url = url,
        //                ImageUrl = imgUrl
        //            });
        //        }
        //    }
        //}

        //protected override bool IsArticleForArenaScraperSpecific(string title)
        //{
        //    var t = title.ToLower();
        //    if (t.Contains("deck advisor") || t.Contains("patch notes") || t.Contains("state of the game")
        //        )
        //        return false;
        //    else
        //        return true;
        //}
    }
}