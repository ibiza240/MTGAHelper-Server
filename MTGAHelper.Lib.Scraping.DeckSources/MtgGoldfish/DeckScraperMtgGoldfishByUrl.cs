using HtmlAgilityPack;
using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using MTGAHelper.Lib.IO.Writer;
using MTGAHelper.Lib.TextDeck;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MTGAHelper.Entity.Config.Decks;

namespace MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish
{
    public class DeckScraperMtgGoldfishByUrl : DeckScraperMtgGoldfishBase
    {
        private IDateDeconstructor dateDeconstructor;

        protected override ScraperType ScraperType => new ScraperType(ScraperTypeEnum.MtgGoldfish, "ByUrl");

        public DeckScraperMtgGoldfishByUrl(
            IWriterDeck writerDeck,
            IMtgaTextDeckConverter converter,
            IDateDeconstructor dateDeconstructor)
            : base(writerDeck, converter)
        {
            this.dateDeconstructor = dateDeconstructor;
        }

        public (string name, string mtgaFormat) GetDeck(string url)
        {
            var info = DownloadDeckFromDeckView(url);
            var text = GetTextAreaContent(info.UrlDownloadDeck);
            return (info.Name, text);
        }

        protected override ICollection<DeckScraperDeckInputs> GetDeckList(int sliceStart)
        {
            throw new System.NotImplementedException();
        }

        protected override ConfigModelDeck GetDeck(DeckScraperDeckInputs input, int sliceStart)
        {
            throw new System.NotImplementedException();
        }
    }
}