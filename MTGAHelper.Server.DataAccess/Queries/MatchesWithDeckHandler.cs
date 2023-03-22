using MTGAHelper.Entity.MtgaOutputLog;
using MTGAHelper.Server.DataAccess.CacheUserHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MTGAHelper.Server.DataAccess.Queries
{
    public class MatchesWithDeckHandler : IQueryHandler<MatchesWithDeckQuery, IEnumerable<MatchResult>>
    {
        private readonly CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches;

        public MatchesWithDeckHandler(
            CacheUserHistoryOld<List<MatchResult>> cacheUserHistoryMatches)
        {
            this.cacheUserHistoryMatches = cacheUserHistoryMatches;
        }

        public async Task<IEnumerable<MatchResult>> Handle(MatchesWithDeckQuery query)
        {
            var date = query.DateStart.Date;
            var dateMax = DateTime.Now.Date.AddDays(1);
            var data = new List<MatchResult>();
            while (date < dateMax)
            {
                var dataForDate = await cacheUserHistoryMatches.Get(query.UserId, date.ToString("yyyyMMdd"));
                data.AddRange(dataForDate.Info);
                date = date.AddDays(1);
            }

            return data.Where(i => i.DeckUsed?.Id == query.DeckId);
        }
    }
}