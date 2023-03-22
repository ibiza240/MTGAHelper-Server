using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MTGAHelper.Entity.Config.App;
using System;
using System.Globalization;

namespace MTGAHelper.Lib.Scraping.DeckSources.Aetherhub
{
    public class DeckScraperAetherhubTournamentBo3 : DeckScraperAetherhubBase
    {
        public DeckScraperAetherhubTournamentBo3(
            IDataPath configPath,
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter,
            IDateDeconstructor dateDeconstructor)
            : base(configPath, writerDeck, converter, dateDeconstructor)
        {
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckVariants(DeckScraperDeckInputs input)
        {
            return new[] { input };
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            HtmlWeb hw = new HtmlWeb();
            HtmlDocument doc = hw.Load(ScraperType.Url);

            var table = doc.DocumentNode.SelectNodes("//table[@id='metalist']//tr");

            var subTables = TraverseRowsBuildSubTables(table);

            return subTables.SelectMany(i => i.Value).ToArray();
        }

        private Dictionary<string, List<DeckScraperDeckInputs>> TraverseRowsBuildSubTables(HtmlNodeCollection rows)
        {
            var result = new Dictionary<string, List<DeckScraperDeckInputs>>();
            string currentTournament = null;
            foreach (var r in rows)
            {
                if (r.GetClasses().Any())
                {
                    // Class means data row
                    var currentTournamentNoDate = currentTournament.Substring(0, currentTournament.LastIndexOf(" "));
                    var pos = r.SelectSingleNode("./td[1]").InnerText.Trim();
                    var deckName = r.SelectSingleNode("./td[2]").InnerText.Trim();
                    var playerName = r.SelectSingleNode("./td[3]").InnerText.Trim();

                    // Aetherhub tournament names always end with the date in dd/MM/yy format
                    var strDate = currentTournament.Split(" ").Last();
                    DateTime.TryParseExact(strDate, "dd/MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateCreated);

                    var urlViewDeck = SiteUrl + r.SelectSingleNode("./td[1]/a").GetAttributeValue("href", "");
                    var name = $"{currentTournamentNoDate} {pos} {deckName} by {playerName}";
                    result[currentTournament].Add(new DeckScraperDeckInputs(name)
                    {
                        DateCreated = dateCreated,
                        UrlDeckList = SiteUrl + ScraperType.Url,
                        UrlDownloadDeck = SiteUrl + $"/Deck/FetchMtgaDeckJson?deckId={urlViewDeck.Split('/').Last().Split('-').Last()}",
                        UrlViewDeck = urlViewDeck
                    });
                }
                else
                {
                    // No class means Header row
                    currentTournament = r.SelectSingleNode("./th[1]/a").InnerText.Trim();
                    result.Add(currentTournament, new List<DeckScraperDeckInputs>());
                }
            }

            return result;
        }
    }
}