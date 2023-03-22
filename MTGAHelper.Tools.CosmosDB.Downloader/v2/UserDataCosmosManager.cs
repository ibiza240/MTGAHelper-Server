using Microsoft.Azure.Cosmos;
using MTGAHelper.Server.Data.CosmosDB;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTGAHelper.Tools.CosmosDB.Downloader.v2
{
    public class UserDataCosmosManager2
    {
        private CosmosClient cosmosClient;
        private Container container;
        private string folderCosmosData;

        public UserDataCosmosManager2()
        {
        }

        public async Task<UserDataCosmosManager2> Init(string folderCosmosData)
        {
            // Put CosmosDB credentials here
            cosmosClient =
                new CosmosClient(
                    "",
                    ""
                );

            var database = await cosmosClient.CreateDatabaseIfNotExistsAsync("UserDataDb");
            container = await database.Database.CreateContainerIfNotExistsAsync("UserDataContainer", "/UserId");

            this.folderCosmosData = folderCosmosData;

            return this;
        }

        public async Task DownloadAll()
        {
            var chars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', };

            foreach (var c in chars)
            {
                var sqlQueryText = $"SELECT * FROM c where c.id LIKE \"{c}%\"";

                Console.WriteLine("Running query: {0}\n", sqlQueryText);

                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

                using (FeedIterator<dynamic> queryResultSetIterator = this.container.GetItemQueryIterator<dynamic>(queryDefinition))
                {
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        try
                        {
                            FeedResponse<dynamic> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                            foreach (dynamic raw in currentResultSet)
                            {
                                string id = await ProcessCollectionItem(raw);

                                Console.WriteLine("[{0}] Reading {1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), raw.id);
                            }
                        }
                        catch (CosmosException ex)
                        {
                            Console.WriteLine("Error! {0} {1}", ex.GetType().Name, ex.Message);
                            Console.WriteLine("Waiting 5 seconds...");
                            await Task.Delay(5000);
                        }
                    }

                    Console.WriteLine("Query finished");
                }
            }
        }

        private async Task<string> ProcessCollectionItem(dynamic raw)
        {
            string json = raw.ToString();

            var id = JsonConvert.DeserializeObject<WithId>(json).id;
            var parts = Regex.Match(id, "^(.*?)_(.*?)(_(.*?))?$");
            if (parts.Success == false)
                Debugger.Break();

            var userId = parts.Groups[1].Value;
            var type = parts.Groups[2].Value;
            var is2Parts = parts.Groups[3].Value == string.Empty;
            var date = is2Parts ? (DateTime?)null : DateTime.ParseExact(parts.Groups[4].Value, "yyyyMMdd", CultureInfo.InvariantCulture);

            await DownloadData(json, id, userId, date);

            return id;
        }

        private async Task DeleteData(string id, string userId)
        {
            await container.DeleteItemAsync<CosmosDbData<string>>(id, new PartitionKey(userId));
            Console.WriteLine($"Deltng data for [{id}] !");
        }

        private async Task DownloadData(string json, string id, string userId, DateTime? date)
        {
            Console.WriteLine($"Saving data for [{id}]...");

            // Date can be null to omit it from the filename
            var folderUserDataForDate = Path.Combine(folderCosmosData, userId, date?.ToString("yyyyMMdd") ?? "");

            Directory.CreateDirectory(folderUserDataForDate);

            var path = Path.Combine(folderUserDataForDate, $"{id}.txt");
            //if (File.Exists(path) == false)
            await File.WriteAllTextAsync(path, json);
        }
    }

    public class WithId
    {
        public string id { get; set; }
    }
}