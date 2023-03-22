using Microsoft.Azure.Cosmos;
using MTGAHelper.Entity;
using MTGAHelper.Server.Data.CosmosDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Tools.CosmosDB.Downloader.v2
{
    public class UserProcessor
    {
        private readonly UserDataCosmosManager cosmosManager;

        public UserProcessor(
            string folderCosmosData,
            UserDataCosmosManager userDataCosmosManager,
            UserDataCosmosManager2 userDataCosmosManager2,
            ICollection<string> supportersUserIds
            )
        {
            this.cosmosManager = userDataCosmosManager;
            this.supportersUserIds = supportersUserIds;
            this.folderCosmosData = folderCosmosData;
        }

        private readonly DateTime DateDownloadOnlyStartingFrom = new DateTime(2020, 09, 16);
        private readonly DateTime DateDeleteAllBefore = new DateTime(2020, 09, 16);

        private readonly ICollection<string> supportersUserIds;

        private string folderCosmosData;

        private bool IsSupporter(string userId) => supportersUserIds.Contains(userId);

        public async Task<ICollection<string>> DownloadDates(string userId)
        {
            var success = false;
            var fails = 0;
            while (success == false)
            {
                try
                {
                    var dataKey = $"{userId}_{InfoByDateKeyEnum.DatesWithData}";
                    var result = await cosmosManager.GetDataForUserId<List<string>>(userId, dataKey);
                    if (result.found)
                    {
                        return result.data;
                    }

                    return Array.Empty<string>();
                }
                catch (CosmosException ex)
                {
                    //Debugger.Break();
                    fails++;
                    Console.WriteLine($"!!! DownloadDates FAILED {fails} for {userId}");
                    if (fails > 10)
                        return Array.Empty<string>();
                }
            }

            throw new Exception("WAT");
        }

        private async Task ProcessData<T>(Dictionary<InfoByDateKeyEnum, string> dataForDate, string userId, string sDate, InfoByDateKeyEnum type, DateTime date, bool deleteAfterDownloading = false)
        {
            T data = default(T);

            if (date >= DateDownloadOnlyStartingFrom)
            {
                if (IsDownloaded(userId, type, sDate) == false)
                {
                    data = await GetRawDataFromCosmosDb<T>(userId, type, date);
                    if (data != null)
                        dataForDate.Add(type, JsonConvert.SerializeObject(data));
                }
                else
                    data = GetDownloaded<T>(userId, type, sDate);
            }
            else
            {
                data = GetDownloaded<T>(userId, type, sDate);
                //Console.Write($" too old");
            }

            if (deleteAfterDownloading || (IsSupporter(userId) == false && date < DateDeleteAllBefore))
            {
                //if (deleteAfterDownloading == false && date >= DateDownloadOnlyStartingFrom)
                //{
                //    Debugger.Break();
                //}

                await DeleteData<T>(userId, type, date);
                //Console.Write($" deleted");
            }

            Console.Write($".");
        }

        private async Task<T> GetRawDataFromCosmosDb<T>(string userId, InfoByDateKeyEnum type, DateTime? date = null, bool useNewConnection = false)
        {
            var connection = cosmosManager;

            var dataKey = $"{userId}_{type}{(date == null ? "" : $"_{ date.Value:yyyyMMdd}")}";

            var stopLoop = false;
            var fails = 0;
            T data = default(T);
            while (stopLoop == false)
            {
                try
                {
                    var result = await connection.GetDataForUserId<T>(userId, dataKey);

                    if (result.found)
                    {
                        data = result.data;
                        stopLoop = true;
                    }
                    else
                    {
                        // No error but data not found
                        return default(T);
                    }
                }
                catch (CosmosException ex)
                {
                    //Debugger.Break();
                    fails++;
                    Console.WriteLine($"??? GetRawDataFromCosmosDb FAILED {fails} for {userId} {type} {date.Value:yyyyMMdd}");
                    stopLoop = fails > 10;
                }
            }

            await Task.Delay(20);
            return data;
        }

        private async Task DeleteData<T>(string userId, InfoByDateKeyEnum type, DateTime? date = null)
        {
            var dataKey = $"{userId}_{type}{(date == null ? "" : $"_{ date.Value:yyyyMMdd}")}";

            var success = false;
            var fails = 0;
            while (success == false)
            {
                try
                {
                    await cosmosManager.DeleteDataForUserId<T>(userId, dataKey);
                    success = true;
                }
                catch (CosmosException ex)
                {
                    //Debugger.Break();
                    fails++;
                    Console.WriteLine($"~~~ DeleteDataForUserId FAILED {fails} for {userId} {type} {date.Value:yyyyMMdd}");
                    if (fails > 10)
                        return;
                }

                await Task.Delay(20);
            }
        }

        private bool IsDownloaded(string userId, InfoByDateKeyEnum type, string sDate)
        {
            var found = File.Exists(Path.Combine(folderCosmosData, userId, sDate, $"{userId}_{type}_{sDate}.txt"));

            //if (found)
            //    Console.Write($" found");

            return found;
        }

        private T GetDownloaded<T>(string userId, InfoByDateKeyEnum type, string sDate)
        {
            var path = Path.Combine(folderCosmosData, userId, sDate, $"{userId}_{type}_{sDate}.txt");
            return File.Exists(path) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) : default(T);
        }

        private async Task UpdateDatesWithData(string userId, ICollection<string> dates)
        {
            // Keep everything for supporters, otherwise remove dates that are too old
            var datesToKeep = dates.Where(i => IsSupporter(userId) || DateTime.ParseExact(i, "yyyyMMdd", CultureInfo.InvariantCulture) >= DateDeleteAllBefore).ToArray();

            var dataKey = $"{userId}_{InfoByDateKeyEnum.DatesWithData}";
            if (datesToKeep.Length < dates.Count)
            {
                // Update CosmosDB entry indicating which dates have data
                if (datesToKeep.Length == 0)
                    await cosmosManager.DeleteDataForUserId<ICollection<string>>(userId, dataKey);
                else
                    await cosmosManager.SetDataForUserId(userId, dataKey, datesToKeep);
            }

            // Save dates locally to disk
            var folderUserData = Path.Combine(folderCosmosData, userId);
            var path = Path.Combine(folderUserData, $"{dataKey}.txt");
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(datesToKeep));
        }

        private async Task CopyDecks(string userId)
        {
            var dataKey = $"{userId}_{InfoByDateKeyEnum.Decks}";
            var result = await cosmosManager.GetDataForUserId<List<ConfigModelRawDeck>>(userId, dataKey);
            if (result.found == false)
                return;
            var decks = result.data;

            // Save decks locally to disk
            var folderUserData = Path.Combine(folderCosmosData, userId);
            var path = Path.Combine(folderUserData, $"{dataKey}.txt");
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(decks));
        }

        public async Task SaveData(string userId, Dictionary<DateTime, Dictionary<InfoByDateKeyEnum, string>> dataByDateThenType)
        {
            if (dataByDateThenType.Count > 0)
                Console.WriteLine($"Saving data for [{userId}]...");

            foreach (var dd in dataByDateThenType)
            {
                var sDate = dd.Key.ToString("yyyyMMdd");

                var folderUserDataForDate = Path.Combine(folderCosmosData, userId, sDate);
                Directory.CreateDirectory(folderUserDataForDate);

                foreach (var ddd in dd.Value)
                {
                    var path = Path.Combine(folderUserDataForDate, $"{userId}_{ddd.Key}_{sDate}.txt");
                    if (File.Exists(path) == false)
                        await File.WriteAllTextAsync(path, ddd.Value);
                }
            }
        }
    }
}