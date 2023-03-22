using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Persist
{
    public class LogResultPersister : ILogResultPersister
    {
        private readonly UserHistoryDatesAvailable userHistoryDatesAvailable;

        private readonly UserMtgaDeckRepository cacheUserAllMtgaDecks;

        private readonly CacheUserHistoryOld<HashSet<string>> cacheUserHistoryMtgaDecksFound;
        private readonly UserHistoryRepositoryGeneric<InfoByDate<IReadOnlyDictionary<int, int>>> repositoryCollection;

        //private readonly CacheUserHistoryOld<Inventory> cacheUserHistoryInventory;
        private readonly CacheUserHistoryOld<List<ConfigModelRankInfo>> cacheUserHistoryRank;

        private readonly CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches;
        private readonly UserHistoryRepositoryGeneric<InfoByDate<Dictionary<string, PlayerProgress>>> cacheUserHistoryPlayerProgress;
        private readonly CacheUserHistoryOld<List<PlayerQuest>> cacheUserHistoryPlayerQuests;

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

        private readonly CacheUserHistory<PostMatchUpdateRaw> cacheUserHistoryPostMatchUpdates;
        private readonly StatsLimitedRepository statsLimitedRepository;

        //private readonly CacheUserHistory<RankUpdatedRaw> cacheUserHistoryRankUpdated;
        // readonly CacheUserHistory<CompleteVaultRaw> cacheUserHistoryCompleteVault;

        public LogResultPersister(
            UserHistoryDatesAvailable userHistoryDatesAvailable,
            UserMtgaDeckRepository cacheUserAllMtgaDecks,
            CacheUserHistoryOld<HashSet<string>> cacheUserHistoryMtgaDecksFound,
            UserHistoryRepositoryGeneric<InfoByDate<IReadOnlyDictionary<int, int>>> repositoryCollection,
            CacheUserHistory<InventoryUpdatedRaw> cacheUserHistoryInventoryUpdated,
            //CacheUserHistoryOld<Inventory> cacheUserHistoryInventory,
            CacheUserHistoryOld<List<ConfigModelRankInfo>> cacheUserHistoryRank,
            CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches,
            UserHistoryRepositoryGeneric<InfoByDate<Dictionary<string, PlayerProgress>>> cacheUserHistoryPlayerProgress,
            CacheUserHistoryOld<List<PlayerQuest>> cacheUserHistoryPlayerQuests,
            //CacheUserHistory<Dictionary<int, int>> cacheUserHistoryCollectionIntraday,
            CacheUserHistory<GetCombinedRankInfoRaw> cacheUserHistoryCombinedRankInfo,
            //CacheUserHistory<CrackBoosterRaw> cacheUserHistoryCrackBooster,
            CacheUserHistory<DraftPickStatusRaw> cacheUserHistoryDraftPickProgressIntraday,
            CacheUserHistory<EventClaimPrizeRaw> cacheUserHistoryEventClaimPrize,
            UserHistoryRepositoryGeneric<Inventory> cacheUserHistoryInventoryIntraday,
            //CacheUserHistory<MythicRatingUpdatedRaw> cacheUserHistoryMythicRatingUpdated,
            //CacheUserHistory<PayEntryRaw> cacheUserHistoryPayEntry,
            //CacheUserHistory<GetPlayerProgressRaw> cacheUserHistoryPlayerProgressIntraday,
            CacheUserHistory<PostMatchUpdateRaw> cacheUserHistoryPostMatchUpdates,
            //CacheUserHistory<RankUpdatedRaw> cacheUserHistoryRankUpdated,
            //CacheUserHistory<CompleteVaultRaw> cacheUserHistoryCompleteVault
            StatsLimitedRepository statsLimitedRepository
            )
        {
            this.userHistoryDatesAvailable = userHistoryDatesAvailable;

            this.cacheUserAllMtgaDecks = cacheUserAllMtgaDecks;
            this.cacheUserHistoryMtgaDecksFound = cacheUserHistoryMtgaDecksFound;
            this.repositoryCollection = repositoryCollection.Init(InfoByDateKeyEnum.Collection.ToString(), false);
            this.cacheUserHistoryInventoryUpdated = cacheUserHistoryInventoryUpdated;
            //this.cacheUserHistoryInventory = cacheUserHistoryInventory;
            this.cacheUserHistoryRank = cacheUserHistoryRank;
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
            this.cacheUserHistoryPostMatchUpdates = cacheUserHistoryPostMatchUpdates;
            this.statsLimitedRepository = statsLimitedRepository;
            //this.cacheUserHistoryRankUpdated = cacheUserHistoryRankUpdated;
            //this.cacheUserHistoryCompleteVault = cacheUserHistoryCompleteVault;
        }

        public async Task SaveHistoryToDisk(IImmutableUser configUser, OutputLogResult newOutputLogResult)
        {
            var latestCollection = newOutputLogResult.CollectionByDate.MaxBy(i => i.DateTime);
            if (latestCollection != default)
                await repositoryCollection.SaveToDisk(configUser.Id, latestCollection);

            //await SaveInfoByDateOldManager(configUser.Id, newOutputLogResult.InventoryByDate, cacheUserHistoryInventory);
            await SaveInfoByDateOldManager(configUser.Id, newOutputLogResult.RankSyntheticByDate, cacheUserHistoryRank);

            var latestProgress = newOutputLogResult.PlayerProgressByDate.MaxBy(i => i.DateTime);
            if (latestProgress != default)
                await cacheUserHistoryPlayerProgress.SaveToDisk(configUser.Id, latestProgress);

            await SaveInfoByDateOldManager(configUser.Id, newOutputLogResult.PlayerQuestsByDate, cacheUserHistoryPlayerQuests);
            //await SaveInfoByDateOldManager(configUser.Id, newOutputLogResult.DraftPickProgressByDate, cacheUserHistoryDraftPickProgress);
            await SaveInfoByDateOldManager(configUser.Id, newOutputLogResult.MtgaDecksFoundByDate, cacheUserHistoryMtgaDecksFound);
            await SaveInfoByDateOldManager(configUser.Id, newOutputLogResult.MatchesByDate, cacheUserHistoryMatches);

            await SaveInfoByDate2(configUser.Id, newOutputLogResult.PostMatchUpdatesByDate, cacheUserHistoryPostMatchUpdates);
            //await SaveInfoByDate2(configUser.Id, newOutputLogResult.PlayerProgressIntradayByDate, cacheUserHistoryPlayerProgressIntraday);
            await SaveInfoByDate2(configUser.Id, newOutputLogResult.DraftPickProgressIntradayByDate, cacheUserHistoryDraftPickProgressIntraday);
            //await SaveInfoByDate2(configUser.Id, newOutputLogResult.CrackedBoostersByDate, cacheUserHistoryCrackBooster);
            //await SaveInfoByDate2(configUser.Id, newOutputLogResult.VaultsOpenedByDate, cacheUserHistoryCompleteVault);
            //await SaveInfoByDate2(configUser.Id, newOutputLogResult.CollectionIntradayByDate, cacheUserHistoryCollectionIntraday);

            var inventoryData = newOutputLogResult.InventoryIntradayByDate.OrderByDescending(i => i.DateTime).SelectMany(i => i.Info).OrderByDescending(x => x.Key);
            if (inventoryData.Any())
                await cacheUserHistoryInventoryIntraday.SaveToDisk(configUser.Id, inventoryData.First().Value);

            await SaveInfoByDate2(configUser.Id, newOutputLogResult.CombinedRankInfoByDate, cacheUserHistoryCombinedRankInfo);
            await SaveInfoByDate2(configUser.Id, newOutputLogResult.EventClaimPriceByDate, cacheUserHistoryEventClaimPrize);
            //await SaveInfoByDate2(configUser.Id, newOutputLogResult.RankUpdatedByDate, cacheUserHistoryRankUpdated);
            //await SaveInfoByDate2(configUser.Id, newOutputLogResult.MythicRatingUpdatedByDate, cacheUserHistoryMythicRatingUpdated);
            await SaveInfoByDate2(configUser.Id, newOutputLogResult.InventoryUpdatesByDate, cacheUserHistoryInventoryUpdated);
            //await SaveInfoByDate2(configUser.Id, newOutputLogResult.PayEntryByDate, cacheUserHistoryPayEntry);

            await SaveDecks(configUser.Id, newOutputLogResult.DecksSynthetic);

            await userHistoryDatesAvailable.UpdateDatesHavingData(configUser.Id, newOutputLogResult.GetDates());

            if (newOutputLogResult.MatchesByDate.SelectMany(i => i.Info).Any(i => /*i.EventType == "Limited"*/ i.EventName.Contains("Draft") || i.EventName.Contains("Sealed")))
                await statsLimitedRepository.UpdateIsUpToDate(configUser.Id, false);
        }

        private async Task SaveDecks(string userId, List<ConfigModelRawDeck> mtgaDecks)
        {
            var mtgaDecksById = (await cacheUserAllMtgaDecks.Get(userId)).ToDictionary(i => i.Id, i => i);
            foreach (var d in mtgaDecks)
                mtgaDecksById[d.Id] = d;

            await cacheUserAllMtgaDecks.Save(userId, mtgaDecksById.Values.ToList());
        }

        private async Task SaveInfoByDateOldManager<T>(string userId, IList<InfoByDate<T>> newDataByDate, CacheUserHistoryOld<T> cache) where T : new()
        {
            foreach (var newData in newDataByDate)
            {
                if (typeof(T) == typeof(List<MatchResult>))
                {
                    // SPECIAL HANDLING BECAUSE OF OLD FORMAT
                    var matchResults = newData.Info as List<MatchResult>;
                    var newMatchIds = matchResults.Select(i => i.MatchId);
                    var existing = await cache.Get(userId, newData.DateTime.ToString("yyyyMMdd"));

                    //foreach (var data in newData.Info as List<MatchResult>)
                    //{
                    //    (existingMergedWithNew.Info as List<MatchResult>).Add(data);
                    //}
                    foreach (var existingDataToKeep in (existing.Info as List<MatchResult>).Where(i => newMatchIds.Contains(i.MatchId) == false))
                    {
                        matchResults.Insert(0, existingDataToKeep);
                    }
                }

                var keyDate = newData.DateTime.ToString("yyyyMMdd");
                await cache.Save(userId, keyDate, newData);
            }
        }

        private async Task SaveInfoByDate2<T>(string userId, IList<InfoByDate<Dictionary<DateTime, T>>> newData, CacheUserHistory<T> cache) where T : new()
        {
            foreach (var newDataForDate in newData)
            {
                var keyDate = newDataForDate.DateTime.ToString("yyyyMMdd");
                await cache.Save(userId, keyDate, newDataForDate);
            }
        }
    }
}