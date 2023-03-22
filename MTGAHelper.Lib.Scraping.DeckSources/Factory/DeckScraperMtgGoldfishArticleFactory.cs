using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish;

namespace MTGAHelper.Lib.Scraping.DeckSources.Factory
{
    public class DeckScraperMtgGoldfishArticleFactory
    {
        private readonly IDeckScraperMtgGoldfishArticle scraper;

        public DeckScraperMtgGoldfishArticleFactory(IDeckScraperMtgGoldfishArticle scraper)
        {
            this.scraper = scraper;
        }

        public IDeckScraperMtgGoldfishArticle Create(MtgGoldfishArticleEnum articleType)
        {
            return scraper.Init(articleType);
        }
    }
}