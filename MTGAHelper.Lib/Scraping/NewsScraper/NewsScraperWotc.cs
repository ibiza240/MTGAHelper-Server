using HtmlAgilityPack;
using MTGAHelper.Lib.Config.News;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace MTGAHelper.Lib.Scraping.NewsScraper
{
    public class NewsScraperWotc : NewsScraperBase
    {
        protected override string UrlSiteRoot => "https://magic.wizards.com";
        protected override string UrlSiteNews => $"{UrlSiteRoot}/en/news/archive";
        protected override string Type => "Wizards of the Coast";

        public NewsScraperWotc(Util util)
            : base(util)
        {
        }

        public override ICollection<ConfigModelNews> GetNews()
        {
            var result = new List<ConfigModelNews>();

            //var handler = new HttpClientHandler();
            //handler.AutomaticDecompression = ~DecompressionMethods.All;
            //using (var httpClient = new HttpClient(handler))
            //{
            //    using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://cdn.contentful.com/spaces/s5n2t79q9icq/environments/master/entries?content_type=article&locale=en&order=-fields.publishedDateTime&links_to_entry=&fields.title%5Bmatch%5D=&fields.category=&limit=50&skip=0&fields.restrictedLocales%5Bne%5D=English %28en%29&select=sys.id%2Cfields.category%2Cfields.tags%2Cfields.metaImage%2Cfields.title%2Cfields.slug%2Cfields.authors%2Cfields.excerpt%2Cfields.metaTitle%2Cfields.metaDescription%2Csys.type"))
            //    {
            //        request.Headers.TryAddWithoutValidation("authority", "cdn.contentful.com");
            //        request.Headers.TryAddWithoutValidation("accept", "application/json, text/plain, */*");
            //        request.Headers.TryAddWithoutValidation("accept-language", "en-US,en;q=0.9,fr;q=0.8");
            //        request.Headers.TryAddWithoutValidation("authorization", "Bearer CPET-V_EFhnj_qi1lfps9BH3Se6V1B_bxE1J1VYi7qo");
            //        request.Headers.TryAddWithoutValidation("origin", "https://magic.wizards.com");
            //        request.Headers.TryAddWithoutValidation("referer", "https://magic.wizards.com/");
            //        request.Headers.TryAddWithoutValidation("sec-ch-ua", "\"Google Chrome\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24\"");
            //        request.Headers.TryAddWithoutValidation("sec-ch-ua-mobile", "?0");
            //        request.Headers.TryAddWithoutValidation("sec-ch-ua-platform", "\"Windows\"");
            //        request.Headers.TryAddWithoutValidation("sec-fetch-dest", "empty");
            //        request.Headers.TryAddWithoutValidation("sec-fetch-mode", "cors");
            //        request.Headers.TryAddWithoutValidation("sec-fetch-site", "cross-site");
            //        request.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36");
            //        request.Headers.TryAddWithoutValidation("x-contentful-user-agent", "sdk contentful.js/0.0.0-determined-by-semantic-release; platform browser; os Windows;");

            //        var response = httpClient.Send(request);
            //        var responseContent = response.Content.ReadAsStringAsync().Result;
            //        var root = JsonConvert.DeserializeObject<Root>(responseContent);
            //    }
            //}

            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(UrlSiteNews);

            var articles = doc.DocumentNode.SelectNodes("//div[@class='articles-listing']/div");
            foreach (var a in articles.Reverse())
            {
                var divText = a.SelectSingleNode("a/div[@class='text']");
                var title = WebUtility.HtmlDecode(divText.SelectSingleNode("div[@class='title']/h3").InnerText).Trim();

                if (IsArticleForArena(title))
                {
                    var url = UrlSiteRoot + a.SelectSingleNode("a").Attributes["href"].Value;
                    var style = WebUtility.HtmlDecode(a.SelectSingleNode("a/div[@class='image']").Attributes["style"].Value);
                    var imgUrl = new Regex(@"background-image: url\('?(.*?)'?\)").Match(style).Groups[1].Value;
                    var id = util.To32BitFnv1aHash(url).ToString();
                    var description = "";// WebUtility.HtmlDecode(divText.SelectSingleNode("div[@class='description']").InnerText).Trim();

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
    }
}