using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish
{
    public class DeckScraperMtgGoldfishSingleScoop : DeckScraperMtgGoldfishBase
    {
        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.MtgGoldfish, "singlescoop", ScraperTypeFormatEnum.ArenaStandard);

        public DeckScraperMtgGoldfishSingleScoop(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter
            )
            : base(writerDeck, converter)
        {
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            Log.Information("Loading decks from {url}", ScraperType.Url);
            var doc = LoadMtgGoldfishUrl(ScraperType.Url);

            //var regex = new Regex("^/archetype/(?:arena_)?standard", RegexOptions.Compiled);

            var listOfArticles = doc.DocumentNode.SelectNodes("//div[@class='article-tile-title']/a");
            var articles = listOfArticles
                .Select(i => new
                {
                    Text = WebUtility.HtmlDecode(i.InnerText),
                    Url = $"{SiteUrl}{i.Attributes["href"].Value}"
                }).ToArray();

            var decksLinks = articles
                .Where(i => i.Text.Contains("Single Scoop"))
                .Select(i => new DeckScraperDeckInputs(i.Text.Replace("Single Scoop:", "").Replace("(Standard, Magic Arena)", "").Trim())
                {
                    UrlViewDeck = i.Url
                })
                .ToArray();

            return decksLinks;
        }

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            var doc = LoadMtgGoldfishUrl(input.UrlViewDeck);

            // Get date created
            var str = doc.DocumentNode.SelectSingleNode("//div[@class='article-author']").InnerText.Trim();
            var idxSplit = str.IndexOf("//") + 2;
            var len = System.Math.Min(15, str.Length - idxSplit);
            var strDate = str.Substring(idxSplit, len).Trim();
            input.DateCreated = ParseDateCreated(strDate, input.Name);

            var dataId = doc.DocumentNode.SelectSingleNode("//ins[@class='widget-deck-placeholder']").Attributes["data-id"].Value;

            var urlForDeckText = $"{SiteUrl}/deck/arena_download/" + dataId;
            input.UrlDownloadDeck = urlForDeckText;

            var deck = GetFromTextArea(input);

            return deck;
        }
    }
}