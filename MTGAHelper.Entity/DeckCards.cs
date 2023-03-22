using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace MTGAHelper.Entity
{
    public class DeckCards
    {
        public ICollection<DeckCard> All { get; protected set; }

        //public ICollection<DeckCard> AllExceptBasicLands { get; protected set; }
        public Dictionary<int, DeckCard> QuickCardsMain { get; protected set; }

        public Dictionary<int, DeckCard> QuickCardsSideboard { get; protected set; }
        public DeckCard QuickCardCommander { get; protected set; }
        public DeckCard QuickCardCompanion { get; protected set; }
        public string ColorsInDeck { get; protected set; }

        public DeckCards(IEnumerable<DeckCard> cards)
        {
            try
            {
                All = cards
                    //.OrderBy(i => i.Card.cmc)
                    //.ThenBy(i => i.Card.name)
                    .OrderBy(i => i.Card.Name)
                    .ToArray();

                //AllExceptBasicLands = All
                //    .Where(i => i.Card.type.StartsWith("Basic Land") == false)
                //    .ToArray();

                QuickCardsMain = All
                    .Where(i => i.Zone == DeckCardZoneEnum.Deck)
                    .GroupBy(i => i.Card.GrpId)
                    .ToDictionary(i => i.Key, i =>
                    {
                        var c = i.First();
                        return new DeckCard
                        {
                            Card = c.Card,
                            Amount = i.Sum(i => i.Amount),
                            Zone = c.Zone,
                        };
                    });

                QuickCardsSideboard = All
                    .Where(i => i.Zone == DeckCardZoneEnum.Sideboard)
                    .GroupBy(i => i.Card.GrpId)
                    .ToDictionary(i => i.Key, i =>
                    {
                        var c = i.First();
                        return new DeckCard
                        {
                            Card = c.Card,
                            Amount = i.Sum(i => i.Amount),
                            Zone = c.Zone,
                        };
                    });

                var colorsInDeck = All.Select(c => c.Card)
                    .Where(c => c.Colors != null && c.Colors.Any())
                    .SelectMany(c => c.Colors)
                    .Distinct()
                    .OrderBy(c => c);
                ColorsInDeck = string.Concat(colorsInDeck);

                QuickCardCommander = All.FirstOrDefault(i => i.Zone == DeckCardZoneEnum.Commander);

                QuickCardCompanion = All.FirstOrDefault(i => i.Zone == DeckCardZoneEnum.Companion);
            }
            catch (Exception ex)
            {
                All = All ?? new DeckCard[0];
                QuickCardsMain = QuickCardsMain ?? new Dictionary<int, DeckCard>();
                QuickCardsSideboard = QuickCardsSideboard ?? new Dictionary<int, DeckCard>();
                ColorsInDeck = ColorsInDeck ?? "";
            }
        }
    }
}