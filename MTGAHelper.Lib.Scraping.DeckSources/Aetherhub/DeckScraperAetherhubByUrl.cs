using HtmlAgilityPack;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using MTGAHelper.Entity.Config.App;

namespace MTGAHelper.Lib.Scraping.DeckSources.Aetherhub
{
    public class DeckScraperAetherhubByUrl : DeckScraperAetherhubBase
    {
        public DeckScraperAetherhubByUrl(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter,
            IDateDeconstructor dateDeconstructor,
            IDataPath configPath)
            : base(configPath, writerDeck, converter, dateDeconstructor)
        {
        }

        public (string name, string mtgaFormat) GetDeck(string url)
        {
            Log.Information("DeckScraperAetherhubByUrl.GetDeck({url})", url);

            // https://aetherhub.com/Deck/Public/52784
            // or
            // https://aetherhub.com/Deck/Public?id=52784
            var regex = new Regex(@"https:\/\/aetherhub\.com\/Deck\/Public\?id=(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var match = regex.Match(url);
            if (match.Success == false)
            {
                var regex2 = new Regex($@"{SiteUrl}/Deck/Public/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                match = regex2.Match(url);

                if (match.Success == false)
                    throw new InvalidOperationException("The Aetherhub url provided is invalid.");
            }

            var deckId = match.Groups[1].Value;

            var web = new HtmlWeb();
            var doc = web.Load(url);

            var deckName = WebUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h3").InnerText).Trim();

            var elemDateCreated = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' card-body ')]//div[@class='row']").First(i => i.InnerText.Contains("Last Updated"));
            var regexDateCreated = new Regex(@"Last Updated: (.*?)\r\n", RegexOptions.Compiled);
            var matchDateCreated = regexDateCreated.Match(elemDateCreated.InnerText);
            var strDateCreated = matchDateCreated.Groups[1].Value;

            DateTime dateCreated = default;
            if (DateTime.TryParseExact(strDateCreated, "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateCreated) == false)
                Log.Error("Cannot convert {strDateCreated} to date", strDateCreated);

            var textDeck = base.GetDeckFromUrlDownloadDeck($"{SiteUrl}/Deck/FetchMtgaDeckJson?deckId={deckId}");

            // Will throw an exception if deck is invalid
            converter.Convert(deckName, textDeck);

            return (deckName, textDeck);
        }
    }
}