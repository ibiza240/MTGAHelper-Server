//using HtmlAgilityPack;
//using MTGAHelper.Entity;
//using MTGAHelper.Lib.Config;
//using MTGAHelper.Lib.IO.Writer;
//using MTGAHelper.Lib.TextDeck;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text.RegularExpressions;
//using MTGAHelper.Entity.Config.Decks;
//using MTGAHelper.Lib.Exceptions;

//namespace MTGAHelper.Lib.DeckScraper
//{
//    public interface IDeckScraperMtgDecksAverageArchetype : IDeckScraper
//    {
//        IDeckScraperMtgDecksAverageArchetype Init(ICollection<Card> allCards, string directory);
//    }

//    public class DeckScraperMtgDecksAverageArchetype : DeckScraperBase, IDeckScraperMtgDecksAverageArchetype
//    {
//        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.MtgDecks, "averagearchetype");

//        public override string SiteUrl => "https://mtgdecks.net";

//        public DeckScraperMtgDecksAverageArchetype(
//            IWriterDeck writerDeck,
//            IMtgaTextDeckConverter converter,
//            ConfigManagerDecks configDecks)
//            : base(writerDeck, converter, configDecks)
//        {
//        }

//        public new IDeckScraperMtgDecksAverageArchetype Init(ICollection<Card> allCards, string directory)
//        {
//            base.Init(allCards, directory);
//            return this;
//        }

//        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
//        {
//            HtmlWeb hw = new HtmlWeb();
//            HtmlDocument doc = hw.Load($"{SiteUrl}/Standard");

//            var decksLinks = doc.DocumentNode.SelectNodes("//table[@id='archetypesTable']//a[@href]")
//                .Where(i => i.Attributes["href"].Value.StartsWith("/Standard/"))
//                .Select(i => new DeckScraperDeckInputs(i.InnerText)
//                {
//                    UrlViewDeck = $"{ SiteUrl }{i.Attributes["href"].Value}",
//                    UrlDownloadDeck = $"{ SiteUrl }{i.Attributes["href"].Value}",
//                })
//                //.Take(5)  // Debug
//                .ToArray();

//            return decksLinks;
//        }

//        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
//        {
//            //Log.Debug("{ScraperType} downloading {Deck} at {deckUrl}", ScraperType, input.Name, deckUrl);

//            HtmlWeb hw = new HtmlWeb();
//            HtmlDocument doc = hw.Load(input.UrlDownloadDeck);

//            var r = doc.DocumentNode.SelectNodes("//a[contains(@href, 'analysis') and contains(@href, '/all')]");
//            if (r == null || r.Count == 0)
//            {
//                //var ex = new DeckScraperException("{ScraperType} No average archetype available for deck {deckName}");
//                //ex.Data.Add("ScraperType", ScraperType);
//                //ex.Data.Add("deckName", input.Name);
//                var ex = new DeckScraperWarningException($"{ScraperType} No average archetype available for deck {input.Name}");
//                throw ex;
//            }

//            var urlAvgArchetype = SiteUrl + r.Last().Attributes["href"].Value;
//            doc = hw.Load(urlAvgArchetype);

//            var tableWithData = "//div[contains(concat(' ', normalize-space(@class), ' '), ' deck ')]";
//            var table = doc.DocumentNode.SelectSingleNode(tableWithData);

//            // Average main deck
//            var t1 = "//div[@class='columns-2']";
//            var tableMainDeck = table.SelectSingleNode(t1);

//            // Second table containing:
//            // Other cards in main
//            // Sideboard cards
//            var t2 = "//div[@class='row']/div[@class='col-md-6']";
//            var table2 = table.SelectNodes(t2);

//            if (table2.Count != 2)
//            {
//                throw new Exception("Incorrect table parsing for average deck");
//            }

//            var tableCardsMainOther = table2.First();
//            var tableSideboardCards = table2.Last();

//            var regex1 = new Regex(@"^(\d+)\t(.+?)\t*$", RegexOptions.Multiline | RegexOptions.Compiled);
//            var cardsMain = regex1.Matches(tableMainDeck.InnerText)
//                .Cast<Match>()
//                .Select(m =>
//                {
//                    var amt = int.Parse(m.Groups[1].Value);
//                    return new DeckCard(new CardWithAmount(GetCard(m.Groups[2].Value), amt), DeckCardZoneEnum.Deck);
//                });

//            var regex2 = new Regex(@"^(?!Other cards in main|---|\$|ad=)(.+?)$", RegexOptions.Multiline | RegexOptions.Compiled);
//            var cardsMainOther = regex2.Matches(tableCardsMainOther.InnerText)
//                .Cast<Match>()
//                .Select(m => GetCard(m.Groups[1].Value));

//            var regex3 = new Regex(@"^(?!Sideboard Card|---|\$|ad=)(.*?)(?:&nbsp;)*$", RegexOptions.Multiline | RegexOptions.Compiled);
//            var cardsSideboard = regex3.Matches(tableSideboardCards.InnerText)
//                .Cast<Match>()
//                .Where(m => string.IsNullOrWhiteSpace(m.Groups[1].Value) == false)
//                .Select(m => GetCard(m.Groups[1].Value));

//            var d = new DeckAverageArchetype($"{input.Name} (Avg)", ScraperType, cardsMain, cardsMainOther, cardsSideboard);

//            //Save(d, false);

//            return new ConfigModelDeck(d, urlAvgArchetype, DateTime.UtcNow);
//        }

//        private Card GetCard(string name)
//        {
//            var cardName = WebUtility.HtmlDecode(name).Replace("/", " // ");

//            try
//            {
//                var card = allCards.Last(i => i.CompareNameWith(cardName));
//                return card;
//            }
//            catch
//            {
//                try
//                {
//                    var card = allCards.Last(i => i.CompareNameWith(cardName + " (a)"));
//                    return card;
//                }
//                catch
//                {
//                    //System.Diagnostics.Debugger.Break();
//                    throw;
//                }
//            }
//        }
//    }
//}