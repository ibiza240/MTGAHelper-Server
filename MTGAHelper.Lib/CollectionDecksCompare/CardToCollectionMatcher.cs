#nullable enable
using MTGAHelper.Entity;
using MTGAHelper.Entity.Services;
using MTGAHelper.Lib.CardProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Lib.CollectionDecksCompare;

public class CardToCollectionMatcher
{
    private readonly BasicLandIdentifier basicLandIdentifier;
    private readonly IReadOnlyDictionary<int, Card> cardRepo;
    private IReadOnlyCollection<CardWithAmount>? collection;
    private IReadOnlyCollection<int>? landsPreference;
    private bool landsPickAll;

    private readonly Random rnd = new();

    public CardToCollectionMatcher(
        BasicLandIdentifier basicLandIdentifier,
        ICardRepository cardRepo)
    {
        this.basicLandIdentifier = basicLandIdentifier;
        this.cardRepo = cardRepo;
    }

    public void Init(
        IReadOnlyCollection<CardWithAmount>? collection,
        IReadOnlyCollection<int> landsPreference,
        bool landsPickAll)
    {
        this.collection = collection;
        this.landsPreference = landsPreference;
        this.landsPickAll = landsPickAll;
    }

    public IEnumerable<CardWithAmount> FindCardsInCollection(IEnumerable<DeckCard> cards)
    {
        return collection == null || !collection.Any()
            ? cards
            : cards.SelectMany(FindCardInCollection);
    }

    private IEnumerable<CardWithAmount> FindCardInCollection(DeckCard toFind)
    {
        //if (c.Card.name == "Teferi, Hero of Dominaria") System.Diagnostics.Debugger.Break();

        var cardLand = TryParseLand(toFind);
        if (cardLand is not null)
        {
            return cardLand;
        }

        var matchingCards = collection.Where(i => i.Card.Name == toFind.Card.Name).ToArray();

        //if (c.Card.name.EndsWith("(a)"))
        //    matchingCards = matchingCards.Union(collection.Where(i => i.Card.name == c.Card.name.Replace("(a)", "(b)")));
        //else if (c.Card.name.EndsWith("(b)"))
        //    matchingCards = matchingCards.Union(collection.Where(i => i.Card.name == c.Card.name.Replace("(b)", "(a)")));

        if (matchingCards.Any() == false)
        {
            // Card not found in collection
            return Enumerable.Repeat(toFind, 1);
        }

        var nbCardsFound = 0;
        var collectionCards = new List<CardWithAmount>();
        //var a = collection.Where(i => i.Card.name == "Teferi, Hero of Dominaria").Count();
        foreach (var mc in matchingCards.OrderByDescending(c => c.Amount))
        {
            var nbCardsUsedInThisSet = Math.Min(toFind.Amount - nbCardsFound, mc.Amount);
            nbCardsFound += nbCardsUsedInThisSet;
            collectionCards.Add(new CardWithAmount(mc.Card, nbCardsUsedInThisSet));

            if (nbCardsFound == toFind.Amount)
                return collectionCards;
        }

        // Card is owned partially in collection
        var nbCardsNotInCollection = toFind.Amount - nbCardsFound;
        return collectionCards.Append(new CardWithAmount(toFind.Card, nbCardsNotInCollection));
    }

    private ICollection<CardWithAmount>? TryParseLand(DeckCard c)
    {
        if (!basicLandIdentifier.IsBasicLand(c.Card))
            return null;

        ICollection<CardWithAmount> TryDefaultBasicLand()
        {
            // respect the card in the deck if there are no alternatives in the collection
            // or if this specific one exists in the collection
            if (collection == null
                || collection.Any(col => col.Card.GrpId == c.Card.GrpId)
                || collection.All(col => col.Card.Name != c.Card.Name))
                return new[] {c};

            // add the newest basic land
            var landToUse = collection.Last(i => i.Card.Name == c.Card.Name).Card;
            return new[] {new CardWithAmount(landToUse, c.Amount)};
        }

        if (landsPreference == null || landsPreference.Count == 0)
            return TryDefaultBasicLand();

        IEnumerable<Card> favoriteLands = landsPreference
            .Select(pref => cardRepo.GetValueOrDefault(pref))
            .Where(card => card != null);
        var candidates = favoriteLands
            .Where(i => i.Name == c.Card.Name || i.Name == $"Snow-Covered {c.Card.Name}")
            .ToArray();

        if (candidates.Length <= 0)
            return TryDefaultBasicLand();

        var cards = landsPickAll
            ? LandsPickAll(candidates, c.Amount)
            : LandsPickOneRandom(candidates, c.Amount);
        return cards;
    }

    private ICollection<CardWithAmount> LandsPickOneRandom(IReadOnlyList<Card> candidates, int amount)
    {
        var landToUse = candidates[rnd.Next(candidates.Count)];
        return new[] {new CardWithAmount(landToUse, amount)};
    }

    private static ICollection<CardWithAmount> LandsPickAll(IReadOnlyCollection<Card> candidates, int amount)
    {
        if (amount < candidates.Count)
            return candidates.Take(amount).Select(c => new CardWithAmount(c, 1)).ToArray();

        var amountPerLand = amount / candidates.Count;
        var extra = amount - amountPerLand * candidates.Count;
        return candidates
            .Select(
                (card, idx) =>
                {
                    var count = idx < extra ? amountPerLand + 1 : amountPerLand;
                    return new CardWithAmount(card, count);
                })
            .ToArray();
    }
}