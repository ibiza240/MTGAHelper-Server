using HtmlAgilityPack;
using MTGAHelper.Lib.AllCards.Scryfall;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

namespace MTGAHelper.Lib.Scraping.ScryfallScraper
{
    public class CardsScryfallScraper
    {
        public CardsScryfallScraper()
        {
        }

        public ICollection<ScryfallModelRootObject> Scrape(string set)
        {
            var ret = new List<ScryfallModelRootObject>();

            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load($"https://www.scryfall.com/sets/{set}?as=text&order=name");

            HtmlWeb hw2 = new HtmlWeb();

            var cards = doc.DocumentNode.SelectNodes("//div[@class='text-grid']//a");
            foreach (var c in cards)
            {
                HtmlDocument doc2 = hw.Load(c.Attributes["href"].Value);
                var jsonLink = doc2.DocumentNode
                    .SelectSingleNode("//a[@data-track='{&quot;category&quot;:&quot;Card Detail&quot;,&quot;action&quot;:&quot;Utility Link&quot;,&quot;label&quot;:&quot;Card JSON&quot;}']")
                    .Attributes["href"].Value;

                string data;
                using (var w = new WebClient())
                {
                    // Get the list of decks (JSON) and parse
                    data = w.DownloadString(jsonLink);
                }

                var card = JsonConvert.DeserializeObject<ScryfallModelRootObject>(data);
                ret.Add(card);
            }

            return ret;

            //var articles = doc.DocumentNode.SelectNodes("//article");
            //foreach (var a in articles.Reverse())
            //{
            //    var nodeTitle = a.SelectSingleNode("div/h2/a | div/div/h3/a");
            //    var title = WebUtility.HtmlDecode(nodeTitle.InnerText);

            //    if (IsArticleForArena(title))
            //    {
            //        var url = nodeTitle.Attributes["href"].Value;
            //        var imgUrl = WebUtility.HtmlDecode(a.SelectSingleNode("div/div/a/img").Attributes["src"].Value);
            //        var id = util.To32BitFnv1aHash(url);
            //        var description = WebUtility.HtmlDecode(a.SelectSingleNode("div[starts-with(@class,'postInfo')]//p[starts-with(@class,'oneupExcerpt') or starts-with(@class,'gridExcerpt')]").InnerText);

            //        //result.Add(new ConfigModelNews
            //        //{
            //        //    Id = id,
            //        //    Type = Type,
            //        //    Title = title,
            //        //    Description = description,
            //        //    DatePosted = DateTime.UtcNow,
            //        //    Url = url,
            //        //    ImageUrl = imgUrl
            //        //});
            //    }
            //}
        }
    }
}