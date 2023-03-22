using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MTGAHelper.Entity;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Server.Data.Files.UserHistory;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Server.DataAccess.CacheUserHistory
{
    public class UserHistoryRepository<T> where T : new()
    {
        private ConcurrentDictionary<string, Task<InfoByDate<Dictionary<DateTime, T>>>> Data { get; } = new ConcurrentDictionary<string, Task<InfoByDate<Dictionary<DateTime, T>>>>();

        public InfoByDateKeyEnum DataKeyEnum { get; }

        private readonly UserHistoryLoaderFromFile userHistoryLoaderFromFile;
        private readonly UserDataCosmosManager userDataCosmosManager;

        private readonly Dictionary<Type, InfoByDateKeyEnum> dictTypeToEnum = new Dictionary<Type, InfoByDateKeyEnum>
        {
            //{ typeof(Dictionary<int, int>), InfoByDateKeyEnum.CollectionIntraday },
            { typeof(GetCombinedRankInfoRaw), InfoByDateKeyEnum.CombinedRankInfo },
            //{ typeof(CrackBoosterRaw), InfoByDateKeyEnum.CrackedBoosters },
            { typeof(DraftPickStatusRaw), InfoByDateKeyEnum.DraftPickProgressIntraday },
            { typeof(EventClaimPrizeRaw), InfoByDateKeyEnum.EventClaimPrice },
            { typeof(Inventory), InfoByDateKeyEnum.InventoryIntraday },
            { typeof(InventoryUpdatedRaw), InfoByDateKeyEnum.InventoryUpdates },
            //{ typeof(MythicRatingUpdatedRaw), InfoByDateKeyEnum.MythicRatingUpdated },
            //{ typeof(PayEntryRaw), InfoByDateKeyEnum.PayEntry },
            { typeof(GetPlayerProgressRaw), InfoByDateKeyEnum.PlayerProgressIntraday },
            { typeof(PostMatchUpdateRaw), InfoByDateKeyEnum.PostMatchUpdates },
            //{ typeof(RankUpdatedRaw), InfoByDateKeyEnum.RankUpdated },
            //{ typeof(CompleteVaultRaw), InfoByDateKeyEnum.VaultsOpened },
        };

        public UserHistoryRepository(
            UserHistoryLoaderFromFile userHistoryLoaderFromFile,
            UserDataCosmosManager userDataCosmosManager)
        {
            if (dictTypeToEnum.ContainsKey(typeof(T)) == false)
                throw new ArgumentOutOfRangeException(nameof(T), typeof(T), "type not supported");
            DataKeyEnum = dictTypeToEnum[typeof(T)];

            this.userHistoryLoaderFromFile = userHistoryLoaderFromFile;
            this.userDataCosmosManager = userDataCosmosManager;
        }

        internal void Invalidate(UserHistoryKeyInput input)
        {
            Data.TryRemove(input.ToKey(), out _);
        }

        public Task<InfoByDate<Dictionary<DateTime, T>>> GetDataForDate(UserHistoryKeyInput input)
        {
            return Data.GetOrAdd(input.ToKey(), s => LoadData(input));
        }

        internal async Task SaveToDisk(UserHistoryKeyInput input, InfoByDate<Dictionary<DateTime, T>> newData)
        {
            var dataKey = $"{input.UserId}_{DataKeyEnum}_{input.DateFor}";

            //#if DEBUG
            //            System.IO.File.WriteAllText(System.IO.Path.Combine(@"D:\repos\MTGAHelper\bak\20200904\TEST", $"{dataKey}.txt"), JsonConvert.SerializeObject(newData.Info));
            //#endif

            var existing = await GetDataForDate(input);
            if (existing != null)
            {
                var jsonExisting = JsonConvert.SerializeObject(existing);
                var jsonNew = JsonConvert.SerializeObject(newData);

                if (jsonExisting == jsonNew)
                {
                    Log.Debug("{userId} Skipping save, new timestamp but same data as in memory: {dataKey}", input.UserId, dataKey);
                    return;
                }

                // Merge both in the newData
                foreach (var existingIntradayData in existing.Info)
                {
                    if (newData.Info.ContainsKey(existingIntradayData.Key) == false)
                        newData.Info.Add(existingIntradayData.Key, existingIntradayData.Value);
                }

                //newData.Info = newData.Info
                //    .Where(i => i.Key != default)
                //    .OrderBy(i => i.Key)
                //    .ToDictionary(i => i.Key, i => i.Value);
                newData.Info = newData.Info
                    .Where(i => i.Key != default)
                    .OrderBy(i => i.Key)
                    ///* Fix for the weird Data duplication bug sometimes */
                    //.GroupBy(i => JsonConvert.SerializeObject(i.Value))
                    //.Select(i => new { i.Last().Key, i.Last().Value })
                    ///**/
                    .ToDictionary(i => i.Key, i => i.Value);
            }

            var hasChanged = await userDataCosmosManager.SetDataForUserId(input.UserId, dataKey, newData);

            //if (hasChanged)
            //    Invalidate(input);

            // Update In-Memory data
            Data[input.ToKey()] = Task.FromResult(newData);

            //userHistoryLoaderFromFile.SaveToDisk(input.UserId, DictTypeToEnum[typeof(T)], input.DateFor, newData);
        }

        private const string thresholdCosmosDb = "20191201";

        private async Task<InfoByDate<Dictionary<DateTime, T>>> LoadData(UserHistoryKeyInput input)
        {
            if (input.DateFor == null || input.DateFor.CompareTo(thresholdCosmosDb) <= 0)
                return await LoadFromDiskAsync(input);

            // Try to load from Cosmos
            var dataKey = $"{input.UserId}_{DataKeyEnum}_{input.DateFor}";
            var fromCosmosDb = await userDataCosmosManager.GetDataForUserId<InfoByDate<Dictionary<DateTime, T>>>(input.UserId, dataKey);

            // TEMP
            if (fromCosmosDb.found)
            {
                if (fromCosmosDb.data.Info != null)
                {
                    fromCosmosDb.data.Info = fromCosmosDb.data.Info.Where(i => i.Key != default).OrderBy(i => i.Key).ToDictionary(i => i.Key, i => i.Value);
                    return fromCosmosDb.data;
                }
                else
                    return new InfoByDate<Dictionary<DateTime, T>>(DateTime.ParseExact(input.DateFor, "yyyyMMdd", CultureInfo.InvariantCulture), new Dictionary<DateTime, T>());
            }

            return await LoadFromDiskAsync(input);
        }

        private async Task<InfoByDate<Dictionary<DateTime, T>>> LoadFromDiskAsync(UserHistoryKeyInput input)
        {
            // Load from file if not found
            return await userHistoryLoaderFromFile.GetDataAsync<T>(input.UserId, input.DateFor, DataKeyEnum) ??
            // ** WARNING **
            // If there is no data, return a new structure with default values
            // instead of a null
            new InfoByDate<Dictionary<DateTime, T>>
            {
                DateTime = DateTime.TryParseExact(input.DateFor, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : default,
                Info = new Dictionary<DateTime, T> { { default, new T() } }
            }; ;
        }
    }
}