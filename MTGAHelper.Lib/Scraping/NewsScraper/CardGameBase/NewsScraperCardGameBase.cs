using HtmlAgilityPack;
using MTGAHelper.Lib.Config.News;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace MTGAHelper.Lib.Scraping.NewsScraper.CardGameBase
{
    public class NewsScraperCardGameBase : NewsScraperBase
    {
        protected override string UrlSiteRoot => "https://cardgamebase.com";
        protected override string UrlSiteNews => $"{UrlSiteRoot}/tag/mtg/";
        protected override string Type => "Card Game Base";

        public NewsScraperCardGameBase(Util util)
            : base(util)
        {
        }

        public override ICollection<ConfigModelNews> GetNews()
        {
            var result = new List<ConfigModelNews>();

            using (var client = new HttpClient())
            {
                ScrapeNews(result, client.GetStringAsync(UrlSiteNews).Result);
            }

            return result;
        }

        private void ScrapeNews(List<ConfigModelNews> result, string response)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(response);

            var news = doc.DocumentNode.SelectNodes("//article[contains(concat(' ', normalize-space(@class), ' '), ' post ')]");
            foreach (var a in news.Reverse())
            {
                var titleUrl = a.SelectSingleNode(".//header//a");
                var url = titleUrl.Attributes["href"].Value;
                var id = util.To32BitFnv1aHash(url).ToString();

                var imgUrl = "";
                var img = a.SelectNodes(".//img").FirstOrDefault(x => x.Attributes["src"].Value.Contains("cardgamebase"));
                if (img != default)
                    imgUrl = WebUtility.HtmlDecode(img.Attributes["src"].Value);

                var title = WebUtility.HtmlDecode(titleUrl.InnerText);
                var description = WebUtility.HtmlDecode(a.SelectSingleNode(".//div[@class='entry-summary']/p[1]").InnerText);

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
}