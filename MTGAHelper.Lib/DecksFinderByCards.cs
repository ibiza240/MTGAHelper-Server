using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Config.Decks;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Lib
{
    public class DecksFinderByCards
    {
        private readonly IReadOnlyDictionary<int, Card> allCards;
        private readonly IReadOnlyCollection<ConfigModelDeck> decks;

        public DecksFinderByCards(
            ICardRepository cardRepo,
            ConfigManagerDecks configDecks
            )
        {
            this.allCards = cardRepo;
            this.decks = configDecks.Get();
        }

        public ICollection<ConfigModelDeck> GetDecksByCards(ICollection<string> cards)
        {
            var results = decks
                .Where(i => cards.Count(c => i.Cards.Any(j => allCards[j.GrpId].Name == c)) >= (0.75d * cards.Count))
                .OrderByDescending(i => i.DateScrapedUtc)
                .ToArray();

            return results;
        }
    }
}