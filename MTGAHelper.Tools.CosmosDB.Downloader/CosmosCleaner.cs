using Microsoft.Azure.Cosmos;
using MTGAHelper.Entity;
using MTGAHelper.Entity.Config.App;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.Data.CosmosDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTGAHelper.Tools.CosmosDB.Downloader
{
    public abstract class ProcessorUserBase
    {
        private readonly ICollection<string> supportersUserIds;
        protected readonly UserDataCosmosManager userDataCosmosManager;

        protected bool IsSupporter(string userId) => supportersUserIds.Contains(userId);

        public ProcessorUserBase(
            UserDataCosmosManager userDataCosmosManager,
            ICollection<string> supportersUserIds
            )
        {
            this.userDataCosmosManager = userDataCosmosManager;
            this.supportersUserIds = supportersUserIds;
        }

        protected async Task<ICollection<string>> DownloadDates(string userId)
        {
            var success = false;
            var fails = 0;
            while (success == false)
            {
                try
                {
                    var dataKey = $"{userId}_{InfoByDateKeyEnum.DatesWithData}";
                    var result = await userDataCosmosManager.GetDataForUserId<List<string>>(userId, dataKey);
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

        protected async Task DeleteData<T>(string userId, InfoByDateKeyEnum type, DateTime? date = null)
        {
            var dataKey = $"{userId}_{type}{(date == null ? "" : $"_{ date.Value:yyyyMMdd}")}";

            var success = false;
            var fails = 0;
            while (success == false)
            {
                try
                {
                    await userDataCosmosManager.DeleteDataForUserId<T>(userId, dataKey);
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

        protected async Task UpdateDatesWithData(string userId, ICollection<string> dates, DateTime dateDeleteAllBefore)
        {
            // Keep everything for supporters, otherwise remove dates that are too old
            var datesToKeep = dates.Where(i => IsSupporter(userId) || DateTime.ParseExact(i, "yyyyMMdd", CultureInfo.InvariantCulture) >= dateDeleteAllBefore).ToArray();

            var dataKey = $"{userId}_{InfoByDateKeyEnum.DatesWithData}";
            if (datesToKeep.Length < dates.Count)
            {
                // Update CosmosDB entry indicating which dates have data

                if (datesToKeep.Length == 0)
                    await userDataCosmosManager.DeleteDataForUserId<ICollection<string>>(userId, dataKey);
                else
                    await userDataCosmosManager.SetDataForUserId(userId, dataKey, datesToKeep);
            }

            //// Save dates locally to disk
            //var folderUserData = Path.Combine(folderCosmosData, userId);
            //var path = Path.Combine(folderUserData, $"{dataKey}.txt");
            //await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(datesToKeep));

            //// Copy dates to new database
            //if (datesToKeep.Length > 0)
            //    await userDataCosmosManagerNewConnection.SetDataForUserId(userId, dataKey, datesToKeep);
        }
    }

    public class CosmosCleaner : ProcessorUserBase
    {
        private readonly DateTime DateDeleteAllBefore = new DateTime(2020, 09, 24);
        private readonly string folderOutputCosmosData;

        public CosmosCleaner(
            UserDataCosmosManager userDataCosmosManager,
            ICollection<string> supportersUserIds,
            string folderOutputCosmosData
            )
            : base(userDataCosmosManager, supportersUserIds)
        {
            this.folderOutputCosmosData = folderOutputCosmosData;
        }

        public async Task ProcessUser(string userId, bool checkForGhostDates = false)
        {
            //if (IsSupporter(userId)) Debugger.Break();

            var dates = await DownloadDates(userId);
            if (dates.Count == 0)
                return;

            foreach (var sDate in dates)
            {
                var date = DateTime.ParseExact(sDate, "yyyyMMdd", CultureInfo.InvariantCulture);

                Console.Write($"Deleting obsolete data for [{userId}] [{sDate}]...");
                await ProcessData<InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>>(userId, InfoByDateKeyEnum.InventoryUpdates, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, PostMatchUpdateRaw>>>(userId, InfoByDateKeyEnum.PostMatchUpdates, date);
                //await ProcessData<InfoByDate<Dictionary<DateTime, RankUpdatedRaw>>>(userId, InfoByDateKeyEnum.RankUpdated, date);
                await ProcessData<InfoByDate<List<MatchResult>>>(userId, InfoByDateKeyEnum.Matches, date);
                await ProcessData<InfoByDate<Inventory>>(userId, InfoByDateKeyEnum.Inventory, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, Inventory>>>(userId, InfoByDateKeyEnum.InventoryIntraday, date);
                await ProcessData<InfoByDate<HashSet<string>>>(userId, InfoByDateKeyEnum.MtgaDecksFound, date);
                await ProcessData<InfoByDate<Dictionary<int, int>>>(userId, InfoByDateKeyEnum.Collection, date);
                await ProcessData<InfoByDate<Dictionary<string, PlayerProgress>>>(userId, InfoByDateKeyEnum.PlayerProgress, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, GetPlayerProgressRaw>>>(userId, InfoByDateKeyEnum.PlayerProgressIntraday, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, DraftPickStatusRaw>>>(userId, InfoByDateKeyEnum.DraftPickProgressIntraday, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, EventClaimPrizeRaw>>>(userId, InfoByDateKeyEnum.EventClaimPrice, date);
                //await ProcessData<InfoByDate<Dictionary<DateTime, MythicRatingUpdatedRaw>>>(userId, InfoByDateKeyEnum.MythicRatingUpdated, date);
                await ProcessData<InfoByDate<Dictionary<DateTime, GetCombinedRankInfoRaw>>>(userId, InfoByDateKeyEnum.CombinedRankInfo, date);
                //await ProcessData<InfoByDate<Dictionary<DateTime, Dictionary<int, int>>>>(userId, InfoByDateKeyEnum.CollectionIntraday, date);

                //// CLEANUP FOR DEPRECATED EVENTS
                //await ProcessData<object>(userId, InfoByDateKeyEnum.CollectionIntraday, date);
                //await ProcessData<object>(userId, InfoByDateKeyEnum.CrackedBoosters, date);
                //await ProcessData<object>(userId, InfoByDateKeyEnum.DraftPickProgress, date);
                //await ProcessData<object>(userId, InfoByDateKeyEnum.VaultsOpened, date);
                //await ProcessData<object>(userId, InfoByDateKeyEnum.MythicRatingUpdated, date);
                //await ProcessData<object>(userId, InfoByDateKeyEnum.PayEntry, date);
                //await ProcessData<object>(userId, InfoByDateKeyEnum.RankUpdated, date);

                Console.WriteLine($"{dates.ToList().IndexOf(sDate) + 1}/{dates.Count}");
            }

            if (checkForGhostDates)
            {
                var dateCosmosStart = new DateTime(2019, 12, 1);
                var date = dates
                    .Select(i => DateTime.ParseExact(i, "yyyyMMdd", CultureInfo.InvariantCulture))
                    .Union(new[] { DateDeleteAllBefore.AddDays(-1) })
                    .Min();
                while (date >= dateCosmosStart)
                {
                    if (Directory.Exists($"{folderOutputCosmosData}\\{userId}\\{date:yyyyMMdd}"))
                    {
                        Console.Write($"Deleting obsolete ghost data for [{userId}] [{date:yyyyMMdd}]...");
                        await ProcessData<InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>>(userId, InfoByDateKeyEnum.InventoryUpdates, date);
                        await ProcessData<InfoByDate<Dictionary<DateTime, PostMatchUpdateRaw>>>(userId, InfoByDateKeyEnum.PostMatchUpdates, date);
                        //await ProcessData<InfoByDate<Dictionary<DateTime, RankUpdatedRaw>>>(userId, InfoByDateKeyEnum.RankUpdated, date);
                        await ProcessData<InfoByDate<List<MatchResult>>>(userId, InfoByDateKeyEnum.Matches, date);
                        await ProcessData<InfoByDate<Inventory>>(userId, InfoByDateKeyEnum.Inventory, date);
                        await ProcessData<InfoByDate<Dictionary<DateTime, Inventory>>>(userId, InfoByDateKeyEnum.InventoryIntraday, date);
                        await ProcessData<InfoByDate<HashSet<string>>>(userId, InfoByDateKeyEnum.MtgaDecksFound, date);
                        await ProcessData<InfoByDate<Dictionary<int, int>>>(userId, InfoByDateKeyEnum.Collection, date);
                        await ProcessData<InfoByDate<Dictionary<string, PlayerProgress>>>(userId, InfoByDateKeyEnum.PlayerProgress, date);
                        await ProcessData<InfoByDate<Dictionary<DateTime, GetPlayerProgressRaw>>>(userId, InfoByDateKeyEnum.PlayerProgressIntraday, date);
                        await ProcessData<InfoByDate<Dictionary<DateTime, DraftPickStatusRaw>>>(userId, InfoByDateKeyEnum.DraftPickProgressIntraday, date);
                        await ProcessData<InfoByDate<Dictionary<DateTime, EventClaimPrizeRaw>>>(userId, InfoByDateKeyEnum.EventClaimPrice, date);
                        //await ProcessData<InfoByDate<Dictionary<DateTime, MythicRatingUpdatedRaw>>>(userId, InfoByDateKeyEnum.MythicRatingUpdated, date);
                        await ProcessData<InfoByDate<Dictionary<DateTime, GetCombinedRankInfoRaw>>>(userId, InfoByDateKeyEnum.CombinedRankInfo, date);
                        //await ProcessData<InfoByDate<Dictionary<DateTime, Dictionary<int, int>>>>(userId, InfoByDateKeyEnum.CollectionIntraday, date);

                        ////// CLEANUP FOR DEPRECATED EVENTS
                        //await ProcessData<object>(userId, InfoByDateKeyEnum.CollectionIntraday, date);
                        //await ProcessData<object>(userId, InfoByDateKeyEnum.CrackedBoosters, date);
                        //await ProcessData<object>(userId, InfoByDateKeyEnum.DraftPickProgress, date);
                        //await ProcessData<object>(userId, InfoByDateKeyEnum.VaultsOpened, date);
                        //await ProcessData<object>(userId, InfoByDateKeyEnum.MythicRatingUpdated, date);
                        //await ProcessData<object>(userId, InfoByDateKeyEnum.PayEntry, date);
                        //await ProcessData<object>(userId, InfoByDateKeyEnum.RankUpdated, date);

                        Console.WriteLine();
                    }
                    date = date.AddDays(-1);
                }
            }

            await UpdateDatesWithData(userId, dates, DateDeleteAllBefore);
        }

        private async Task ProcessData<T>(string userId, InfoByDateKeyEnum type, DateTime date)
        {
            if (IsSupporter(userId) == false && date < DateDeleteAllBefore)
            {
                Console.Write($"d");
                await DeleteData<T>(userId, type, date);
            }

            Console.Write($".");
        }
    }
}
