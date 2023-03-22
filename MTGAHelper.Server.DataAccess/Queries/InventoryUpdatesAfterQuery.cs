using MTGAHelper.Entity.OutputLogParsing;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class InventoryUpdatesAfterQuery : IQuery<IEnumerable<(DateTime, InventoryUpdatedRaw)>>
    {
        public InventoryUpdatesAfterQuery(string userId, DateTime dateTime)
        {
            UserId = userId;
            DateTime = dateTime;
        }

        public string UserId { get; }
        public DateTime DateTime { get; }
    }
}
