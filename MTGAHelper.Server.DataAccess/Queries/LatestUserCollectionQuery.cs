using MTGAHelper.Entity;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestUserCollectionQuery : IQuery<InfoByDate<IReadOnlyDictionary<int, int>>>
    {
        public LatestUserCollectionQuery(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
}