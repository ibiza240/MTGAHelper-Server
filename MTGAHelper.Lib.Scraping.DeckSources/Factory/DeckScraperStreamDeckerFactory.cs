using MTGAHelper.Lib.Scraping.DeckSources.Streamdecker;

namespace MTGAHelper.Lib.Scraping.DeckSources.Factory
{
    public class DeckScraperStreamDeckerFactory
    {
        private readonly IDeckScraperStreamDecker scraper;

        public DeckScraperStreamDeckerFactory(IDeckScraperStreamDecker scraper)
        {
            this.scraper = scraper;
        }

        public IDeckScraperStreamDecker Create(string name)
        {
            return scraper.Init(name);
        }
    }
}