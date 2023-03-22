using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using MTGAHelper.Entity.Config.App;

namespace MTGAHelper.Lib.Scraping.DeckSources.Aetherhub
{
    public class DeckScraperAetherhubUserDecks : DeckScraperAetherhubBase
    {
        public DeckScraperAetherhubUserDecks(
            IDataPath configPath,
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter,
            IDateDeconstructor dateDeconstructor)
            : base(configPath, writerDeck, converter, dateDeconstructor)
        {
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            var decksInfo = base.GetDeckList(sliceStart);

            var info = decksInfo
                //.Take(10)   // DEBUG
                .GroupBy(i => i.Name.ToUpper());

            var totalSkipped = 0;
            var duplicates = info.Where(i => i.Count() > 1);
            foreach (var i in duplicates)
            {
                var nbSkipped = i.Count() - 1;
                totalSkipped += nbSkipped;
                var nbSame = i.Count();
                var deckName = i.Key;
                AddWarning($"{ScraperType} [slice {sliceStart}] found {nbSame} decks with name {deckName} ({nbSkipped} skipped)", nbSkipped);
            }

            if (totalSkipped > 0)
                Log.Information("{ScraperType} [slice {sliceStart}] skipped a total of {totalSkipped} decks", ScraperType, sliceStart, totalSkipped);

            var decksToDownload = info
            .Select(i => i.First())
            .ToArray();

            return decksToDownload;
        }
    }
}