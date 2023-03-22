using MTGAHelper.Entity;
using MTGAHelper.Entity.Services;
using MTGAHelper.Lib.CardProviders;
using MTGAHelper.Lib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGAHelper.Lib
{
    public class RawDeckConverter
    {
        private readonly CardRepositoryProvider cardRepoProvider;
        private readonly BasicLandIdentifier basicLandIdentifier;

        public RawDeckConverter(
            CardRepositoryProvider cardRepoProvider,
            BasicLandIdentifier basicLandIdentifier
        )
        {
            this.cardRepoProvider = cardRepoProvider;
            this.basicLandIdentifier = basicLandIdentifier;
        }

        public IReadOnlyCollection<CardWithAmount> LoadCollection(IReadOnlyDictionary<int, int> info)
        {
            if (info == null)
                return Array.Empty<CardWithAmount>();

            var allCards = cardRepoProvider.GetRepository();
            
            return info
                .Where(i => allCards.ContainsKey(i.Key))
                .Select(i => SelectCardWithAmount(i.Key, i.Value))
                .Where(i => i.Card.IsRebalanced == false)
                .Where(i => basicLandIdentifier.IsBasicLand(i.Card) == false)
                .Where(i => i.Card.IsToken == false)
                .Where(i => i.Card.LinkedFaceType != enumLinkedFace.SplitCard)
                .Where(i => i.Card.LinkedFaceType != enumLinkedFace.DFC_Front)
                .OrderBy(i => i.Card.Name)
                .ToArray();

            CardWithAmount SelectCardWithAmount(int grpId, int amount)
            {
                try
                {
                    return new CardWithAmount(allCards[grpId], amount);
                }
                catch (Exception ex)
                {
                    throw new CardsLoaderException($"Problem loading card {grpId} in collection", ex);
                }
            }
        }
    }
}