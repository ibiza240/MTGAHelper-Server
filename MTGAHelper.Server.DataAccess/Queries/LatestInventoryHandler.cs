using MTGAHelper.Entity;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestInventoryHandler : IQueryHandler<LatestInventoryQuery, Inventory>
    {
        private readonly UserHistoryRepositoryGeneric<Inventory> cacheUserHistoryInventoryIntraday;
        private readonly UserHistoryDatesAvailable userHistoryDatesAvailable;
        private readonly CacheUserHistory<Inventory> cacheUserHistoryInventoryIntradayOld;

        public LatestInventoryHandler(
            UserHistoryRepositoryGeneric<Inventory> cacheUserHistoryInventoryIntraday,
            UserHistoryDatesAvailable userHistoryDatesAvailable,
            CacheUserHistory<Inventory> cacheUserHistoryInventoryIntradayOld
            )
        {
            this.cacheUserHistoryInventoryIntraday = cacheUserHistoryInventoryIntraday.Init(InfoByDateKeyEnum.InventoryIntraday.ToString(), false);
            this.userHistoryDatesAvailable = userHistoryDatesAvailable;
            this.cacheUserHistoryInventoryIntradayOld = cacheUserHistoryInventoryIntradayOld;
        }

        public async Task<Inventory> Handle(LatestInventoryQuery query)
        {
            var info = (await cacheUserHistoryInventoryIntraday.GetData(query.UserId));

            if (info.Gold == 0 && info.Gems == 0 && info.VaultProgress == 0 && info.Xp == 0 && info.Wildcards.All(i => i.Value == 0))
            {
                // TEMP!!! While transitioning from inventory stored daily
                // to latest inventory stored only
                var dateString = DateTime.Now.ToString("yyyyMMdd");
                var datesAvailable = (await userHistoryDatesAvailable.GetDatesRecentFirst(query.UserId))
                    .Where(i => i.CompareTo(dateString) <= 0)
                    .OrderByDescending(i => i);

                foreach (var currentDateStr in datesAvailable)
                {
                    var info2 = (await cacheUserHistoryInventoryIntradayOld.Get(query.UserId, currentDateStr)).Info;

                    if (info2 == null || info2.Count == 0)
                        continue;

                    var kvp = info2.Last();
                    return kvp.Value;
                }
                return new Inventory();
            }

            return info;
        }
    }
}