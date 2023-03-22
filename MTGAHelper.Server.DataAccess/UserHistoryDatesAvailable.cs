using MTGAHelper.Entity;
using MTGAHelper.Server.Data.CosmosDB;
using MTGAHelper.Server.Data.Files.UserHistory;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess
{
    public class UserHistoryDatesAvailable
    {
        private readonly DateTime dateCosmosDbStart = new DateTime(2019, 12, 1);

        private readonly UserHistoryDatesAvailableFromFile userHistoryDatesAvailableFromFile;
        private readonly UserDataCosmosManager userDataCosmosManager;
        private readonly UserHistoryRepositoryGeneric<List<string>> cacheDatesAvailable;

        public UserHistoryDatesAvailable(
            UserHistoryDatesAvailableFromFile userHistoryDatesAvailableFromFile,
            UserDataCosmosManager userDataCosmosManager,
            UserHistoryRepositoryGeneric<List<string>> cacheDatesAvailable
        )
        {
            this.userHistoryDatesAvailableFromFile = userHistoryDatesAvailableFromFile;
            this.userDataCosmosManager = userDataCosmosManager;
            this.cacheDatesAvailable = cacheDatesAvailable.Init(InfoByDateKeyEnum.DatesWithData.ToString(), false);
        }

        public async Task<ICollection<string>> GetDatesOldestFirst(string userId)
        {
            return (await GetDates(userId)).OrderBy(i => i).ToArray();
        }

        public async Task<ICollection<string>> GetDatesRecentFirst(string userId)
        {
            return (await GetDates(userId)).OrderByDescending(i => i).ToArray();
        }

        public async Task<string> GetMostRecent(string userId)
        {
            return (await GetDates(userId)).MaxBy(str => str);
        }

        private async Task<IEnumerable<string>> GetDates(string userId)
        {
            var datesFile = userHistoryDatesAvailableFromFile.GetDatesOrderedDesc(userId);

            List<string> datesCosmosDb = await cacheDatesAvailable.GetData(userId);

            if (datesCosmosDb.Count == 0)
            {
                // No dates found, OK

                ////// TEMP
                ////datesCosmosDb = new List<string> {
                ////    "20191201",
                ////    "20191202",
                ////    "20191203",
                ////    "20191204",
                ////};

                //var dateToCheckForData = DateTime.UtcNow.AddHours(12).Date;
                //while (dateToCheckForData >= dateCosmosDbStart)
                //{
                //    var dateStr = dateToCheckForData.ToString("yyyyMMdd");
                //    var dataKey = $"{userId}_{InfoByDateKeyEnum.Collection}_{dateStr}";
                //    var data = await userDataCosmosManager.GetDataForUserId<InfoByDate<Dictionary<int, int>>>(userId, dataKey);
                //    if (data.found)
                //        datesCosmosDb.Add(dateStr);

                //    dateToCheckForData = dateToCheckForData.AddDays(-1);
                //}

                //if (datesCosmosDb.Count > 0)
                //{
                //    cacheDatesAvailable.Invalidate(userId);
                //    await UpdateDatesHavingData(userId, datesCosmosDb);
                //}
            }

            return datesFile.Union(datesCosmosDb).Distinct();
        }

        public async Task UpdateDatesHavingData(string userId, List<string> newDates)
        {
            var data = newDates.OrderBy(i => i);
            var dataKey = $"{userId}_{InfoByDateKeyEnum.DatesWithData}";

            var existing = await userDataCosmosManager.GetDataForUserId<List<string>>(userId, dataKey);
            if (existing.found)
                data = existing.data.Union(newDates).Distinct().OrderBy(i => i);

            if (await userDataCosmosManager.SetDataForUserId(userId, dataKey, data.ToList()))
            {
                cacheDatesAvailable.Invalidate(userId);
            }
        }
    }
}