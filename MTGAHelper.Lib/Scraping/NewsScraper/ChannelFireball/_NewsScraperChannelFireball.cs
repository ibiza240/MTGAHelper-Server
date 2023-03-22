//using HtmlAgilityPack;
//using MTGAHelper.Entity;
//using MTGAHelper.Lib.Config;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text.RegularExpressions;

//namespace MTGAHelper.Lib.NewsScraper.MtgGoldfish
//{
//    public class NewsScraperChannelFireball : NewsScraperBase
//    {
//        protected override string UrlSiteRoot => "https://www.channelfireball.com";
//        protected override string UrlSiteNews => $"{UrlSiteRoot}/category/articles";
//        protected override string Type => "ChannelFireball";

//        public NewsScraperChannelFireball()
//            : base()
//        {
//        }

//        public override ICollection<ConfigModelNews> GetNews()
//        {
//            var result = new List<ConfigModelNews>();
//            HtmlWeb hw = new HtmlWeb();
//            HtmlDocument doc = hw.Load(UrlSiteNews);

//            var articles = doc.DocumentNode.SelectNodes("//article");
//            foreach (var a in articles.Reverse())
//            {
//                //var nodeTitle = a.SelectSingleNode("div/h2/a | div/div/h3/a");
//                var nodeTitle = a.SelectSingleNode("//header/h2/a");
//                var title = WebUtility.HtmlDecode(nodeTitle.InnerText).Trim();

//                if (IsArticleForArena(title))
//                {
//                    var url = nodeTitle.Attributes["href"].Value;
//                    var imgUrl = WebUtility.HtmlDecode(a.SelectSingleNode("//div[starts-with(@class,'post-thumb-img-content')]//img").Attributes["src"].Value);
//                    var id = util.To32BitFnv1aHash(url).ToString();
//                    var description = WebUtility.HtmlDecode(a.SelectSingleNode("//div[starts-with(@class,'entry-content')]").InnerText).Trim();

//                    result.Add(new ConfigModelNews
//                    {
//                        Id = id,
//                        Type = Type,
//                        Title = title,
//                        Description = description,
//                        DatePosted = DateTime.UtcNow,
//                        Url = url,
//                        ImageUrl = imgUrl
//                    });
//                }
//            }

//            return result;
//        }
//    }
//}