using Microsoft.Azure.Cosmos;
using MTGAHelper.Entity;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.Data.CosmosDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MTGAHelper.Tools.CosmosDB.Downloader
{
    public class ProcessorUser
    {
        private readonly DateTime DateDownloadOnlyStartingFrom = new DateTime(2020, 09, 16);
        private readonly DateTime DateDeleteAllBefore = new DateTime(2020, 09, 16);

        //readonly UserDataCosmosManager userDataCosmosManagerOldConnection;
        private readonly UserDataCosmosManager userDataCosmosManagerNewConnection;

        private readonly ICollection<string> supportersUserIds;

        //private readonly bool copyToNewDatabase;
        private string folderCosmosData;

        private bool IsSupporter(string userId) => supportersUserIds.Contains(userId);

        public ProcessorUser(
            string folderCosmosData,
            UserDataCosmosManager userDataCosmosManager,
            ICollection<string> supportersUserIds
            //bool copyToNewDatabase = false
            )
        {
            this.userDataCosmosManagerNewConnection = userDataCosmosManager;
            //this.userDataCosmosManagerOldConnection = userDataCosmosManagerOldConnection;
            //this.userDataCosmosManagerNewConnection = new UserDataCosmosManager().Init("", "").Result;
            this.supportersUserIds = supportersUserIds;
            //this.copyToNewDatabase = copyToNewDatabase;
            this.folderCosmosData = folderCosmosData;
        }

        public async Task<ICollection<string>> DownloadDates(string userId)
        {
            var success = false;
            var fails = 0;
            while (success == false)
            {
                try
                {
                    var dataKey = $"{userId}_{InfoByDateKeyEnum.DatesWithData}";
                    var result = await userDataCosmosManagerNewConnection.GetDataForUserId<List<string>>(userId, dataKey);
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

        public async Task ProcessUser(string userId)
        {
            //if (IsSupporter(userId)) Debugger.Break();

            var dates = await DownloadDates(userId);
            if (dates.Count == 0)
                return;

            Dictionary<DateTime, Dictionary<InfoByDateKeyEnum, string>> data = new Dictionary<DateTime, Dictionary<InfoByDateKeyEnum, string>>();

            var threshold = DateTime.Now.Date.AddDays(-2);
            foreach (var sDate in dates.Where(i => DateTime.ParseExact(i, "yyyyMMdd", CultureInfo.InvariantCulture) < threshold))
            {
                var dataForDate = new Dictionary<InfoByDateKeyEnum, string>();
                var date = DateTime.ParseExact(sDate, "yyyyMMdd", CultureInfo.InvariantCulture);

                Console.Write($"Getting data for [{userId}] [{sDate}]...");
                await ProcessData<InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.InventoryUpdates, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, PostMatchUpdateRaw>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.PostMatchUpdates, date);
                //await ProcessData<InfoByDate<Dictionary<DateTime, RankUpdatedRaw>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.RankUpdated, date);
                await ProcessData<InfoByDate<List<MatchResult>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.Matches, date);
                await ProcessData<InfoByDate<Inventory>>(dataForDate, userId, sDate, InfoByDateKeyEnum.Inventory, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, Inventory>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.InventoryIntraday, date);
                await ProcessData<InfoByDate<HashSet<string>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.MtgaDecksFound, date);
                await ProcessData<InfoByDate<Dictionary<int, int>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.Collection, date);
                await ProcessData<InfoByDate<Dictionary<string, PlayerProgress>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.PlayerProgress, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, GetPlayerProgressRaw>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.PlayerProgressIntraday, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, DraftPickStatusRaw>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.DraftPickProgressIntraday, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, EventClaimPrizeRaw>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.EventClaimPrice, date);
                //await ProcessData<InfoByDate<Dictionary<DateTime, MythicRatingUpdatedRaw>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.MythicRatingUpdated, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, GetCombinedRankInfoRaw>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.CombinedRankInfo, date);
                //await ProcessData<InfoByDate<Dictionary<DateTime, Dictionary<int, int>>>>(dataForDate, userId, sDate, InfoByDateKeyEnum.CollectionIntraday, date);

                // CLEANUP FOR DEPRECATED EVENTS
                await ProcessData<object>(dataForDate, userId, sDate, InfoByDateKeyEnum.CollectionIntraday, date, true);
                await ProcessData<object>(dataForDate, userId, sDate, InfoByDateKeyEnum.CrackedBoosters, date, true);
                await ProcessData<object>(dataForDate, userId, sDate, InfoByDateKeyEnum.DraftPickProgress, date, true);
                await ProcessData<object>(dataForDate, userId, sDate, InfoByDateKeyEnum.VaultsOpened, date, true);
                await ProcessData<object>(dataForDate, userId, sDate, InfoByDateKeyEnum.MythicRatingUpdated, date, true);
                await ProcessData<object>(dataForDate, userId, sDate, InfoByDateKeyEnum.PayEntry, date, true);
                await ProcessData<object>(dataForDate, userId, sDate, InfoByDateKeyEnum.RankUpdated, date, true);

                Console.WriteLine($"{dates.ToList().IndexOf(sDate) + 1}/{dates.Count}");
                data.Add(date, dataForDate);
            }

            var folderUserData = Path.Combine(folderCosmosData, userId);
            Directory.CreateDirectory(folderUserData);

            await UpdateDatesWithData(userId, dates);

            // Copy Decks to new database
            await CopyDecks(userId);

            // Save data locally to disk
            await SaveData(userId, data);
            Console.WriteLine($"");
        }

        private async Task ProcessData<T>(Dictionary<InfoByDateKeyEnum, string> dataForDate, string userId, string sDate, InfoByDateKeyEnum type, DateTime date, bool deleteAfterDownloading = false)
        {
            //Console.Write(type);
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

            //if (copyToNewDatabase && date >= DateDeleteAllBefore)
            //    await CopyToNewDatabase(userId, type, data, date);

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
            var connection = userDataCosmosManagerNewConnection;//useNewConnection ? userDataCosmosManagerNewConnection : userDataCosmosManagerOldConnection;

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

            Thread.Sleep(20);
            return data;
        }

        //private async Task CopyToNewDatabase<T>(string userId, InfoByDateKeyEnum type, T data, DateTime? date = null)
        //{
        //    if (data == null)
        //        return;

        //    var existing = await GetRawDataFromCosmosDb<T>(userId, type, date, true);
        //    if (existing == null)
        //    {
        //        var dataKey = $"{userId}_{type}{(date == null ? "" : $"_{ date.Value:yyyyMMdd}")}";
        //        await userDataCosmosManagerNewConnection.SetDataForUserId(userId, dataKey, data);
        //    }
        //}

        private async Task DeleteData<T>(string userId, InfoByDateKeyEnum type, DateTime? date = null)
        {
            var dataKey = $"{userId}_{type}{(date == null ? "" : $"_{ date.Value:yyyyMMdd}")}";

            var success = false;
            var fails = 0;
            while (success == false)
            {
                try
                {
                    await userDataCosmosManagerNewConnection.DeleteDataForUserId<T>(userId, dataKey);
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

                Thread.Sleep(20);
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
                    await userDataCosmosManagerNewConnection.DeleteDataForUserId<ICollection<string>>(userId, dataKey);
                else
                    await userDataCosmosManagerNewConnection.SetDataForUserId(userId, dataKey, datesToKeep);
            }

            // Save dates locally to disk
            var folderUserData = Path.Combine(folderCosmosData, userId);
            var path = Path.Combine(folderUserData, $"{dataKey}.txt");
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(datesToKeep));

            //// Copy dates to new database
            //if (datesToKeep.Length > 0)
            //    await userDataCosmosManagerNewConnection.SetDataForUserId(userId, dataKey, datesToKeep);
        }

        private async Task CopyDecks(string userId)
        {
            var dataKey = $"{userId}_{InfoByDateKeyEnum.Decks}";
            var result = await userDataCosmosManagerNewConnection.GetDataForUserId<List<ConfigModelRawDeck>>(userId, dataKey);
            if (result.found == false)
                return;
            var decks = result.data;

            // Save decks locally to disk
            var folderUserData = Path.Combine(folderCosmosData, userId);
            var path = Path.Combine(folderUserData, $"{dataKey}.txt");
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(decks));

            //// Copy to new database
            //await userDataCosmosManagerNewConnection.SetDataForUserId(userId, dataKey, decks);
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