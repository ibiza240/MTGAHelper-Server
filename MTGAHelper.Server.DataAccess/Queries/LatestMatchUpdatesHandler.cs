using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class LatestMatchUpdatesHandler : IQueryHandler<LatestMatchUpdatesQuery, IReadOnlyDictionary<DateTime, PostMatchUpdateRaw>>
    {
        private readonly CacheUserHistory<PostMatchUpdateRaw> cacheUserHistoryPostMatchUpdates;

        public LatestMatchUpdatesHandler(CacheUserHistory<PostMatchUpdateRaw> cacheUserHistoryPostMatchUpdates)
        {
            this.cacheUserHistoryPostMatchUpdates = cacheUserHistoryPostMatchUpdates;
        }

        public async Task<IReadOnlyDictionary<DateTime, PostMatchUpdateRaw>> Handle(LatestMatchUpdatesQuery query)
        {
            var infoByDate = await cacheUserHistoryPostMatchUpdates.GetLastDateInfo(query.UserId);
            return infoByDate.Info;
        }
    }
}