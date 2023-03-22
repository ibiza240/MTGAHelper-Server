using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class InventoryUpdatesAfterHandler : IQueryHandler<InventoryUpdatesAfterQuery, IEnumerable<(DateTime, InventoryUpdatedRaw)>>
    {
        private readonly CacheUserHistory<InventoryUpdatedRaw> cacheInventory;
        private readonly UserHistoryDatesAvailable datesAvailable;

        public InventoryUpdatesAfterHandler(
            CacheUserHistory<InventoryUpdatedRaw> cacheInventory,
            UserHistoryDatesAvailable datesAvailable)
        {
            this.cacheInventory = cacheInventory;
            this.datesAvailable = datesAvailable;
        }

        public async Task<IEnumerable<(DateTime, InventoryUpdatedRaw)>> Handle(InventoryUpdatesAfterQuery query)
        {
            var fromStr = query.DateTime.ToString("yyyyMMdd");
            var datesToLoad = (await datesAvailable.GetDatesOldestFirst(query.UserId))
                .Where(i => i.CompareTo(fromStr) >= 0);

            var postMatchUpdateDays = await Task.WhenAll(datesToLoad.Select(dateFor => cacheInventory.Get(query.UserId, dateFor)));
            return postMatchUpdateDays
                .SelectMany(d => d.Info)
                .Where(kvp => kvp.Key >= query.DateTime)
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToArray();
        }
    }
}