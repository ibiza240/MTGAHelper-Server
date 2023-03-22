using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Lib
{
    internal class CompositeClearUserCache : IClearUserCache
    {
        private readonly CacheUserHistoryOld<HashSet<string>> cacheUserHistoryMtgaDecksFound;
        private readonly UserHistoryRepositoryGeneric<Dictionary<int, int>> repositoryCollection;

        //private readonly CacheUserHistoryOld<Inventory> cacheUserHistoryInventory;
        private readonly CacheUserHistoryOld<List<ConfigModelRankInfo>> cacheUserHistoryRank;

        private readonly CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches;
        private readonly UserHistoryRepositoryGeneric<Dictionary<string, PlayerProgress>> cacheUserHistoryPlayerProgress;

        private readonly CacheUserHistoryOld<List<PlayerQuest>> cacheUserHistoryPlayerQuests;
        //CacheUserHistoryOld<List<DraftMakePickRaw>> cacheUserHistoryDraftPickProgress;

        //readonly CacheUserHistory<Dictionary<int, int>> cacheUserHistoryCollectionIntraday;
        private readonly CacheUserHistory<GetCombinedRankInfoRaw> cacheUserHistoryCombinedRankInfo;

        //readonly CacheUserHistory<CrackBoosterRaw> cacheUserHistoryCrackBooster;
        private readonly CacheUserHistory<DraftPickStatusRaw> cacheUserHistoryDraftPickProgressIntraday;

        private readonly CacheUserHistory<EventClaimPrizeRaw> cacheUserHistoryEventClaimPrize;
        private readonly UserHistoryRepositoryGeneric<Inventory> cacheUserHistoryInventoryIntraday;
        private readonly CacheUserHistory<InventoryUpdatedRaw> cacheUserHistoryInventoryUpdated;

        //readonly CacheUserHistory<MythicRatingUpdatedRaw> cacheUserHistoryMythicRatingUpdated;
        //readonly CacheUserHistory<PayEntryRaw> cacheUserHistoryPayEntry;
        //private readonly CacheUserHistory<GetPlayerProgressRaw> cacheUserHistoryPlayerProgressIntraday;

        //private readonly CacheUserHistory<PostMatchUpdateRaw> cacheUserHistoryPostMatchUpdates;
        //private readonly CacheUserHistory<RankUpdatedRaw> cacheUserHistoryRankUpdated;
        //readonly CacheUserHistory<CompleteVaultRaw> cacheUserHistoryCompleteVault;

        public CompositeClearUserCache(
            CacheUserHistoryOld<HashSet<string>> cacheUserHistoryMtgaDecksFound,
            UserHistoryRepositoryGeneric<Dictionary<int, int>> repositoryCollection,
            //CacheUserHistoryOld<Inventory> cacheUserHistoryInventory,
            CacheUserHistoryOld<List<ConfigModelRankInfo>> cacheUserHistoryRank,
            CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches,
            UserHistoryRepositoryGeneric<Dictionary<string, PlayerProgress>> cacheUserHistoryPlayerProgress,
            CacheUserHistoryOld<List<PlayerQuest>> cacheUserHistoryPlayerQuests,
            //CacheUserHistoryOld<List<DraftMakePickRaw>> cacheUserHistoryDraftPickProgress,
            //CacheUserHistory<Dictionary<int, int>> cacheUserHistoryCollectionIntraday,
            CacheUserHistory<GetCombinedRankInfoRaw> cacheUserHistoryCombinedRankInfo,
            //CacheUserHistory<CrackBoosterRaw> cacheUserHistoryCrackBooster,
            CacheUserHistory<DraftPickStatusRaw> cacheUserHistoryDraftPickProgressIntraday,
            CacheUserHistory<EventClaimPrizeRaw> cacheUserHistoryEventClaimPrize,
            UserHistoryRepositoryGeneric<Inventory> cacheUserHistoryInventoryIntraday,
            CacheUserHistory<InventoryUpdatedRaw> cacheUserHistoryInventoryUpdated
            //CacheUserHistory<MythicRatingUpdatedRaw> cacheUserHistoryMythicRatingUpdated,
            //CacheUserHistory<PayEntryRaw> cacheUserHistoryPayEntry,
            //CacheUserHistory<GetPlayerProgressRaw> cacheUserHistoryPlayerProgressIntraday,
            //CacheUserHistory<PostMatchUpdateRaw> cacheUserHistoryPostMatchUpdates,
            //CacheUserHistory<RankUpdatedRaw> cacheUserHistoryRankUpdated
            //CacheUserHistory<CompleteVaultRaw> cacheUserHistoryCompleteVault
            )
        {
            this.cacheUserHistoryMtgaDecksFound = cacheUserHistoryMtgaDecksFound;
            this.repositoryCollection = repositoryCollection.Init(InfoByDateKeyEnum.Collection.ToString(), false);
            this.cacheUserHistoryInventoryUpdated = cacheUserHistoryInventoryUpdated;
            //this.cacheUserHistoryInventory = cacheUserHistoryInventory;
            this.cacheUserHistoryRank = cacheUserHistoryRank;
            //this.cacheUserHistoryDraftPickProgress = cacheUserHistoryDraftPickProgress.Init(folderDataConfigUsers);
            this.cacheUserHistoryMatches = cacheUserHistoryMatches;
            this.cacheUserHistoryPlayerProgress = cacheUserHistoryPlayerProgress.Init(InfoByDateKeyEnum.PlayerProgress.ToString(), false);
            this.cacheUserHistoryPlayerQuests = cacheUserHistoryPlayerQuests;
            //this.cacheUserHistoryCollectionIntraday = cacheUserHistoryCollectionIntraday;
            this.cacheUserHistoryCombinedRankInfo = cacheUserHistoryCombinedRankInfo;
            //this.cacheUserHistoryCrackBooster = cacheUserHistoryCrackBooster;
            this.cacheUserHistoryDraftPickProgressIntraday = cacheUserHistoryDraftPickProgressIntraday;
            this.cacheUserHistoryEventClaimPrize = cacheUserHistoryEventClaimPrize;
            this.cacheUserHistoryInventoryIntraday = cacheUserHistoryInventoryIntraday.Init(InfoByDateKeyEnum.InventoryIntraday.ToString(), false);
            //this.cacheUserHistoryMythicRatingUpdated = cacheUserHistoryMythicRatingUpdated;
            //this.cacheUserHistoryPayEntry = cacheUserHistoryPayEntry;
            //this.cacheUserHistoryPlayerProgressIntraday = cacheUserHistoryPlayerProgressIntraday;
            //this.cacheUserHistoryPostMatchUpdates = cacheUserHistoryPostMatchUpdates;
            //this.cacheUserHistoryRankUpdated = cacheUserHistoryRankUpdated;
            //this.cacheUserHistoryCompleteVault = cacheUserHistoryCompleteVault;
        }

        public void ClearCacheForUser(string userId)
        {
            repositoryCollection.Invalidate(userId);
            cacheUserHistoryInventoryIntraday.Invalidate(userId);
            cacheUserHistoryPlayerProgress.Invalidate(userId);

            var tasks = new[]
            {
                //cacheUserHistoryCollectionIntraday.InvalidateAll(userId),
                cacheUserHistoryCombinedRankInfo.InvalidateAll(userId),
                //cacheUserHistoryCompleteVault.InvalidateAll(userId),
                //cacheUserHistoryCrackBooster.InvalidateAll(userId),
                //cacheUserHistoryDraftPickProgress.InvalidateAll(userId),
                cacheUserHistoryDraftPickProgressIntraday.InvalidateAll(userId),
                cacheUserHistoryEventClaimPrize.InvalidateAll(userId),
                //cacheUserHistoryInventory.InvalidateAll(userId),
                cacheUserHistoryInventoryUpdated.InvalidateAll(userId),
                cacheUserHistoryMatches.InvalidateAll(userId),
                cacheUserHistoryMtgaDecksFound.InvalidateAll(userId),
                //cacheUserHistoryMythicRatingUpdated.InvalidateAll(userId),
                //cacheUserHistoryPayEntry.InvalidateAll(userId),
                //cacheUserHistoryPlayerProgressIntraday.InvalidateAll(userId),
                cacheUserHistoryPlayerQuests.InvalidateAll(userId),
                //cacheUserHistoryPostMatchUpdates.InvalidateAll(userId),
                cacheUserHistoryRank.InvalidateAll(userId),
                //cacheUserHistoryRankUpdated.InvalidateAll(userId)
            };

            Task.WaitAll(tasks);
        }

        public void FreeMemory()
        {
        }
    }
}