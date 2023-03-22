using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib.Exceptions;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish
{
    public abstract class DeckScraperMtgGoldfishBase : DeckScraperBase
    {
        public class MtgGoldfishThrottlingException : Exception
        {
        }

        //protected abstract string DeckUrl(string urlPart);
        public override string SiteUrl => "https://www.mtggoldfish.com";

        public DeckScraperMtgGoldfishBase(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter
            )
            : base(writerDeck, converter)
        {
        }

        protected DateTime ParseDateCreated(string strDate, string deckName, string url = null)
        {
            if (DateTime.TryParse(strDate, out DateTime dt))
            {
                return dt;
            }
            else
            {
                Log.Error("{ScraperType} Cannot decode string '{strDateCreated}' to date for deck {deckName}, url: {url}", ScraperType, strDate, deckName, url);
                return DateTime.MinValue;
            }
        }

        protected HtmlDocument LoadMtgGoldfishUrl(string url)
        {
            try
            {
                HtmlWeb hw = new HtmlWeb();
                HtmlDocument doc = hw.Load(url);

                // MTGGoldfish throttling
                if (doc.DocumentNode.InnerHtml.Trim() == "Throttled")
                    throw new MtgGoldfishThrottlingException();

                return doc;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debugger.Break();
                Log.Error("Problem trying to download deck at: {url}", url);
                throw;
            }
        }

        protected string GetTextAreaContent(string urlDownloadDeck)
        {
            //try
            //{
            var doc = LoadMtgGoldfishUrl(urlDownloadDeck);

            string deckText = "";
            try
            {
                deckText = WebUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//textarea").InnerText);
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debugger.Break();
                throw;
            }
            // Copy the textarea content
            return deckText;
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex, urlDownloadDeck);
            //    System.Diagnostics.Debugger.Break();
            //    return "";
            //}
        }

        protected ConfigModelDeck GetFromTextArea(DeckScraperDeckInputs input)
        {
            var deckText = GetTextAreaContent(input.UrlDownloadDeck);

            ConfigModelDeck res = null;
            var cards = converter.Convert(input.Name, deckText);
            var nbCards = cards?.Where(i => i.Zone == DeckCardZoneEnum.Deck).Sum(i => i.Amount);
            if (nbCards >= 40)
            {
                var deck = new Deck(input.Name, ScraperType, cards);
                res = new ConfigModelDeck(deck, input.UrlViewDeck, input.DateCreated);

                //Save(deck, false);
            }
            else
            {
                //var ex = new DeckScraperInvalidFormatException("{ScraperType} skipped parsing for {deckName} (only {strNbCards})");
                //ex.Data.Add("ScraperType", ScraperType);
                //ex.Data.Add("deckName", input.Name);
                //ex.Data.Add("strNbCards", $"{nbCards} cards");
                var ex = new DeckScraperWarningException($"{ScraperType} skipped parsing for {input.Name} (only {nbCards} cards)");
                throw ex;
            }

            return res;
        }

        public DeckScraperDeckInputs DownloadDeckFromDeckView(string urlViewDeck)
        {
            var doc = LoadMtgGoldfishUrl(urlViewDeck);

            try
            {
                var name = doc.DocumentNode.SelectSingleNode("//div[@class='layout-breadcrumb']").InnerText.Split('/').Last().Trim();

                // Get date created
                var str = doc.DocumentNode.SelectSingleNode("//p[@class='deck-container-information']").InnerText.Trim();
                var regexDate = new Regex(@"Deck Date: (.*?)$", RegexOptions.Multiline);
                var strDate = regexDate.Match(str).Groups[1].Value;
                if (string.IsNullOrWhiteSpace(strDate))
                {
                    strDate = str.Split('\n').Last().Replace("Date", "").Replace(":", "").Trim();
                }

                var dateCreated = ParseDateCreated(strDate, name, urlViewDeck);

                var linksContainingArenaDownload = doc.DocumentNode.SelectNodes("//a").Where(i => i.Attributes.Any(i => i.Name == "href")).ToArray();
                var urlDownloadDeck = SiteUrl + linksContainingArenaDownload
                    .First(i => i.Attributes["href"].Value.StartsWith("/deck/arena_download"))
                    .Attributes["href"].Value;

                var inputDeck = new DeckScraperDeckInputs(name, dateCreated) { UrlViewDeck = urlViewDeck, UrlDownloadDeck = urlDownloadDeck };
                return inputDeck;
            }
            catch (Exception ex)
            {
                //System.Diagnostics.Debugger.Break();
                throw;
            }
        }
    }
}