using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.UserHistory;
using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Server.Data.Files.UserHistory;
using Newtonsoft.Json;
using Serilog;

namespace MTGAHelper.Server.DataAccess.CacheUserHistory
{
    public class UserHistoryRepositoryOld<T> where T : new()
    {
        private ConcurrentDictionary<string, Task<InfoByDate<T>>> Data { get; } = new ConcurrentDictionary<string, Task<InfoByDate<T>>>();

        public InfoByDateKeyEnum DataKeyEnum { get; }

        private readonly UserHistoryLoaderFromFile userHistoryLoaderFromFile;
        private readonly UserDataCosmosManager userDataCosmosManager;

        private readonly Dictionary<Type, InfoByDateKeyEnum> dictTypeToEnum = new Dictionary<Type, InfoByDateKeyEnum>
        {
            { typeof(HashSet<string>), InfoByDateKeyEnum.MtgaDecksFound },
            { typeof(Dictionary<int, int>), InfoByDateKeyEnum.Collection },
            { typeof(Inventory), InfoByDateKeyEnum.Inventory },
            { typeof(DateSnapshotDiff), InfoByDateKeyEnum.Diff },
            { typeof(List<ConfigModelRankInfo>), InfoByDateKeyEnum.Rank },
            { typeof(List<MatchResult>), InfoByDateKeyEnum.Matches },
            { typeof(Dictionary<string, PlayerProgress>), InfoByDateKeyEnum.PlayerProgress },
            { typeof(List<PlayerQuest>), InfoByDateKeyEnum.PlayerQuests },
            //{ typeof(List<DraftMakePickRaw>), InfoByDateKeyEnum.DraftPickProgress },
        };

        public UserHistoryRepositoryOld(
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

        public Task<InfoByDate<T>> GetDataForDate(UserHistoryKeyInput input)
        {
            return Data.GetOrAdd(input.ToKey(), s => LoadDataNotNull(input));
        }

        private async Task<InfoByDate<T>> LoadDataNotNull(UserHistoryKeyInput input)
        {
            var data = await LoadData(input);
            if (data != null)
                return data;

            return new InfoByDate<T>
            {
                DateTime = DateTime.TryParseExact(input.DateFor, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : default,
                Info = new T()
            };
        }

        internal async Task Save(UserHistoryKeyInput input, InfoByDate<T> newData)
        {
            var dataKey = $"{input.UserId}_{DataKeyEnum}_{input.DateFor}";

            //#if DEBUG
            //            System.IO.File.WriteAllText(System.IO.Path.Combine(@"D:\repos\MTGAHelper\bak\20200904\TEST", $"{dataKey}.txt"), JsonConvert.SerializeObject(newData.Info));
            //#endif

            var existing = await GetDataForDate(input);
            if (existing != null)
            {
                var jsonExisting = JsonConvert.SerializeObject(existing.Info);
                var jsonNew = JsonConvert.SerializeObject(newData.Info);

                if (jsonExisting == jsonNew)
                {
                    Log.Debug("{userId} Skipping save for OLD format, new timestamp but same data: {dataKey}", input.UserId, dataKey);
                    return;
                }
            }

            var hasChanged = await userDataCosmosManager.SetDataForUserId(input.UserId, dataKey, newData);

            // Update In-Memory data
            Log.Debug("Update In-Memory data for {userId}", input.UserId);
            if (hasChanged && Data.TryRemove(input.ToKey(), out _))
                Data.TryAdd(input.ToKey(), Task.FromResult(newData));
        }

        private const string thresholdCosmosDb = "20191201";

        private async Task<InfoByDate<T>> LoadData(UserHistoryKeyInput input)
        {
            if (input.DateFor == null || input.DateFor.CompareTo(thresholdCosmosDb) <= 0)
                return await LoadFromDiskAsync(input);

            // Try to load from Cosmos
            var dataKey = $"{input.UserId}_{DataKeyEnum}_{input.DateFor}";
            var fromCosmosDb = await userDataCosmosManager.GetDataForUserId<InfoByDate<T>>(input.UserId, dataKey);

            return fromCosmosDb.found ? fromCosmosDb.data : await LoadFromDiskAsync(input);
        }

        private Task<InfoByDate<T>> LoadFromDiskAsync(UserHistoryKeyInput input)
        {
            // Load from file if not found
            return userHistoryLoaderFromFile.GetDataOldAsync<T>(input.UserId, input.DateFor, DataKeyEnum);
        }
    }
}