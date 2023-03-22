using MTGAHelper.Lib.Scraping.DeckSources.MtgDecks;

namespace MTGAHelper.Lib.Scraping.DeckSources.Factory
{
    public class DeckScraperMtgDecksMetaFullFactory
    {
        private readonly IDeckScraperMtgDecksMetaFull scraper;

        public DeckScraperMtgDecksMetaFullFactory(IDeckScraperMtgDecksMetaFull scraper)
        {
            this.scraper = scraper;
        }

        public IDeckScraperMtgDecksMetaFull Create()
        {
            return scraper;
        }
    }

    //public class DeckScraperMtgDecksAverageArchetypeFactory
    //{
    //    private readonly IDeckScraperMtgDecksAverageArchetype scraper;
    //    public DeckScraperMtgDecksAverageArchetypeFactory(IDeckScraperMtgDecksAverageArchetype scraper)
    //    {
    //        this.scraper = scraper;
    //    }

    //    public IDeckScraperMtgDecksAverageArchetype Create(ICollection<Card> allCards, string directory)
    //    {
    //        return scraper.Init(allCards, directory);
    //    }
    //}
}