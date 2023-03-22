using MTGAHelper.Entity;
using MTGAHelper.Entity.OutputLogParsing;
using System;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class InventoryUpdatesOnDayQuery : IQuery<InfoByDate<Dictionary<DateTime, InventoryUpdatedRaw>>>
    {
        public InventoryUpdatesOnDayQuery(string userId, DateTime date)
        {
            UserId = userId;
            Date = date;
        }

        public string UserId { get; }
        public DateTime Date { get; }
    }
}
