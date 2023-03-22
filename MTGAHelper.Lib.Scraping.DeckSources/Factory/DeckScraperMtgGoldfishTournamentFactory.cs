using MTGAHelper.Entity;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib.Scraping.DeckSources.MtgGoldfish;

namespace MTGAHelper.Lib.Scraping.DeckSources.Factory
{
    public class DeckScraperMtgGoldfishTournamentFactory
    {
        private readonly DeckScraperMtgGoldfishTournament scraper;

        public DeckScraperMtgGoldfishTournamentFactory(DeckScraperMtgGoldfishTournament scraper)
        {
            this.scraper = scraper;
        }

        public DeckScraperMtgGoldfishTournament Create(ScraperTypeFormatEnum format)
        {
            scraper.Init(format, MtgGoldfishArticleEnum.Tournaments);
            return scraper;
        }
    }
}