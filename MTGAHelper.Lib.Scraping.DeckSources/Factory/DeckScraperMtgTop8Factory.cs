using MTGAHelper.Lib.Scraping.DeckSources.MtgTop8;

namespace MTGAHelper.Lib.Scraping.DeckSources.Factory
{
    public class DeckScraperMtgTop8Factory : IDeckScraperFactory
    {
        private readonly DeckScraperMtgTop8DecksToBeat scraperDecksToBeat;

        public DeckScraperMtgTop8Factory(DeckScraperMtgTop8DecksToBeat scraperDecksToBeat)
        {
            this.scraperDecksToBeat = scraperDecksToBeat;
        }

        public IDeckScraper Create()
        {
            return scraperDecksToBeat;
        }
    }
}