using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgDecks
{
    public interface IDeckScraperMtgDecksMetaFull : IDeckScraper
    {
    }

    public class DeckScraperMtgDecksMetaFull : DeckScraperBase, IDeckScraperMtgDecksMetaFull
    {
        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.MtgDecks, "meta");

        public override string SiteUrl => "https://mtgdecks.net";

        public DeckScraperMtgDecksMetaFull(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter
            )
            : base(writerDeck, converter)
        {
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load($"{SiteUrl}/Standard");

            var decksLinks = doc.DocumentNode.SelectNodes("//table[@id='archetypesTable']//a[@href]")
                .Where(i => i.Attributes["href"].Value.StartsWith("/Standard/"))
                .Select(i => new DeckScraperDeckInputs(i.InnerText) { UrlDownloadDeck = i.Attributes["href"].Value })
                //.Take(5)  // Debug
                .ToArray();

            return decksLinks;
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckVariants(DeckScraperDeckInputs input)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(SiteUrl + input.UrlDownloadDeck);

            var tableFound = doc.DocumentNode.SelectNodes("//table//tr")?.Count > 0;
            if (tableFound == false)
            {
                throw new Exception($"HTML table cannot be found");
                //logger.LogError("For deck {Deck}, no table found at " + urlDeck, deckName);
                //return null;
            }

            var decksUrls = doc.DocumentNode.SelectNodes("//table//tr")
                    .Where(i => i.InnerText?.Contains("1st") == true || i.InnerText?.Contains("Top4") == true || i.InnerText?.Contains("Top8") == true || i.InnerText?.Contains("Top16") == true)
                    .Select(i => i.SelectNodes("td/a[@href]").First().Attributes["href"].Value)
                    .Select((i, idx) => new DeckScraperDeckInputs($"{input.Name} ({idx + 1})") { UrlViewDeck = SiteUrl + i })
                    .ToArray();

            //Log.Information("{ScraperType} found " + decksUrls.Length + " variations for {Deck}", ScraperType, input.Name);

            return decksUrls;
        }

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            HtmlWeb hwDeck = new HtmlWeb();
            HtmlDocument docDeck = hwDeck.Load(input.UrlViewDeck);

            var textContainingDate = docDeck.DocumentNode.SelectSingleNode("//div[contains(concat(' ', normalize-space(@class), ' '), ' deckInfo ')]").InnerText.Trim();
            var strDateCreated = textContainingDate.Substring(textContainingDate.Length - 11, 11);
            DateTime dateCreated = DateTime.MinValue;
            if (DateTime.TryParseExact(strDateCreated, "dd-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime res))
                dateCreated = res;
            else
                Log.Error("{ScraperType} Cannot decode string '{strDateCreated}' to date for deck {deckName}", ScraperType, strDateCreated, input.Name);

            var txt = DownloadDeckString(input.Name, input.UrlViewDeck + "/txt");
            var cards = converter.Convert(input.Name, txt);

            //if (cards == null)
            //{
            //    Log.Warning("{ScraperType} skipped {Deck} because it has no cards", ScraperType, input.Name);
            //    return null;
            //}

            var d = new Deck(input.Name, ScraperType, cards);

            //Save(d, false);

            return new ConfigModelDeck(d, input.UrlViewDeck, dateCreated);
        }
    }
}