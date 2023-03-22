using MTGAHelper.Entity;
using MTGAHelper.Entity.DeckScraper;
using MTGAHelper.Lib.Scraping.DeckSources.Aetherhub;

namespace MTGAHelper.Lib.Scraping.DeckSources.Factory
{
    public interface IDeckScraperFactory
    {
    }

    public class DeckScraperAetherhubFactory : IDeckScraperFactory
    {
        private readonly DeckScraperAetherhubTierOne scraperTierOne;
        private readonly DeckScraperAetherhubMetaPaper scraperMetaPaper;
        private readonly DeckScraperAetherhubUserDecks scraperUser;
        private readonly DeckScraperAetherhubTournamentBo3 deckScraperAetherhubTournamentBo3;

        public DeckScraperAetherhubFactory(
            DeckScraperAetherhubTierOne scraperTierOne,
            DeckScraperAetherhubMetaPaper scraperMetaPaper,
            DeckScraperAetherhubUserDecks scraperUser,
            DeckScraperAetherhubTournamentBo3 deckScraperAetherhubTournamentBo3
            )
        {
            this.scraperTierOne = scraperTierOne;
            this.scraperMetaPaper = scraperMetaPaper;
            this.scraperUser = scraperUser;
            this.deckScraperAetherhubTournamentBo3 = deckScraperAetherhubTournamentBo3;
        }

        public IDeckScraperAetherhub Create(AetherhubListingEnum listingType, ScraperTypeFormatEnum format, string tokenUsername = null)
        {
            switch (listingType)
            {
                case AetherhubListingEnum.Tier1:
                    scraperTierOne.Init(listingType, format, tokenUsername);
                    return scraperTierOne;

                case AetherhubListingEnum.Meta:
                    scraperMetaPaper.Init(listingType, format, tokenUsername);
                    return scraperMetaPaper;

                case AetherhubListingEnum.User:
                    scraperUser.Init(listingType, format, tokenUsername);
                    return scraperUser;

                case AetherhubListingEnum.TournamentBo3:
                    deckScraperAetherhubTournamentBo3.Init(listingType, format, tokenUsername);
                    return deckScraperAetherhubTournamentBo3;

                default:
                    return null;
            }
        }
    }
}