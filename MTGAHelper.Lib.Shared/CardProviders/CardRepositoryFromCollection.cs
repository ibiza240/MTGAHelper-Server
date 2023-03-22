#nullable enable
using MTGAHelper.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MTGAHelper.Lib.CardProviders;

public class CardRepositoryFromCollection : ICardRepository
{
    private static readonly IComparer<string> COMPARER = StringComparer.OrdinalIgnoreCase; 
    internal readonly IReadOnlyCollection<Card> Cards;
    private readonly Lazy<Card[]> _orderedByName;
    private readonly Lazy<IReadOnlyDictionary<int, Card>> _cardsById;

    public CardRepositoryFromCollection(IReadOnlyCollection<Card> cards)
    {
        this.Cards = cards;
        this._orderedByName = new Lazy<Card[]>(() => this.Cards.OrderBy(x => x.Name, COMPARER).ToArray());
        this._cardsById = new Lazy<IReadOnlyDictionary<int, Card>>(() => Cards.ToDictionary(x => x.GrpId));
    }

    public IReadOnlyCollection<Card> CardsByName(string name)
    {
        var orderedByName = _orderedByName.Value;
        return SortedArrayHelper.BinarySearchContiguousEquals(orderedByName, c => c.Name, name, COMPARER);
    }

    public IReadOnlyCollection<Card> FindNameStartingWith(string firstPartOfName)
    {
        var orderedByName = _orderedByName.Value;
        return SortedArrayHelper.BinarySearchContiguousEquals(orderedByName, c => ChopEnd(c.Name, firstPartOfName.Length), firstPartOfName, COMPARER);

        string ChopEnd(string name, int length)
        {
            return name.Length > length ? name[..length] : name;
        }
    }

    public bool ContainsKey(int key)
    {
        return _cardsById.Value.ContainsKey(key);
    }

    public bool TryGetValue(int key, [MaybeNullWhen(false)] out Card value)
    {
        return _cardsById.Value.TryGetValue(key, out value);
    }

    public Card this[int grpId] => _cardsById.Value.GetValueOrDefault(grpId, Card.Unknown);

    IEnumerable<int> IReadOnlyDictionary<int, Card>.Keys => Cards.Select(c => c.GrpId);
    IEnumerable<Card> IReadOnlyDictionary<int, Card>.Values => Cards;

    public int Count => Cards.Count;

    IEnumerator<KeyValuePair<int, Card>> IEnumerable<KeyValuePair<int, Card>>.GetEnumerator()
    {
        return _cardsById.Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) _cardsById.Value).GetEnumerator();
    }
}