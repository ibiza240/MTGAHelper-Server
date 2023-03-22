using MTGAHelper.Entity;
using MTGAHelper.Lib.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.UserHistory;
using MTGAHelper.Entity.Services;
using MTGAHelper.Lib.CardProviders;

namespace MTGAHelper.Lib.UserHistory
{
    public class UserHistoryParser
    {
        LockableOutputLogResult historyDetails;
        readonly IReadOnlyDictionary<int, Card> cardsByGrpId;
        private readonly BasicLandIdentifier basicLandIdentifier;

        public UserHistoryParser(
            ICardRepository cardRepo,
            BasicLandIdentifier basicLandIdentifier
            )
        {
            cardsByGrpId = cardRepo;
            this.basicLandIdentifier = basicLandIdentifier;
        }

        public UserHistoryParser Init(LockableOutputLogResult historyDetails)
        {
            this.historyDetails = historyDetails;
            return this;
        }

        public ICollection<DateSnapshot> GetUserHistory()
        {
            try
            {
                var result = new List<DateSnapshot>();

                DateSnapshotInfo previous = null;
                foreach (var s in historyDetails.BuildHistory())
                {
                    var snapshot = new DateSnapshot(s);

                    if (previous == null)
                    {
                        // First point (initial load) special case
                        snapshot.Diff = new DateSnapshotDiff(s.Collection)
                        {
                            GemsChange = s.Inventory.Gems,
                            GoldChange = s.Inventory.Gold,
                            VaultProgressChange = s.Inventory.VaultProgress,
                            WildcardsChange = s.Inventory.Wildcards,
                        };
                    }
                    else
                        snapshot.Diff = ComputeDiff(s, previous);

                    result.Add(snapshot);

                    previous = s;
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debugger.Break();
                return Array.Empty<DateSnapshot>();
            }
        }

        private DateSnapshotDiff ComputeDiff(DateSnapshotInfo current, DateSnapshotInfo previous)
        {
            var newCards = new Dictionary<int, int>();

            foreach (var currentCard in current.Collection.Where(i => cardsByGrpId.ContainsKey(i.Key) == false || basicLandIdentifier.IsBasicLand(cardsByGrpId[i.Key]) == false))
            {
                if (previous.Collection.ContainsKey(currentCard.Key) == false)
                {
                    // New card that was not owned previously
                    newCards.Add(currentCard.Key, currentCard.Value);
                }
                else
                {
                    var newCopies = currentCard.Value - previous.Collection[currentCard.Key];
                    if (newCopies > 0)
                    {
                        // New card that we had at least 1 copy of before
                        newCards.Add(currentCard.Key, newCopies);
                    }
                }
            }

            var diff = new DateSnapshotDiff(newCards);
            if (previous != null)
            {
                diff.GoldChange = current.Inventory.Gold - previous.Inventory.Gold;
                diff.GemsChange = current.Inventory.Gems - previous.Inventory.Gems;
                diff.VaultProgressChange = current.Inventory.VaultProgress - previous.Inventory.VaultProgress;
                diff.WildcardsChange[RarityEnum.Mythic] = current.Inventory.Wildcards[RarityEnum.Mythic] - previous.Inventory.Wildcards[RarityEnum.Mythic];
                diff.WildcardsChange[RarityEnum.Rare] = current.Inventory.Wildcards[RarityEnum.Rare] - previous.Inventory.Wildcards[RarityEnum.Rare];
                diff.WildcardsChange[RarityEnum.Uncommon] = current.Inventory.Wildcards[RarityEnum.Uncommon] - previous.Inventory.Wildcards[RarityEnum.Uncommon];
                diff.WildcardsChange[RarityEnum.Common] = current.Inventory.Wildcards[RarityEnum.Common] - previous.Inventory.Wildcards[RarityEnum.Common];

                var currentXpByTrack = current.PlayerProgress.ToDictionary(i => i.Key, i => i.Value.CurrentLevel * 1000 + i.Value.CurrentExp);
                var previousXpByTrack = previous.PlayerProgress.ToDictionary(i => i.Key, i => i.Value.CurrentLevel * 1000 + i.Value.CurrentExp);
                diff.XpChangeByTrack = currentXpByTrack.ToDictionary(i => i.Key, i => i.Value - (previousXpByTrack.ContainsKey(i.Key) ? previousXpByTrack[i.Key] : 0));
            }

            return diff;
        }
    }
}