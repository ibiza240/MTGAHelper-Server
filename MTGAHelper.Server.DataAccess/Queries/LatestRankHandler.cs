using MTGAHelper.Entity.Config.Users;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestRankHandler : IQueryHandler<LatestRankQuery, IReadOnlyList<ConfigModelRankInfo>>
    {
        private readonly CacheUserHistoryOld<List<ConfigModelRankInfo>> cache;

        public LatestRankHandler(CacheUserHistoryOld<List<ConfigModelRankInfo>> cache)
        {
            this.cache = cache;
        }

        public async Task<IReadOnlyList<ConfigModelRankInfo>> Handle(LatestRankQuery query)
        {
            var infoByDate = await cache.GetLast(query.UserId);
            return infoByDate.Info.AsReadOnly();
        }
    }
}