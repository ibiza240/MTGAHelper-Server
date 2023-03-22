using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib.Config.Decks;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish
{
    public interface IDeckScraperMtgGoldfishMeta : IDeckScraper
    {
        IDeckScraperMtgGoldfishMeta Init(ScraperTypeFormatEnum format);
    }

    public class DeckScraperMtgGoldfishMeta : DeckScraperMtgGoldfishBase, IDeckScraperMtgGoldfishMeta
    {
        private ScraperType _scraperType = null;
        protected override ScraperType ScraperType => _scraperType;

        public DeckScraperMtgGoldfishMeta(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter,
            ConfigManagerDecks configDecks
        )
            : base(writerDeck, converter)
        {
        }

        public IDeckScraperMtgGoldfishMeta Init(ScraperTypeFormatEnum format)
        {
            _scraperType = new ScraperType(ScraperTypeEnum.MtgGoldfish, MtgGoldfishArticleEnum.Meta.ToString().ToLower(), format);
            return this;
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckVariants(DeckScraperDeckInputs input)
        {
            return base.GetDeckVariants(input);
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            var doc = LoadMtgGoldfishUrl(ScraperType.Url);

            var decksLinks = doc.DocumentNode.SelectNodes("//div[@class='metagame-list-full-content']//span[@class='deck-price-paper']//a")
                .Take(100)
                .Select(i => new DeckScraperDeckInputs(WebUtility.HtmlDecode(i.InnerText.Trim()))
                {
                    UrlViewDeck = $"{SiteUrl}{i.Attributes["href"].Value.Replace("#paper", "#arena")}"
                })
                //.Take(2)  // Debug;
                .ToArray();

            var order = 1;
            foreach (var d in decksLinks)
            {
                d.OrderIndex = order++;
            }

            var res = new List<DeckScraperDeckInputs>();
            foreach (var links in decksLinks.GroupBy(i => i.Name).Where(i => i.Key != "Other"))
            {
                if (links.Count() > 1)
                {
                    int idx = 1;
                    foreach (var i in links)
                    {
                        res.Add(new DeckScraperDeckInputs(i.Name + $" ({idx})") { UrlViewDeck = i.UrlViewDeck, OrderIndex = i.OrderIndex });
                        idx++;
                    }
                }
                else
                {
                    res.Add(links.First());
                }
            }

            return res;
        }

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            var res = TryGetResults(() =>
            {
                try
                {
                    var inputDeck = DownloadDeckFromDeckView(input.UrlViewDeck);
                    return GetFromTextArea(inputDeck);
                }
                catch (Exception ex)
                {
                    Log.Error("Url: {url}", input.UrlViewDeck);
                    throw;
                }
            });

            return res;
        }
    }
}