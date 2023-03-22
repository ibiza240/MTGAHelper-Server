using System;
using System.Collections.Generic;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Analyzers;
using MTGAHelper.Entity.Config.Decks;
using MTGAHelper.Lib.CardProviders;

namespace MTGAHelper.Lib.Analyzers.Cards
{
    public class DecksAnalyzer
    {
        const double SIMILAR_THRESHOLD = 0.55d;

        readonly ICardRepository cardRepo;
        readonly UtilColors utilColors;

        public DecksAnalyzer(ICardRepository cardRepo, UtilColors utilColors)
        {
            this.cardRepo = cardRepo;
            this.utilColors = utilColors;
        }

        public DecksAnalyzerResult GetResults(ICollection<ConfigModelDeck> decks)
        {
            var cardsInfo = GetCardsInfo(decks.Select(i => i.Deck));
            var decksInfo = GetDecksInfo(decks);

            var result = new DecksAnalyzerResult
            {
                Cards = cardsInfo,
                Archetypes = decksInfo,
            };

            return result;
        }

        ICollection<DecksAnalyzerResultByDeck> GetDecksInfo(ICollection<ConfigModelDeck> decks)
        {
            var similarities = CalcDecksSimilarities(decks.OrderBy(i => i.ScraperTypeOrderIndex).Select(i => i.Deck));
            var decksGrouped = decks.GroupBy(i => similarities[i.Deck].SimilarityKey);

            var result = decksGrouped
                .Select(i =>
                {
                    var decksInfo = i.Select(x => new DeckInfo
                    {
                        ConfigDeck = x,
                        Similars = similarities[x.Deck].Similars,//.Where(y => y.Similarity >= SIMILAR_THRESHOLD).ToArray(),
                    }).ToArray();

                    return new DecksAnalyzerResultByDeck
                    {
                        GroupKey = i.Key,
                        Decks = decksInfo,
                        Cards = GetCardsInfo(i.Select(x => x.Deck))
                    };
                })
                .OrderBy(i => i.Decks.Min(x => x.ConfigDeck.ScraperTypeOrderIndex))
                .ToArray();

            return result;
        }

        ICollection<DecksAnalyzerResultByCard> GetCardsInfo(IEnumerable<IDeck> decks)
        {
            var cardsMain = decks
                .SelectMany(i => i.Cards.QuickCardsMain.Values.Select(x => x.Card))
                .Where(i => i.Type.Contains("Land") == false)
                .Distinct();

            var resultsByCard = cardsMain
                .Select(i => new DecksAnalyzerResultByCard
                {
                    Card = i,
                    Decks = decks.Where(d => d.Cards.QuickCardsMain.ContainsKey(i.GrpId)).ToArray()
                })
                .OrderByDescending(i => i.Decks.Count)
                .ToArray();


            return resultsByCard;
        }

        Dictionary<IDeck, SimilarityGroup> CalcDecksSimilarities(IEnumerable<IDeck> decks)
        {
            var result = decks.ToDictionary(i => i, i => new SimilarityGroup());

            foreach (var d in decks)
            {
                var similar = decks.Where(i => i != d)
                    .Select(deckToCompare => new DeckWithSimilarity
                    {
                        Deck = deckToCompare,
                        Similarity = CalcDeckSimilarity(d, deckToCompare)
                    })
                    .OrderByDescending(i => i.Similarity)
                    .ToList();

                result[d] = new SimilarityGroup { Similars = similar };
            }

            var keys = new Dictionary<string, string>();
            foreach (var r in result)
            {
                //if (decks.Single(i => i.Id == r.Key).Name.Contains("Gruul"))
                //    System.Diagnostics.Debugger.Break();

                if (keys.ContainsKey(r.Key.Id) == false)
                {
                    var similarDecks = r.Value.Similars.Where(y => utilColors.FromDeck(y.Deck) == utilColors.FromDeck(r.Key) && y.Similarity >= SIMILAR_THRESHOLD).Select(y => y.Deck.Id);
                    var key = utilColors.FromDeck(r.Key) + string.Join("_", similarDecks.Union(new[] { r.Key.Id }).OrderBy(y => y));
                    foreach (var k in similarDecks)
                        if (keys.ContainsKey(k) == false)
                            keys[k] = key;

                    keys[r.Key.Id] = key;
                }

                r.Value.SimilarityKey = keys[r.Key.Id];
            }

            return result;
        }

        double CalcDeckSimilarity(IDeck deck1, IDeck deck2)
        {
            var nbCardsSame = 0;
            var cardsToCompare = deck1.Cards.QuickCardsMain.Values.Where(i => i.Card.Type.Contains("Land") == false);
            foreach (var card1 in cardsToCompare)
            {
                var card1Key = card1.Card.GrpId;
                if (deck2.Cards.QuickCardsMain.ContainsKey(card1Key))
                {
                    var card2 = deck2.Cards.QuickCardsMain[card1Key];
                    nbCardsSame += Math.Min(card1.Amount, card2.Amount);
                }
            }

            //if (nbCardsSame > cardsToCompare.Sum(i => i.Amount) || nbCardsSame < 0)
            //    System.Diagnostics.Debugger.Break();

            return (double)nbCardsSame / cardsToCompare.Sum(i => i.Amount);
        }
    }
}
