using HtmlAgilityPack;
using MTGAHelper.Lib.Config.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace MTGAHelper.Lib.Scraping.NewsScraper.AetherHub
{
    public class NewsScraperAetherHub : NewsScraperBase
    {
        protected override string UrlSiteRoot => "https://aetherhub.com";
        protected override string UrlSiteNews => $"{UrlSiteRoot}/Article/";
        protected override string Type => "AetherHub";

        public NewsScraperAetherHub(Util util)
            : base(util)
        {
        }

        public override ICollection<ConfigModelNews> GetNews()
        {
            var result = new List<ConfigModelNews>();

            using (var client = new HttpClient())
            {
                var response = client.GetStringAsync(UrlSiteNews).Result;
                var doc = new HtmlDocument();
                doc.LoadHtml(response);

                var articles = doc.DocumentNode.SelectNodes("//div[starts-with(@class,'article-card')]");
                foreach (var a in articles.Reverse())
                {
                    var nodeTitle = a.SelectSingleNode(".//h6//a");
                    var title = WebUtility.HtmlDecode(nodeTitle.InnerText).Trim();

                    if (IsArticleForArena(a.InnerText))
                    {
                        var url = $"{UrlSiteRoot}{nodeTitle.Attributes["href"].Value}";
                        var style = WebUtility.HtmlDecode(a.SelectSingleNode(".//div[@class='article-imgheader']").Attributes["style"].Value);
                        var imgUrl = new Regex(@"background-image: url\(?(.*?)?\)").Match(style).Groups[1].Value;
                        var id = util.To32BitFnv1aHash(url).ToString();
                        var description = WebUtility.HtmlDecode(a.SelectSingleNode(".//p[@class='card-text m-1']").InnerText);

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
            }

            return result;
        }

        protected override bool IsArticleForArenaScraperSpecific(string title)
        {
            var t = title.ToLower();
            if (t.Contains("assistant") || t.Contains("andreliverod")
                )
                return false;
            else
                return true;
        }
    }
}