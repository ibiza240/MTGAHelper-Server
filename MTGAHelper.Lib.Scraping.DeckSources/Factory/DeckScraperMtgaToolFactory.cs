using MTGAHelper.Entity;
using MTGAHelper.Lib.Scraping.DeckSources.MtgaTool;

namespace MTGAHelper.Lib.Scraping.DeckSources.Factory
{
    public class DeckScraperMtgaToolFactory
    {
        private readonly DeckScraperMtgaTool scraper;

        public DeckScraperMtgaToolFactory(DeckScraperMtgaTool scraper)
        {
            this.scraper = scraper;
        }

        public DeckScraperMtgaTool Create(ScraperTypeFormatEnum format)
        {
            return scraper.Init(format);
        }
    }
}