using MTGAHelper.Entity;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestPlayerProgressQuery : IQuery<InfoByDate<IReadOnlyDictionary<string, PlayerProgress>>>
    {
        public LatestPlayerProgressQuery(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
}
