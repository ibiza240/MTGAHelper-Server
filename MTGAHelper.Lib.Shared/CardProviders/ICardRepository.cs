#nullable enable
using MTGAHelper.Entity;
using System.Collections.Generic;

namespace MTGAHelper.Lib.CardProviders;

public interface ICardRepository : IReadOnlyDictionary<int, Card>
{
    IReadOnlyCollection<Card> CardsByName(string name);

    IReadOnlyCollection<Card> FindNameStartingWith(string firstPartOfName);
}