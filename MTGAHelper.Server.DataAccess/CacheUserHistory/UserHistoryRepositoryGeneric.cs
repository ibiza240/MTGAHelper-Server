using System.Collections.Concurrent;
using System.Threading.Tasks;
using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Server.Data.Files.UserHistory;

namespace MTGAHelper.Server.DataAccess.CacheUserHistory
{
    public class UserHistoryRepositoryGeneric<T> where T : new()
    {
        private string fileKey;
        private bool checkOnDiskIfNotInCosmosDb;
        private ConcurrentDictionary<string, Task<T>> Data { get; } = new ConcurrentDictionary<string, Task<T>>();
        private readonly UserHistoryLoaderFromFile userHistoryLoaderFromFile;
        private readonly UserDataCosmosManager userDataCosmosManager;

        public UserHistoryRepositoryGeneric(
            UserHistoryLoaderFromFile userHistoryLoaderFromFile,
            UserDataCosmosManager userDataCosmosManager
            )
        {
            this.userDataCosmosManager = userDataCosmosManager;
            this.userHistoryLoaderFromFile = userHistoryLoaderFromFile;
        }

        public UserHistoryRepositoryGeneric<T> Init(string fileKey, bool checkOnDiskIfNotInCosmosDb = true)
        {
            this.fileKey = fileKey;
            this.checkOnDiskIfNotInCosmosDb = checkOnDiskIfNotInCosmosDb;
            return this;
        }

        public void Invalidate(string userId)
        {
            Data.TryRemove(userId, out _);
        }

        public Task<T> GetData(string userId)
        {
            return Data.GetOrAdd(userId, LoadDataNotNull);
        }

        private async Task<T> LoadDataNotNull(string userId)
        {
            var data = await LoadData(userId);
            return data != null ? data : new T();
        }

        internal async Task SaveToDisk(string userId, T newData)
        {
            var dataKey = $"{userId}_{fileKey}";

            //#if DEBUG
            //            System.IO.File.WriteAllText(System.IO.Path.Combine(@"D:\repos\MTGAHelper\bak\20200904\TEST", $"{dataKey}.txt"), Newtonsoft.Json.JsonConvert.SerializeObject(newData));
            //#endif

            var hasChanged = await userDataCosmosManager.SetDataForUserId(userId, dataKey, newData);

            if (hasChanged && Data.TryRemove(userId, out _))
                Data.TryAdd(userId, Task.FromResult(newData));

            //userHistoryLoaderFromFile.SaveToDiskGeneric(userId, fileKey, newData);
        }

        private async Task<T> LoadData(string userId)
        {
            // Try to load from Cosmos
            var dataKey = $"{userId}_{fileKey}";
            var fromCosmosDb = await userDataCosmosManager.GetDataForUserId<T>(userId, dataKey);

            if (fromCosmosDb.found)
                return fromCosmosDb.data;

            return checkOnDiskIfNotInCosmosDb
                ? await userHistoryLoaderFromFile.GetDataGenericAsync<T>(userId, fileKey)
                : default;
        }
    }
}