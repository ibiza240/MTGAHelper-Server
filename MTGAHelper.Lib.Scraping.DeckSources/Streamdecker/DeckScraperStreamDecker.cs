using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib.Exceptions;

namespace MTGAHelper.Lib.Scraping.DeckSources.Streamdecker
{
    public interface IDeckScraperStreamDecker : IDeckScraper
    {
        string TokenUsername { get; }

        IDeckScraperStreamDecker Init(string tokenUsername);
    }

    public class DeckScraperStreamDecker : DeckScraperBase, IDeckScraperStreamDecker
    {
        private IDateDeconstructor dateDeconstructor;

        public override string SiteUrl => "https://www.streamdecker.com";

        public string TokenUsername { get; private set; }
        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.Streamdecker, TokenUsername);

        public DeckScraperStreamDecker(
            IWriterDeck writerDeck,
            IDateDeconstructor dateDeconstructor,
            IMtgaTextDeckConverter converter
            )
            : base(writerDeck, converter)
        {
            this.dateDeconstructor = dateDeconstructor;
        }

        public IDeckScraperStreamDecker Init(string tokenUsername)
        {
            TokenUsername = tokenUsername.ToLower();
            return this;
        }

        protected override ICollection<int> GetDecksSliceList()
        {
            return new[] { 0, 20, 40, 60, 80 };
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            string data;
            using (var w = new WebClient())
            {
                // Get the list of decks (JSON) and parse
                data = w.DownloadString($"{SiteUrl}/api/userdecks/{TokenUsername}/20/{sliceStart}");
            }

            var decksInfo = JsonConvert.DeserializeObject<RootObjectDecksList>(data);

            if (ScraperType.Id == "streamdecker-mtgarenazone")
            {
                foreach (var d in decksInfo.data.decks.Where(i => i.name.Trim() == string.Empty))
                    d.name = $"Deck {d.deckLink}";
            }

            var info = decksInfo.data.decks
                //.Take(10)   // DEBUG
                .GroupBy(i => i.name.ToUpper());

            var totalSkipped = 0;
            var duplicates = info.Where(i => i.Count() > 1);
            if (duplicates.Count() > 0)
            {
                foreach (var i in duplicates)
                {
                    var nbSkipped = i.Count() - 1;
                    totalSkipped += nbSkipped;
                    var nbSame = i.Count();
                    var deckName = i.Key;
                    AddWarning($"{ScraperType} [slice {sliceStart}] found {nbSame} decks with name {deckName} ({nbSkipped} skipped)", nbSkipped);
                }
            }

            if (totalSkipped > 0)
                Log.Information("{ScraperType} [slice {sliceStart}] skipped a total of {totalSkipped} decks", ScraperType, sliceStart, totalSkipped);

            var decksToDownload = info
            .Select(i => i.First());

            var res = decksToDownload
            .Select((i, idx) => new DeckScraperDeckInputs(i.name, dateDeconstructor.Deconstruct(i.createdAt))
            {
                UrlViewDeck = $"{SiteUrl}/deck/{i.deckLink}",
                UrlDownloadDeck = i.deckLink
            })
            .ToArray();

            return res;
        }

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            if (input.DateCreated == default(DateTime))
                Log.Error("{ScraperType} invalid date created for deck {deckName}", ScraperType, input.Name);

            //if (input.Name.Contains("yro")) System.Diagnostics.Debugger.Break();

            //var txt = DownloadDeckString(input.Name, $"{SiteUrl}/api/deck/download/{input.UrlDownloadDeck}");

            string data;
            using (var w = new WebClient())
            {
                data = w.DownloadString($"{SiteUrl}/api/deck/{input.UrlDownloadDeck}");
            }

            var deckInfo = JsonConvert.DeserializeObject<RootObjectDeck>(data);

            //var test = deckInfo.data.cardList
            //        .Where(i => i.commander > 0)
            //        .Where(i => i.name.Contains("innan"))
            //        .ToArray();
            //if (test.Any()) System.Diagnostics.Debugger.Break();

            try
            {
                var cards = deckInfo.data.cardList
                    .Where(i => i.main > 0)
                    .Select(i => new DeckCard(converter.Convert("", $"1 {i.name}").First().Card, i.main, DeckCardZoneEnum.Deck))
                    .Union(
                        deckInfo.data.cardList
                            .Where(i => i.sideboard > 0 && i.companion == 0)
                            .Select(i => new DeckCard(converter.Convert("", $"1 {i.name}").First().Card, i.sideboard, DeckCardZoneEnum.Sideboard))
                    )
                    .Union(
                        deckInfo.data.cardList
                            .Where(i => i.companion > 0)
                            .Select(i => new DeckCard(converter.Convert("", $"1 {i.name}").First().Card, i.companion, DeckCardZoneEnum.Companion))
                    )
                    .Union(
                        deckInfo.data.cardList
                            .Where(i => i.commander > 0)
                            .Select(i => new DeckCard(converter.Convert("", $"1 {i.name}").First().Card, i.commander, DeckCardZoneEnum.Commander))
                    )
                    .ToArray();

                var deck = new ConfigModelDeck(new Entity.Deck(input.Name, ScraperType, cards), input.UrlViewDeck, input.DateCreated);

                System.Threading.Thread.Sleep(1000);

                return deck;
            }
            catch (InvalidDeckFormatException ex)
            {
                var ex2 = new DeckScraperWarningException($"{ScraperType} Invalid deck {input.Name}: " + ex.InnerException.Message);
                throw ex2;
            }
        }
    }
}