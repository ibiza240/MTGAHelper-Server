using Microsoft.Azure.Cosmos;
using MTGAHelper.Server.Data.CosmosDB;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTGAHelper.Tools.CosmosDB.Downloader.v2
{
    public class asd
    {
        public string key { get; set; }
        public string date { get; set; }
    }

    public class DownloaderAll : BaseDownloader
    {
        private readonly UserDataCosmosManager2 userDataCosmosManager2;
        private ICollection<string> supportersUserIds;

        public DownloaderAll(
            UserDataCosmosManager userDataCosmosManager,
            UserDataCosmosManager2 userDataCosmosManager2
            )
            : base(userDataCosmosManager)
        {
            this.userDataCosmosManager2 = userDataCosmosManager2;
        }

        public async Task Download(string folderOutputCosmosData)
        {
            var downloader = await userDataCosmosManager2.Init(folderOutputCosmosData);
            await downloader.DownloadAll();
        }

        public void GatherStats(ICollection<string> supportersUserIds, string folderOutputCosmosData, DateTime deleteOlderThan)
        {
            this.supportersUserIds = supportersUserIds;
            var foldersUserId = Directory.GetDirectories(folderOutputCosmosData);

            int x = 0;

            var lst = new List<asd>();
            foreach (var file in foldersUserId)
            {
                foreach (var file2 in Directory.GetDirectories(file))
                {
                    foreach (var file3 in Directory.GetFiles(file2))
                    {
                        var parts = Path.GetFileNameWithoutExtension(file3).Split("_");
                        var tt = new asd
                        {
                            key = parts[1],
                            date = parts.Length == 3 ? parts[2] : default(string),
                        };
                        lst.Add(tt);
                    }
                }

                x++;
                if (x % 2000 == 0)
                {
                    var allFiles = lst
                        .OrderByDescending(i => i.date)
                        .GroupBy(i => i.key)
                        .Select(i => new
                        {
                            i.Key,
                            i.First().date,
                            count = i.Count(),
                        })
                        .OrderBy(i => i.Key)
                        .ToArray()
                        ;

                    foreach (var t in allFiles)
                    {
                        Console.WriteLine($"{t.Key}\t\t{t.date}\t\t{t.count}");
                    }
                    Console.WriteLine();
                }
            }
        }

        private static string regexDatePattern = @"^(.{32})_.+?(?:_(\d{4})(\d{2})(\d{2}))?$";
        private static Regex regexDate = new Regex(regexDatePattern, RegexOptions.Compiled);

        internal async Task DeleteOlderThan2(ICollection<string> supportersUserIds, string folderOutputCosmosData, DateTime deleteOlderThan)
        {
            var userFolders = Directory.GetDirectories(folderOutputCosmosData);
            foreach (var folder in userFolders)
            {
                try
                {
                    var userFiles = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
                    var filesToDelete = userFiles
                        // Do not delete any file which belongs to a supporter
                        .Where(i => supportersUserIds.All(x => i.Contains(x) == false))
                        .Select(i => new
                        {
                            key = Path.GetFileNameWithoutExtension(i),
                            match = regexDate.Match(Path.GetFileNameWithoutExtension(i)),
                        })
                        // Do not delete any files not old enough
                        .Where(i =>
                        {
                            if (i.match.Success == false)
                            {
                                Console.WriteLine($"regex broke for {i.key}");
                                return false;
                            }

                            if (i.match.Groups.Count < 4 || int.TryParse(i.match.Groups[4].Value, out _) == false)
                                return false;

                            var dateFound = new DateTime(int.Parse(i.match.Groups[2].Value), int.Parse(i.match.Groups[3].Value), int.Parse(i.match.Groups[4].Value));
                            return dateFound < deleteOlderThan;
                        })
                        .ToArray();

                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            var userId = file.match.Groups[1].Value;

                            Console.WriteLine($"Deleting {file.key}");
                            await DeleteData<string>(userId, file.key);

                            var dateStr = file.key.Substring(file.key.Length - 8).All(i => int.TryParse(i.ToString(), out _)) ? file.key.Substring(file.key.Length - 8) : "";
                            var source = Path.Combine(folder, dateStr, $"{file.key}.txt");
                            var folderDestination = Path.Combine(@"D:\repos-data\MTGAHelper\bak\20220319\data\cosmos", userId, dateStr);
                            var destination = Path.Combine(folderDestination, $"{file.key}.json");
                            Directory.CreateDirectory(folderDestination);
                            File.Move(source, destination);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debugger.Break();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
        }

        public async Task DeleteOlderThan(ICollection<string> supportersUserIds, string folderOutputCosmosData, DateTime deleteOlderThan)
        {
            this.supportersUserIds = supportersUserIds;

            var processor = new UserProcessor(folderOutputCosmosData, userDataCosmosManager, userDataCosmosManager2, supportersUserIds);

            var foldersUserId = Directory.GetDirectories(folderOutputCosmosData);

            foreach (var folderUserId in foldersUserId)
            {
                Console.WriteLine(folderUserId);

                // Keep track of data for dates
                var dictDataByDay = new HashSet<string>();

                var isSupporter = supportersUserIds.Contains(Path.GetFileNameWithoutExtension(folderUserId));
                //if (isSupporter)
                //    System.Diagnostics.Debugger.Break();

                // Keep track of files with dates
                var dictDays = new Dictionary<string, int>();

                // Clean dates
                var foldersDate = Directory.GetDirectories(folderUserId);

                //if (foldersDate.Any(i => System.Text.RegularExpressions.Regex.Match(i, "(?:2021....|202009..|202010..|202011..|202012..)$").Success))
                //    System.Diagnostics.Debugger.Break();

                foreach (var folderDate in foldersDate)
                {
                    dictDataByDay.Add(folderDate);

                    var filesForDate = Directory.GetFiles(folderDate);
                    dictDays[folderDate] = filesForDate.Length;
                    foreach (var fileDataWithDate in filesForDate)
                    {
                        var deleted = await DeleteIfNeeded(deleteOlderThan, isSupporter, foldersDate, fileDataWithDate);
                        if (deleted)
                            dictDays[folderDate]--;
                    }

                    if (dictDays[folderDate] == 0)
                        dictDataByDay.Remove(folderDate);
                }

                // Clean non dates files if required
                if (dictDataByDay.Count == 0)
                {
                    foreach (var fileData in Directory.GetFiles(folderUserId))
                    {
                        await DeleteIfNeeded(deleteOlderThan, isSupporter, Array.Empty<string>(), fileData);
                    }
                }
            }
        }

        private async Task<bool> DeleteIfNeeded(DateTime deleteOlderThan, bool isSupporter, string[] foldersDate, string file)
        {
            var key = Path.GetFileNameWithoutExtension(file);
            var parts = key.Split("_");

            var userId = parts[0];

            var dataType = parts[1];
            var date = parts.Length == 3 ? parts[2] : default(string);

            var delete = IsObsolete(isSupporter, foldersDate.Length, dataType, date, deleteOlderThan);

            if (delete)
            {
                await DeleteData<string>(userId, key);
            }

            return delete;
        }

        private bool IsObsolete(bool isSupporter, int nbFoldersDate, string dataType, string date, DateTime dateThreshold)
        {
            // Always delete data for users which have no data for any days
            if (nbFoldersDate == 0)
            {
                //if (date == default)
                //    System.Diagnostics.Debugger.Break();

                return true;
            }

            // If there is data with date existing, never delete the data without date
            if (date == default)
                return false;

            var deprecated = new[]
            {
                "1",
                "10",
                "11",
                "12",
                "13",
                "14",
                "15",
                "16",
                "17",
                "18",
                "2",
                "21",
                "3",
                "4",
                "7",
                "8",
                "9",
                "Collection",
                "CollectionIntraday",
                "CrackedBoosters",
                "DraftPickProgress",
                "Inventory",
                "InventoryIntraday",
                "PayEntry",
                "PlayerQuests",
                "PlayerProgress",
                "PlayerProgressIntraday",
                "RankUpdated",
                "VaultsOpened",
            };
            // Always delete deprecated files
            if (int.TryParse(dataType, out int _) || deprecated.Contains(dataType))
                return true;

            // Never delete supporter data
            if (isSupporter)
                return false;

            // Never delete recent data
            var isDateRecent = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture) >= dateThreshold;
            return isDateRecent == false;
        }

        private async Task DeleteData<T>(string userId, string dataKey)
        {
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
                    if (fails > 10)
                    {
                        Console.WriteLine($"~~~ DeleteDataForUserId FAILED {dataKey}");
                        return;
                    }
                    else
                    {
                        Console.WriteLine($"warn: Retry DeleteDataForUserId {dataKey}");
                        await Task.Delay(10000);
                    }
                }

                await Task.Delay(20);
            }
        }
    }
}