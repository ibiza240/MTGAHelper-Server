using Microsoft.Azure.Cosmos;
using MTGAHelper.Entity;
using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.Data.CosmosDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Tools.CosmosDB.Downloader
{
    public class DataUploader
    {
        private readonly string folderCosmosData;
        private readonly UserDataCosmosManager cosmosManager;

        public DataUploader(
            string folderCosmosData,
            UserDataCosmosManager cosmosManager
            )
        {
            this.folderCosmosData = folderCosmosData;
            this.cosmosManager = cosmosManager;
        }

        private Dictionary<InfoByDateKeyEnum, List<(string sDate, object data)>> ReadDataFromDisk(string userId)
        {
            var foldersDate = Directory.GetDirectories(Path.Combine(folderCosmosData, userId));
            var result = new Dictionary<InfoByDateKeyEnum, List<(string sDate, object data)>>()
            {
                { InfoByDateKeyEnum.Collection, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.CombinedRankInfo, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.DraftPickProgressIntraday, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.EventClaimPrice, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.Inventory, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.InventoryIntraday, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.InventoryUpdates, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.Matches, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.MtgaDecksFound, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.PlayerProgress, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.PlayerProgressIntraday, new List<(string sDate, object data)>() },
                { InfoByDateKeyEnum.Rank, new List<(string sDate, object data)>() },
            };

            foreach (var f in foldersDate.SelectMany(i => Directory.GetFiles(i)))
            {
                var info = Path.GetFileNameWithoutExtension(f).Split("_");
                var datatType = info[1];
                var sDate = info[2];

                if (sDate.CompareTo("20191201") < 0)
                    continue;

                if (datatType == InfoByDateKeyEnum.InventoryUpdates.ToString())
                    result[InfoByDateKeyEnum.InventoryUpdates].Add(GetDownloaded<InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>>(userId, InfoByDateKeyEnum.InventoryUpdates, sDate));

                //else if (datatType == InfoByDateKeyEnum.PostMatchUpdates.ToString())
                //    result[InfoByDateKeyEnum.PostMatchUpdates].Add(GetDownloaded<InfoByDate<Dictionary<DateTime, PostMatchUpdateRaw>>>(userId, InfoByDateKeyEnum.PostMatchUpdates, sDate));
                else if (datatType == InfoByDateKeyEnum.Matches.ToString())
                    result[InfoByDateKeyEnum.Matches].Add(GetDownloaded<InfoByDate<List<MatchResult>>>(userId, InfoByDateKeyEnum.Matches, sDate));
                else if (datatType == InfoByDateKeyEnum.Inventory.ToString())
                    result[InfoByDateKeyEnum.Inventory].Add(GetDownloaded<InfoByDate<Inventory>>(userId, InfoByDateKeyEnum.Inventory, sDate));
                else if (datatType == InfoByDateKeyEnum.InventoryIntraday.ToString())
                    result[InfoByDateKeyEnum.InventoryIntraday].Add(GetDownloaded<InfoByDate<Dictionary<DateTime, Inventory>>>(userId, InfoByDateKeyEnum.InventoryIntraday, sDate));
                else if (datatType == InfoByDateKeyEnum.MtgaDecksFound.ToString())
                    result[InfoByDateKeyEnum.MtgaDecksFound].Add(GetDownloaded<InfoByDate<HashSet<string>>>(userId, InfoByDateKeyEnum.MtgaDecksFound, sDate));
                else if (datatType == InfoByDateKeyEnum.Collection.ToString())
                    result[InfoByDateKeyEnum.Collection].Add(GetDownloaded<InfoByDate<Dictionary<int, int>>>(userId, InfoByDateKeyEnum.Collection, sDate));
                else if (datatType == InfoByDateKeyEnum.PlayerProgress.ToString())
                    result[InfoByDateKeyEnum.PlayerProgress].Add(GetDownloaded<InfoByDate<Dictionary<string, PlayerProgress>>>(userId, InfoByDateKeyEnum.PlayerProgress, sDate));
                else if (datatType == InfoByDateKeyEnum.PlayerProgressIntraday.ToString())
                    result[InfoByDateKeyEnum.PlayerProgressIntraday].Add(GetDownloaded<InfoByDate<Dictionary<DateTime, GetPlayerProgressRaw>>>(userId, InfoByDateKeyEnum.PlayerProgressIntraday, sDate));
                else if (datatType == InfoByDateKeyEnum.DraftPickProgressIntraday.ToString())
                    result[InfoByDateKeyEnum.DraftPickProgressIntraday].Add(GetDownloaded<InfoByDate<Dictionary<DateTime, DraftPickStatusRaw>>>(userId, InfoByDateKeyEnum.DraftPickProgressIntraday, sDate));
                else if (datatType == InfoByDateKeyEnum.EventClaimPrice.ToString())
                    result[InfoByDateKeyEnum.EventClaimPrice].Add(GetDownloaded<InfoByDate<Dictionary<DateTime, EventClaimPrizeRaw>>>(userId, InfoByDateKeyEnum.EventClaimPrice, sDate));
                else if (datatType == InfoByDateKeyEnum.CombinedRankInfo.ToString())
                    result[InfoByDateKeyEnum.CombinedRankInfo].Add(GetDownloaded<InfoByDate<Dictionary<DateTime, GetCombinedRankInfoRaw>>>(userId, InfoByDateKeyEnum.CombinedRankInfo, sDate));
            };

            return result;
        }

        private (string, T) GetDownloaded<T>(string userId, InfoByDateKeyEnum type, string sDate)
        {
            var path = Path.Combine(folderCosmosData, userId, sDate, $"{userId}_{type}_{sDate}.txt");
            Console.WriteLine($"Reading {path}");
            return (sDate, File.Exists(path) ? JsonConvert.DeserializeObject<CData<T>>(File.ReadAllText(path)).Data : default(T));
        }

        public class CData<T>
        {
            public T Data { get; set; }
        }

        public async Task UploadData(string userId)
        {
            var data = ReadDataFromDisk(userId);

            var existingdates = await DownloadDates(userId);
            await UpdateDatesWithData(userId, existingdates, data.SelectMany(i => i.Value.Select(x => x.sDate)).ToArray());

            foreach (var type in data)
            {
                //Task.Factory.StartNew(async () =>
                //{
                var tuples = type.Value.Select(x => new { type = type.Key, x.data, x.sDate });
                foreach (var d in tuples)
                {
                    if (d.type == InfoByDateKeyEnum.InventoryUpdates)
                        await Upload<InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.PostMatchUpdates)
                        await Upload<InfoByDate<Dictionary<DateTime, PostMatchUpdateRaw>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.Matches)
                        await Upload<InfoByDate<List<MatchResult>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.Inventory)
                        await Upload<InfoByDate<Inventory>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.InventoryIntraday)
                        await Upload<InfoByDate<Dictionary<DateTime, Inventory>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.MtgaDecksFound)
                        await Upload<InfoByDate<HashSet<string>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.Collection)
                        await Upload<InfoByDate<Dictionary<int, int>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.PlayerProgress)
                        await Upload<InfoByDate<Dictionary<string, PlayerProgress>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.PlayerProgressIntraday)
                        await Upload<InfoByDate<Dictionary<DateTime, GetPlayerProgressRaw>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.DraftPickProgressIntraday)
                        await Upload<InfoByDate<Dictionary<DateTime, DraftPickStatusRaw>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.EventClaimPrice)
                        await Upload<InfoByDate<Dictionary<DateTime, EventClaimPrizeRaw>>>(userId, d.type, d.data, d.sDate);
                    else if (d.type == InfoByDateKeyEnum.CombinedRankInfo)
                        await Upload<InfoByDate<Dictionary<DateTime, GetCombinedRankInfoRaw>>>(userId, d.type, d.data, d.sDate);
                }
                //});
            }
        }

        private async Task<ICollection<string>> DownloadDates(string userId)
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

        private async Task UpdateDatesWithData(string userId, ICollection<string> existingdates, ICollection<string> localDates)
        {
            var dataKey = $"{userId}_{InfoByDateKeyEnum.DatesWithData}";

            var dates = existingdates.Union(localDates).OrderBy(i => i).ToArray();
            await cosmosManager.SetDataForUserId(userId, dataKey, dates);
            Console.WriteLine($"Uploaded {dataKey}");
        }

        private async Task Upload<T>(string userId, InfoByDateKeyEnum type, object data, string sDate)
        {
            if (sDate.CompareTo("20200925") > 0)
                return;

            var dataKey = $"{userId}_{type}_{sDate}";

            //var result = await cosmosManager.GetDataForUserId<T>(userId, dataKey);
            //if (result.found)
            //    return;

            Console.WriteLine($"Uploading {dataKey}");
            await cosmosManager.SetDataForUserId(userId, dataKey, data);
        }
    }
}