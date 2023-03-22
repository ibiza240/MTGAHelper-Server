using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class MatchesOnDayHandler : IQueryHandler<MatchesOnDayQuery, IReadOnlyList<MatchResult>>
    {
        private readonly CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches;

        public MatchesOnDayHandler(
            CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches)
        {
            this.cacheUserHistoryMatches = cacheUserHistoryMatches;
        }

        public async Task<IReadOnlyList<MatchResult>> Handle(MatchesOnDayQuery query)
        {
            var matches = await cacheUserHistoryMatches.Get(query.UserId, query.Date.ToString("yyyyMMdd"));
            return matches.Info
                .Where(i => i.DeckUsed != default)
                .Where(i => i.SecondsCount > 0)
                .OrderByDescending(i => i.StartDateTime)
                .ToArray();
        }
    }
}