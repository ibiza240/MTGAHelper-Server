#nullable enable
using MTGAHelper.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MTGAHelper.Lib.CardProviders;

public class CardRepositoryFromDict : ICardRepository
{
    private static readonly IComparer<string> COMPARER = StringComparer.OrdinalIgnoreCase; 
    internal readonly IReadOnlyDictionary<int, Card> CardsById;
    private readonly Lazy<Card[]> _orderedByName;

    internal CardRepositoryFromDict(IReadOnlyDictionary<int,Card> cardsById)
    {
        this.CardsById = cardsById;
        this._orderedByName = new Lazy<Card[]>(() => this.CardsById.Values.OrderBy(x => x.Name, COMPARER).ToArray());
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
        return CardsById.ContainsKey(key);
    }

    public bool TryGetValue(int key, [MaybeNullWhen(false)] out Card value)
    {
        return CardsById.TryGetValue(key, out value);
    }

    public Card this[int grpId] => CardsById.GetValueOrDefault(grpId, Card.Unknown);

    IEnumerable<int> IReadOnlyDictionary<int, Card>.Keys => CardsById.Keys;
    IEnumerable<Card> IReadOnlyDictionary<int, Card>.Values => CardsById.Values;

    public int Count => CardsById.Count;

    IEnumerator<KeyValuePair<int, Card>> IEnumerable<KeyValuePair<int, Card>>.GetEnumerator()
    {
        return CardsById.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) CardsById).GetEnumerator();
    }
}