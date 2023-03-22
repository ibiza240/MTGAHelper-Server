using MTGAHelper.Entity;
using System;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestInventoryQuery : IQuery<Inventory>
    {
        public LatestInventoryQuery(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
}