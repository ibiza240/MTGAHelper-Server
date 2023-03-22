using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MTGAHelper.Entity.Config.Decks;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish
{
    public class DeckScraperMtgGoldfishTournament : DeckScraperMtgGoldfishBase, IDeckScraper
    {
        private ScraperTypeFormatEnum format;
        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.MtgGoldfish, articleType.ToString().ToLower(), format);
        private readonly Regex regexTournamentOnDate = new Regex(@"^\s?(.*)\son\s(\d{4}-\d{2}-\d{2})\s?$", RegexOptions.Compiled | RegexOptions.Multiline);
        private MtgGoldfishArticleEnum articleType;

        public DeckScraperMtgGoldfishTournament(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter
            )
            : base(writerDeck, converter)
        {
        }

        public IDeckScraper Init(ScraperTypeFormatEnum format, MtgGoldfishArticleEnum articleType)
        {
            this.format = format;
            SetArticleType(articleType);
            return this;
        }

        private void SetArticleType(MtgGoldfishArticleEnum articleType)
        {
            this.articleType = articleType;
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            var hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(ScraperType.Url);

            var div = doc.DocumentNode.SelectNodes("//div").FirstOrDefault(i => i.ChildNodes.Any(n => n.InnerText.ToLowerInvariant() == "recent events"));

            if (div == null)
                return Array.Empty<DeckScraperDeckInputs>();

            // H3 node with tournament name and date is followed by a table with the decks
            var h3AndTable = div.ChildNodes
                .Where(n => n.NodeType == HtmlNodeType.Element)
                .WithPrevious()
                .Where(x => x.current.Name == "table");

            var tourneys = h3AndTable.Select(x =>
            (
                regexMatch: regexTournamentOnDate.Match(x.previous.InnerText),
                link: x.previous.SelectSingleNode(".//a")?.GetAttributeValue("href", null),
                decksTable: x.current)
            )
            .Where(x => x.regexMatch.Success)
            .Select(x =>
                (
                    x.link,
                    name: x.regexMatch.Groups[1].Value,
                    date: DateTime.ParseExact(x.regexMatch.Groups[2].Value,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None),
                    x.decksTable
                )
            );

            return tourneys
                .SelectMany(t => ParseDeckInfos(t.decksTable, t.date, t.name, t.link))
                .ToArray();
        }

        private IEnumerable<DeckScraperDeckInputs> ParseDeckInfos(HtmlNode htmlNode, DateTime tDate, string tName, string tLink)
        {
            var rows = htmlNode.SelectNodes(".//tr").Where(r => r.SelectNodes("td") != null);
            return rows
                .Select(r => r.SelectNodes("td"))
                .Select(r =>
                {
                    var hasWinLossRows = r.Count == 5;
                    var iRow0 = hasWinLossRows ? 2 : 1;

                    return new
                    {
                        //wins = r[0].InnerText.Trim(),
                        //losses = r[1].InnerText.Trim(),
                        deckName = r[iRow0].SelectSingleNode(".//a").InnerText,
                        deckLink = r[iRow0].SelectSingleNode(".//a").GetAttributeValue("href", string.Empty).Split('#')[0],
                        playerName = r[iRow0 + 1].SelectSingleNode(".//a").InnerText
                    };
                })
                .Select(x => new DeckScraperDeckInputs($"{x.deckName} by {x.playerName} ({tName})", tDate)
                {
                    UrlViewDeck = $"{SiteUrl}{x.deckLink}",
                    UrlDownloadDeck = $"{SiteUrl}{x.deckLink.Replace("deck/", "deck/arena_download/")}",
                    UrlDeckList = $"{SiteUrl}{tLink}",
                })
                .ToArray();
        }

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            var deck = GetFromTextArea(input);
            return deck;
        }
    }

    internal static class Extensions
    {
        /// <summary>
        /// Provides a windowed view of the collection, with window size 2.
        /// </summary>
        /// <returns>A tuple of the previous and current elements;
        /// the first 'current' element is the second element of the original collection</returns>
        public static IEnumerable<(T previous, T current)> WithPrevious<T>(this IEnumerable<T> collection)
        {
            using (var e = collection.GetEnumerator())
            {
                if (!e.MoveNext())
                    yield break;

                var prev = e.Current;
                while (e.MoveNext())
                {
                    yield return (prev, e.Current);
                    prev = e.Current;
                }
            }
        }
    }
}