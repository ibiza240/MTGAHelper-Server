using MTGAHelper.Entity;
using System.Collections.Generic;

#nullable enable
namespace MTGAHelper.Lib.CardProviders;

public class CardRepositoryProvider
{
    private readonly CacheSingleton<IReadOnlyDictionary<int, Card>> _cardCache;
    private CardRepositoryFromDict? _cachedProvider;

    public CardRepositoryProvider(CacheSingleton<IReadOnlyDictionary<int, Card>> cardCache)
    {
        _cardCache = cardCache;
    }

    public ICardRepository GetRepository()
    {
        var freshDict = _cardCache.Get();
        if (_cachedProvider is null || !ReferenceEquals(freshDict, _cachedProvider.CardsById))
        {
            _cachedProvider = new CardRepositoryFromDict(freshDict);
        }
        return _cachedProvider;
    }
}