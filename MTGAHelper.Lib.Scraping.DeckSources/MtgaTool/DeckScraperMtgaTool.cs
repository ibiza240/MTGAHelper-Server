using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgaTool
{
    public class DeckScraperMtgaTool : DeckScraperBase
    {
        private readonly IReadOnlyDictionary<int, Card> dictAllCards;
        //private static Regex databaseRegex => new Regex("let database = (\\{.*\\});");
        //private static Regex responseRegex => new Regex("let response = (\\{.*\\});");

        private ScraperTypeFormatEnum format;

        public override string SiteUrl => "http://mtgatool.com/";

        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.MtgaTool, "meta", format);

        public DeckScraperMtgaTool(
            ICardRepository cardRepo,
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter
            )
            : base(writerDeck, converter)
        {
            this.dictAllCards = cardRepo;
        }

        public DeckScraperMtgaTool Init(ScraperTypeFormatEnum format)
        {
            this.format = format;
            return this;
        }

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            var deck = new Entity.Deck(input.Name, ScraperType, converter.Convert(input.Name, input.DeckText));
            return new ConfigModelDeck(deck, input.UrlViewDeck, input.DateCreated) { UrlDeckList = input.UrlDeckList };
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            var mtgaToolFormat =
                ScraperType.Format == ScraperTypeFormatEnum.ArenaStandard ? "BO1" :
                ScraperType.Format == ScraperTypeFormatEnum.Standard ? "BO3" :
                ScraperType.Format == ScraperTypeFormatEnum.HistoricBo1 ? "HBO1" : "HBO3";

            var urlDeckList = $"{SiteUrl}api/get_metagame.php?event={mtgaToolFormat}";

            var client = new HttpClient();
            var response = client.GetAsync(urlDeckList).Result;
            response.EnsureSuccessStatusCode();

            var strResponse = response.Content.ReadAsStringAsync().Result;
            var parsed = JsonConvert.DeserializeObject<MtgaToolModel>(strResponse);

            var dayId = parsed._id.Split(".").First();

            var metaDecks = parsed.meta
                .Select(i =>
                {
                    var deck = new Entity.Deck("", null,
                        i.best_deck.mainDeck
                            .Select(c => new DeckCard(dictAllCards[c.id], c.quantity, DeckCardZoneEnum.Deck))
                            .Concat(i.best_deck.sideboard.Select(c => new DeckCard(dictAllCards[c.id], c.quantity, DeckCardZoneEnum.Sideboard)))
                        .ToArray()
                    );

                    return new DeckScraperDeckInputs(i.name)
                    {
                        DateCreated = parsed.date,
                        DeckText = writerDeck.ToText(deck),
                        UrlDeckList = $"{SiteUrl}metagame/{mtgaToolFormat}",
                        UrlViewDeck = $"{SiteUrl}metagame/{mtgaToolFormat}/{dayId}/{Uri.EscapeUriString(i.name)}"
                    };
                })
                .ToArray();

            return metaDecks;

            //var hw = new HtmlWeb();
            //var doc = hw.Load(urlDeckList);

            //// Get the 'response' variable from the JavaScript, it's a JSON string.
            //var scriptNodes = doc.DocumentNode.SelectNodes("/html/script");
            //var innerHtml = scriptNodes[0].InnerHtml;

            //var jsonDb = databaseRegex.Match(innerHtml).Groups[1].Value;
            //var jsonResp = responseRegex.Match(innerHtml).Groups[1].Value;

            //var response = JsonConvert.DeserializeObject<MtgaToolResponse>(jsonResp);
            //var db = JsonConvert.DeserializeObject<MtgaToolDatabase>(jsonDb);

            //var dayId = response.Id.Split('.')[0];

            //return response.Archetypes
            //    .Where(a => a.Name != "Unknown")
            //    .Select(a => new DeckScraperDeckInputs(a.Name, response.Date)
            //    {
            //        DeckText = getDeckText(a.BestDeck, db),
            //        UrlDeckList = urlDeckList,
            //        UrlViewDeck = $"{SiteUrl}metagame/{format.ToString().ToUpper()}/{dayId}/{Uri.EscapeUriString(a.Name)}"
            //    })
            //    .ToArray();
        }

        //private string getDeckText(MtgaToolDeck deck, MtgaToolDatabase db)
        //{
        //    var mainDeckTxt = string.Join(Environment.NewLine, deck.MainDeck.Select(c => getCardText(c, db)));
        //    var sideBoardTxt = string.Join(Environment.NewLine, deck.SideBoard.Select(c => getCardText(c, db)));

        //    return $"{mainDeckTxt}{Environment.NewLine}{Environment.NewLine}{sideBoardTxt}";
        //}

        //private string getCardText(MtgaToolBoard cardInDeck, MtgaToolDatabase db)
        //{
        //    var card = db.Cards[cardInDeck.Id];
        //    var setCode = db.Sets[card.Set].SetCode;
        //    return $"{cardInDeck.Qty} {card.Name} ({setCode}) {card.CardId}";
        //}
    }
}