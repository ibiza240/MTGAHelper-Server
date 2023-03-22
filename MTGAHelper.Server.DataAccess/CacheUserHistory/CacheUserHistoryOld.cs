using MTGAHelper.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.CacheUserHistory
{
    public class CacheUserHistoryOld<T> where T : new()
    {
        private readonly UserHistoryDatesAvailable userHistoryDatesAvailable;
        private readonly UserHistoryRepositoryOld<T> cache;

        public CacheUserHistoryOld(
            UserHistoryDatesAvailable userHistoryDatesAvailable,
            UserHistoryRepositoryOld<T> cache
            )
        {
            this.userHistoryDatesAvailable = userHistoryDatesAvailable;
            this.cache = cache;
        }

        public Task<InfoByDate<T>> Get(string userId, string dateFor)
        {
            return cache.GetDataForDate(new UserHistoryKeyInput(userId, dateFor));
        }

        public async Task<InfoByDate<T>> GetLast(string userId)
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

        public async Task<IEnumerable<InfoByDate<T>>> GetAll(string userId)
        {
            // PATCH: GET ALL is only used by matches for now
            // only retrieve matches after ELD for performance
            var allDates = (await userHistoryDatesAvailable.GetDatesOldestFirst(userId))
                .Where(i => i.CompareTo("20190926") >= 0);

            var result = new List<InfoByDate<T>>();
            foreach (var d in allDates)
            {
                result.Add(await Get(userId, d));
                await Task.Delay(100);
            }

            return result;
        }

        public async Task Save(string userId, string dateFor, InfoByDate<T> newData)
        {
            await cache.Save(new UserHistoryKeyInput(userId, dateFor), newData);
        }

        public async Task InvalidateAll(string userId)
        {
            var allDates = await userHistoryDatesAvailable.GetDatesOldestFirst(userId);
            foreach (var d in allDates)
                Invalidate(userId, d);
        }
    }
}