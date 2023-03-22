using MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish;

namespace MTGAHelper.Lib.Scraping.DeckSources.Factory
{
    public class DeckScraperMtgGoldfishSingleScoopFactory
    {
        private readonly DeckScraperMtgGoldfishSingleScoop scraper;

        public DeckScraperMtgGoldfishSingleScoopFactory(DeckScraperMtgGoldfishSingleScoop scraper)
        {
            this.scraper = scraper;
        }

        public DeckScraperMtgGoldfishSingleScoop Create()
        {
            return scraper;
        }
    }
}