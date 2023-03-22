using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MTGAHelper.Entity.Config.App;

namespace MTGAHelper.Lib.Scraping.DeckSources.Aetherhub
{
    public class DeckScraperAetherhubTierOne : DeckScraperAetherhubBase
    {
        public DeckScraperAetherhubTierOne(
            IDataPath configPath,
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter,
            IDateDeconstructor dateDeconstructor)
            : base(configPath, writerDeck, converter, dateDeconstructor)
        {
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(ScraperType.Url);

            string date = null;
            var decksLinks = doc.DocumentNode.SelectNodes("//table[@id='metalist']/tbody/tr")
                .Select(i =>
                {
                    //if (i.SelectNodes("td") == null)
                    //{
                    //    Log.Warning("DeckScraperAetherhubTierOne GetDeckList - No deck found: {innerHtml}", i.InnerHtml);
                    //    return null;
                    //}
                    var th = i.SelectSingleNode("th");
                    if (th != null)
                    {
                        var tagSmall = th.SelectSingleNode("small");
                        date = tagSmall.InnerText.Split('-')[0].Trim();
                        return null;
                    }

                    //var date = i.SelectSingleNode("td[6]").InnerText;
                    var dateDeconstructed = dateDeconstructor.Deconstruct(date);
                    var link = i.SelectSingleNode("td[2]/a");
                    return new DeckScraperDeckInputs(WebUtility.HtmlDecode(link.InnerText), dateDeconstructed) { UrlViewDeck = link.Attributes["href"].Value };
                })
                .Where(i => i != null)
                //.Take(5)  // Debug
                .ToArray();

            var order = 1;
            foreach (var d in decksLinks)
                d.OrderIndex = order++;

            dictInputsByVariant = decksLinks
                .GroupBy(i => i.Name)
                .ToDictionary(i => i.Key, i => i.Select((x, idx) =>
                    new DeckScraperDeckInputs($"{x.Name}{(i.Count() == 1 ? "" : $" ({idx + 1})")}", x.DateCreated)
                    {
                        OrderIndex = x.OrderIndex,
                        UrlDownloadDeck = $"{SiteUrl}/Deck/FetchMtgaDeckJson?deckId={x.UrlViewDeck.Split('/').Last().Split('-').Last()}",
                        UrlViewDeck = SiteUrl + x.UrlViewDeck,
                        VariantId = idx + 1,
                        DateCreated = x.DateCreated,
                    }).ToArray());

            // Return group names (in GetDeckVariants, the variants are retrieved by this value)
            return dictInputsByVariant.Select(i => new DeckScraperDeckInputs(i.Key)).ToArray();
        }
    }
}