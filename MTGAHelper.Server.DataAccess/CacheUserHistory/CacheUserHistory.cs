using MTGAHelper.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.CacheUserHistory
{
    public class CacheUserHistory<T> where T : new()
    {
        private readonly UserHistoryDatesAvailable userHistoryDatesAvailable;
        private readonly UserHistoryRepository<T> cache;

        public CacheUserHistory(
            UserHistoryDatesAvailable userHistoryDatesAvailable,
            UserHistoryRepository<T> cache
            )
        {
            this.userHistoryDatesAvailable = userHistoryDatesAvailable;
            this.cache = cache;
        }

        public async Task<InfoByDate<Dictionary<DateTime, T>>> Get(string userId, string dateFor)
        {
            //if (dateFor == "20191217") System.Diagnostics.Debugger.Break();

            return await cache.GetDataForDate(new UserHistoryKeyInput(userId, dateFor));
        }

        public async Task<KeyValuePair<DateTime, T>> GetLatest(string userId)
        {
            var last = await GetLastDateInfo(userId);
            return last.Info.MaxBy(i => i.Key);
        }

        public async Task<InfoByDate<Dictionary<DateTime, T>>> GetLastDateInfo(string userId)
        {
            var lastDate = await userHistoryDatesAvailable.GetMostRecent(userId);
            //if (lastDate != default(string))
            var result = await Get(userId, lastDate);

            return result;
        }

        private void Invalidate(string userId, string dateFor)
        {
            cache.Invalidate(new UserHistoryKeyInput(userId, dateFor));
        }

        public async Task InvalidateAll(string userId)
        {
            var allDates = await userHistoryDatesAvailable.GetDatesOldestFirst(userId);
            foreach (var d in allDates)
                Invalidate(userId, d);
        }

        public async Task Save(string userId, string dateFor, InfoByDate<Dictionary<DateTime, T>> newData)
        {
            await cache.SaveToDisk(new UserHistoryKeyInput(userId, dateFor), newData);
        }
    }
}