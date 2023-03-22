using MTGAHelper.Entity.Config.Users;
using System.Collections.Generic;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestRankQuery : IQuery<IReadOnlyList<ConfigModelRankInfo>>
    {
        public LatestRankQuery(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }
    }
}
