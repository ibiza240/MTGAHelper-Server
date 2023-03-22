using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MTGAHelper.Server.Data.CosmosDB
{
    public class UserDataCosmosManager
    {
        private CosmosClient cosmosClient;
        private Database database;
        private Container container;

        public UserDataCosmosManager()
        {
        }

        public async Task<UserDataCosmosManager> Init()
        {
            // Put CosmosDB credentials here
            cosmosClient =
                new CosmosClient(
                    "",
                    ""
                );

            database = await cosmosClient.CreateDatabaseIfNotExistsAsync("UserDataDb");
            container = await database.CreateContainerIfNotExistsAsync("UserDataContainer", "/UserId");

            return this;
        }

        public async Task<(bool found, T data)> GetDataForUserId<T>(string userId, string dataKey)
        {
            try
            {
                ItemResponse<CosmosDbData<T>> response = await container.ReadItemAsync<CosmosDbData<T>>(dataKey, new PartitionKey(userId));
                Log.Debug("{userId} {fileLoader}: {dataKey}", userId, "Loaded from CosmosDB", dataKey);
                return (true, response.Resource.Data);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return (false, default);
            }
        }

        public async Task<bool> SetDataForUserId<T>(string userId, string dataKey, T newData)
        {
            try
            {
                var existing = await container.ReadItemAsync<CosmosDbData<T>>(dataKey, new PartitionKey(userId));
                return await SaveIfNotIdentical(userId, dataKey, existing, newData);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                await CreateNew(userId, dataKey, newData);
                return true;
            }
        }

        public async Task<bool> DeleteDataForUserId<T>(string userId, string dataKey)
        {
            try
            {
                //var existing = await container.ReadItemAsync<CosmosDbData<T>>(dataKey, new PartitionKey(userId));
                var result = await container.DeleteItemAsync<CosmosDbData<T>>(dataKey, new PartitionKey(userId));
                Console.Write("el");
                if (result.StatusCode != HttpStatusCode.NoContent)
                {
                    Console.WriteLine($"\r\n\r\n###CANNOT DELETE {dataKey}: {result.StatusCode}");
                    return false;
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                //Console.WriteLine($"~~~DELETE not found {dataKey}");
                return false;
            }

            return true;
        }

        private async Task<bool> SaveIfNotIdentical<T>(string userId, string dataKey, ItemResponse<CosmosDbData<T>> existingItem, T newData)
        {
            // Update existing only if different
            var strExisting = JsonConvert.SerializeObject(existingItem.Resource.Data);
            var strNewData = JsonConvert.SerializeObject(newData);
            if (strExisting.GetHashCode() == strNewData.GetHashCode())
            {
                Log.Debug("{userId} {fileLoader}: {dataKey}", userId, "Skipping Save, no change", dataKey);
                return false;
            }

            //if (dataKey.Contains("_Collection_"))
            //{
            //    System.Diagnostics.Debugger.Break();
            //}

            existingItem.Resource.Data = newData;
            Log.Debug("{userId} {fileLoader}: {dataKey}", userId, "Saving (existing) to CosmosDB", dataKey);
            await container.ReplaceItemAsync(existingItem.Resource, dataKey, new PartitionKey(userId));
            return true;
        }

        private async Task CreateNew<T>(string userId, string dataKey, T newData)
        {
            // Create new
            var cosmosData = new CosmosDbData<T>
            {
                UserId = userId,
                DataKey = dataKey,
                Data = newData,
            };
            Log.Debug("{userId} {fileLoader}: {dataKey}", userId, "Saving (new) to CosmosDB", dataKey);

            try
            {
                await container.CreateItemAsync(cosmosData, new PartitionKey(userId));
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                Log.Error("{userId} {fileLoader}: Conflict already existing {dataKey}", userId, "CreateNew to CosmosDB", dataKey);
            }
        }
    }
}