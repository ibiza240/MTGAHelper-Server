using MTGAHelper.Entity.OutputLogParsing;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class PostMatchUpdatesOnDayHandler : IQueryHandler<PostMatchUpdatesOnDayQuery, IReadOnlyDictionary<DateTime, PostMatchUpdateRaw>>
    {
        private readonly CacheUserHistory<PostMatchUpdateRaw> matchRepo;

        public PostMatchUpdatesOnDayHandler(CacheUserHistory<PostMatchUpdateRaw> matchRepo)
        {
            this.matchRepo = matchRepo;
        }

        public async Task<IReadOnlyDictionary<DateTime, PostMatchUpdateRaw>> Handle(PostMatchUpdatesOnDayQuery query)
        {
            var dateFor = query.Date.ToString("yyyMMdd");

            var postMatchUpdates = await matchRepo.Get(query.UserId, dateFor);
            return postMatchUpdates.Info;
        }
    }
}