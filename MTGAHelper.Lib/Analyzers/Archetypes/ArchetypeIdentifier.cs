using System;
using System.Collections.Generic;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Archetypes;
using MTGAHelper.Lib.CardProviders;

namespace MTGAHelper.Lib.Analyzers.Archetypes
{
    public class ArchetypeIdentifier
    {
        readonly IReadOnlyDictionary<int, Card> dictAllCards;
        readonly UtilColors utilColors;

        IList<Archetype> archetypesIdentified;

        public ArchetypeIdentifier(ICardRepository cardRepo, UtilColors utilColors)
        {
            dictAllCards = cardRepo;
            this.utilColors = utilColors;
        }

        public ICollection<Archetype> Identify(string set, ICollection<int> grpIds)
        {
            archetypesIdentified = new List<Archetype>();

            var cards = grpIds
                .Select(i => dictAllCards[i])
                .ToArray();

            foreach (var archetype in Archetype.DefaultList)
                Compare(archetype, cards);

            return archetypesIdentified;
        }

        void Compare(Archetype archetype, Card[] cards)
        {
            var cardNames = cards.Select(i => i.Name).ToArray();

            if (archetype.Cards.All(i => cardNames.Contains(i)) &&
                archetype.CardsExcluded.Any(i => cardNames.Contains(i)) == false &&
                utilColors.FromCards(cards) == archetype.Color)
            {
                archetypesIdentified.Add(archetype);
            }
        }
    }
}
