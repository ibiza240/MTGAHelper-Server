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
using System.Text.RegularExpressions;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish
{
    public interface IDeckScraperMtgGoldfishArticle : IDeckScraper
    {
        IDeckScraperMtgGoldfishArticle Init(MtgGoldfishArticleEnum articleType);
    }

    public class DeckScraperMtgGoldfishArticle : DeckScraperMtgGoldfishBase, IDeckScraperMtgGoldfishArticle
    {
        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.MtgGoldfish, articleType.ToString().ToLower());

        private MtgGoldfishArticleEnum articleType;

        public DeckScraperMtgGoldfishArticle(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter,
            ConfigManagerDecks configDecks)
            : base(writerDeck, converter)
        {
        }

        public IDeckScraperMtgGoldfishArticle Init(MtgGoldfishArticleEnum articleType)
        {
            SetArticleType(articleType);
            return this;
        }

        private void SetArticleType(MtgGoldfishArticleEnum articleType)
        {
            this.articleType = articleType;
        }

        private string ExtractName(string innerText)
        {
            var res = innerText;

            var regex = new Regex(@"^.*:(.*)\(.*\).*$", RegexOptions.Compiled);
            var m = regex.Match(innerText);
            if (m.Success)
                res = WebUtility.HtmlDecode(m.Groups[1].Value.Trim());

            //return res.Replace("$", "USD");
            return res;
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            Log.Information("Loading decks from {url}", ScraperType.Url);
            var doc = LoadMtgGoldfishUrl(ScraperType.Url);

            try
            {
                var d1 = doc.DocumentNode.SelectNodes("//div[@class='article-tile']")
                    .Where(i => i.SelectSingleNode("div[@class='article-tile-contents']/div/a").InnerText.Contains("Standard") ||
                                i.SelectSingleNode("div[@class='article-tile-contents']/div/a").InnerText.Contains("Historic"))
                    .Select(i =>
                    {
                        var link = i.SelectSingleNode("div[@class='article-tile-contents']/div/a");
                        return new DeckScraperDeckInputs(ExtractName(link.InnerText)) { UrlViewDeck = $"{SiteUrl}{link.Attributes["href"].Value}" };
                    })
                    .ToArray();

                return d1;
            }
            catch (Exception ex)
            {
                Log.Error(doc.DocumentNode.InnerHtml);
                //System.Diagnostics.Debugger.Break();
                throw;
            }
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckVariants(DeckScraperDeckInputs input)
        {
            var doc = LoadMtgGoldfishUrl(input.UrlViewDeck);

            // Get date created
            var str = doc.DocumentNode.SelectSingleNode("//*[@class='article-author']").InnerText.Trim();
            var strDate = str.Substring(str.IndexOf("//") + 2).Trim();
            var dateCreated = ParseDateCreated(strDate, input.Name);

            //if (input.UrlViewDeck.Contains("argons")) System.Diagnostics.Debugger.Break();

            // Look for deck widgets on the page
            //try
            //{
            var s = doc.DocumentNode
                .SelectNodes("//ins[@class='widget-deck-placeholder' and @data-id]")
                .Select((i, idx) => new DeckScraperDeckInputs(input.Name, dateCreated)
                {
                    UrlViewDeck = input.UrlViewDeck,
                    UrlDownloadDeck = i.Attributes["data-id"].Value,
                    VariantId = idx + 1
                })
                .ToArray();

            return s;
            //}
            //catch (MtgGoldfishThrottlingException)
            //{
            //    throw;
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debugger.Break();
            //    //Log.Error("Error with downloader: {scraperType} - {input}", ScraperType.Id, JsonConvert.SerializeObject(input));
            //    //throw;
            //    var msg = $"Error with downloader: {ScraperType.Id} - {JsonConvert.SerializeObject(input)}";
            //    throw new Exceptions.DeckScraperErrorException(msg);
            //}
        }

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            input.Name += $" ({input.VariantId})";

            var urlForDeckText = input.UrlDownloadDeck.StartsWith("http") ? input.UrlDownloadDeck : $"{SiteUrl}/deck/arena_download/" + input.UrlDownloadDeck;
            input.UrlDownloadDeck = urlForDeckText;

            ConfigModelDeck deck = null;

            //var isThrottled = false;
            //do
            //{
            //    try
            //    {
            deck = GetFromTextArea(input);
            //    }
            //    catch (MtgGoldfishThrottlingException)
            //    {
            //        isThrottled = true;
            //        Log.Warning("{ScraperType} - Request GetFromTextArea throttled for deck {deckName}, waiting 64 seconds...", ScraperType, input.Name);
            //        System.Threading.Thread.Sleep(64000);
            //    }
            //}
            //while (isThrottled);

            return deck;
        }
    }
}