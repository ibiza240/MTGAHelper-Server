using System.Collections.Generic;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Lib.CardProviders;

namespace MTGAHelper.Lib.InventoryUpdatedConverters
{
    public class CraftedCardsConverter
    {
        readonly IReadOnlyDictionary<int, Card> allCards;
        
        public CraftedCardsConverter(ICardRepository cardRepo)
        {
            allCards = cardRepo;
        }

        public ICollection<Card> Convert(InventoryUpdatedRaw raw)
        {
            var cards = raw.updates.SelectMany(i => i.delta.cardsAdded)
                .Select(i => allCards[i])
                .ToArray();

            return cards;
        }
    }
}
