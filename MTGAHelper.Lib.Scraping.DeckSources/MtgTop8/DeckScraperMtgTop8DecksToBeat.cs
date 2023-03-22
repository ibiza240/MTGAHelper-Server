using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MTGAHelper.Entity.Config.Decks;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgTop8
{
    public class DeckScraperMtgTop8DecksToBeat : DeckScraperBase
    {
        public DeckScraperMtgTop8DecksToBeat(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter
            )
            : base(writerDeck, converter)
        {
        }

        //public void Init(ICollection<Card> allCards, string lastSet)
        //{
        //    base.Init(allCards, lastSet);
        //}

        public override string SiteUrl => "https://www.mtgtop8.com/";

        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.MtgTop8, "deckstobeat");

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(input.UrlDownloadDeck);

            var regexTextDeck = new Regex("innerHTML=\"(.*?)\"");

            var textDeck = regexTextDeck.Match(doc.DocumentNode.InnerHtml).Groups[1].Value.Replace(@"\n", Environment.NewLine);
            var deck = new Deck(input.Name, ScraperType, converter.Convert(input.Name, textDeck));
            return new ConfigModelDeck(deck, input.UrlViewDeck, input.DateCreated) { UrlDeckList = input.UrlDeckList };
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load($"{SiteUrl}format?f=ST");

            var linkDecksToBeat = doc.DocumentNode.SelectNodes("//a").FirstOrDefault(i => i.InnerText.ToLower().Contains("decks to beat"));

            if (linkDecksToBeat == null)
                return Array.Empty<DeckScraperDeckInputs>();

            var urlDeckList = $"{SiteUrl}{linkDecksToBeat.Attributes["href"].Value}";
            Log.Information("Loading decks from {url}", urlDeckList);
            doc = hw.Load(urlDeckList);

            var divDecks = doc.DocumentNode.SelectNodes("//table//div").FirstOrDefault(i => i.InnerText.ToLower().Contains("tier 1"));

            if (divDecks == null)
                return Array.Empty<DeckScraperDeckInputs>();

            var links = divDecks.SelectNodes(".//a").Where(i => i.Attributes["href"].Value.Contains("d=")).ToArray();
            var regexDeckId = new Regex(@"d=(\d+)");

            var strDate = divDecks.SelectSingleNode(".//td").InnerText.Replace("Standard", "").Trim();
            var date = DateTime.ParseExact(strDate, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None);

            var inputs = links
                .Select(i =>
                {
                    var urlDeck = i.Attributes["href"].Value;
                    var deckId = regexDeckId.Match(urlDeck).Groups[1].Value;

                    return new DeckScraperDeckInputs(i.InnerText)
                    {
                        UrlViewDeck = $"{SiteUrl}event{urlDeck}",
                        UrlDownloadDeck = $"{SiteUrl}mtgarena?d={deckId}",
                        DateCreated = date,
                        UrlDeckList = urlDeckList,
                    };
                })
                .ToArray();

            return inputs;
        }
    }
}