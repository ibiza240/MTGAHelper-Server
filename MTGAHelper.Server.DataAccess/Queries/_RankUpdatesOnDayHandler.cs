//using MTGAHelper.Entity;
//using MTGAHelper.Entity.OutputLogParsing;
//using MTGAHelper.Server.DataAccess.CacheUserHistory;
//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace MTGAHelper.Server.DataAccess.Queries
//{
//    public class RankUpdatesOnDayHandler : IQueryHandler<RankUpdatesOnDayQuery, InfoByDate<Dictionary<DateTime, RankUpdatedRaw>>>
//    {
//        private readonly CacheUserHistory<RankUpdatedRaw> cacheUserHistoryRankUpdated;

//        public RankUpdatesOnDayHandler(
//            CacheUserHistory<RankUpdatedRaw> cacheUserHistoryRankUpdated)
//        {
//            this.cacheUserHistoryRankUpdated = cacheUserHistoryRankUpdated;
//        }

//        public async Task<InfoByDate<Dictionary<DateTime, RankUpdatedRaw>>> Handle(RankUpdatesOnDayQuery query)
//        {
//            return await cacheUserHistoryRankUpdated.Get(query.UserId, query.Day);
//        }
//    }
//}