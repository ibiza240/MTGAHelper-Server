using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.Exceptions;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.Logging;
using MTGAHelper.Lib.TextDeck;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Lib.Scraping.DeckSources.Aetherhub
{
    public interface IDeckScraperAetherhub : IDeckScraper
    {
        void Init(AetherhubListingEnum listingType, ScraperTypeFormatEnum format, string tokenUsername = null);
    }

    public abstract class DeckScraperAetherhubBase : DeckScraperBase, IDeckScraperAetherhub
    {
        public override string SiteUrl => "https://aetherhub.com";

        protected AetherhubListingEnum listingType;
        protected ScraperTypeFormatEnum format;
        protected readonly IDataPath configPath;
        protected readonly IDateDeconstructor dateDeconstructor;

        protected const int MAX_PER_VARIANT = 3;

        protected string tokenUsername;
        protected Dictionary<string, DeckScraperDeckInputs[]> dictInputsByVariant;

        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.Aetherhub,
            listingType == AetherhubListingEnum.User ? ScraperType.NAME_PREFIX_USER + tokenUsername : listingType.ToString().ToLower(), format);

        public DeckScraperAetherhubBase(
            IDataPath configPath,
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter,
            IDateDeconstructor dateDeconstructor)
            : base(writerDeck, converter)
        {
            this.configPath = configPath;
            this.dateDeconstructor = dateDeconstructor;
        }

        public void Init(AetherhubListingEnum listingType, ScraperTypeFormatEnum format, string tokenUsername = null)
        {
            this.format = format;
            SetArticleType(listingType, tokenUsername);
        }

        private void SetArticleType(AetherhubListingEnum listingType, string tokenUsername)
        {
            this.listingType = listingType;

            if (listingType == AetherhubListingEnum.User)
            {
                if (string.IsNullOrWhiteSpace(tokenUsername))
                    throw new DeckScraperErrorException("Must provide a tokenUsername when using scraper Aetherhub User");

                // Don't set the decksListUrl, special logic in DeckScraperAetherhubUserDecks
                this.tokenUsername = tokenUsername.Replace(ScraperType.NAME_PREFIX_USER, "");
            }
        }

        private string GetUserId()
        {
            try
            {
                HtmlWeb hw = new HtmlWeb();
                HtmlDocument doc = hw.Load($"{SiteUrl}/User/{tokenUsername}/Decks/Standard");

                var userId = doc.GetElementbyId("metaHubTable").Attributes["data-user-id"].Value;
                return userId;
            }
            catch (Exception ex)
            {
                Log.Error("Aetherhub scraper: Id not found for username '{tokenUsername}' ({message})", tokenUsername, ex.Message);
                return null;
            }
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            var userId = "";
            if (ScraperType.Name.StartsWith(ScraperType.NAME_PREFIX_USER))
            {
                // u=16177 is the Aetherhub
                userId = GetUserId();
                if (userId == null) return Array.Empty<DeckScraperDeckInputs>();
                userId = "&u=" + userId;
            }

            int formatId = -1;
            switch (format)
            {
                case ScraperTypeFormatEnum.Standard:
                    formatId = 1;   // Standard
                    break;

                case ScraperTypeFormatEnum.ArenaStandard:
                    formatId = 14;  // Arena Standard
                    break;

                case ScraperTypeFormatEnum.HistoricBo1:
                    formatId = 16;
                    break;

                case ScraperTypeFormatEnum.HistoricBo3:
                    formatId = 18;
                    break;

                default:
                    throw new DeckScraperErrorException("Format is required and unknown");
            }

            string strResponse = null;

            var url = $"{SiteUrl}/Meta/FetchMetaListAdv?formatId={formatId}{userId}";
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            var p = Path.Combine(configPath.FolderData, "aetherhubDecklistPayload.txt");
            LogExt.LogReadFile(p);
            var requestBody = File.ReadAllText(p);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                var response = httpClient.SendAsync(request).Result;
                strResponse = response.Content.ReadAsStringAsync().Result;
            }

            var parsed = JsonConvert.DeserializeObject<AetherhubDecklistResponse>(strResponse);
            var decksLinksGroupedByName = parsed.metadecks
                .Select(i => new DeckScraperDeckInputs(i.name, new DateTime(1970, 1, 1).AddMilliseconds(i.updated))
                {
                    UrlDownloadDeck = $"{SiteUrl}/Deck/FetchMtgaDeckJson?deckId={i.id}",
                    UrlViewDeck = $"{SiteUrl}/Deck/Public/{i.id}"
                })
                .GroupBy(i => i.Name);

            var decksLinksGroupedByNameFiltered = new Dictionary<string, ICollection<DeckScraperDeckInputs>>();
            foreach (var gByName in decksLinksGroupedByName)
            {
                var cnt = gByName.Count();
                if (cnt <= MAX_PER_VARIANT)
                    decksLinksGroupedByNameFiltered.Add(gByName.Key, gByName.ToArray());
                else
                {
                    var nbSkipped = cnt - MAX_PER_VARIANT;
                    AddWarning($"Skipping {nbSkipped}. {MAX_PER_VARIANT} / {cnt} taken for deck {gByName.Key}", nbSkipped);
                    decksLinksGroupedByNameFiltered.Add(gByName.Key, gByName.Take(MAX_PER_VARIANT).ToArray());
                }
            }

            dictInputsByVariant = decksLinksGroupedByNameFiltered
                .ToDictionary(i => i.Key, i => i.Value.Select((x, idx) =>
                    new DeckScraperDeckInputs($"{x.Name}{(i.Value.Count == 1 ? "" : $" ({idx + 1})")}", x.DateCreated)
                    {
                        UrlDownloadDeck = x.UrlDownloadDeck,
                        UrlViewDeck = x.UrlViewDeck,
                        VariantId = idx + 1,
                    }).ToArray());

            return decksLinksGroupedByNameFiltered.Select(i => new DeckScraperDeckInputs(i.Key)).ToArray();
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckVariants(DeckScraperDeckInputs input)
        {
            return dictInputsByVariant[input.Name];
        }

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            //if (input.Name == "Jeskai Superfriends (3)")
            //    System.Diagnostics.Debugger.Break();

            //if (input.UrlDownloadDeck == "https://aetherhub.com/Deck/FetchMtgaDeckJson?deckId=303954")
            //    Debugger.Break();

            var textDeck = GetDeckFromUrlDownloadDeck(input.UrlDownloadDeck);
            if (textDeck == null)
                throw new DeckScraperWarningException($"{ScraperType.Id} - NULL deck '{input.Name}' from Aetherhub: {input.UrlViewDeck}");

            //if (input.Name.StartsWith("afterlife123"))
            //    System.Diagnostics.Debugger.Break();

            var deck = new Deck(input.Name, ScraperType, converter.Convert(input.Name, textDeck));

            var nbCardsMain = deck.Cards.All.Where(i => i.Zone == DeckCardZoneEnum.Deck).Sum(i => i.Amount);
            if (nbCardsMain < 60)
            {
                // Aetherhub has a bug where exporting a deck that contains cards not in standard, these cards are not in the export text.
                // Thus, such decks have less than 60 cards and we don't want to keep them
                throw new DeckScraperWarningException($"{ScraperType.Id} - Invalid deck '{input.Name}' ({deck.Cards.All.Count} cards) from Aetherhub: {input.UrlViewDeck}");
            }

            return new ConfigModelDeck(deck, input.UrlViewDeck, input.DateCreated);
        }

        protected string GetDeckFromUrlDownloadDeck(string urlDownloadDeck)
        {
            string data;
            using (var w = new WebClient())
            {
                // Get the list of decks (JSON) and parse
                data = w.DownloadString(urlDownloadDeck);
            }

            var deckInfo = JsonConvert.DeserializeObject<AetherhubDeckModel>(data);
            if (deckInfo == null)
                return null;

            var headers = new[] { "", "deck", "sideboard", "companion", "commander" };

            var textLines = deckInfo.convertedDeck
                .Select(i => headers.Contains(i.name.ToLower().Trim()) ? i.name : $"{i.quantity} {i.name}");
            var textDeck = string.Join(Environment.NewLine, textLines);

            return textDeck;
        }
    }
}