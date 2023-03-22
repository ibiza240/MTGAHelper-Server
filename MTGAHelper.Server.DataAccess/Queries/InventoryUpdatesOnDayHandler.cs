using MTGAHelper.Entity;
using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class InventoryUpdatesOnDayHandler : IQueryHandler<InventoryUpdatesOnDayQuery, InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>>
    {
        private readonly CacheUserHistory<InventoryUpdatedRaw> cacheInventory;

        public InventoryUpdatesOnDayHandler(
            CacheUserHistory<InventoryUpdatedRaw> cacheInventory)
        {
            this.cacheInventory = cacheInventory;
        }

        public async Task<InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>> Handle(InventoryUpdatesOnDayQuery query)
        {
            var infoByDate = await cacheInventory.Get(query.UserId, query.Date.ToString("yyyyMMdd"));
            return infoByDate;
        }
    }
}