using MTGAHelper.Entity;
using MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish;

namespace MTGAHelper.Lib.Scraping.DeckSources.Factory
{
    public class DeckScraperMtgGoldfishMetaFactory
    {
        private readonly IDeckScraperMtgGoldfishMeta scraper;

        public DeckScraperMtgGoldfishMetaFactory(IDeckScraperMtgGoldfishMeta scraper)
        {
            this.scraper = scraper;
        }

        public IDeckScraperMtgGoldfishMeta Create(ScraperTypeFormatEnum format)
        {
            return scraper.Init(format);
        }
    }
}