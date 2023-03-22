using System.Collections.Generic;
using System.Linq;
using MTGAHelper.Entity;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Lib.CardProviders;

namespace MTGAHelper.Lib.InventoryUpdatedConverters
{
    public class BoosterOpenConverter
    {
        readonly IReadOnlyDictionary<int, Card> allCards;
        
        public BoosterOpenConverter(ICardRepository cardRepo)
        {
            allCards = cardRepo;
        }

        public BoosterOpened Convert(InventoryUpdatedRaw raw)
        {
            var lstCardsAdded = raw.updates
                .SelectMany(i => i.delta.cardsAdded)
                .ToList();

            var lstCardsAetherized = raw.updates
                .SelectMany(i => i.aetherizedCards)
                .Select(i => i.grpId)
                .ToList();

            var openedCards = new List<BoosterOpenedCard>();
            //foreach (var c in lstCardsAdded)
            //{
            //    var aetherizedIndex = lstCardsAetherized.IndexOf(c);
            //    if (aetherizedIndex >= 0)
            //    {
            //        lstCardsAetherized.RemoveAt(aetherizedIndex);
            //        openedCards.Add(new BoosterOpenedCard
            //        {
            //            GrpId = c,
            //            IsAetherized = true,
            //            Wildcard = allCards[c].GetRarityEnum()
            //        });
            //    }
            //    else
            //    {
            //        openedCards.Add(new BoosterOpenedCard
            //        {
            //            GrpId = c,
            //            IsAetherized = false,
            //            Wildcard = RarityEnum.Unknown
            //        });
            //    }
            //}

            foreach (var c in lstCardsAetherized)
            {
                var indexAdded = lstCardsAdded.IndexOf(c);
                if (indexAdded >= 0)
                {
                    openedCards.Add(new BoosterOpenedCard
                    {
                        GrpId = c,
                        IsNew = true,
                        Rarity = allCards[c].Rarity,
                        IsWildcard = false
                    });
                }
                else
                {
                    openedCards.Add(new BoosterOpenedCard
                    {
                        GrpId = c,
                        IsNew = false,
                        Rarity = allCards[c].Rarity,
                        IsWildcard = false
                    });
                }
            }

            var missingCommons = 5 - openedCards.Count(i => i.Rarity == RarityEnum.Common);
            var missingUncommons = 2 - openedCards.Count(i => i.Rarity == RarityEnum.Uncommon);
            var missingRareOrMythic = 1 - openedCards.Count(i => i.Rarity == RarityEnum.Rare || i.Rarity == RarityEnum.Mythic);

            var totalWcC = raw.updates.Sum(i => i.delta.wcCommonDelta);
            var totalWcU = raw.updates.Sum(i => i.delta.wcUncommonDelta);
            var totalWcR = raw.updates.Sum(i => i.delta.wcRareDelta);
            var totalWcM = raw.updates.Sum(i => i.delta.wcMythicDelta);

            for (int i = 0; i < missingCommons; i++)
                openedCards.Add(new BoosterOpenedCard { IsWildcard = true, Rarity = RarityEnum.Common });

            for (int i = 0; i < missingUncommons; i++)
                openedCards.Add(new BoosterOpenedCard { IsWildcard = true, Rarity = RarityEnum.Uncommon });

            if (missingRareOrMythic < totalWcR + totalWcM)
                openedCards.Add(new BoosterOpenedCard { IsWildcard = true, Rarity = totalWcR > 0 ? RarityEnum.Rare : RarityEnum.Mythic });

            //var totalCardsCommon = openedCards.Count(i => allCards[i.GrpId].GetRarityEnum() == RarityEnum.Common);
            //var totalCardsUncommon = openedCards.Count(i => allCards[i.GrpId].GetRarityEnum() == RarityEnum.Uncommon);
            //var totalCardsRare = openedCards.Count(i => allCards[i.GrpId].GetRarityEnum() == RarityEnum.Rare);
            //var totalCardsMythic = openedCards.Count(i => allCards[i.GrpId].GetRarityEnum() == RarityEnum.Mythic);

            //foreach (var rarity in new Dictionary<RarityEnum, int>
            //{
            //    { RarityEnum.Common, 5 - totalCardsCommon },
            //    { RarityEnum.Uncommon, 2 - totalCardsUncommon },
            //    { RarityEnum.Rare, totalCardsRare },
            //    { RarityEnum.Mythic, totalCardsMythic },
            //})
            //    for (int i = 0; i < rarity.Value; i++)
            //        openedCards.Add(new BoosterOpenedCard { Wildcard = rarity.Key });

            var wcC = raw.updates.Sum(i => i.delta.wcCommonDelta) - openedCards.Count(i => i.IsWildcard && i.Rarity == RarityEnum.Common);
            var wcU = raw.updates.Sum(i => i.delta.wcUncommonDelta) - openedCards.Count(i => i.IsWildcard && i.Rarity == RarityEnum.Uncommon);
            var wcR = raw.updates.Sum(i => i.delta.wcRareDelta) - openedCards.Count(i => i.IsWildcard && i.Rarity == RarityEnum.Rare);
            var wcM = raw.updates.Sum(i => i.delta.wcMythicDelta) - openedCards.Count(i => i.IsWildcard && i.Rarity == RarityEnum.Mythic);
            var wildcardFromTrack = wcC > 0 ? RarityEnum.Common :
                 wcC > 0 ? RarityEnum.Common :
                  wcU > 0 ? RarityEnum.Uncommon :
                   wcR > 0 ? RarityEnum.Rare :
                    wcM > 0 ? RarityEnum.Mythic : RarityEnum.Unknown;

            var booster = new BoosterOpened
            {
                Set = allCards[openedCards.First().GrpId].Set,
                Cards = openedCards,
                VaultProgress = raw.updates.Sum(i => i.delta.vaultProgressDelta),
                WildcardFromTrack = wildcardFromTrack,
                Gems = raw.updates.Sum(i => i.delta.gemsDelta)
            };
            return booster;
        }
    }
}
